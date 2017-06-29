using Microsoft.VisualStudio.TestTools.UnitTesting;
using SocialTalents.Hp.Events;
using SocialTalents.Hp.Events.Queue;
using SocialTalents.Hp.UnitTests.Events.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialTalents.Hp.UnitTests.Events
{
    [TestClass]
    public class EventQueueTest : ICanPublish<TestEvent>, ICanPublish<UniqueEvent>
    {
        [TestInitialize]
        public void EventQueueTestInit()
        {
            bus = new EventBusService();
            repository = new InMemoryQueueRepository();
            testService = new EventQueueService(repository, bus);
        }

        InMemoryQueueRepository repository;
        EventQueueService testService;
        EventBusService bus;

        [TestMethod]
        public void Queue_ResultTest()
        {
            ProcessEventsResult result = new ProcessEventsResult();
            DateTime started = result.Started;
            result.Processed++;
            Task.Delay(1).Wait();
            // Verifying that started not changed
            Assert.AreEqual(started, result.Started);
            Assert.AreEqual(1, result.Processed);
        }

        [TestMethod]
        public void Queue_ProcessEmpty()
        {
            var result = testService.ProcessEvents();
            Assert.AreEqual(0, result.Processed);
        }

        [TestMethod]
        public void Queue_ProcessingTimelimit()
        {
            // Enqueue events for further handling
            bus.Subscribe(testService.Enque<TestEvent>());

            // Subscribe handler which waits 50 ms
            bus.Subscribe<QueuedEvent<TestEvent>>((e) => Task.Delay(50).Wait());

            for (int i = 0; i < 10; i++) { bus.Publish(new TestEvent(), this); }

            // expect to handle no more than 5 events
            testService.ProcessingTimeLimit = TimeSpan.FromMilliseconds(249);
            var result = testService.ProcessEvents();
            Console.Write($"Number of items processed: {result.Processed}");
            Assert.IsTrue(4 == result.Processed || 5 == result.Processed);
        }

        [TestMethod]
        public void Queue_UniqueEventsDeduped()
        {
            int counter = 0;

            bus.Subscribe(testService.Enque<UniqueEvent>());
            Delegate<UniqueEvent> handler = (e) => counter++;
            bus.Subscribe(handler.AsQueued());

            for (int i = 0; i < 10; i++) { bus.Publish(new UniqueEvent() { UniqueKey = (i % 2).ToString() }, this); }

            // expect to handle 2 events = all other get deduped
            var result = testService.ProcessEvents();
            Assert.AreEqual(2, result.Processed);

            result = testService.ProcessEvents();
            Assert.AreEqual(0, result.Processed);
        }

        [TestMethod]
        public void Queue_Processing_WithOnFail()
        {
            // Enqueue events for further handling
            bus.Subscribe(testService.Enque<TestEvent>());

            int counter = 0;
            int backofIntervalMs = 50;
            // Subscribe handler which waits 50 ms
            Delegate<TestEvent> handler = (e) => { counter++; throw new NotImplementedException(); };
            bus.Subscribe(handler.AsQueued().RetryQueued(3,
                Backoff.Fixed(TimeSpan.FromMilliseconds(backofIntervalMs))
                ));

            bus.Publish(new TestEvent(), this);

            DateTime start = DateTime.Now;
            while (counter <= 3)
            {
                testService.ProcessEvents();
            }
            Assert.IsTrue(DateTime.Now.Subtract(start).TotalMilliseconds > 3 * backofIntervalMs);
        }

        [TestMethod]
        public void Queue_Processing_HandlingExceptions()
        {
            // Enqueue events for further handling
            bus.Subscribe(testService.Enque<TestEvent>());

            StringBuilder log = new StringBuilder();
            int registeredFailures = 0;
            // Subscribe handler which waits 50 ms
            Delegate<TestEvent> handler = (e) => { log.Append("MainHandler|"); throw new NotImplementedException(); };
            bus.Subscribe(
                handler
                .AsQueued().RetryQueued(3, Backoff.None())
                .AddOnFail<QueuedEvent<TestEvent>, NotImplementedException>((failedEvent) => log.Append("FailureHandler|"))
                );

            bus.Publish(new TestEvent(), this);

            while (testService.ProcessEvents().Processed > 0) { Task.Delay(1).Wait(); };
            Assert.AreEqual("MainHandler|MainHandler|MainHandler|MainHandler|FailureHandler|", log.ToString());
        }

        [TestMethod]
        public void ExponentialBackoff_CannotBeZero()
        {
            var backoffAction = Backoff.ExponentialBackoff(TimeSpan.FromSeconds(2), 5);
            var initialDateTime = DateTime.Now;

            InMemoryQueueItem item = new InMemoryQueueItem();
            for(int i = 0; i < (5 * 2^5); i++)
            {
                item.HandleAfter = initialDateTime;
                backoffAction(item);
                Assert.IsTrue(item.HandleAfter.Subtract(initialDateTime).TotalSeconds > 1);
                Assert.IsTrue(item.HandleAfter.Subtract(initialDateTime).TotalSeconds < (2^5));
            }
        }
    }
}

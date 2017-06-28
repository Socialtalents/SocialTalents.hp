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
        [TestMethod]
        public void Queue_ProcessEmpty()
        {
            InMemoryQueueRepository repository = new InMemoryQueueRepository();
            EventQueueService testService = new EventQueueService(repository, null);

            var result = testService.ProcessEvents();
            Assert.AreEqual(0, result.Processed);
        }

        [TestMethod]
        public void Queue_ProcessingTimelimit()
        {
            var bus = new EventBusService();
            InMemoryQueueRepository repository = new InMemoryQueueRepository();
            EventQueueService testService = new EventQueueService(repository, bus);

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
            var bus = new EventBusService();
            InMemoryQueueRepository repository = new InMemoryQueueRepository();
            EventQueueService testService = new EventQueueService(repository, bus);

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
            var bus = new EventBusService();
            InMemoryQueueRepository repository = new InMemoryQueueRepository();
            EventQueueService testService = new EventQueueService(repository, bus);

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
            while(counter <= 3)
            {
                testService.ProcessEvents();
            }
            Assert.IsTrue(DateTime.Now.Subtract(start).TotalMilliseconds > 3 * backofIntervalMs);
        }
    }
}

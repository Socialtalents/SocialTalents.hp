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
    public class FailingEventQueueTests: ICanPublish<TestEvent>
    {
        [TestInitialize]
        public void EventQueueTestInit()
        {
            bus = new EventBusService();
            repository = new InMemoryQueueRepository();
            testService = new EventQueueServiceWithDeserializationFailure(repository, bus);
        }

        InMemoryQueueRepository repository;
        EventQueueService testService;
        EventBusService bus;

        [TestMethod]
        public void Queue_Processing_CorruptedEvent()
        {
            // Enqueue events for further handling
            bus.Subscribe(testService.Enque<TestEvent>());

            // except test to throw Exception event
            Exception exception = null;
            bus.Subscribe<Exception>(e => 
                exception = e);

            int counter = 0;
            int backofIntervalMs = 50;
            // Subscribe handler which waits 50 ms
            Delegate<TestEvent> handler = (e) => { counter++; throw new NotImplementedException(); };
            bus.Subscribe(handler.WhenQueued().RetryQueued(3,
                Backoff.Fixed(TimeSpan.FromMilliseconds(backofIntervalMs))
                ));

            bus.Publish(new TestEvent(), this);

            // There is event to handle
            Assert.AreEqual(1, repository.Queue.Where(e => e.DeclaringEventType == typeof(TestEvent).AssemblyQualifiedName).Count());

            testService.ProcessEvents();

            Assert.AreEqual(0, repository.Queue.Where(e => e.DeclaringEventType == typeof(TestEvent).ToString()).Count());
            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof(FormatException));
        }

    }

    // This class simulate failure to deserialize event details
    public class EventQueueServiceWithDeserializationFailure : EventQueueService
    {
        public EventQueueServiceWithDeserializationFailure(IEventQueueRepository repository, EventBusService eventBusService = null) : base(repository, eventBusService) { }

        protected override object BuildGenericQueueEvent(IQueueItem eventInstance)
        {
            throw new FormatException();
        }
    }
}

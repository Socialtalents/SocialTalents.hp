using Microsoft.VisualStudio.TestTools.UnitTesting;
using SocialTalents.Hp.Events;
using SocialTalents.Hp.Events.Exceptions;
using SocialTalents.Hp.Events.Internal;
using SocialTalents.Hp.UnitTest.Events.Internal;
using System;
using System.Diagnostics;

namespace SocialTalents.Hp.UnitTest.Events
{
    [TestClass]
    public class EventsTest : ICanPublish<TestEvent>
    {
        [TestMethod]
        public void Events_MultipleSubscribtions_HappyCase()
        {
            bool subscription1 = false;
            bool subscription2 = false;
            EventBus.Subscribe<TestEvent>((args) => { subscription1 = true; });
            EventBus.Subscribe<TestEvent>((args) => { subscription2 = true; });

            EventBus.Publish<TestEvent>(new TestEvent(), this);

            Assert.IsTrue(subscription1);
            Assert.IsTrue(subscription2);
        }
        
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Events_MaxSubscriptions_ValidationFails()
        {
            for (int i = 0; i < EventBus.Default.MaxSubscriptionsPerEventType + 1; i++)
                EventBus.Subscribe<TestEvent>(
                    (result) => { }
                );
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Events_NoSender_ValidationException()
        {
            EventBus.Publish(new EventArgs(), null);
        }

        [TestMethod]
        public void Events_Performance_AtLeastMillionOpPerSecond()
        {
            int counter = 0;

            EventBus.Subscribe<TestEvent>((ev) => counter++);

            Stopwatch w = new Stopwatch();
            TestEvent e = new TestEvent();
            w.Start();
            while (w.ElapsedMilliseconds < 50)
            {
                for (int i = 0; i < 10000; i++)
                {
                    EventBus.Publish(e, this);
                    EventBus.Publish(e, this);
                    EventBus.Publish(e, this);
                    EventBus.Publish(e, this);
                    EventBus.Publish(e, this);
                    EventBus.Publish(e, this);
                    EventBus.Publish(e, this);
                    EventBus.Publish(e, this);
                }
            }
            w.Stop();
            // counter / elapsed = ops/ms, * 1000 - counter / s
            long operationsPerSecond = counter * 1000 / w.ElapsedMilliseconds;
            Console.WriteLine(string.Format("{0} op/sec, {1} total", operationsPerSecond, counter));
            Assert.IsTrue(operationsPerSecond > 1000000);
        }
        
        [TestMethod]
        public void NoSubscribers_EventAndCounterWorks()
        {
            NoSubscribers e = null;

            EventBus.Subscribe<NoSubscribers>((p) => e = p);

            TestEvent t = new TestEvent();
            EventBus.Publish(t, this);
            Assert.IsNotNull(e);
            Assert.AreEqual(t, e.LastEvent);
            Assert.AreEqual(1, e.Counter);

            ChildEvent c = new ChildEvent();
            EventBus.Publish<TestEvent>(c, this);
            Assert.IsNotNull(e);
            Assert.AreEqual(c, e.LastEvent);
            Assert.AreEqual(2, e.Counter);
        }
        
        [TestMethod]
        public void Events_AbortException_BreaksExecutionSequence()
        {
            int counter = 0;
            EventBus.Subscribe<TestEvent>(a => counter += 2);
            EventBus.Subscribe<TestEvent>(a => { throw new AbortExecutionException(); });
            EventBus.Subscribe<TestEvent>(a => counter += 3);

            EventBus.Publish(new TestEvent(), this);

            Assert.AreEqual(2, counter);
        }
        
        [TestCleanup]
        public void Setup()
        {
            EventBus.Clear();
        }
    }
}

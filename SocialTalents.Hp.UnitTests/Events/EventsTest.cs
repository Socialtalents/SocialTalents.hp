using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SocialTalents.Hp.Events;
using SocialTalents.Hp.Events.Internal;
using SocialTalents.Hp.Events.Exceptions;
using SocialTalents.Hp.UnitTest.Events.Internal;
using System.Threading;
using System.Diagnostics;

namespace SocialTalents.Hp.Events.UnitTest.Events
{
    [TestClass]
    public class EventsTest : ICanRaise<TestEvent>
    {
        [TestMethod]
        public void Req_MultipleSubsriptions()
        {
            bool subscription1 = false;
            bool subscription2 = false;
            EventBus.Subscribe<TestEvent>((args) => { subscription1 = true; });
            EventBus.Subscribe<TestEvent>((args) => { subscription2 = true; });

            EventBus.Raise<TestEvent>(new TestEvent(), this);

            Assert.IsTrue(subscription1);
            Assert.IsTrue(subscription2);
        }

        [TestMethod]
        public void Req_AsyncSubscription_HappyCase()
        {
            bool asyncSubscription = false;
            Exception ex = null;
            EventBus.Subscribe<Exception>((e) => ex = e);
            EventBus.Subscribe<TestEvent>(
                EventExtensions.Async((TestEvent args) => { Thread.Sleep(1); asyncSubscription = true; })
            );
            EventBus.Raise<TestEvent>(new TestEvent(), this);
            Assert.IsFalse(asyncSubscription);
            waitFor(() => asyncSubscription);
            Assert.IsNull(ex);
        }

        [TestMethod]
        public void Req_AsyncSubscription_WithException()
        {
            bool asyncSubscription = false;
            Exception ex = null;
            EventBus.Subscribe<Exception>((e) => ex = e);
            EventBus.Subscribe<TestEvent>(
                EventExtensions.Async((TestEvent args) => { throw new InvalidTimeZoneException(); })
            );
            EventBus.Raise<TestEvent>(new TestEvent(), this);
            Assert.IsFalse(asyncSubscription);

            waitFor(() => ex != null);

            Assert.IsFalse(asyncSubscription);
            Assert.AreEqual(ex.GetType(), typeof(InvalidTimeZoneException));
        }

        private void waitFor(Func<bool> condition)
        {
            int loops = 0;
            while (!condition() && loops < 100)
            {
                Thread.Sleep(10);
                loops++;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Req_CheckForWrongSubscriptions()
        {
            for (int i = 0; i < EventBus.MaxSubscriptionsPerEvent + 1; i++)
                EventBus.Subscribe<TestEvent>(
                    (result) => { }
                );
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Test_No_ICanRaise()
        {
            EventBus.Raise(new EventArgs(), null);
        }

        [TestMethod]
        public void Perf_BasicEventTest()
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
                    EventBus.Raise(e, this);
                    EventBus.Raise(e, this);
                    EventBus.Raise(e, this);
                    EventBus.Raise(e, this);
                    EventBus.Raise(e, this);
                    EventBus.Raise(e, this);
                    EventBus.Raise(e, this);
                    EventBus.Raise(e, this);
                }
            }
            w.Stop();
            // counter / elapsed = ops/ms, * 1000 - counter / s
            long operationsPerSecond = counter * 1000 / w.ElapsedMilliseconds;
            Console.WriteLine(string.Format("{0} op/sec, {1} total", operationsPerSecond, counter));
            Assert.IsTrue(operationsPerSecond > 1000000);
        }

        [TestMethod]
        public void Execute_Second_Step_When_First_Fails()
        {
            bool secondCall = false;
            bool exceptionRaised = false;
            WorkflowStepDelegate<TestEvent> firstHandler = (p) => { throw new InvalidOperationException(); };
            WorkflowStepDelegate<TestEvent> secondHandler = (p) => { secondCall = true; };

            EventBus.Subscribe<TestEvent>(firstHandler.AddOnFail<TestEvent, Exception>(secondHandler));


            EventBus.Subscribe<Exception>((param) => { exceptionRaised = true; });

            EventBus.Raise(new TestEvent(), this);

            Assert.IsTrue(secondCall);
            Assert.IsTrue(exceptionRaised);
        }


        [TestMethod]
        public void Execute_Second_Step_When_First_Fails_Without_Exception()
        {
            bool secondCall = false;
            bool exceptionRaised = false;

            WorkflowStepDelegate<TestEvent> firstHandler = (p) => { throw new InvalidOperationException(); };
            WorkflowStepDelegate<TestEvent> secondHandler = (p) => { secondCall = true; };

            EventBus.Subscribe<TestEvent>(
                firstHandler.AddOnFail<TestEvent, Exception>(secondHandler, false)
            );

            EventBus.Subscribe<Exception>((param) => { exceptionRaised = true; });

            EventBus.Raise(new TestEvent(), this);

            Assert.IsTrue(secondCall);
            Assert.IsFalse(exceptionRaised);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Execute_Second_Step_When_First_Fails_OptionB()
        {
            bool secondCall = false;

            WorkflowStepDelegate<TestEvent> firstHandler = (p) => { throw new InvalidOperationException(); };
            WorkflowStepDelegate<TestEvent> secondHandler = (p) => { secondCall = true; };

            EventBus.Subscribe<TestEvent>(
                firstHandler.AddOnFail<TestEvent, ArgumentException>(secondHandler)
            );

            EventBus.Raise(new TestEvent(), this);

            Assert.IsFalse(secondCall);
        }

        [TestMethod]
        public void No_Subscribers_Returned()
        {
            NoSubscribers e = null;

            EventBus.Subscribe<NoSubscribers>((p) => e = p);

            TestEvent t = new TestEvent();
            EventBus.Raise(t, this);
            Assert.IsNotNull(e);
            Assert.AreEqual(t, e.LastEvent);
            Assert.AreEqual(1, e.Counter);

            ChildEvent c = new ChildEvent();
            EventBus.Raise<TestEvent>(c, this);
            Assert.IsNotNull(e);
            Assert.AreEqual(c, e.LastEvent);
            Assert.AreEqual(2, e.Counter);
        }
        
        [TestMethod]
        public void RaiseAbortException()
        {
            int counter = 0;
            EventBus.Subscribe<TestEvent>(a => counter += 2);
            EventBus.Subscribe<TestEvent>(a => { throw new AbortWorkflowException(); });
            EventBus.Subscribe<TestEvent>(a => counter += 3);

            EventBus.Raise(new TestEvent(), this);

            Assert.AreEqual(2, counter);
        }
        
        [TestCleanup]
        public void Setup()
        {
            EventBus.Clear();
        }
    }
}

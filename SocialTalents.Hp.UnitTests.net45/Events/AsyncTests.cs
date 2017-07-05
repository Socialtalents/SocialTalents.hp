using Microsoft.VisualStudio.TestTools.UnitTesting;
using SocialTalents.Hp.Events;
using SocialTalents.Hp.UnitTests.Events.Internal;
using System;
using System.Threading;

namespace SocialTalents.Hp.UnitTests.Events
{
    [TestClass]
    public class AsyncTests : ICanPublish<TestEvent>
    {
        [TestMethod]
        public void Async_HappyCase()
        {
            bool asyncSubscription = false;
            Exception ex = null;
            EventBus.Subscribe<Exception>((e) => ex = e);
            Delegate<TestEvent> handler = (TestEvent args) => { Thread.Sleep(1); asyncSubscription = true; };
            EventBus.Subscribe(handler.Async());
            EventBus.Publish(new TestEvent(), this);
            Assert.IsFalse(asyncSubscription);
            waitFor(() => asyncSubscription);
            Assert.IsNull(ex);
        }

        [TestMethod]
        public void Async_ExceptionInHandler_ThrowsException()
        {
            bool asyncSubscription = false;
            Exception ex = null;
            EventBus.Subscribe<Exception>((e) => ex = e);
            Delegate<TestEvent> handler = (TestEvent args) => { throw new InvalidOperationException(); };
            EventBus.Subscribe(
                handler.Async()
            );
            EventBus.Publish<TestEvent>(new TestEvent(), this);
            Assert.IsFalse(asyncSubscription);

            waitFor(() => ex != null);

            Assert.IsFalse(asyncSubscription);
            Assert.AreEqual(ex.GetType(), typeof(InvalidOperationException));
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

        [TestCleanup]
        public void Setup()
        {
            EventBus.Clear();
        }
    }
}

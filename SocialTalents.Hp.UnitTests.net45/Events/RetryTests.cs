using Microsoft.VisualStudio.TestTools.UnitTesting;
using SocialTalents.Hp.Events;
using SocialTalents.Hp.Events.Exceptions;
using SocialTalents.Hp.UnitTest.Events.Internal;
using System;
using System.Diagnostics;

namespace SocialTalents.Hp.UnitTests.Events
{
    [TestClass]
    public class RetryTests : ICanPublish<TestEvent>
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Retry_ZeroTimes_ValidationException()
        {
            Delegate<TestEvent> eventHandler = (args) => { };
            EventBus.Subscribe<TestEvent>(eventHandler.Retry<TestEvent, Exception>(0));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Retry_NegativeInterval_ValidationException()
        {
            Delegate<TestEvent> eventHandler = (args) => { };
            EventBus.Subscribe<TestEvent>(eventHandler.Retry<TestEvent, Exception>(1, TimeSpan.FromMinutes(-1)));
        }

        [TestMethod]
        public void Retry_1TimeNoErrors_ExecutedOnce()
        {
            int times = 0;
            Delegate<TestEvent> eventHandler = (args) => { times++; };
            EventBus.Subscribe(eventHandler.Retry<TestEvent, Exception>(1));

            EventBus.Publish(new TestEvent(), this);

            Assert.AreEqual(1, times);
        }

        [TestMethod]
        public void Retry_5TimesNoErrors_ExecutedOnce()
        {
            int times = 0;
            Delegate<TestEvent> eventHandler = (args) => { times++; };
            EventBus.Subscribe(eventHandler.Retry<TestEvent, Exception>(5));

            EventBus.Publish(new TestEvent(), this);

            Assert.AreEqual(1, times);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Retry_1TimesFailure_ExceptionThrown()
        {
            int times = 0;
            Delegate<TestEvent> eventHandler = (args) => { times++; throw new InvalidOperationException();  };
            EventBus.Subscribe(eventHandler.Retry<TestEvent, Exception>(1));

            try
            {
                EventBus.Publish(new TestEvent(), this);
            }
            catch (RetryFailedException ex)
            {
                Assert.AreEqual("Maximum number of attempts (1) exceeded", ex.Message);
                throw ex.InnerException;
            }
            finally
            {
                Assert.AreEqual(1, times);
            }
        }

        [TestMethod]
        public void Retry_3timesWith2FailuresWithInterval_HappyCase()
        {
            int times = 0;
            Delegate<TestEvent> eventHandler = (args) => { times++; if (times < 3) { throw new InvalidOperationException(); } };
            EventBus.Subscribe(eventHandler.Retry<TestEvent, Exception>(3, TimeSpan.FromMilliseconds(100)));

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            try
            {
                EventBus.Publish(new TestEvent(), this);
            }
            finally
            {
                Assert.AreEqual(3, times);
                Assert.IsTrue(stopwatch.ElapsedMilliseconds > 200);
            }
        }

        [TestCleanup]
        public void Setup()
        {
            EventBus.Clear();
        }
    }
}

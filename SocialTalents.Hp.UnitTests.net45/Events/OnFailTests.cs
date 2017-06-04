using Microsoft.VisualStudio.TestTools.UnitTesting;
using SocialTalents.Hp.Events;
using SocialTalents.Hp.Events.Exceptions;
using SocialTalents.Hp.UnitTest.Events.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialTalents.Hp.UnitTests.Events
{
    [TestClass]
    public class OnFailTests : ICanPublish<TestEvent>
    {
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void OnFail_ExceptionDoNotMatch_ThrowsException()
        {
            bool secondCall = false;

            Delegate<TestEvent> firstHandler = (p) => { throw new InvalidOperationException(); };
            Delegate<TestEvent> secondHandler = (p) => { secondCall = true; };

            EventBus.Subscribe<TestEvent>(
                firstHandler.AddOnFail<TestEvent, ArgumentException>(secondHandler)
            );

            EventBus.Publish(new TestEvent(), this);

            Assert.IsFalse(secondCall);
        }

        [TestMethod]
        public void OnFail_MatchingException_OnFailCalled()
        {
            bool secondCall = false;
            bool exceptionRaised = false;
            Delegate<TestEvent> firstHandler = (p) => { throw new InvalidOperationException(); };
            Delegate<TestEvent> secondHandler = (p) => { secondCall = true; };

            EventBus.Subscribe<TestEvent>(firstHandler.AddOnFail<TestEvent, Exception>(secondHandler, OnException.RaiseException));


            EventBus.Subscribe<Exception>((param) => { exceptionRaised = true; });

            EventBus.Publish(new TestEvent(), this);

            Assert.IsTrue(secondCall);
            Assert.IsTrue(exceptionRaised);
        }

        [TestMethod]
        public void OnFail_ExceptionMatchButNotReThrown_HappyCase()
        {
            bool secondCall = false;
            bool exceptionRaised = false;

            Delegate<TestEvent> firstHandler = (p) => { throw new InvalidOperationException(); };
            Delegate<TestEvent> secondHandler = (p) => { secondCall = true; };

            EventBus.Subscribe<TestEvent>(
                firstHandler.AddOnFail<TestEvent, Exception>(secondHandler)
            );

            EventBus.Subscribe<Exception>((param) => { exceptionRaised = true; });

            EventBus.Publish(new TestEvent(), this);

            Assert.IsTrue(secondCall);
            Assert.IsFalse(exceptionRaised);
        }

        [TestMethod]
        public void Integration_OnFailWithRetry_Succeeds()
        {
            int firstCall = 0;
            int secondCall = 0;

            Delegate<TestEvent> firstHandler = (p) => { firstCall++;  if (firstCall < 3) { throw new InvalidOperationException(); } };
            Delegate<TestEvent> secondHandler = (p) => { secondCall++; };

            EventBus.Subscribe<TestEvent>(
                firstHandler
                    // try and call secondHandler if failed
                    .AddOnFail<TestEvent, Exception>(secondHandler, OnException.ThrowException)
                    // and retry this up to 5 times
                    .Retry<TestEvent, Exception>(5)
            );

            EventBus.Publish(new TestEvent(), this);

            Assert.AreEqual(3, firstCall);
            Assert.AreEqual(2, secondCall);
        }

        [TestMethod]
        [ExpectedException(typeof(RetryFailedException))]
        public void Integration_RetryWithOnFail_Succeeds()
        {
            int firstCall = 0;
            int secondCall = 0;

            Delegate<TestEvent> firstHandler = (p) => { firstCall++; throw new InvalidOperationException(); };
            Delegate<TestEvent> secondHandler = (p) => { secondCall++; };

            EventBus.Subscribe<TestEvent>(
                firstHandler
                    // and retry this up to 5 times
                    .Retry<TestEvent, Exception>(5)
                    // if failed, call second handler. Note that exception wrapped in RetryFailedException 
                    .AddOnFail<TestEvent, RetryFailedException>(secondHandler, OnException.ThrowException)
            );

            try
            {
                EventBus.Publish(new TestEvent(), this);
            }
            finally
            {
                Assert.AreEqual(5, firstCall);
                Assert.AreEqual(1, secondCall);
            }
        }

        [TestCleanup]
        public void Setup()
        {
            EventBus.Clear();
        }
    }
}

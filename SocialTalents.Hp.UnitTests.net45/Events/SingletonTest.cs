using Microsoft.VisualStudio.TestTools.UnitTesting;
using SocialTalents.Hp.Events;
using SocialTalents.Hp.UnitTest.Events.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialTalents.Hp.UnitTests.Events
{
    [TestClass]
    public class SingletonTest : ICanPublish<TestEvent>
    {
        [TestMethod]
        public void Singleton_Override_Success()
        {
            string handlerResult = null;

            // first handler created in default service
            EventBus.Subscribe<TestEvent>(e => handlerResult = "handler1");

            var firstDefaultService = EventBus.Default;

            // new service with second handler
            EventBus.Default = new EventBusService();
            EventBus.Subscribe<TestEvent>(e => handlerResult = "handler2");

            EventBus.Publish(new TestEvent(), this);

            Assert.AreEqual("handler2", handlerResult);

            firstDefaultService.Publish(new TestEvent(), this);
            Assert.AreEqual("handler1", handlerResult);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Singleton_ValidateNotNull_Exception()
        {
            EventBus.Default = null;
        }
    }
}

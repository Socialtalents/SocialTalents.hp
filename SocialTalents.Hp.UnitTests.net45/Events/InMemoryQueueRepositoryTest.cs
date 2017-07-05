using Microsoft.VisualStudio.TestTools.UnitTesting;
using SocialTalents.Hp.Events.Queue;
using SocialTalents.Hp.UnitTests.Events.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SocialTalents.Hp.UnitTests.Events
{
    [TestClass]
    public class InMemoryQueueRepositoryTest
    {
        [TestMethod]
        public void QueueRepository_BasicProcessing()
        {
            TestEvent e1 = new TestEvent();
            TestEvent e2 = new TestEvent();

            InMemoryQueueRepository testRepo = new InMemoryQueueRepository();

            testRepo.AddItem(testRepo.BuildNewItem(e1));
            testRepo.AddItem(testRepo.BuildNewItem(e2));

            
            // In total we have 2 items to process at the moment
            Assert.AreEqual(2, testRepo.GetItemsToHandle().Count());

            // Get first item and "process" it
            var readedItem = testRepo.GetItemsToHandle().First();

            Assert.AreEqual(readedItem.Event, e1);
            testRepo.DeleteItem(readedItem);

            Assert.AreEqual(1, testRepo.GetItemsToHandle().Count());
            readedItem = testRepo.GetItemsToHandle().First();

            Assert.AreEqual(readedItem.Event, e2);
        }

        [TestMethod]
        public void QueueRepository_IndexProcessing()
        {
            TestEvent e1 = new TestEvent();
            TestEvent e2 = new TestEvent();

            InMemoryQueueRepository testRepo = new InMemoryQueueRepository();

            var i1 = testRepo.BuildNewItem(e1);
            var i2 = testRepo.BuildNewItem(e2);
            i1.UniqueKey = i2.UniqueKey = "1";

            testRepo.AddItem(i1);
            testRepo.AddItem(i2);

            // In total we have only 1 items to process at the moment
            Assert.AreEqual(1, testRepo.GetItemsToHandle().Count());

            // Get first item and "process" it
            var readedItem = testRepo.GetItemsToHandle().First();

            Assert.AreEqual(readedItem.Event, e1);
            testRepo.DeleteItem(readedItem);

            Assert.AreEqual(0, testRepo.GetItemsToHandle().Count());
            readedItem = testRepo.GetItemsToHandle().FirstOrDefault();

            Assert.IsNull(readedItem);
        }

        [TestMethod]
        public void QueueRepository_HandleAfter()
        {
            int sleepDelay = 200;

            TestEvent e1 = new TestEvent();
            TestEvent e2 = new TestEvent();

            InMemoryQueueRepository testRepo = new InMemoryQueueRepository();

            var i1 = testRepo.BuildNewItem(e1);
            var i2 = testRepo.BuildNewItem(e2);
            i1.HandleAfter = DateTime.Now.AddMilliseconds(sleepDelay);

            testRepo.AddItem(i1);
            testRepo.AddItem(i2);

            // In total we have only 1 items to process at the moment
            Assert.AreEqual(1, testRepo.GetItemsToHandle().Count());

            // Get first item and "process" it, it is E2 since e1 has HandleAfter set to future
            var readedItem = testRepo.GetItemsToHandle().First();

            Assert.AreEqual(readedItem.Event, e2);

            Thread.Sleep(sleepDelay);

            // First Event to handle changed to e1
            Assert.AreEqual(2, testRepo.GetItemsToHandle().Count());
            readedItem = testRepo.GetItemsToHandle().FirstOrDefault();

            Assert.AreEqual(readedItem.Event, e1);
        }
    }
}

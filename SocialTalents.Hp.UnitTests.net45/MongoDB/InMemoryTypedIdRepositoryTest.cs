using Microsoft.VisualStudio.TestTools.UnitTesting;
using SocialTalents.Hp.MongoDB;
using SocialTalents.Hp.MongoDB.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialTalents.Hp.UnitTests.MongoDB
{
    [TestClass]
    public class InMemoryTypedIdRepositoryTest
    {
        public InMemoryTypedIdRepositoryTest()
        {
            BuildRepository();
        }

        TestDatabase _testDb;
        protected IRepository<MyTestDocument> _repositoryWithOptimisticLock;
        protected IRepository<MyTestDocument> _repositoryNoLock;

        protected virtual void BuildRepository()
        {
            _testDb = new TestDatabase();
            _repositoryWithOptimisticLock = new InMemoryTypedIdRepository<MyTestDocument>(true);
            _repositoryNoLock = new InMemoryTypedIdRepository<MyTestDocument>(false);
        }

        public class MyTestDocument : TypedIdMongoDocument<MyTestDocument>
        {
            public string Message { get; set; }
        }

        [TestMethod]
        public void RepositoryWithLock_NoLock()
        {
            _repositoryWithOptimisticLock.DeleteMany(x => true); //delete all

            //any record with to help us to make sure that code updates just a single record, not all of them
            _repositoryWithOptimisticLock.Insert(new MyTestDocument { Message = "any other record" });


            var testId = Id<MyTestDocument>.GenerateNewId();
            var testEntity = new MyTestDocument { Id = testId, Message = "test" };
            _repositoryWithOptimisticLock.Insert(testEntity);

            var testEntityUpdated = new MyTestDocument
            {
                Id = testId,
                LastUpdated = testEntity.LastUpdated,
                Message = "updated"
            };
            _repositoryWithOptimisticLock.Replace(testEntityUpdated); //should pass sucessfully

            var existingEntities = _repositoryWithOptimisticLock.ToArray();

            Assert.IsNotNull(existingEntities);
            Assert.AreEqual("updated", existingEntities.Where(x => x.Id == testId).First().Message);
        }

        [TestMethod]
        public void RepositoryWithLock_NotExist()
        {
            _repositoryWithOptimisticLock.DeleteMany(x => true); //delete all

            //any record with to help us to make sure that code updates just a single record, not all of them
            _repositoryWithOptimisticLock.Insert(new MyTestDocument { Message = "any other record" });


            var testId = Id<MyTestDocument>.GenerateNewId();

            var testEntityUpdated = new MyTestDocument
            {
                Id = testId,
                Message = "updated"
            };
            _repositoryWithOptimisticLock.Replace(testEntityUpdated); //should pass sucessfully

            var existingEntities = _repositoryWithOptimisticLock.ToArray();

            Assert.IsNotNull(existingEntities);
            Assert.AreEqual(1, existingEntities.Length);
            Assert.AreEqual("any other record", existingEntities[0].Message);
        }

        [TestMethod]
        [ExpectedException(typeof(OptimistickLockException))]
        public void RepositoryWithLock_Lock()
        {
            _repositoryWithOptimisticLock.DeleteMany(x => true); //delete all

            var testId = Id<MyTestDocument>.GenerateNewId();
            var testEntity = new MyTestDocument { Id = testId, Message = "test" };
            _repositoryWithOptimisticLock.Insert(testEntity);

            var testEntityUpdated = new MyTestDocument
            {
                Id = testId,
                LastUpdated = testEntity.LastUpdated.AddMinutes(-1), //simulate that this entity has beed fetched from database a minute ago and contains obsolete data
                Message = "updated"
            };

            _repositoryWithOptimisticLock.Replace(testEntityUpdated);
        }

        [TestMethod]
        public void Repository_NoLock()
        {
            _repositoryNoLock.DeleteMany(x => true); //delete all

            //any record with to help us to make sure that code updates just a single record, not all of them
            _repositoryNoLock.Insert(new MyTestDocument { Message = "any other record" });


            var testId = Id<MyTestDocument>.GenerateNewId();
            var testEntity = new MyTestDocument { Id = testId, Message = "test" };
            _repositoryNoLock.Insert(testEntity);

            var testEntityUpdated = new MyTestDocument
            {
                Id = testId,
                LastUpdated = testEntity.LastUpdated,
                Message = "updated"
            };
            _repositoryNoLock.Replace(testEntityUpdated); //should pass sucessfully

            var existingEntities = _repositoryNoLock.ToArray();

            Assert.IsNotNull(existingEntities);
            Assert.AreEqual("updated", existingEntities.Where(x => x.Id == testId).First().Message);
        }

        [TestMethod]
        public void Repository_NotExist()
        {
            _repositoryNoLock.DeleteMany(x => true); //delete all

            //any record with to help us to make sure that code updates just a single record, not all of them
            _repositoryNoLock.Insert(new MyTestDocument { Message = "any other record" });


            var testId = Id<MyTestDocument>.GenerateNewId();

            var testEntityUpdated = new MyTestDocument
            {
                Id = testId,
                Message = "updated"
            };
            _repositoryNoLock.Replace(testEntityUpdated); //should pass sucessfully

            var existingEntities = _repositoryNoLock.ToArray();

            Assert.IsNotNull(existingEntities);
            Assert.AreEqual(1, existingEntities.Length);
            Assert.AreEqual("any other record", existingEntities[0].Message);
        }

        [TestMethod]
        public void Repository_Lock()
        {
            _repositoryNoLock.DeleteMany(x => true); //delete all

            var testId = Id<MyTestDocument>.GenerateNewId();
            var testEntity = new MyTestDocument { Id = testId, Message = "test" };
            _repositoryNoLock.Insert(testEntity);

            var testEntityUpdated = new MyTestDocument
            {
                Id = testId,
                LastUpdated = testEntity.LastUpdated.AddMinutes(-1), //simulate that this entity has beed fetched from database a minute ago and contains obsolete data
                Message = "updated"
            };

            _repositoryNoLock.Replace(testEntityUpdated);

            var existingEntities = _repositoryNoLock.ToArray();

            Assert.IsNotNull(existingEntities);
            Assert.AreEqual("updated", existingEntities.Where(x => x.Id == testId).First().Message);
        }

    }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;
using SocialTalents.Hp.MongoDB;
using System;
using System.Linq;

namespace SocialTalents.Hp.UnitTests.MongoDB
{
    [TestClass]
    public class RepositoryTest
    {
        public RepositoryTest()
        {
            BuildRepository();
        }

        TestDatabase _testDb;
        protected IRepository<MyDocument> _repository;

        protected virtual void BuildRepository()
        {
            _testDb = new TestDatabase();
            _repository = new MongoRepository<MyDocument>(_testDb.MongoDatabase);
        }

        public class MyDocument : BaseMongoDocument
        {
            public string MyProperty { get; set; }
        }

        public class ChildDocument : MyDocument
        {
            public string ChildProperty { get; set; }
        }

        [TestMethod]
        public void Repository_CRUD()
        {
            MyDocument newDoc = new MyDocument() { MyProperty = "Test!" };
            _repository.Insert(newDoc);

            AssertState(newDoc, _repository.First(a => a.Id == newDoc.Id));

            newDoc.MyProperty = "Changed!";

            _repository.Replace(newDoc);
            AssertState(newDoc, _repository.First(a => a.Id == newDoc.Id));

            _repository.Delete(newDoc);
            AssertState(null, _repository.FirstOrDefault(a => a.Id == newDoc.Id));
        }

        [TestMethod]
        public void Repository_Supports_Inheritance()
        {
            MyDocument newDoc1 = new MyDocument() { MyProperty = "Test!" };
            MyDocument newDoc2 = new ChildDocument() { MyProperty = "Child", ChildProperty = "some extra string" };
            _repository.Insert(newDoc1);
            _repository.Insert(newDoc2);

            MyDocument[] fromDatabase = _repository.OrderBy(x => x.MyProperty).ToArray();
            AssertState(newDoc2, fromDatabase[0]);
            AssertState(newDoc1, fromDatabase[1]);
        }

        [TestMethod]
        public void Repository_OnBefore_CanPrevent_Execution()
        {
            MyDocument newDoc = null;

            // Verify that mongo do not accept null document
            Exception ex = Assert.ThrowsException<NullReferenceException>(() => _repository.Insert(newDoc));
            
            _repository.OnBeforeDelete += x => throw new NotImplementedException();
            _repository.OnBeforeInsert+= x => throw new ArgumentException();
            _repository.OnBeforeReplace += x => throw new ArgumentNullException();
            _repository.OnBeforeDeleteMany += x => throw new ArgumentOutOfRangeException();

            // verifying that custom expecptions are thrown
            Assert.ThrowsException<NotImplementedException>(() => _repository.Delete(newDoc));
            Assert.ThrowsException<ArgumentException>(() => _repository.Insert(newDoc));
            Assert.ThrowsException<ArgumentNullException>(() => _repository.Replace(newDoc));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => _repository.DeleteMany(
                x => true));
        }

        [TestMethod]
        public virtual void Repository_CanAddIndexes()
        {
            Action addIndexAction = () => _repository.AddIndex("Repository_CanAddIndexes",
                builder => builder.Ascending(d => d.MyProperty),
                options => options.Background = true,
                options => options.ExpireAfter = TimeSpan.FromDays(1));

            // We can create index twice
            addIndexAction();
            addIndexAction();

            if (_repository is MongoRepository<MyDocument> mongoRepository)
            {
                // Index exist on mongodb
                mongoRepository.Collection.Indexes.DropOne("Repository_CanAddIndexes");
            }
        }

        public void AssertState(MyDocument expected, MyDocument actual)
        {
            if (expected == null)
            {
                Assert.IsNull(actual, "Expected document as null");
                return;
            }
            Assert.IsNotNull(actual, "Expected document not null");
            Assert.AreEqual(expected.Id, actual.Id);
            Assert.AreEqual(expected.MyProperty, actual.MyProperty);
            Assert.AreEqual(expected.GetType(), actual.GetType());
        }

        [TestCleanup]
        public void TearDown()
        {
            if (_testDb != null)
            {
                _testDb.Dispose();
            }
        }
    }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson;
using SocialTalents.Hp.MongoDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialTalents.Hp.UnitTests.MongoDB
{
    [TestClass]
    public class RepositoryOutParametersTest
    {
        public void TestOutParameter()
        {
            var repoParent = new MongoRepository<ParentEntity, ParentEntity, Id<ParentEntity>>(null);

            var repoChild = new MongoRepository<ParentEntity, ChildEntity, Id<ParentEntity>>(null);
        }

        public class ParentEntity : TypedIdMongoDocument<ParentEntity>
        {

        }

        public class ChildEntity : ParentEntity
        {

        }
    }
}

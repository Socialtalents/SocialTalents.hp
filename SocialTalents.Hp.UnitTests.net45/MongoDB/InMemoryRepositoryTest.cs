using Microsoft.VisualStudio.TestTools.UnitTesting;
using SocialTalents.Hp.MongoDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialTalents.Hp.UnitTests.MongoDB
{
    [TestClass]
    public class InMemoryRepositoryTest : RepositoryTest
    {
        public InMemoryRepositoryTest()
        {

        }

        protected override void BuildRepository()
        {
            _repository = new InMemoryRepository<MyDocument>();
        }
    }
}

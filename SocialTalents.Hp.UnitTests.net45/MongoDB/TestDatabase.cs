using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialTalents.Hp.UnitTests.MongoDB
{
    public class TestDatabase : IDisposable
    {
        public TestDatabase(string connection = "mongodb://127.0.0.1/")
        {
            _databaseName = Guid.NewGuid().ToString();
            _mongoClient = new MongoClient(connection);
            _mongoDatabase = _mongoClient.GetDatabase(_databaseName);
        }

        private string _databaseName;
        public string DatabaseName { get { return _databaseName; } }

        private MongoClient _mongoClient;
        private IMongoDatabase _mongoDatabase;
        public IMongoDatabase MongoDatabase { get { return _mongoDatabase; } }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_mongoDatabase != null)
            {
                _mongoClient.DropDatabase(_databaseName);
            }
        }
    }
}

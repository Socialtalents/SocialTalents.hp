using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialTalents.Hp.MongoDB
{
    public static class MongoIndexExtension
    {
        public static IRepository<T> AddIndex<T>(this IRepository<T> repo, string indexName, Func<IndexKeysDefinitionBuilder<T>, IndexKeysDefinition<T>> builderRule, params Action<CreateIndexOptions>[] optionsModifiers) where T: BaseMongoDocument
        {
            // InMemoryRepository do not have to support indexes since target use case is Unit Testing only
            if (repo is MongoRepository<T> mongoRepository)
            {
                var indexKeysDefinition = builderRule(Builders<T>.IndexKeys);
                CreateIndexOptions defOptions = new CreateIndexOptions();
                optionsModifiers.Select(modifier => { modifier(defOptions); return true; });
                // We need index name so we can delete it (TBD)
                // TODO: Generate index name automatically
                defOptions.Name = indexName;
                mongoRepository.Collection.Indexes.CreateOne(indexKeysDefinition, defOptions);
            }
            // to support chaining
            return repo;
        }
    }
}

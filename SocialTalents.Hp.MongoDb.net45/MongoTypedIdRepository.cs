using MongoDB.Driver;
using SocialTalents.Hp.MongoDB.Exceptions;
using System;

namespace SocialTalents.Hp.MongoDB
{
    /// <summary>
    /// Mongo repository for <see cref="TypedIdMongoDocument{T}"/>
    /// </summary>
    /// <typeparam name="T">Document type.</typeparam>
    public class MongoTypedIdRepository<T>: MongoRepository<T, Id<T>>
        where T: TypedIdMongoDocument<T>
    {
        public MongoTypedIdRepository(IMongoDatabase database, string collectionName = null, MongoCollectionSettings settings = null, bool allowOptimisticLock = true) :
            base(database, collectionName, settings)
        {
            AllowOptimistickLock = allowOptimisticLock;
        }

        public bool AllowOptimistickLock { get; private set; }

        public override void Replace(T entity)
        {
            RaiseOnBeforeReplace(entity);

            var id = entity.Id;
            var lastUpdated = entity.LastUpdated;
            entity.LastUpdated = DateTime.Now;

            ReplaceOneResult replaceResult;

            if (AllowOptimistickLock)
            {
                replaceResult = Collection.ReplaceOne(x => x.Id.Equals(id) && x.LastUpdated.Equals(lastUpdated), entity);
            }
            else
            {
                replaceResult = Collection.ReplaceOne(x => x.Id.Equals(id), entity);
            }

            if (AllowOptimistickLock && replaceResult.IsModifiedCountAvailable && replaceResult.ModifiedCount == 0)
            {
                var count = Collection.Count(x => x.Id.Equals(id));

                if (count > 0)
                {
                    throw new OptimistickLockException(this.CollectionName);
                }
            }

            RaiseOnReplace(entity);
        }
    }
}
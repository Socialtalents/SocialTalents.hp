using MongoDB.Driver;

namespace SocialTalents.Hp.MongoDB
{
    /// <summary>
    /// Mongo repository for <see cref="TypedIdMongoDocument{T}"/>
    /// </summary>
    /// <typeparam name="T">Document type.</typeparam>
    public class MongoTypedIdRepository<T>: MongoRepository<T, Id<T>>
        where T: TypedIdMongoDocument<T>
    {
        public MongoTypedIdRepository(IMongoDatabase database, string collectionName = null, MongoCollectionSettings settings = null):
            base(database, collectionName, settings) { }
    }
}
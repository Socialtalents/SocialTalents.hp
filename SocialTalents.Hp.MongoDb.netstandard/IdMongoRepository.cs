using MongoDB.Driver;

namespace SocialTalents.Hp.MongoDB
{
    /// <summary>
    /// Mongo repository for <see cref="IdMongoDocument{T}"/>
    /// </summary>
    /// <typeparam name="T">Document type.</typeparam>
    public class IdMongoRepository<T>: MongoRepository<T, Id<T>>
        where T: IdMongoDocument<T>
    {
        public IdMongoRepository(IMongoDatabase database, string collectionName = null, MongoCollectionSettings settings = null):
            base(database, collectionName, settings) { }
    }
}
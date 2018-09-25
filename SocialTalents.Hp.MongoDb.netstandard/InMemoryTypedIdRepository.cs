namespace SocialTalents.Hp.MongoDB
{
    /// <summary>
    /// In-memory repository for <see cref="TypedIdMongoDocument{T}"/>
    /// </summary>
    /// <typeparam name="T">Document type.</typeparam>
    public class InMemoryTypedIdRepository<T>: InMemoryRepository<T, Id<T>>
        where T: TypedIdMongoDocument<T> { }
}
namespace SocialTalents.Hp.MongoDB
{
    /// <summary>
    /// In-memory repository for <see cref="IdMongoDocument{T}"/>
    /// </summary>
    /// <typeparam name="T">Document type.</typeparam>
    public class InMemoryIdRepository<T>: InMemoryRepository<T, Id<T>>
        where T: IdMongoDocument<T> { }
}
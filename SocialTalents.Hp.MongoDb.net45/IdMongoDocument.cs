namespace SocialTalents.Hp.MongoDB
{
    /// <summary>
    /// Base Mongo document with strongly-typed represented via <see cref="Id{T}"/>.
    /// </summary>
    public class IdMongoDocument<T>: BaseMongoDocument<Id<T>>
    {
        public override Id<T> GenerateNewId() => Id<T>.GenerateNewId();
    }
}
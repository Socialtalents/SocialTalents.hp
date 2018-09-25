namespace SocialTalents.Hp.MongoDB
{
    /// <summary>
    /// Base Mongo document with strongly-typed id represented via <see cref="Id{T}"/>.
    /// </summary>
    public class TypedIdMongoDocument<T>: BaseMongoDocument<Id<T>>
    {
        public override Id<T> GenerateNewId() => Id<T>.GenerateNewId();
    }
}
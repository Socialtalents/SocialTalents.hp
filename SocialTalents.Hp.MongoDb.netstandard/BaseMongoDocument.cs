using MongoDB.Bson;
using System;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;

namespace SocialTalents.Hp.MongoDB
{
    /// <summary>
    /// Base Mongo document with custom id type.
    /// </summary>
    /// <remarks> All inheritors should ensure that <typeparamref name="TId"/> has correct <see cref="IBsonSerializer"/>. </remarks>
    public abstract class BaseMongoDocument<TId>: IEquatable<BaseMongoDocument<TId>>
        where TId: struct, IEquatable<TId>, IComparable<TId>
    {
        public TId Id { get; set; }

        [BsonElement("_lu")]
        public DateTime LastUpdated { get; set; }

        #region Equality members

        public bool Equals(BaseMongoDocument<TId> other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id.Equals(other.Id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((BaseMongoDocument<TId>)obj);
        }

        public override int GetHashCode() => Id.GetHashCode();

        public static bool operator ==(BaseMongoDocument<TId> left, BaseMongoDocument<TId> right) => Equals(left, right);

        public static bool operator !=(BaseMongoDocument<TId> left, BaseMongoDocument<TId> right) => !Equals(left, right);

        #endregion

        public abstract TId GenerateNewId();

        protected BaseMongoDocument() => Id = GenerateNewId();
    }

    /// <summary>
    /// Base Mongo document with <see cref="ObjectId"/> used as id.
    /// </summary>
    public class BaseMongoDocument: BaseMongoDocument<ObjectId>
    {
        public override ObjectId GenerateNewId() => ObjectId.GenerateNewId();
    }
}
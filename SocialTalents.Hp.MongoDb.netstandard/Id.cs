using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace SocialTalents.Hp.MongoDB
{
    /// <summary>
    /// Represents strongly-typed id to use with Mongo repositories.
    /// </summary>
    /// <typeparam name="T">Document type this id is for.</typeparam>
    /// <remarks>Is tied to <see cref="IdSerializer{T}"/> for mapping it to <see cref="ObjectId"/> in Mongo.</remarks>
    public struct Id<T>: IEquatable<Id<T>>, IComparable<Id<T>>, IEquatable<ObjectId>
    {
        static Id() => BsonSerializer.RegisterSerializer(typeof(Id<T>), IdSerializer<T>.Instance);

        public ObjectId ObjectId { get; }

        public Id(ObjectId objectId)
        {
            ObjectId = objectId;
        }

        public DateTime CreationTime => ObjectId.CreationTime;

        public static Id<T> Parse(string s) => new Id<T>(ObjectId.Parse(s));

        public static Id<T> GenerateNewId() => new Id<T>(ObjectId.GenerateNewId());

        #region Conversion operators

        public static implicit operator ObjectId(Id<T> id)
        {
            return id.ObjectId;
        }

        public static implicit operator Id<T>(ObjectId objectId)
        {
            return new Id<T>(objectId);
        }

        #endregion

        #region Equality members

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Id<T> other && Equals(other);
        }

        public bool Equals(Id<T> other) => Equals(other.ObjectId);

        public bool Equals(ObjectId other) => ObjectId.Equals(other);

        public override int GetHashCode() => ObjectId.GetHashCode();

        public static bool operator ==(Id<T> left, Id<T> right) => left.Equals(right);

        public static bool operator !=(Id<T> left, Id<T> right) => !(left == right);

        public static bool operator ==(Id<T> left, ObjectId right) => left.ObjectId.Equals(right);

        public static bool operator !=(Id<T> left, ObjectId right) => !(left == right);

        public static bool operator ==(ObjectId left, Id<T> right) => right.ObjectId.Equals(left);

        public static bool operator !=(ObjectId left, Id<T> right) => !(left == right);

        public int CompareTo(Id<T> other) => ObjectId.CompareTo(other.ObjectId);

        #endregion

        public override string ToString() => ObjectId.ToString();
    }
}
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
    public struct Id<T>: IEquatable<Id<T>>, IEquatable<ObjectId>, IComparable<Id<T>>, IComparable<ObjectId>, IConvertible
    {
        static Id() => BsonSerializer.RegisterSerializer(typeof(Id<T>), IdSerializer<T>.Instance);

        public static Id<T> Empty => ObjectId.Empty;

        public static Id<T> GenerateNewId() => ObjectId.GenerateNewId();

        public static Id<T> GenerateNewId(DateTime timestamp) => ObjectId.GenerateNewId(timestamp);

        public static Id<T> GenerateNewId(int timestamp) => ObjectId.GenerateNewId(timestamp);

        public static Id<T> Parse(string s) => ObjectId.Parse(s);

        public static bool TryParse(string s, out Id<T> id)
        {
            var success = ObjectId.TryParse(s, out var objectId);
            id = objectId;
            return success;
        }

        public static byte[] Pack(int timestamp, int machine, short pid, int increment) =>
            ObjectId.Pack(timestamp, machine, pid, increment);

        public static void Unpack(byte[] bytes, out int timestamp, out int machine, out short pid, out int increment) =>
            ObjectId.Unpack(bytes, out timestamp, out machine, out pid, out increment);

        public ObjectId ObjectId { get; }

        public Id(ObjectId objectId) => ObjectId = objectId;

        public Id(byte[] bytes) => ObjectId = new ObjectId(bytes);

        public Id(DateTime timestamp, int machine, short pid, int increment) =>
            ObjectId = new ObjectId(timestamp, machine, pid, increment);

        public Id(int timestamp, int machine, short pid, int increment) =>
            ObjectId = new ObjectId(timestamp, machine, pid, increment);

        public Id(string value) => ObjectId = new ObjectId(value);

        public int Timestamp => ObjectId.Timestamp;

        public int Machine => ObjectId.Machine;

        public int Pid => ObjectId.Pid;

        public int Increment => ObjectId.Increment;

        public DateTime CreationTime => ObjectId.CreationTime;

        #region Conversion operators

        public static implicit operator ObjectId(Id<T> id) => id.ObjectId;

        public static implicit operator Id<T>(ObjectId objectId) => new Id<T>(objectId);

        #endregion

        #region Comparison operators

        public static bool operator <(Id<T> left, Id<T> right) => left.ObjectId < right.ObjectId;

        public static bool operator >(Id<T> left, Id<T> right) => left.ObjectId > right.ObjectId;

        public static bool operator <=(Id<T> left, Id<T> right) => left.ObjectId <= right.ObjectId;

        public static bool operator >=(Id<T> left, Id<T> right) => left.ObjectId >= right.ObjectId;

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

        public int CompareTo(Id<T> other) => CompareTo(other.ObjectId);

        public int CompareTo(ObjectId other) => ObjectId.CompareTo(other);

        #endregion

        public override string ToString() => ObjectId.ToString();

        public byte[] ToByteArray() => ObjectId.ToByteArray();

        public void ToByteArray(byte[] destination, int offset) => ObjectId.ToByteArray(destination, offset);

        #region Implementation of IConvertible

        TypeCode IConvertible.GetTypeCode() => ((IConvertible)ObjectId).GetTypeCode();

        bool IConvertible.ToBoolean(IFormatProvider provider) => ((IConvertible)ObjectId).ToBoolean(provider);

        char IConvertible.ToChar(IFormatProvider provider) => ((IConvertible)ObjectId).ToChar(provider);

        sbyte IConvertible.ToSByte(IFormatProvider provider) => ((IConvertible)ObjectId).ToSByte(provider);

        byte IConvertible.ToByte(IFormatProvider provider) => ((IConvertible)ObjectId).ToByte(provider);

        short IConvertible.ToInt16(IFormatProvider provider) => ((IConvertible)ObjectId).ToInt16(provider);

        ushort IConvertible.ToUInt16(IFormatProvider provider) => ((IConvertible)ObjectId).ToUInt16(provider);

        int IConvertible.ToInt32(IFormatProvider provider) => ((IConvertible)ObjectId).ToInt32(provider);

        uint IConvertible.ToUInt32(IFormatProvider provider) => ((IConvertible)ObjectId).ToUInt32(provider);

        long IConvertible.ToInt64(IFormatProvider provider) => ((IConvertible)ObjectId).ToInt64(provider);

        ulong IConvertible.ToUInt64(IFormatProvider provider) => ((IConvertible)ObjectId).ToUInt64(provider);

        float IConvertible.ToSingle(IFormatProvider provider) => ((IConvertible)ObjectId).ToSingle(provider);

        double IConvertible.ToDouble(IFormatProvider provider) => ((IConvertible)ObjectId).ToDouble(provider);

        decimal IConvertible.ToDecimal(IFormatProvider provider) => ((IConvertible)ObjectId).ToDecimal(provider);

        DateTime IConvertible.ToDateTime(IFormatProvider provider) => ((IConvertible)ObjectId).ToDateTime(provider);

        string IConvertible.ToString(IFormatProvider provider) => ((IConvertible)ObjectId).ToString(provider);

        object IConvertible.ToType(Type conversionType, IFormatProvider provider) => ((IConvertible)ObjectId).ToType(conversionType, provider);

        #endregion

    }
}
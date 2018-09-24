using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace SocialTalents.Hp.MongoDB
{
    /// <summary>
    /// Mongo serializer used for <see cref="Id{T}"/>.
    /// </summary>
    public class IdSerializer<T>: IBsonSerializer<Id<T>>
    {
        public static IdSerializer<T> Instance { get; } = new IdSerializer<T>();

        public Id<T> Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var objectId = (ObjectId)BsonObjectIdSerializer.Instance.Deserialize(context);
            return new Id<T>(objectId);
        }

        public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, Id<T> value)
        {
            BsonObjectIdSerializer.Instance.Serialize(context, args, value.ObjectId);
        }

        object IBsonSerializer.Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            return Deserialize(context, args);
        }

        public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, object value)
        {
            Serialize(context, args, (Id<T>)value);
        }

        public Type ValueType => typeof(Id<T>);
    }
}
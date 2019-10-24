using MongoDB.Bson.Serialization;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace SocialTalents.Hp.MongoDB
{
    public class IdCustomSerializationProvider : IBsonSerializationProvider
    {
        public IBsonSerializer GetSerializer(Type type)
        {
            if (!type.GetTypeInfo().IsGenericType)
            {
                return null; // falls back to Mongo defaults
            }

            var genericType = type.GetGenericTypeDefinition();

            if (genericType == typeof(Id<>))
            {
                var constructedType = typeof(IdSerializer<>).MakeGenericType(type.GetTypeInfo().GetGenericArguments());
                var instanceProperty = constructedType.GetTypeInfo().GetProperty("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                var instance = (IBsonSerializer)instanceProperty.GetValue(null, null);

                return instance;
            }

            return null; // falls back to Mongo defaults
        }
    }
}

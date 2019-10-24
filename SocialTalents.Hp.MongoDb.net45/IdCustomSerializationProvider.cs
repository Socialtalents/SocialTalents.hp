using MongoDB.Bson.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialTalents.Hp.MongoDB
{
    public class IdCustomSerializationProvider : IBsonSerializationProvider
    {
        public IBsonSerializer GetSerializer(Type type)
        {
            if (!type.IsGenericType)
            {
                return null; // falls back to Mongo defaults
            }

            var genericType = type.GetGenericTypeDefinition();

            if (genericType == typeof(Id<>))
            {
                var constructedType = typeof(IdSerializer<>).MakeGenericType(type.GetGenericArguments());
                var instanceProperty = constructedType.GetProperty("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                var instance = (IBsonSerializer)instanceProperty.GetValue(null, null);

                return instance;
            }

            return null; // falls back to Mongo defaults
        }
    }
}

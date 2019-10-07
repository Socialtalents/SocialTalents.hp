using MongoDB.Bson.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialTalents.Hp.MongoDB
{
    public static class IdCustomSerializationProviderHelper
    {
        public static IBsonSerializationProvider RegisterBsonCustomSerializers()
        {
            var provider = new IdCustomSerializationProvider();
            BsonSerializer.RegisterSerializationProvider(provider);

            return provider;
        }
    }
}

using MongoDB.Bson.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

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

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialTalents.Hp.MongoDB
{
    public class BaseMongoDocument
    {
        public ObjectId Id { get; set; }
        [BsonElement("_lu")]
        public DateTime LastUpdated { get; set; }

        public BaseMongoDocument()
        {
            Id = ObjectId.GenerateNewId();
        }

        protected bool Equals(BaseMongoDocument other)
        {
            return Id.Equals(other.Id);
        }

        public static bool operator ==(BaseMongoDocument left, BaseMongoDocument right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(BaseMongoDocument left, BaseMongoDocument right)
        {
            return !Equals(left, right);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((BaseMongoDocument)obj);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}

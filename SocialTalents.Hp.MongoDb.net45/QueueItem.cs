using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using SocialTalents.Hp.Events.Queue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialTalents.Hp.MongoDB
{
    public class QueueItem : BaseMongoDocument, IQueueItem
    {
        public string UniqueKey { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime HandleAfter { get; set; }
        public int Attempts { get; set; }
        public string DeclaringEventType { get; set; }

        public string DataType { get; set; }
        public string DataJson { get; set; }

        object _value = null;
        [BsonIgnore]
        public object Event
        {
            get
            {
                if (_value == null)
                {
                    Type t = Type.GetType(DataType);
                    _value = BsonSerializer.Deserialize(DataJson, t);
                }
                return _value;
            }
            set
            {
                if (value == null)
                {
                    DataType = NULL;
                    DataJson = string.Empty;
                    return;
                }
                Type objectType = value.GetType();
                // Building non-qualified name to be able to load saved object when assembly version changed
                // TopNamespace.SubNameSpace.ContainingClass+NestedClass, MyAssembly, Version=1.3.0.0, Culture=neutral, PublicKeyToken=b17a5c561934e089 
                // ->
                // TopNamespace.SubNameSpace.ContainingClass+NestedClass, MyAssembly
                DataType = objectType.AssemblyQualifiedName;
                DataJson = value.ToJson();
                _value = value;
            }
        }

        public string HandlerId { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime HandlerStarted { get; set; }

        private const string NULL = "null";
    }
}

using MongoDB.Bson;
using MongoDB.Driver;
using SocialTalents.Hp.Events.Queue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SocialTalents.Hp.MongoDB
{
    public class EventQueueRepository : MongoRepository<QueueItem>, IEventQueueRepository
    {
        public EventQueueRepository(IMongoDatabase database, string collectionName = null, MongoCollectionSettings settings = null):
            base(database, collectionName, settings)
        {
        }

        public virtual void AddIndexes()
        {
            this.AddIndex("HandleAfter", b => b.Ascending(e => e.HandleAfter));
            this.AddIndex("Unique", b => b.Ascending(e => e.UniqueKey),
                options => options.Unique = true);
        }

        public void AddItem(IQueueItem item)
        {
            try
            {
                base.Insert(item as QueueItem);
            }
            catch (MongoWriteException ex)
            {
                if (ex.InnerException is MongoBulkWriteException innerException)
                {
                    if (innerException.WriteErrors.All(code => code.Code == 11000))
                    {
                        // duplicate key error collection, ignoring
                    }
                    else
                    {
                        throw;
                    }
                }
                else
                {
                    throw;
                }
            }
        }

        public virtual IQueueItem BuildNewItem(object eventInstance)
        {
            return new QueueItem() { Event = eventInstance };
        }

        public void DeleteItem(IQueueItem item)
        {
            base.Delete(item as QueueItem);
        }

        public string HandlerId { get; set; } = ObjectId.GenerateNewId().ToString();
        public TimeSpan QueueExecutionTimeout { get; set; } = TimeSpan.FromSeconds(300);

        // UniqueKet of special event which QueueService uses to requeue stuck events
        private static readonly string RequeueStuckEventsUniqueKey = new RequeueStuckEvents().UniqueKey;

        public virtual IEnumerable<IQueueItem> GetItemsToHandle(int limit = 6)
        {
            int found = 0;
            QueueItem r = null;
            List<QueueItem> result = new List<QueueItem>();
            do
            {
                var newEventsFilter = Builders<QueueItem>.Filter.Where(e => e.HandleAfter < DateTime.Now.ToUniversalTime());
                var notStartedFilter = Builders<QueueItem>.Filter.Where(e => e.HandlerId == null);
                var stuckRequeueFilter = Builders<QueueItem>.Filter
                    .Where(e => e.UniqueKey == RequeueStuckEventsUniqueKey &&
                        e.HandlerId != null &&
                        e.HandlerStarted < DateTime.UtcNow.Subtract(QueueExecutionTimeout));
                var regularEventsFilter = Builders<QueueItem>.Filter.And(newEventsFilter, notStartedFilter);
                var regularEventsOrStuckRequeueFilter = Builders<QueueItem>.Filter.Or(regularEventsFilter, stuckRequeueFilter);

                var setUpdate = Builders<QueueItem>.Update
                    .Set(e => e.HandlerId, HandlerId)
                    .Set(e => e.HandlerStarted, DateTime.UtcNow);
                
                // Find first event matching filter and update HandlerId so other handlers will not pick it up
                r = Collection.FindOneAndUpdate(regularEventsOrStuckRequeueFilter, setUpdate,
                    new FindOneAndUpdateOptions<QueueItem>() { IsUpsert = false, ReturnDocument = ReturnDocument.After });

                if (r != null)
                {
                    found++;
                    result.Add(r);
                }
            } while (r != null && found < limit);

            return result;
        }

        public virtual void UpdateItem(IQueueItem item)
        {
            base.Replace(item as QueueItem);
        }

        public virtual long RequeueOldEvents()
        {
            var find = Builders<QueueItem>.Filter.Where(e => e.HandlerStarted < DateTime.UtcNow.Subtract(QueueExecutionTimeout) && e.HandlerId != null);
            var setUpdate = Builders<QueueItem>.Update
                    .Set(e => e.HandlerId, null);
            var r = Collection.UpdateMany(find, setUpdate);
            return r.ModifiedCount;
        }
    }
}

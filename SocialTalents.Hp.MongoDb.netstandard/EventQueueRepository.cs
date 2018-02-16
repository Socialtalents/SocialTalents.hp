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

        public IQueueItem BuildNewItem(object eventInstance)
        {
            return new QueueItem() { Event = eventInstance };
        }

        public void DeleteItem(IQueueItem item)
        {
            base.Delete(item as QueueItem);
        }

        public string HandlerId { get; set; } = ObjectId.GenerateNewId().ToString();
        public Expression<Func<QueueItem, bool>> FilterExpression { get; set; } = e => e.HandleAfter < DateTime.Now.ToUniversalTime();

        public virtual IEnumerable<IQueueItem> GetItemsToHandle(int limit = 6)
        {
            int found = 0;
            QueueItem r = null;
            List<QueueItem> result = new List<QueueItem>();
            do
            {
                var findNewEvents = Builders<QueueItem>.Filter.Where(FilterExpression);
                var findNotStartedEvents = Builders<QueueItem>.Filter.Where(e => e.HandlerId == null);
                var filter = Builders<QueueItem>.Filter.And(findNewEvents, findNotStartedEvents);
                var setUpdate = Builders<QueueItem>.Update
                    .Set(e => e.HandlerId, HandlerId)
                    .Set(e => e.HandlerStarted, DateTime.UtcNow);
                r = Collection.FindOneAndUpdate(filter, setUpdate,
                    new FindOneAndUpdateOptions<QueueItem>() { IsUpsert = false, ReturnDocument = ReturnDocument.After });

                if (r != null)
                {
                    found++;
                    result.Add(r);
                }
            } while (r != null && found < limit);

            return result;
        }

        public void UpdateItem(IQueueItem item)
        {
            base.Replace(item as QueueItem);
        }

        public long RequeueOldEvents(TimeSpan timeout)
        {
            var find = Builders<QueueItem>.Filter.Where(e => e.HandlerStarted < DateTime.UtcNow.Subtract(timeout) && e.HandlerId != null);
            var setUpdate = Builders<QueueItem>.Update
                    .Set(e => e.HandlerId, null);
            var r = Collection.UpdateMany(find, setUpdate);
            return r.ModifiedCount;
        }
    }
}

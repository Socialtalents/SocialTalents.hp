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
            base.Insert(item as QueueItem);
        }

        public IQueueItem BuildNewItem(object eventInstance)
        {
            return new QueueItem() { Event = eventInstance };
        }

        public void DeleteItem(IQueueItem item)
        {
            base.Delete(item as QueueItem);
        }

        public Expression<Func<QueueItem, bool>> FilterExpression { get; set; } = e => e.HandleAfter < DateTime.Now.ToUniversalTime();

        public IEnumerable<IQueueItem> GetItemsToHandle(int limit = 10)
        {
            return this.Where(FilterExpression).OrderBy(e => e.HandleAfter).ThenBy(e => e.Id).Take(limit);
        }

        public void UpdateItem(IQueueItem item)
        {
            base.Replace(item as QueueItem);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialTalents.Hp.Events.Queue
{
    public class InMemoryQueueRepository : IEventQueueRepository
    {
        public List<InMemoryQueueItem> Queue{ get; set; }
        protected HashSet<string> UniqueIndex { get; set; }
        public string HandlerId { get; set; } = Guid.NewGuid().ToString();

        public InMemoryQueueRepository()
        {
            Queue = new List<InMemoryQueueItem>();
            UniqueIndex = new HashSet<string>();
        }

        public IQueueItem BuildNewItem(object eventInstance)
        {
            return new InMemoryQueueItem() { Event = eventInstance };
        }

        public void AddItem(IQueueItem item)
        {
            lock(this)
            {
                bool canBeAdded = item.UniqueKey == null || !UniqueIndex.Contains(item.UniqueKey);
                if (canBeAdded)
                {
                    Queue.Add(item as InMemoryQueueItem);
                    if (item.UniqueKey != null)
                    {
                        UniqueIndex.Add(item.UniqueKey);
                    }
                }
            }
        }

        public void DeleteItem(IQueueItem item)
        {
            lock(this)
            {
                Queue.Remove(item as InMemoryQueueItem);
                if (item.UniqueKey != null)
                {
                    UniqueIndex.Remove(item.UniqueKey);
                } 
            }
        }

        public void UpdateItem(IQueueItem item)
        {
            // since all is in memory no need to do anything
        }

        public IEnumerable<IQueueItem> GetItemsToHandle(int limit = 10)
        {
            return Queue.Where(i => DateTime.Now >= i.HandleAfter).Take(limit)
                // to allow deletion from Queue
                .ToArray();
        }

        public virtual long RequeueOldEvents(TimeSpan timeout)
        {
            throw new NotImplementedException("InMemory repository do not support multiple workers");
        }
    }
}

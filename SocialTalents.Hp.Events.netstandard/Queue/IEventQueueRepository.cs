using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialTalents.Hp.Events.Queue
{
    public interface IEventQueueRepository
    {
        IQueueItem BuildNewItem(object eventInstance);

        void AddItem(IQueueItem item);
        void UpdateItem(IQueueItem item);
        void DeleteItem(IQueueItem item);

        IEnumerable<IQueueItem> GetItemsToHandle(int limit = 10);
    }
}

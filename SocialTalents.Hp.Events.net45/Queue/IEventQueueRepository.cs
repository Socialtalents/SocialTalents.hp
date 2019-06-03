using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialTalents.Hp.Events.Queue
{
    /// <summary>
    /// DAL Interface for EventQueue
    /// </summary>
    public interface IEventQueueRepository
    {
        /// <summary>
        /// Factory method to build DAL compatible object from event instance
        /// </summary>
        /// <param name="eventInstance"></param>
        /// <returns></returns>
        IQueueItem BuildNewItem(object eventInstance);

        /// <summary>
        /// CRUD Create
        /// </summary>
        /// <param name="item"></param>
        void AddItem(IQueueItem item);
        /// <summary>
        /// CRUD Update
        /// </summary>
        /// <param name="item"></param>
        void UpdateItem(IQueueItem item);
        /// <summary>
        /// CRUD Delete
        /// </summary>
        /// <param name="item"></param>
        void DeleteItem(IQueueItem item);

        /// <summary>
        /// Lookup events to handle
        /// </summary>
        /// <param name="limit"></param>
        /// <returns></returns>
        IEnumerable<IQueueItem> GetItemsToHandle(int limit = 10);
        /// <summary>
        /// returns to Queue events which stuck to handle
        /// </summary>
        /// <returns></returns>
        long RequeueStuckEvents();
        
        /// <summary>
        /// Controls execution timeout
        /// </summary>
        TimeSpan QueueExecutionTimeout { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialTalents.Hp.Events.Queue
{
    /// <summary>
    /// DAL queue item has to implement following fields
    /// </summary>
    public interface IQueueItem
    {
        /// <summary>
        /// De-serialised Instance of event
        /// </summary>
        object Event { get; set; }
        /// <summary>
        /// Nullable Unique key, can be index
        /// </summary>
        string UniqueKey { get; set; }
        /// <summary>
        /// DateTime when event can be handled. Please Take care of timezones
        /// </summary>
        DateTime HandleAfter { get; set; }
        /// <summary>
        /// Handling Attempts count, starts from 0
        /// </summary>
        int Attempts { get; set; }
        /// <summary>
        /// Event type to be used for Publish (can be different from Event.getType())
        /// </summary>
        string DeclaringEventType { get; set; }
    }

    /// <summary>
    /// InMemory implementation for queue item 
    /// </summary>
    public class InMemoryQueueItem : IQueueItem
    {
        public InMemoryQueueItem()
        {
            HandleAfter = DateTime.MinValue;
        }

        public object Event { get; set; }
        public string UniqueKey { get; set; }
        public DateTime HandleAfter { get; set; }
        public int Attempts { get; set; }
        public string DeclaringEventType { get; set; }
    }
}

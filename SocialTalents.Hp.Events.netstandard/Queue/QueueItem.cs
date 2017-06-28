using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialTalents.Hp.Events.Queue
{
    public interface IQueueItem
    {
        object Event { get; set; }
        string UniqueKey { get; set; }
        DateTime HandleAfter { get; set; }
        int Attempts { get; set; }
        string DeclaringEventType { get; set; }
    }

    public class InMemoryQueueItem : IQueueItem
    {
        public InMemoryQueueItem()
        {
            HandleAfter = DateTime.MinValue;
        }

        public object Event { get; set; }
        /// <summary>
        /// UniqueKey, ideally nullable unique index at database 
        /// </summary>
        public string UniqueKey { get; set; }
        public DateTime HandleAfter { get; set; }
        public int Attempts { get; set; }
        public string DeclaringEventType { get; set; }
    }
}

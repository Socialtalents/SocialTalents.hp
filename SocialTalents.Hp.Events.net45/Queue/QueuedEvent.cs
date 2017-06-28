using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialTalents.Hp.Events.Queue
{
    public class QueuedEvent<TEvent>
    {
        public IQueueItem Item { get; set; }
        public TEvent Event { get; set; }

        public QueuedEvent(IQueueItem item, TEvent eventInstance)
        {
            Item = item;
            Event = eventInstance;
        }
    }
}

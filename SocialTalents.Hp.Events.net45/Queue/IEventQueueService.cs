using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SocialTalents.Hp.Events.Queue
{
    public interface IEventQueueService
    {
        ProcessEventsResult ProcessEvents();
        Delegate<TEvent> Enque<TEvent>();
        Delegate<TEvent> Enque<TEvent>(TimeSpan handleAfter);
    }
}

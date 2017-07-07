using SocialTalents.Hp.Events;
using SocialTalents.Hp.Events.Queue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Samples.Console.Utils
{
    public class EventQueueWithDebugLogs : EventQueueService
    {
        public EventQueueWithDebugLogs(IEventQueueRepository repository, EventBusService eventBusService = null)
            : base(repository, eventBusService)
        {
        }

        protected override void onAddEvent(IQueueItem item, object eventInstance)
        {
            Output.Log("Adding to queue : " + item.Event.ToString(), "Queue");
            base.onAddEvent(item, eventInstance);
        }

        protected override void onEventFailed(IQueueItem item, Exception ex)
        {
            Output.Error("Handling failed, removing from queue : " + item.Event.ToString(), "Queue");
            base.onEventFailed(item, ex);
        }

        protected override void onEventRetried(IQueueItem item, RetryNeededException ex)
        {
            Output.Log($"Handling failed for {item.Event.ToString()}, retrrying after {item.HandleAfter}", "Queue");
            base.onEventRetried(item, ex);
        }
    }
}

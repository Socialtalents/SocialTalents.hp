using System;
using System.Collections.Generic;
using System.Text;

namespace SocialTalents.Hp.Events.Internal
{
    public class AsyncCallback<TEvent> : ICanPublish<Exception>
    {
        public AsyncCallback(Delegate<TEvent> handler, EventBusService eventBusService)
        {
            _handler = handler;
            _eventBusService = eventBusService;
        }

        Delegate<TEvent> _handler;
        EventBusService _eventBusService;

        public void Callback(IAsyncResult result)
        {
            try
            {
                _handler.EndInvoke(result);
            }
            catch (Exception ex)
            {
                _eventBusService.Publish<Exception>(ex, this);
            }
        }
    }
}

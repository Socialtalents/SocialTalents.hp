using System;
using System.Collections.Generic;
using System.Text;

namespace SocialTalents.Hp.Events.Internal
{
    public class AsyncCallback<TEvent> : ICanPublish<Exception>
    {
        public AsyncCallback(Delegate<TEvent> handler)
        {
            _handler = handler;
        }

        Delegate<TEvent> _handler;

        public void Callback(IAsyncResult result)
        {
            try
            {
                _handler.EndInvoke(result);
            }
            catch (Exception ex)
            {
                EventBus.Publish<Exception>(ex, this);
            }
        }
    }
}

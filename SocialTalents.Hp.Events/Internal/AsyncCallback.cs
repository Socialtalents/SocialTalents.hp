using System;
using System.Collections.Generic;
using System.Text;

namespace SocialTalents.Hp.Events.Internal
{
    public class AsyncCallback<TEvent>
    {
        public AsyncCallback(WorkflowStepDelegate<TEvent> handler)
        {
            _handler = handler;
        }

        WorkflowStepDelegate<TEvent> _handler;

        public void Callback(IAsyncResult result)
        {
            try
            {
                _handler.EndInvoke(result);
            }
            catch (Exception ex)
            {
                EventBus.Raise<Exception>(ex, new SenderStub<Exception>());
            }
        }
    }
}

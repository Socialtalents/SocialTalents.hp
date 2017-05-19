using SocialTalents.Hp.Events.Internal;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocialTalents.Hp.Events
{
    public static class EventExtensions
    {
        public static WorkflowStepDelegate<TEvent> Async<TEvent>(this WorkflowStepDelegate<TEvent> handler)
        {
            return (param) =>
            {
                handler.BeginInvoke(param, new AsyncCallback<TEvent>(handler).Callback, null);
            };
        }

        public static WorkflowStepDelegate<TEvent> Async<TEvent>(this ICanHandle<TEvent> handler)
        {
            WorkflowStepDelegate<TEvent> handlerAsDelegate = handler.Handle;
            return Async<TEvent>(handlerAsDelegate);
        }

        /// <summary>
        /// Subscribe to event with ICanHandle interface. If it fails with TException - onFail is called
        /// </summary>
        public static WorkflowStepDelegate<TEvent> AddOnFail<TEvent, TException>(this ICanHandle<TEvent> handler, ICanHandle<TEvent> onFail, bool raiseException = true) where TException : Exception
        {
            return AddOnFail<TEvent, TException>(handler.Handle, onFail.Handle, raiseException);
        }
        
        /// <summary>
        /// Subscribe to event with ICanHandle interface. If it fails with TException - onFail is called
        /// </summary>
        public static WorkflowStepDelegate<TEvent> AddOnFail<TEvent, TException>(this WorkflowStepDelegate<TEvent> handler, ICanHandle<TEvent> onFail, bool raiseException = true) where TException : Exception
        {
            return AddOnFail<TEvent, TException>(handler, onFail.Handle, raiseException);
        }

        /// <summary>
        /// Subscribe to event with ICanHandle interface. If it fails with TException - onFail is called
        /// </summary>
        public static WorkflowStepDelegate<TEvent> AddOnFail<TEvent, TException>(this ICanHandle<TEvent> handler, WorkflowStepDelegate<TEvent> onFail, bool raiseException = true) where TException : Exception
        {
            return AddOnFail<TEvent, TException>(handler.Handle, onFail, raiseException);
        }

        /// <summary>
        /// Subscribe to event with ICanHandle interface. If it fails with TException - onFail is called
        /// </summary>
        public static WorkflowStepDelegate<TEvent> AddOnFail<TEvent, TException>(this WorkflowStepDelegate<TEvent> handler, WorkflowStepDelegate<TEvent> onFail, bool raiseException = true) where TException : Exception
        {
            return (param) =>
            {
                try
                {
                    handler(param);
                }
                catch (TException ex)
                {
                    if (raiseException)
                    {
                        EventBus.Raise<Exception>(ex, new SenderStub<Exception>());
                    }
                    onFail(param);
                }
            };
        }

    }
}

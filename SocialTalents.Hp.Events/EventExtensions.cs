using SocialTalents.Hp.Events.Internal;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocialTalents.Hp.Events
{
    public static class EventExtensions
    {
        public static Delegate<TEvent> Async<TEvent>(this Delegate<TEvent> handler)
        {
            return (param) =>
            {
                handler.BeginInvoke(param, new AsyncCallback<TEvent>(handler).Callback, null);
            };
        }

        public static Delegate<TEvent> Async<TEvent>(this ICanHandle<TEvent> handler)
        {
            Delegate<TEvent> handlerAsDelegate = handler.Handle;
            return Async<TEvent>(handlerAsDelegate);
        }

        /// <summary>
        /// Subscribe to event with ICanHandle interface. If it fails with TException - onFail is called
        /// </summary>
        public static Delegate<TEvent> AddOnFail<TEvent, TException>(this ICanHandle<TEvent> handler, ICanHandle<TEvent> onFailHandler, bool publishException = true) where TException : Exception
        {
            return AddOnFail<TEvent, TException>(handler.Handle, onFailHandler.Handle, publishException);
        }
        
        /// <summary>
        /// Subscribe to event with ICanHandle interface. If it fails with TException - onFail is called
        /// </summary>
        public static Delegate<TEvent> AddOnFail<TEvent, TException>(this Delegate<TEvent> handler, ICanHandle<TEvent> onFailHandler, bool publishException = true) where TException : Exception
        {
            return AddOnFail<TEvent, TException>(handler, onFailHandler.Handle, publishException);
        }

        /// <summary>
        /// Subscribe to event with ICanHandle interface. If it fails with TException - onFail is called
        /// </summary>
        public static Delegate<TEvent> AddOnFail<TEvent, TException>(this ICanHandle<TEvent> handler, Delegate<TEvent> onFailHandler, bool publishException = true) where TException : Exception
        {
            return AddOnFail<TEvent, TException>(handler.Handle, onFailHandler, publishException);
        }

        /// <summary>
        /// Subscribe to event with ICanHandle interface. If it fails with TException - onFail is called
        /// </summary>
        public static Delegate<TEvent> AddOnFail<TEvent, TException>(this Delegate<TEvent> handler, Delegate<TEvent> onFailHandler, bool publishException = true) where TException : Exception
        {
            return (param) =>
            {
                try
                {
                    handler(param);
                }
                catch (TException ex)
                {
                    if (publishException)
                    {
                        EventBus.Publish<Exception>(ex, new SenderStub<Exception>());
                    }
                    onFailHandler(param);
                }
            };
        }

    }
}

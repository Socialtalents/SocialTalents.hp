using SocialTalents.Hp.Events.Internal;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocialTalents.Hp.Events
{
    /// <summary>
    /// Default event extensions to control how handler is executed, you can easily add your own
    /// See https://github.com/Socialtalents/SocialTalents.hp for sources and samples
    /// </summary>
    public static class EventExtensions
    {
        /// <summary>
        /// Wraps handler for Asynchronous execution
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <param name="handler"></param>
        /// <returns></returns>
        public static Delegate<TEvent> Async<TEvent>(this Delegate<TEvent> handler)
        {
            return (param) =>
            {
                handler.BeginInvoke(param, new AsyncCallback<TEvent>(handler).Callback, null);
            };
        }

        /// <summary>
        /// Wraps handler for Asynchronous execution
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <param name="handler"></param>
        /// <returns></returns>
        public static Delegate<TEvent> Async<TEvent>(this ICanHandle<TEvent> handler)
        {
            Delegate<TEvent> handlerAsDelegate = handler.Handle;
            return Async<TEvent>(handlerAsDelegate);
        }

        /// <summary>
        /// Subscribe to event with special behavior when handler failed. If it fails with TException - onFail is called
        /// </summary>
        public static Delegate<TEvent> AddOnFail<TEvent, TException>(this ICanHandle<TEvent> handler, ICanHandle<TEvent> onFailHandler, bool publishException = true) where TException : Exception
        {
            return AddOnFail<TEvent, TException>(handler.Handle, onFailHandler.Handle, publishException);
        }

        /// <summary>
        /// Subscribe to event with special behavior when handler failed. If it fails with TException - onFail is called
        /// </summary>
        public static Delegate<TEvent> AddOnFail<TEvent, TException>(this Delegate<TEvent> handler, ICanHandle<TEvent> onFailHandler, bool publishException = true) where TException : Exception
        {
            return AddOnFail<TEvent, TException>(handler, onFailHandler.Handle, publishException);
        }

        /// <summary>
        /// Subscribe to event with special behavior when handler failed. If it fails with TException - onFail is called
        /// </summary>
        public static Delegate<TEvent> AddOnFail<TEvent, TException>(this ICanHandle<TEvent> handler, Delegate<TEvent> onFailHandler, bool publishException = true) where TException : Exception
        {
            return AddOnFail<TEvent, TException>(handler.Handle, onFailHandler, publishException);
        }

        /// <summary>
        /// Subscribe to event with special behavior when handler failed. If it fails with TException - onFail is called
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

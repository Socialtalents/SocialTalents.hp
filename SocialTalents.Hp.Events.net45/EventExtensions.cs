using SocialTalents.Hp.Events.Exceptions;
using SocialTalents.Hp.Events.Internal;
using System;
using System.Threading.Tasks;

namespace SocialTalents.Hp.Events
{
    /// <summary>
    /// Default event extensions to control how handler is executed, you can easily add your own
    /// Refer to https://github.com/Socialtalents/SocialTalents.hp for sources and samples
    /// </summary>
    public static class EventExtensions
    {
        /// <summary>
        /// Wraps handler for Asynchronous execution
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <param name="handler"></param>
        /// <param name="eventBusService">eventBusService to use, EventBus.Default if null</param>
        /// <returns></returns>
        public static Delegate<TEvent> Async<TEvent>(this Delegate<TEvent> handler, EventBusService eventBusService = null)
        {
            return (param) =>
            {
                handler.BeginInvoke(param, new AsyncCallback<TEvent>(handler,
                    eventBusService == null ? EventBus.Default : eventBusService)
                    .Callback, null);
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
        public static Delegate<TEvent> AddOnFail<TEvent, TException>(this ICanHandle<TEvent> handler, ICanHandle<TEvent> onFailHandler, OnException.DelegateInterface onException = null) where TException : Exception
        {
            return AddOnFail<TEvent, TException>(handler.Handle, onFailHandler.Handle, onException);
        }

        /// <summary>
        /// Subscribe to event with special behavior when handler failed. If it fails with TException - onFail is called
        /// </summary>
        public static Delegate<TEvent> AddOnFail<TEvent, TException>(this Delegate<TEvent> handler, ICanHandle<TEvent> onFailHandler, OnException.DelegateInterface onException = null) where TException : Exception
        {
            return AddOnFail<TEvent, TException>(handler, onFailHandler.Handle, onException);
        }

        /// <summary>
        /// Subscribe to event with special behavior when handler failed. If it fails with TException - onFail is called
        /// </summary>
        public static Delegate<TEvent> AddOnFail<TEvent, TException>(this ICanHandle<TEvent> handler, Delegate<TEvent> onFailHandler, OnException.DelegateInterface onException = null) where TException : Exception
        {
            return AddOnFail<TEvent, TException>(handler.Handle, onFailHandler, onException);
        }

        /// <summary>
        /// Subscribe to event with special behavior when handler failed. If it fails with TException - onFail is called
        /// </summary>
        public static Delegate<TEvent> AddOnFail<TEvent, TException>(this Delegate<TEvent> handler, Delegate<TEvent> onFailHandler, OnException.DelegateInterface onException = null) where TException : Exception
        {
            return (param) =>
            {
                try
                {
                    handler(param);
                }
                catch (TException ex)
                {
                    onFailHandler(param);
                    onException?.Invoke(ex);
                }
            };
        }

        /// <summary>
        /// Default timespan for no interval for Retry 
        /// </summary>
        public readonly static TimeSpan NoDelayForRetry = TimeSpan.FromSeconds(0);

        public static Delegate<TEvent> Retry<TEvent, TException>(this ICanHandle<TEvent> handler, int times) where TException : Exception
        {
            return Retry<TEvent, TException>(handler.Handle, times, NoDelayForRetry);
        }

        public static Delegate<TEvent> Retry<TEvent, TException>(this Delegate<TEvent> handler, int times) where TException : Exception
        {
            return Retry<TEvent, TException>(handler, times, NoDelayForRetry);
        }

        public static Delegate<TEvent> Retry<TEvent, TException>(this ICanHandle<TEvent> handler, int times, TimeSpan interval) where TException : Exception
        {
            return Retry<TEvent, TException>(handler.Handle, times, interval);
        }

        public static Delegate<TEvent> Retry<TEvent, TException>(this Delegate<TEvent> handler, int times, TimeSpan interval) where TException : Exception
        {
            if (times < 1)
            {
                throw new ArgumentException("Times should be greater than 0");
            }
            if (interval.CompareTo(NoDelayForRetry) == -1)
            {
                throw new ArgumentException("Delay interval could not be negative");
            }
            return (param) =>
            {
                int calls = 0;
                Exception lastException = null;
                while (calls < times)
                {
                    try
                    {
                        handler(param);
                        // to know that there is nothing to throw
                        lastException = null;
                        // to exit while
                        calls = times;
                    }
                    catch (TException ex)
                    {
                        lastException = ex;
                        calls++;

                        if(interval != NoDelayForRetry)
                        {
                            Task.Delay(interval).Wait();
                        }
                    }
                }
                if (lastException != null)
                {
                    throw new RetryFailedException($"Maximum number of attempts ({times}) exceeded", lastException);
                }
            };
        }
    }
}

using SocialTalents.Hp.Events.Exceptions;
using SocialTalents.Hp.Events.Internal;
using SocialTalents.Hp.Events.Queue;
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
        public static Delegate<TEvent> Async<TEvent>(this ICanHandle<TEvent> handler, EventBusService eventBusService = null)
        {
            Delegate<TEvent> handlerAsDelegate = handler.Handle;
            return Async<TEvent>(handlerAsDelegate, eventBusService);
        }

        /// <summary>
        /// Subscribe to event with special behavior when handler failed. If it fails with TException - onFail is called
        /// </summary>
        public static Delegate<TEvent> AddOnFail<TEvent, TException>(this ICanHandle<TEvent> handler, ICanHandle<TEvent> onFailHandler, OnException.DelegateInterface onException = null) where TException : Exception
        {
            return AddOnFail<TEvent, TException>(handler.Handle, (a, e) => onFailHandler.Handle(a), onException);
        }

        /// <summary>
        /// Subscribe to event with special behavior when handler failed. If it fails with TException - onFail is called
        /// </summary>
        public static Delegate<TEvent> AddOnFail<TEvent, TException>(this Delegate<TEvent> handler, ICanHandle<TEvent> onFailHandler, OnException.DelegateInterface onException = null) where TException : Exception
        {
            return AddOnFail<TEvent, TException>(handler, (a, e) => onFailHandler.Handle(a), onException);
        }

        /// <summary>
        /// Subscribe to event with special behavior when handler failed. If it fails with TException - onFail is called
        /// </summary>
        public static Delegate<TEvent> AddOnFail<TEvent, TException>(this ICanHandle<TEvent> handler, FailDelegate<TEvent> onFailHandler, OnException.DelegateInterface onException = null) where TException : Exception
        {
            return AddOnFail<TEvent, TException>(handler.Handle, onFailHandler, onException);
        }

        /// <summary>
        /// Subscribe to event with special behavior when handler failed. If it fails with TException - onFail is called
        /// </summary>
        public static Delegate<TEvent> AddOnFail<TEvent, TException>(this Delegate<TEvent> handler, FailDelegate<TEvent> onFailHandler, OnException.DelegateInterface onException = null) where TException : Exception
        {
            return (param) =>
            {
                try
                {
                    handler(param);
                }
                catch (TException ex)
                {
                    onFailHandler(param, ex);
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

        /// <summary>
        /// Wraps handler to handle events which has been queued
        /// </summary>
        /// <typeparam name="TEvent">Initial event type</typeparam>
        /// <param name="handler">Handler to use`</param>
        /// <returns></returns>
        public static Delegate<QueuedEvent<TEvent>> AsQueued<TEvent>(this Delegate<TEvent> handler)
        {
            return (QueuedEvent<TEvent> param) =>
            {
                handler(param.Event);
            };
        }

        /// <summary>
        /// Wraps handler to handle events which has been queued
        /// </summary>
        /// <typeparam name="TEvent">Handler to use</typeparam>
        /// <param name="handler">Handler to use</param>
        /// <returns></returns>
        public static Delegate<QueuedEvent<TEvent>> AsQueued<TEvent>(this ICanHandle<TEvent> handler)
        {
            Delegate<TEvent> handlerAsDelegate = handler.Handle;
            return AsQueued(handlerAsDelegate);
        }

        /// <summary>
        /// Retry strategy for queued events
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <param name="handler"></param>
        /// <param name="times"></param>
        /// <param name="backOffStrategy"></param>
        /// <returns></returns>
        public static Delegate<QueuedEvent<TEvent>> RetryQueued<TEvent>(this Delegate<QueuedEvent<TEvent>> handler, int times, Action<IQueueItem> backOffStrategy)
        {
            return RetryQueued<TEvent, Exception>(handler, times, backOffStrategy);
        }
        
        /// <summary>
        /// Retry strategy for qued events with specific exception to catch
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <typeparam name="TException"></typeparam>
        /// <param name="handler"></param>
        /// <param name="times"></param>
        /// <param name="backOffStrategy"></param>
        /// <returns></returns>
        public static Delegate<QueuedEvent<TEvent>> RetryQueued<TEvent, TException>(this Delegate<QueuedEvent<TEvent>> handler, int times, Action<IQueueItem> backOffStrategy) where TException : Exception
        {
            return (arg) =>
            {
                try
                {
                    handler(arg);
                }
                catch (TException ex)
                {
                    arg.Item.Attempts++;
                    if (arg.Item.Attempts <= times)
                    {
                        backOffStrategy(arg.Item);
                        throw new RetryNeededException($"Max Failure number not exceeded ({arg.Item.Attempts} < {times})", ex);
                    }
                    throw new RetryFailedException($"Maximum number of attempts ({times}) exceeded", ex);
                }
            };
        }

        public static Delegate<QueuedEvent<TEvent>> WhenRetryQueueFailed<TEvent>(this ICanHandle<QueuedEvent<TEvent>> handler, ICanHandle<QueuedEvent<TEvent>> onFailHandler)
        {
            return WhenRetryQueueFailed<TEvent>(handler.Handle, (a, e) => onFailHandler.Handle(a));
        }

        /// <summary>
        /// Subscribe to event with special behavior when handler failed. If it fails with TException - onFail is called
        /// </summary>
        public static Delegate<QueuedEvent<TEvent>> WhenRetryQueueFailed<TEvent>(this Delegate<QueuedEvent<TEvent>> handler, ICanHandle<QueuedEvent<TEvent>> onFailHandler) 
        {
            return WhenRetryQueueFailed<TEvent>(handler, (a, e) => onFailHandler.Handle(a));
        }

        /// <summary>
        /// Subscribe to event with special behavior when handler failed. If it fails with TException - onFail is called
        /// </summary>
        public static Delegate<QueuedEvent<TEvent>> WhenRetryQueueFailed<TEvent>(this ICanHandle<QueuedEvent<TEvent>> handler, 
            FailDelegate<QueuedEvent<TEvent>> onFailHandler)
        {
            return WhenRetryQueueFailed<TEvent>(handler.Handle, onFailHandler);
        }

        public static Delegate<QueuedEvent<TEvent>> WhenRetryQueueFailed<TEvent>(this Delegate<QueuedEvent<TEvent>> handler,
            FailDelegate<QueuedEvent<TEvent>> onFailHandler)
        {
            return (arg) =>
            {
                try
                {
                    handler(arg);
                }
                catch (RetryFailedException ex)
                {
                    onFailHandler(arg, ex);
                    // re-throwing, so event can be marked as deleted
                    throw ex;
                }
            };
        }
    }
}

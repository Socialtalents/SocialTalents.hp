using SocialTalents.Hp.Events.Internal;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace SocialTalents.Hp.Events
{
    public delegate void Delegate<TArg>(TArg param);
    public delegate void FailDelegate<TArg>(TArg param, Exception ex);
    public delegate DateTime ExecutionTimeCalculator();

    /// <summary>
    /// EventBus used to Subcsribe to handle events and Publish them
    /// Refer to https://github.com/Socialtalents/SocialTalents.hp for sources and samples
    /// </summary>
    public static class EventBus
    {
        private static EventBusService _eventBusService = new EventBusService();

        /// <summary>
        /// Default convinient instance of EventBusService, used in static methods
        /// </summary>
        public static EventBusService Default
        {
            get { return _eventBusService; }
            set
            {
                _eventBusService = value ?? throw new ArgumentNullException("Default EventBusService instance cannot be null");
            }
        }


        /// <summary>
        /// Subscribe handler to specific TEvent type
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <param name="handler">delegate to handle event</param>
        public static void Subscribe<TEvent>(Delegate<TEvent> handler)
        {
            Default.Subscribe(handler);
        }

        /// <summary>
        /// Subscribe handler to specific TEvent type by interface
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <param name="handler"></param>
        public static void Subscribe<TEvent>(ICanHandle<TEvent> handler)
        {
            Default.Subscribe<TEvent>(handler.Handle);
        }

        /// <summary>
        /// Publish event of type TEvent for handling.
        /// When there are no subscribers for this event NoSubscribers event published
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <param name="eventInstance">Event to publish</param>
        /// <param name="sender">sender instance</param>
        public static void Publish<TEvent>(TEvent eventInstance, ICanPublish<TEvent> sender)
        {
            Default.Publish(eventInstance, sender);
        }

        /// <summary>
        /// Clear all wokrflows state
        /// </summary>
        public static void Clear()
        {
            Default.Clear();
        }
    }
}

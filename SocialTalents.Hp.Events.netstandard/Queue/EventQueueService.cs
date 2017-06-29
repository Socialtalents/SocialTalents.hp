using SocialTalents.Hp.Events.Internal;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;

namespace SocialTalents.Hp.Events.Queue
{
    /// <summary>
    /// Store events in queue and process them syncronously
    /// </summary>
    public class EventQueueService
    {
        protected IEventQueueRepository _repository;
        protected EventBusService _eventBusService;

        public EventQueueService(IEventQueueRepository repository, EventBusService eventBusService = null)
        {
            _repository = repository;
            _eventBusService = eventBusService == null ? EventBus.Default : eventBusService;
        }

        /// <summary>
        /// handler to enque event when it published
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <returns></returns>
        public Delegate<TEvent> Enque<TEvent>()
        {
            return Enque<TEvent>(TimeSpan.Zero);
        }

        /// <summary>
        /// Handler to enque event when it published to be handled after some interval
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <param name="handleAfter"></param>
        /// <returns></returns>
        public virtual Delegate<TEvent> Enque<TEvent>(TimeSpan handleAfter)
        {
            return (TEvent e) => AddEvent(e, typeof(TEvent), handleAfter);
        }
        
        /// <summary>
        /// Use repository to build new QueueItem and fill it with various data
        /// </summary>
        /// <param name="eventInstance"></param>
        /// <param name="queueType"></param>
        /// <param name="handleAfter"></param>
        protected virtual void AddEvent(object eventInstance, Type queueType, TimeSpan handleAfter) 
        {
            var item = _repository.BuildNewItem(eventInstance);
            item.HandleAfter = DateTime.Now.Add(handleAfter);
            // storing only type name and assembly name, otherwise it will be impossible to deserialise object with newer assembly
            item.DeclaringEventType =
                string.Join(",",
                    queueType.AssemblyQualifiedName.Split(',').Take(2)).Trim();

            onAddEvent(item, eventInstance);

            _repository.AddItem(item);
        }

        /// <summary>
        /// Likely this method can be overrided to use custom fields to control HandleAfter, UniqueKey or add shards
        /// </summary>
        /// <param name="eventInstance"></param>
        protected virtual void onAddEvent(IQueueItem item, object eventInstance)
        {
            var uniqueKey = eventInstance as IUniqueEvent;
            if (uniqueKey != null)
            {
                item.UniqueKey = uniqueKey.UniqueKey;
            }
        }

        /// <summary>
        /// Default timeout interval for ProcessEvents
        /// </summary>
        public TimeSpan ProcessingTimeLimit { get; set; } = TimeSpan.FromSeconds(30);
        /// <summary>
        /// Default portion size to read from queue
        /// </summary>
        public int PortionSize { get; set; } = 50;

        /// <summary>
        /// Process events from queue till queue is empty or ProcessingTimeLimit hits 
        /// </summary>
        /// <returns>Statistics run</returns>
        public virtual ProcessEventsResult ProcessEvents()
        {
            ProcessEventsResult result = onBuildProcessEventsResult();

            while (DateTime.Now.Subtract(result.Started).CompareTo(ProcessingTimeLimit) < 0)
            {
                var itemsPortion = _repository.GetItemsToHandle(PortionSize);

                foreach(var item in itemsPortion)
                {
                    if (DateTime.Now.Subtract(result.Started).CompareTo(ProcessingTimeLimit) >= 0)
                        break;
                    onProcessQueueItem(item);
                    result.Processed++;
                }
                if (itemsPortion.Count() < PortionSize) { break; }
            }
            return result;
        }

        /// <summary>
        /// Thos method used to publish event and handle exception, if happened
        /// </summary>
        /// <param name="item"></param>
        protected virtual void onProcessQueueItem(IQueueItem item)
        {
            MethodInfo genericMethod = GetPublishMethodInfo(item.DeclaringEventType);
            object typedEvent = BuildGenericQueueEvent(item);
            object sender = BuildSender(item.DeclaringEventType);
            try
            {
                genericMethod.Invoke(_eventBusService, new[] { typedEvent, sender });
                onEventHandled(item);
            }
            // Exceptions from actual delegate are wrapped into TargetInvocationException by MethodBase.Invoke()
            catch (TargetInvocationException ex)
            {
                RetryNeededException retry = ex.InnerException as RetryNeededException;
                if (retry != null)
                {
                    onEventRetried(item, retry);
                }
                else
                {
                    onEventFailed(item, ex.InnerException);
                }
            }
        }

        /// <summary>
        /// Called when event handling failed and retry not needed
        /// </summary>
        /// <param name="item"></param>
        /// <param name="ex"></param>
        protected virtual void onEventFailed(IQueueItem item, Exception ex)
        {
            _repository.DeleteItem(item);
        }

        /// <summary>
        /// Called when we need to retry event
        /// </summary>
        /// <param name="item"></param>
        /// <param name="ex"></param>
        protected virtual void onEventRetried(IQueueItem item, RetryNeededException ex)
        {
            _repository.UpdateItem(item);
        }

        /// <summary>
        /// Called when event handled successfully
        /// </summary>
        /// <param name="item"></param>
        protected virtual void onEventHandled(IQueueItem item)
        {
            _repository.DeleteItem(item);
        }

        #region A bit of refleciton magic to support Publish<TEvent>
        private ConcurrentDictionary<string, MethodInfo> _raiseMethodByType = new ConcurrentDictionary<string, MethodInfo>();
        private ConcurrentDictionary<string, ConstructorInfo> _typedEventConstructorByType = new ConcurrentDictionary<string, ConstructorInfo>();
        private ConcurrentDictionary<string, object> _sendersByType = new ConcurrentDictionary<string, object>();

        protected MethodInfo GetPublishMethodInfo(string declaredEventType)
        {
            var result = _raiseMethodByType.GetOrAdd(declaredEventType,
                (i) =>
                {
                    // building QueuedEvent<declaredEventType>
                    Type genericType = typeof(QueuedEvent<>).MakeGenericType(Type.GetType(i));

                    // lookup for EventBusService.Publish<QueuedEvent<declaredEventType>>(event, sender)
                    MethodInfo method = typeof(EventBusService).GetRuntimeMethods()
                        .Single(m => m.Name == "Publish" && m.GetParameters().Length == 2);

                    MethodInfo genericMethod = method.MakeGenericMethod(genericType);
                    return genericMethod;
                });
            return result;
        }
        
        protected object BuildGenericQueueEvent(IQueueItem eventInstance)
        {
            Type genericType = typeof(QueuedEvent<>).MakeGenericType(Type.GetType(eventInstance.DeclaringEventType));

            return Activator.CreateInstance(genericType, new object[] { eventInstance, eventInstance.Event });
        }

        protected object BuildSender(string declaredEventType)
        {
            var result = _sendersByType.GetOrAdd(declaredEventType,
                (i) =>
                {
                    // We are simulating new SenderProxy<QueuedEvent<declaredEventType>>(this)
                    Type genericEvent = typeof(QueuedEvent<>).MakeGenericType(Type.GetType(i));
                    Type genericType = typeof(SenderProxy<>).MakeGenericType(genericEvent);

                    return Activator.CreateInstance(genericType, new object[] { this });
                }
            );
            return result;
        }
        #endregion

        protected virtual ProcessEventsResult onBuildProcessEventsResult()
        {
            return new ProcessEventsResult();
        }
    }
}

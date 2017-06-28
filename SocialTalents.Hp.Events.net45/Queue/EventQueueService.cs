using SocialTalents.Hp.Events.Internal;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;

namespace SocialTalents.Hp.Events.Queue
{
    /// <summary>
    /// Store events in memory and process them syncronously
    /// </summary>
    public class EventQueueService : IEventQueueService
    {
        protected IEventQueueRepository _repository;
        protected EventBusService _eventBusService;
        public EventQueueService(IEventQueueRepository repository, EventBusService eventBusService = null)
        {
            _repository = repository;
            _eventBusService = eventBusService == null ? EventBus.Default : eventBusService;
        }

        public virtual Delegate<TEvent> Enque<TEvent>()
        {
            return Enque<TEvent>(TimeSpan.Zero);
        }

        public virtual Delegate<TEvent> Enque<TEvent>(TimeSpan handleAfter)
        {
            return (TEvent e) => AddEvent(e, typeof(TEvent), handleAfter);
        }
        
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

        public TimeSpan ProcessingTimeLimit{ get; set; } = TimeSpan.FromSeconds(30);
        public int PortionSize { get; set; } = 50;

        public ProcessEventsResult ProcessEvents()
        {
            ProcessEventsResult result = onBuildProcessEventsResult();

            while (DateTime.Now.Subtract(result.Started).CompareTo(ProcessingTimeLimit) < 0)
            {
                var itemsPortion = _repository.GetItemsToHandle(PortionSize);

                foreach(var item in itemsPortion)
                {
                    if (DateTime.Now.Subtract(result.Started).CompareTo(ProcessingTimeLimit) >= 0)
                        break;

                    MethodInfo genericMethod = GetPublishMethodInfo(item.DeclaringEventType);
                    object typedEvent = BuildGenericQueueEvent(item);
                    object sender = BuildSender(item.DeclaringEventType);
                    try
                    {
                        genericMethod.Invoke(_eventBusService, new[] { typedEvent, sender });
                        onEventHandled(item);    
                    }
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
                    result.Processed++;
                }
                if (itemsPortion.Count() < PortionSize) { break; }
            }
            return onProcessEventsCompleted(result);
        }

        private void onEventFailed(IQueueItem item, Exception ex)
        {
            _repository.DeleteItem(item);
        }

        protected virtual void onEventRetried(IQueueItem item, RetryNeededException ex)
        {
            _repository.UpdateItem(item);
        }

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
                    // получаем DelayedEvent<TEvent>
                    Type genericType = typeof(QueuedEvent<>).MakeGenericType(Type.GetType(i));

                    // Вызываем EventBus.Raise<DelayedEvent<TEvent>>(DelayedEvent)
                    MethodInfo method = typeof(EventBusService).GetRuntimeMethods()
                        .Single(m => m.Name == "Publish" && m.GetParameters().Length == 2);

                    MethodInfo genericMethod = method.MakeGenericMethod(genericType);
                    return genericMethod;
                });
            return result;
        }
        
        private object BuildGenericQueueEvent(IQueueItem eventInstance)
        {
            Type genericType = typeof(QueuedEvent<>).MakeGenericType(Type.GetType(eventInstance.DeclaringEventType));

            return Activator.CreateInstance(genericType, new object[] { eventInstance, eventInstance.Event });
        }

        private object BuildSender(string declaredEventType)
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

        protected virtual ProcessEventsResult onProcessEventsCompleted(ProcessEventsResult stat)
        {
            return stat;
        }

        protected virtual ProcessEventsResult onBuildProcessEventsResult()
        {
            return new ProcessEventsResult();
        }
    }
}

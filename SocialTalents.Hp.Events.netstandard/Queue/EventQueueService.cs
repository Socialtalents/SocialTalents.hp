using SocialTalents.Hp.Events.Internal;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SocialTalents.Hp.Events.Queue
{
    /// <summary>
    /// Store events in queue and process them syncronously
    /// </summary>
    public class EventQueueService:
        ICanHandle<RequeueStuckEvents>,
        ICanPublish<RequeueStuckEvents>,
        ICanPublish<Exception>
    {
        protected IEventQueueRepository _repository;
        protected EventBusService _eventBusService;

        public EventQueueService(IEventQueueRepository repository, EventBusService eventBusService = null)
        {
            _repository = repository;
            
            _eventBusService = eventBusService == null ? EventBus.Default : eventBusService;

            SubscribeForRequeueStuckEvents(eventBusService);
        }

        protected virtual void SubscribeForRequeueStuckEvents(EventBusService eventBus)
        {
            eventBus.Subscribe(Enque<RequeueStuckEvents>(TimeSpan.FromSeconds(40)));
            eventBus.Subscribe(this.WhenQueued());
            // enqueue a first event
            eventBus.Publish(new RequeueStuckEvents(), this);
        }

        /// <summary>
        /// handler to enque event when it published
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <returns></returns>
        public Delegate<TEvent> Enque<TEvent>()
        {
            return (TEvent e) => AddEventHandler(e, typeof(TEvent), TimeSpan.Zero);
        }

        /// <summary>
        /// Handler to enque event when it published to be handled after some interval
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <param name="handlingDelay"></param>
        /// <returns></returns>
        public virtual Delegate<TEvent> Enque<TEvent>(TimeSpan handlingDelay)
        {
            return (TEvent e) => AddEventHandler(e, typeof(TEvent), handlingDelay);
        }
        
        /// <summary>
        /// Use repository to build new QueueItem and fill it with various data
        /// </summary>
        /// <param name="eventInstance"></param>
        /// <param name="queueType"></param>
        /// <param name="handlingDelay"></param>
        protected virtual void AddEventHandler(object eventInstance, Type queueType, TimeSpan handlingDelay) 
        {
            var item = _repository.BuildNewItem(eventInstance);
            var eventAsHandleAfter = eventInstance as IHandleAfter;
            if (eventAsHandleAfter != null && handlingDelay != TimeSpan.Zero)
            {
                throw new InvalidOperationException("Events which implements IHandleAfter cannot support handlingDelay");
            }
            item.HandleAfter = eventAsHandleAfter != null ? eventAsHandleAfter.HandleAfter : DateTime.Now.Add(handlingDelay);
            // storing only type name and assembly name, otherwise it will be impossible to deserialise object with newer assembly
            item.DeclaringEventType = queueType.AssemblyQualifiedName;

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
            else
            {
                item.UniqueKey = Guid.NewGuid().ToString();
            }
        }

        /// <summary>
        /// Default timeout interval for ProcessEvents
        /// </summary>
        public TimeSpan ProcessingTimeLimit { get; set; } = TimeSpan.FromSeconds(30);
        /// <summary>
        /// Default timeout when not completed event returned to execution pool
        /// </summary>
        public TimeSpan RequeueTimeLimit
        {
            get { return _repository.QueueExecutionTimeout; }
            set { _repository.QueueExecutionTimeout = value; }
        }

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

            bool exitLoop = false;

            Task.Delay(ProcessingTimeLimit).ContinueWith((a) => exitLoop = true);

            while (!exitLoop)
            {
                var itemsPortion = _repository.GetItemsToHandle(PortionSize);
                
                foreach(var item in itemsPortion)
                {
                    // exitLoop value changed by parallel tread, might make sense to disable it for debug
                    if (exitLoop)
                        break;
                    onProcessQueueItem(item);
                    result.Processed++;
                }

                if (!itemsPortion.Any()) { break; }
            }
            return result;
        }

        /// <summary>
        /// Thos method used to publish event and handle exception, if happened
        /// </summary>
        /// <param name="item"></param>
        protected virtual void onProcessQueueItem(IQueueItem item)
        {
            MethodInfo genericMethod;
            object typedEvent;
            object sender;
            // Any exception within following try/catch block is final
            // report exception for handing and remove event from queue as failed
            try
            {
                genericMethod = GetPublishMethodInfo(item.DeclaringEventType);
                typedEvent = BuildGenericQueueEvent(item);
                sender = BuildSender(item.DeclaringEventType);
            } 
            catch (Exception ex)
            {
                onEventFailed(item, ex);
                _eventBusService.Publish<Exception>(ex, this);
                return;
            }

            // Event handling here
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
        protected ConcurrentDictionary<string, MethodInfo> _raiseMethodByType = new ConcurrentDictionary<string, MethodInfo>();
        protected ConcurrentDictionary<string, ConstructorInfo> _typedEventConstructorByType = new ConcurrentDictionary<string, ConstructorInfo>();
        protected ConcurrentDictionary<string, object> _sendersByType = new ConcurrentDictionary<string, object>();

        protected virtual MethodInfo GetPublishMethodInfo(string declaredEventType)
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
        
        protected virtual object BuildGenericQueueEvent(IQueueItem eventInstance)
        {
            Type genericType = typeof(QueuedEvent<>).MakeGenericType(Type.GetType(eventInstance.DeclaringEventType));

            return Activator.CreateInstance(genericType, new object[] { eventInstance, eventInstance.Event });
        }

        protected virtual object BuildSender(string declaredEventType)
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

        public virtual void Handle(RequeueStuckEvents e)
        {
            try
            {
                _repository.RequeueOldEvents();
            }
            finally
            {
                _eventBusService.Publish(e.Clone(), this);
            }
        }
    }
}

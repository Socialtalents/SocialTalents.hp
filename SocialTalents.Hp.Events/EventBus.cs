using SocialTalents.Hp.Events.Internal;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace SocialTalents.Hp.Events
{
    public delegate void WorkflowStepDelegate<TArg>(TArg param);
    public delegate DateTime ExecutionTimeCalculator();

    public static class EventBus
    {
        private static ConcurrentDictionary<Type, object> _workflows = new ConcurrentDictionary<Type, object>();
        private static ConcurrentDictionary<Type, NoSubscribers> _noSubscribersRegistry = new ConcurrentDictionary<Type, NoSubscribers>();
        private static int _maxSubscriptionsPerEvent = 20;

        /// <summary>
        /// MaxSubscriptionsPerEvent used to catch possible developers errors, when they assign subscribers in loop, etc. Default value is 20.
        /// </summary>
        public static int MaxSubscriptionsPerEvent
        {
            get { return _maxSubscriptionsPerEvent; }
            set { _maxSubscriptionsPerEvent = value; }
        }

        /// <summary>
        /// Subscribe handler to specific TEvent type
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <param name="handler"></param>
        public static void Subscribe<TEvent>(WorkflowStepDelegate<TEvent> handler)
        {
            object workflowAsObject = _workflows.GetOrAdd(typeof(TEvent), (i) => { return new Workflow<TEvent>(); });
            lock (workflowAsObject)
            {
                Workflow<TEvent> workflow = (Workflow<TEvent>)workflowAsObject;
                workflow.OnEvent += handler;
                if (workflow._steps.Count > MaxSubscriptionsPerEvent)
                    throw new InvalidOperationException(
                        string.Format("More than {0} subscriptions per event is not allowed by default to prevent possible errors. Increase MaxSubscriptionsPerEvent or check your code for errors", MaxSubscriptionsPerEvent));
            }
        }

        /// <summary>
        /// Subscribe handler to specific TEvent type by interface
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <param name="handler"></param>
        public static void Subscribe<TEvent>(ICanHandle<TEvent> handler)
        {
            Subscribe<TEvent>(handler.Handle);
        }

        public static void Raise<TEvent>(TEvent eventInstance, ICanRaise<TEvent> sender)
        {
            if (sender == null)
                throw new ArgumentException("sender could not be null");

            object outValue = null;
            if (_workflows.TryGetValue(typeof(TEvent), out outValue))
            {
                Workflow<TEvent> chain = (Workflow<TEvent>)outValue;
                if (chain != null)
                {
                    chain.Execute(eventInstance);
                }
            }
            else
            {
                if (typeof(TEvent) != typeof(NoSubscribers))
                {
                    Raise<NoSubscribers>(countNoSubscribers<TEvent>(eventInstance), _NoSubscriberSource);
                }
            }
        }

        internal delegate void OnEventDelegate(Type e);
        internal static ICanRaise<NoSubscribers> _NoSubscriberSource = new SenderStub<NoSubscribers>();

        /// <summary>
        /// Clear all wokrflows state
        /// </summary>
        public static void Clear()
        {
            _workflows = new ConcurrentDictionary<Type, object>();
            _noSubscribersRegistry = new ConcurrentDictionary<Type, NoSubscribers>();
        }
        /*
        public static void SubscribeAsync<TEvent>(WorkflowStepDelegate<TEvent> handler)
        {
            Subscribe<TEvent>(Async(handler));
        }

        public static void SubscribeAsync<TEvent>(ICanHandle<TEvent> handler)
        {
            SubscribeAsync<TEvent>(handler.Handle);
        }

        private static WorkflowStepDelegate<TEvent> Async<TEvent>(WorkflowStepDelegate<TEvent> handler)
        {
            return (param) =>
            {
                handler.BeginInvoke(param, new AsyncCallback<TEvent>(handler).Callback, null);
            };
        }*/

        private static NoSubscribers countNoSubscribers<TEvent>(TEvent data)
        {
            NoSubscribers result = _noSubscribersRegistry.GetOrAdd(typeof(TEvent), new NoSubscribers());
            result.Counter++;
            result.LastEvent = data;
            return result;
        }
    }
}

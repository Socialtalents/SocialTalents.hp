using SocialTalents.Hp.Events.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace SocialTalents.Hp.Events.Internal
{
    internal class SubscribersList<TArg>
    {
        internal List<Delegate<TArg>> _subscribers = new List<Delegate<TArg>>();

        internal SubscribersList()
        {

        }

        internal event Delegate<TArg> OnEvent
        {
            add
            {
                _subscribers.Add(value);
            }
            remove
            {
                if (_subscribers.Contains(value))
                    _subscribers.Remove(value);
            }
        }

        internal void Execute(TArg args)
        {
            foreach (var stepHandler in _subscribers)
            {
                try
                {
                    stepHandler(args);
                }
                // handler can throw AbortExecutionException to indicate that no further processing is possible
                catch (AbortExecutionException)
                { break; }
            }
        }

        internal SubscribersList<TArg> AddStep(Delegate<TArg> handler)
        {
            OnEvent += handler;
            return this;
        }
    }
}

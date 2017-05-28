using System;
using System.Collections.Generic;
using System.Text;

namespace SocialTalents.Hp.Events
{
    /// <summary>
    /// Declarative interface to exppose that class can handle events
    /// You still need to add actual subscription with EventBus.Subscribe()
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ICanHandle<T>
    {
        void Handle(T param);
    }
}

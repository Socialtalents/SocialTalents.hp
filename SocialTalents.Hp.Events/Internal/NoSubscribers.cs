using System;
using System.Collections.Generic;
using System.Text;

namespace SocialTalents.Hp.Events.Internal
{
    /// <summary>
    /// Counter for events with no subscribers for them
    /// </summary>
    public class NoSubscribers
    {
        public NoSubscribers() { Counter = 0; }

        public object LastEvent { get; set; }
        public int Counter { get; internal set; }
    }
}

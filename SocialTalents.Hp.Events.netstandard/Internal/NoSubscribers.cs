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
        /// <summary>
        /// Default constructor
        /// </summary>
        public NoSubscribers() { Counter = 0; }

        /// <summary>
        /// Contains last non handled event instance
        /// </summary>
        public object LastEvent { get; set; }
        /// <summary>
        /// Counter for not handled events since app start
        /// </summary>
        public int Counter { get; internal set; }
    }
}

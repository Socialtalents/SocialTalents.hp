using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialTalents.Hp.Events.Queue
{
    /// <summary>
    /// Allow to control handling time for delayed events
    /// </summary>
    public interface IHandleAfter
    {
        /// <summary>
        /// Queued events handling will not be handled till this moment
        /// </summary>
        DateTime HandleAfter { get; }
    }
}

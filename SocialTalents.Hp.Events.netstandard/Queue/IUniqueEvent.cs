using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialTalents.Hp.Events.Queue
{
    public interface IUniqueEvent
    {
        /// <summary>
        /// Key to deduplicate incoming events for
        /// </summary>
        string UniqueKey { get; }
    }
}

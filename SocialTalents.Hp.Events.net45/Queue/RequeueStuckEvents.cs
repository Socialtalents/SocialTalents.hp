using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialTalents.Hp.Events.Queue
{
    public class RequeueStuckEvents : IUniqueEvent
    {
        public string UniqueKey => nameof(RequeueStuckEvents);

        public virtual RequeueStuckEvents Clone()
        {
            return new RequeueStuckEvents();
        }
    }
}

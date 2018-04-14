using SocialTalents.Hp.Events.Queue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialTalents.Hp.UnitTests.Events.Internal
{
    class HandleAfterEvent : IHandleAfter
    {
        public DateTime HandleAfter { get; set; }
    }
}

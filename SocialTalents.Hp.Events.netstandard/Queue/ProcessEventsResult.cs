using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialTalents.Hp.Events.Queue
{
    public class ProcessEventsResult
    {
        public ProcessEventsResult() { }
        public int Processed { get; set; } = 0;
        public DateTime Started { get; } = DateTime.Now;
    }
}

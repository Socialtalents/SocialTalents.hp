using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialTalents.Hp.Events.Queue
{

#if !NETSTANDARD1_2
    [Serializable]
#endif
    public class RetryNeededException : Exception
    {
        public RetryNeededException() { }
        public RetryNeededException(string message) : base(message) { }
        public RetryNeededException(string message, Exception inner) : base(message, inner) { }
#if !NETSTANDARD1_2
        protected RetryNeededException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
#endif
    }
}

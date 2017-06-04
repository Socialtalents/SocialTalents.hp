using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialTalents.Hp.Events.Exceptions
{
    /// <summary>
    /// RetryFailedException thrown by Retry when maximum number of attempts exceeded
    /// </summary>
#if !NETSTANDARD1_2
    [Serializable]
#endif
    public class RetryFailedException : Exception
    {
        public RetryFailedException() { }
        public RetryFailedException(string message) : base(message) { }
        public RetryFailedException(string message, Exception inner) : base(message, inner) { }
#if !NETSTANDARD1_2 
        protected RetryFailedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
#endif
    }
}

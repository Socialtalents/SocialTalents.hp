using System;

namespace SocialTalents.Hp.Events.Exceptions
{
    /// <summary>
    /// Event handler method can throw this exception to prevent further execution of event subscribers chain
    /// </summary>
#if !NETSTANDARD1_2
    [Serializable]
#endif
    public class AbortExecutionException : Exception
    {
        public AbortExecutionException() { }
        public AbortExecutionException(string message) : base(message) { }
        public AbortExecutionException(string message, Exception inner) : base(message, inner) { }
#if !NETSTANDARD1_2
        protected AbortExecutionException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
#endif
    }
}

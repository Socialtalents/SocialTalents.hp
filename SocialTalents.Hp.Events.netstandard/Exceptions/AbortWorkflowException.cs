using System;

namespace SocialTalents.Hp.Events.Exceptions
{

    /// <summary>
    /// Event handler method can throw this exception to prevent further execution of event subscribers chain
    /// </summary>
    public class AbortExecutionException : Exception
    {
        public AbortExecutionException() { }
        public AbortExecutionException(string message) : base(message) { }
        public AbortExecutionException(string message, Exception inner) : base(message, inner) { }
    }
}

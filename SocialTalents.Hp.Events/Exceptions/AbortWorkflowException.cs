using System;

namespace SocialTalents.Hp.Events.Exceptions
{

    /// <summary>
    /// Raise handler method can throw this exception to prevent further execution of event subscribers chain
    /// </summary>
    public class AbortWorkflowException : Exception
    {
        public AbortWorkflowException() { }
        public AbortWorkflowException(string message) : base(message) { }
        public AbortWorkflowException(string message, Exception inner) : base(message, inner) { }
    }
}

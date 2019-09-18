using System;

namespace SocialTalents.Hp.MongoDB.Exceptions
{
    public class OptimistickLockException : Exception
    {
        public OptimistickLockException() : base() { }

        public OptimistickLockException(string message) : base(message) { }

        public OptimistickLockException(string message, Exception innerException) : base(message, innerException) { }
    }
}
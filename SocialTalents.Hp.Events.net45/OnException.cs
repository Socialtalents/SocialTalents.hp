using SocialTalents.Hp.Events.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialTalents.Hp.Events
{
    /// <summary>
    /// OnException used to define what to do when event bus meet exceptions in code
    /// In case you need own implementaiton you can easily define any method to match OnException.DelegateInterface
    /// </summary>
    // Yes, it could be generic interface but that it would have 3 implementations...
    public class OnException
    {
        public delegate void DelegateInterface(Exception ex);

        public static DelegateInterface DoNothing;
        public static DelegateInterface ThrowException;
        public static DelegateInterface PublishExceptionToDefaultEventBus;

        static OnException()
        {
            DoNothing = (Exception e) => { };
            ThrowException = (Exception e) => throw e;
            PublishExceptionToDefaultEventBus = (Exception e) => EventBus.Publish<Exception>(e, new SenderStub<Exception>());
        }
    }
}

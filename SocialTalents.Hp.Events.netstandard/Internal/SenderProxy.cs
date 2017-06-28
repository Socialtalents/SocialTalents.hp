using System;
using System.Collections.Generic;
using System.Text;

namespace SocialTalents.Hp.Events.Internal
{
    /// <summary>
    /// Sender stub used to 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SenderProxy<T> : ICanPublish<T>
    {
        public SenderProxy(object realSender)
        {
            Sender = realSender;
        }
        
        public object Sender{ get; private set; }
    }

    public class SenderUnknown
    {
        public static Object Instance = new SenderUnknown();
    }
}

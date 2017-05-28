using System;
using System.Collections.Generic;
using System.Text;

namespace SocialTalents.Hp.Events
{
    /// <summary>
    /// Decalarite interface to mark classes which can raise certain type of events
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ICanPublish<T> { }
}

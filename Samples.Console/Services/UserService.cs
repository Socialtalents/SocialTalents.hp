using Samples.Console.Event;
using Samples.Console.Model;
using SocialTalents.Hp.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Samples.Console.Services
{
    public class UserService : 
        // Mark service with ICanRaise interface to keep event visible for consumers 
        ICanPublish<UserRegistered>
    {
        public void RegisterUser(User user)
        {
            // Save user to Database here

            // Publish event so we can do something extra with user outside UserService without adding dependancies
            EventBus.Publish(new UserRegistered(user), this);
        }
    }
}

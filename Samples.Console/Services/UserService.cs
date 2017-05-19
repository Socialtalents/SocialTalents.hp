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
        ICanRaise<UserRegistered>
    {
        public void RegisterUser(User user)
        {
            // Save user to Database here

            // Raise event so we can do somthing with user outside UserService without adding dependancies
            EventBus.Raise(new UserRegistered(user), this);
        }
    }
}

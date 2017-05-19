using Samples.Console.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Samples.Console.Event
{
    public class UserRegistered
    {
        public UserRegistered(User user) { User = user; }
        public User User { get; set; }
    }
}

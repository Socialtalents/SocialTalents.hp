using Samples.Console.Event;
using SocialTalents.Hp.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Samples.Console.Services
{
    public class EmailService : 
        // Interfaces is best way to declare which events service can handle and publish
        ICanHandle<UserRegistered>, 
        ICanPublish<Exception>
    {
        public void Handle(UserRegistered param)
        {
            // we need to send email here....

            int delay = 1000 + _rnd.Next(4000);
            
            Output.Log($"Trying to send email to {param.User.Email}, Delay = {delay} ms, Chance to succeed: {SuccessRate}%", "EmailService");

            Thread.Sleep(delay);

            // Simluating failure
            if (_rnd.Next(100) > SuccessRate)
            {
                Output.Error("Simulating some error in SMTP", "EmailService");
                throw new InvalidOperationException("Smpt sending failed");
            }
            else
            {
                Output.Success($"Email to {param.User.Email} sent!", "EmailService");
            }
        }

        Random _rnd = new Random();

        public int SuccessRate { get; set; } = 100;
    }
}

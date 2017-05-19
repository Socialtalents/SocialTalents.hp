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
    public class EmailService : ICanHandle<UserRegistered>
    {
        public void Handle(UserRegistered param)
        {
            // we need to send email here....

            int delay = 1000 + rnd.Next(5000);
            
            Output.Log($"Trying to send email to {param.User.Email}, Delay = {delay} ms, ChanceToSucceed: {SuccessRate}%");

            // Let's say our email crashes too often

            if (rnd.Next(100) > SuccessRate)
            {
                throw new InvalidOperationException("Some error in SMTP");
            }

            Thread.Sleep(delay);

            Output.Log($"Email to {param.User.Email} sent!");
        }

        Random rnd = new Random();

        public int SuccessRate { get; set; } = 100;
    }
}

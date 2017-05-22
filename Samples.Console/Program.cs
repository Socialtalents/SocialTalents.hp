using Samples.Console.Event;
using Samples.Console.Model;
using Samples.Console.Services;
using SocialTalents.Hp.Events;
using System;
using System.Linq;

namespace Samples.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            SimpulateIoc();

            #region Request subscription mode from user
            string mode = null;

            while (mode == null)
            {
                Output.Message("i - In Proc handling");
                Output.Message("a - Async handling");
                Output.Message("f - Failure handling");
                mode = Output.Question("Please select mode:").ToLower();

                switch (mode)
                {
                    default:
                        mode = null;
                        Output.Error("Incorrect input, try again");
                        break;
                    case "i": SetupEventsInProc();
                        break;  
                    case "a": SetupEventsAsync();
                        break;
                    case "f":
                        SetupEventsWithOnFail();
                        break;
                }
            }
            #endregion
            
            while(true)
            {
                string userEmail = Output.Question("Enter user email:");

                User u = new User() { Email = userEmail };

                userService.RegisterUser(u);
            }
        }
        
        private static void SetupEventsInProc()
        {
            // Control returned to code only when handler execution completed
            EventBus.Subscribe(emailService);
            EventBus.Subscribe<Exception>((e) => Output.Error("Exception discovered in event handling:" + e.Message));
        }

        private static void SetupEventsAsync()
        {
            // Control returned immediately, execution happens in separate thread
            EventBus.Subscribe(emailService.Async());
            EventBus.Subscribe<Exception>((e) => Output.Error("Exception discovered in event handling:" + e.Message));
        }

        private static void SetupEventsWithOnFail()
        {
            // if handler fail we can do something else
            emailService.SuccessRate = 40;
            EventBus.Subscribe(
                emailService.AddOnFail<UserRegistered, Exception>(
                    (userRegistered) => Output.Error("Use alternative method to notify user " + userRegistered.User.Email)
                )
                // also, let's make it async
                .Async()
            );

            EventBus.Subscribe<Exception>((e) => Output.Error("Exception discovered in event handling:" + e.Message));
        }

        private static void SimpulateIoc()
        {
            emailService = new EmailService();
            userService = new UserService();
        }

        static EmailService emailService;
        static UserService userService;
    }
}

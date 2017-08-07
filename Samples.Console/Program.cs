using Samples.Console.Event;
using Samples.Console.Model;
using Samples.Console.Services;
using Samples.Console.Utils;
using SocialTalents.Hp.Events;
using SocialTalents.Hp.Events.Exceptions;
using SocialTalents.Hp.Events.Queue;
using System;
using System.Linq;
using System.Timers;

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
                Output.Message("A - Async handling");
                Output.Message("I - In Proc handling");
                Output.Message("F - Failure handling");
                Output.Message("R - retry + async handling");
                Output.Message("Q - using Even Queue");
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
                    case "r":
                        SetupEventsWithRetryAndAsync();
                        break;
                    case "q":
                        SetupEventsWithQueue();
                        break;
                }
            }
            #endregion
            
            Output.Message("Simulating registration for 5 users");

            SimulateUserRegistered("jay@example.net");
            SimulateUserRegistered("info@example.com");
            SimulateUserRegistered("some-test-email@mailinator.com");
            SimulateUserRegistered("get.help@hotmail.com");
            SimulateUserRegistered("hp@socialtalents.com");

            Output.Message("Press Enter to exit");
            System.Console.ReadLine();
        }

        /// <summary>
        /// We are simluating user registration, which triggers sending welcome email to user
        /// </summary>
        /// <param name="email"></param>
        private static void SimulateUserRegistered(string email)
        {
            User u = new User() { Email = email };
            Output.Message($"Registering user {email}", "UserInput");
            userService.RegisterUser(u);
        }
        
        private static void SetupEventsInProc()
        {
            // Control returned to code only when handler execution completed
            EventBus.Subscribe(emailService);
        }

        private static void SetupEventsAsync()
        {
            // Control returned immediately, execution happens in separate thread
            EventBus.Subscribe(emailService.Async());
        }

        private static void SetupEventsWithOnFail()
        {
            // if handler fail we can do something else
            emailService.SuccessRate = 40;
            EventBus.Subscribe(
                emailService.AddOnFail<UserRegistered, Exception>(
                    (userRegistered, ex) => Output.Error("OnFail handler triggered to notify user " + userRegistered.User.Email, "OnFailHandler")
                )
                // also, let's make it async
                .Async()
            );
        }

        private static void SetupEventsWithRetryAndAsync()
        {
            // if handler fail we can do something else
            emailService.SuccessRate = 25;
            
           EventBus.Subscribe(
                // retry attempt to send email 3 times
                emailService.Retry<UserRegistered, Exception>(3)
                // also, let's make it async
                .Async()
            );
        }

        private static void SetupEventsWithQueue()
        {
            // if handler fail we can do something else
            emailService.SuccessRate = 25;

            // Save incoming messages to queue
            EventBus.Subscribe(queueService.Enque<UserRegistered>());

            EventBus.Subscribe(
                 // We expectet UserRegistered events from EventQueue
                 emailService.AsQueued()
                 // Once received, retry event 6 times with exponential delays
                 .RetryQueued(6, Backoff.ExponentialBackoff(TimeSpan.FromSeconds(10)))
                 .WhenRetryQueueFailed((item, e) => Output.Error("Queued handler failed answer 6 attempts"))
            );

            // In this sample we are using timer to invoke queue
            // For web apps, it is more reliable to trigger execution via url
            Timer t = new Timer(100);
            t.Elapsed += timer_Elapsed;
            t.Start();
        }

        private static void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            lock (queueService)
            {
                queueService.ProcessEvents();
            }
        }

        private static void SimpulateIoc()
        {
            emailService = new EmailService();
            userService = new UserService();
            queueRepository = new InMemoryQueueRepository();
            queueService = new EventQueueWithDebugLogs(queueRepository);

            EventBus.Subscribe<Exception>((e) => Output.Error("Exception in event handling:" + e.Message, "OnError"));
        }

        static EmailService emailService;
        static UserService userService;
        static EventQueueService queueService;
        static InMemoryQueueRepository queueRepository;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialTalents.Hp.Events.Queue
{
    public static class Backoff
    {
        public static Action<IQueueItem> None()
        {
            return (a) => { };
        }

        public static Action<IQueueItem> Fixed(TimeSpan intervall)
        {
            return (queueItem) => queueItem.HandleAfter = DateTime.Now.Add(intervall);
        }

        static Random ExponentialBackoffRandomGenerator = new Random();

        public static Action<IQueueItem> ExponentialBackoff(TimeSpan intervall, int maximum = 15)
        {
            if (maximum <= 0 || maximum > 31)
            {
                throw new ArgumentException("maximum should be in range [1,31]");
            }
            return (queueItem) =>
            {
                // https://en.wikipedia.org/wiki/Exponential_backoff
                int maxC = Math.Min(queueItem.Attempts, maximum);
                int rndMax = 1 << maxC;
                queueItem.HandleAfter = DateTime.Now.Add(new TimeSpan(intervall.Ticks * ExponentialBackoffRandomGenerator.Next(rndMax)));
            };
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialTalents.Hp.Events.Queue
{
    /// <summary>
    /// Typicial backoff strategy to be used with EventQueue
    /// </summary>
    public static class Backoff
    {
        /// <summary>
        /// None, retry event as soon as possible
        /// </summary>
        /// <returns></returns>
        public static Action<IQueueItem> None()
        {
            return (a) => { };
        }

        /// <summary>
        /// Fixed delay
        /// </summary>
        /// <param name="interval"></param>
        /// <returns></returns>
        public static Action<IQueueItem> Fixed(TimeSpan interval)
        {
            return (queueItem) => queueItem.HandleAfter = DateTime.Now.Add(interval);
        }

        static Random ExponentialBackoffRandomGenerator = new Random();

        /// <summary>
        /// Random backoff with exponentially defined upper margin
        /// </summary>
        /// <param name="intervall"></param>
        /// <param name="maximum"></param>
        /// <returns></returns>
        public static Action<IQueueItem> ExponentialBackoff(TimeSpan intervall, int maximum = 15)
        {
            if (maximum <= 0 || maximum > 31)
            {
                throw new ArgumentException("Argument 'maximum' should be in range [1,31]");
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

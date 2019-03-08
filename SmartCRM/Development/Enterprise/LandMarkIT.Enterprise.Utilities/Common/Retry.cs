using LandmarkIT.Enterprise.Utilities.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LandmarkIT.Enterprise.Utilities.Common
{
    public static class Retry
    {
        /*
         * Example Usage 
         * try
            {
                Retry.Do<bool>(() => Action(), TimeSpan.FromSeconds(10));

            }
            catch (Exception ex)
            {
                ExceptionHandler.Current.HandleException(ex, DefaultExceptionPolicies.LOG_ONLY_POLICY, values: source);
            }
         * 
         */
        /// <summary>
        /// Retry's given action with given number of times.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="retryInterval"></param>
        /// <param name="retryCount"></param>
        public static void Do(Action action, TimeSpan retryInterval,int retryCount = 3)
        {
            Do<object>(() =>
            {
                action();
                return null;
            }, retryInterval, retryCount);
        }

        public static T Do<T>(Func<T> action, TimeSpan retryInterval, int retryCount = 3)
        {
            var exceptions = new List<Exception>();

            for (int retry = 0; retry < retryCount; retry++)
            {
                try
                {
                    if (retry > 0)
                    {
#if DEBUG
                        Console.WriteLine("Retry.. " + (retry + 1));
#endif
                        Logger.Current.Informational("Retry.. " + (retry + 1));
                    }
                    return action();
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                    Thread.Sleep(retryInterval);
                }
            }

            throw new AggregateException(exceptions);
        }
    }
}

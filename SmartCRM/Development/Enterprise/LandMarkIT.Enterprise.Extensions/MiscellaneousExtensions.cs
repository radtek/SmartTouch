using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TidyManaged;

namespace LandmarkIT.Enterprise.Extensions
{
    public static class MiscellaneousExtensions
    {
        public static T Execute<T>(Func<T> func, int timeout)
        {
            T result;
            TryExecute(func, timeout, out result);
            return result;
        }

        public static bool TryExecute<T>(Func<T> func, int timeout, out T result)
        {
            var t = default(T);
            var thread = new Thread(() => t = func());
            thread.Start();
            var completed = thread.Join(timeout);
            if (!completed) thread.Abort();
            result = t;
            return completed;
        }

        /// <summary>
        /// Provides proper intents and beautifies HTML
        /// </summary>
        /// <param name="htmlContent"></param>
        /// <returns></returns>
        public static string FormatHTML(this string htmlContent)
        {
            using (var doc = Document.FromString(htmlContent))
            {
                try
                {
                    doc.OutputBodyOnly = AutoBool.Yes;
                    doc.Quiet = true;
                    doc.IndentBlockElements = AutoBool.Auto;
                    doc.IndentCdata = true;
                    doc.CleanAndRepair();
                    var content = doc.Save();
                    return content;
                }
                catch
                {
                    return htmlContent;
                }
            }
        }
        public static string NullSafeToLower(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }
            return value.ToLower();
        }
        //public static T Execute<T>(Action<T> action, int timeout)
        //{
        //    T result;
        //    TryExecute(action, timeout, out result);
        //    return result;
        //}

        //public static bool TryExecute<T>(Action<T> action, int timeout, out T result)
        //{
        //    var t = default(T);
        //    var thread = new Thread(() => t = action());
        //    thread.Start();
        //    var completed = thread.Join(timeout);
        //    if (!completed) thread.Abort();
        //    result = t;
        //    return completed;
        //}
    }
}

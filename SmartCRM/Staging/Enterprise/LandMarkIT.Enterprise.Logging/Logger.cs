using Microsoft.Practices.EnterpriseLibrary.SemanticLogging;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging.Sinks;
using System;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace LandmarkIT.Enterprise.Logging
{
    public class Logger
    {
        private static Logger _instance = new Logger();

        public static Logger Current
        {
            get { if (_instance == null)_instance = new Logger(); return _instance; }
        }

        #region Listeners
        public void CreateConsoleListener(EventLevel eventLevel)
        {
            var listener = ConsoleLog.CreateListener();
            listener.EnableEvents(LogEventSource.Log, eventLevel, Keywords.All);
        }
        public void CreateFlatFileListener(EventLevel eventLevel, string fileName)
        {
            var listener = FlatFileLog.CreateListener(fileName);
            listener.EnableEvents(LogEventSource.Log, eventLevel, Keywords.All);
        }
        public void CreateRollingFlatFileListener(EventLevel eventLevel, string fileName, int rollSizeKB)
        {
            var listener = RollingFlatFileLog.CreateListener(fileName, rollSizeKB, "yyyy-MM-dd hh-mm-ss", RollFileExistsBehavior.Increment, RollInterval.Midnight);
            listener.EnableEvents(LogEventSource.Log, eventLevel, Keywords.All);
        }
        public void CreateSqlDatabaseLogListener(EventLevel eventLevel, string instanceName, string connectionString)
        {
            var listener = SqlDatabaseLog.CreateListener(instanceName, connectionString);
            listener.EnableEvents(LogEventSource.Log, eventLevel, Keywords.All);
        }
        public void CreateEventViewerListener(EventLevel eventLevel)
        {
            //TODO:: implement this
        }
        #endregion

        #region Private methods
        private string Format(string message,
            Exception exception,
            string methodName, string sourceFile, int lineNumber,
            TimeSpan timeSpan, params object[] values)
        {
            var stringBuilder = new StringBuilder();
            try
            {
                stringBuilder.AppendLine(string.Format("message: {0}", message));
                stringBuilder.AppendLine(string.Format("total execution time: {0}-{1}-{2}-{3}-{4}", timeSpan.Days, timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds));
                stringBuilder.AppendLine(string.Format("exception message: {0}", exception == null ? string.Empty : exception.Message));
                stringBuilder.AppendLine(string.Format("exception: {0}", exception == null ? string.Empty : exception.ToString()));
                stringBuilder.AppendLine(string.Format("method name: {0}", methodName));
                stringBuilder.AppendLine(string.Format("source file: {0}", sourceFile));
                stringBuilder.AppendLine(string.Format("line number: {0}", lineNumber));
                stringBuilder.AppendLine(string.Format("CorrelationId: {0}", Trace.CorrelationManager.ActivityId));
                stringBuilder.AppendLine(string.Format("ManagedThreadId: {0}", Thread.CurrentThread.ManagedThreadId));

                foreach (object arg in values)
                {
                    if (values.GetType().Name == typeof(String).Name)
                    {
                        stringBuilder.AppendLine((string)arg);
                    }
                    else if (values.GetType().Name == typeof(DateTime).Name)
                    {
                        var dateArg = (DateTime)arg;
                        stringBuilder.AppendLine(dateArg.ToString());
                    }
                    else if (arg == null)
                    {
                        stringBuilder.AppendLine(string.Empty);
                    }
                    else
                    {
                        stringBuilder.AppendLine(arg.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                stringBuilder.AppendLine(String.Format("Error while formatting logging message:{0}", ex.Message));
            }
            return stringBuilder.ToString();
        }
        #endregion

        #region Log Events
        public void Critical(string message,
            Exception exception = default(Exception),
            [CallerMemberName] string methodName = null, [CallerFilePath] string sourceFile = null, [CallerLineNumber] int lineNumber = 0,
            TimeSpan timeSpan = default(TimeSpan), params object[] values)
        {
            if (LogEventSource.Log.IsLogEnabled)
            {
                LogEventSource.Log.Critical(message, Format(message, exception, methodName, sourceFile, lineNumber, timeSpan, values));
            }
        }

        public void Error(string message,
            Exception exception = default(Exception),
            [CallerMemberName] string methodName = null, [CallerFilePath] string sourceFile = null, [CallerLineNumber] int lineNumber = 0,
            TimeSpan timeSpan = default(TimeSpan), params object[] values)
        {
            if (LogEventSource.Log.IsLogEnabled)
            {
                LogEventSource.Log.Error(message, Format(message, exception, methodName, sourceFile, lineNumber, timeSpan, values));
            }
        }

        public void Warning(string message,
            [CallerMemberName] string methodName = null, [CallerFilePath] string sourceFile = null, [CallerLineNumber] int lineNumber = 0,
            TimeSpan timeSpan = default(TimeSpan), params object[] values)
        {
            if (LogEventSource.Log.IsLogEnabled)
            {
                LogEventSource.Log.Warning(message, Format(message, default(Exception), methodName, sourceFile, lineNumber, timeSpan, values));
            }
        }
        public void Informational(string message,
            [CallerMemberName] string methodName = null, [CallerFilePath] string sourceFile = null, [CallerLineNumber] int lineNumber = 0,
              TimeSpan timeSpan = default(TimeSpan), params object[] values)
        {
            if (LogEventSource.Log.IsLogEnabled)
            {
                LogEventSource.Log.Informational(message, Format(message, default(Exception), methodName, sourceFile, lineNumber, timeSpan, values));
            }
        }
        public void Verbose(string message,
            [CallerMemberName] string methodName = null, [CallerFilePath] string sourceFile = null, [CallerLineNumber] int lineNumber = 0,
              TimeSpan timeSpan = default(TimeSpan), params object[] values)
        {
            if (LogEventSource.Log.IsLogEnabled)
            {
                LogEventSource.Log.Verbose(message, Format(message, default(Exception), methodName, sourceFile, lineNumber, timeSpan, values));
            }
        }
        #endregion
    }
}

using Microsoft.ApplicationInsights;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging.Sinks;
using System;
using System.Collections;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace LandmarkIT.Enterprise.Utilities.Logging
{
    public class Logger
    {
        private static Logger _instance = null;

        private static readonly object padlock = new object();
        //private TelemetryClient telemetry  = null;
        Logger()
        {
            //telemetry = new TelemetryClient();
        }

        public static Logger Current
        {
            get
            {
                if (_instance == null)
                {
                    lock (padlock)
                    {
                        if (_instance == null)
                            _instance = new Logger();
                    }
                }
                return _instance;
            }
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
            var basefilepath = fileName;
            fileName = Path.Combine(Path.GetDirectoryName(fileName), Path.GetFileNameWithoutExtension(fileName) + "." + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-ff") + Path.GetExtension(fileName));
            var listener = new ObservableEventListener();
            listener.EnableEvents(LogEventSource.Log, eventLevel, Keywords.All);
            listener.LogToRollingFlatFile(fileName, rollSizeKB, "yyyy-MM-dd HH-mm-ss", RollFileExistsBehavior.Increment, RollInterval.Midnight);
            var purger = new RollingFlatFilePurger(Path.GetDirectoryName(basefilepath), Path.GetFileName(basefilepath), 1000);
            purger.Purge();
            try
            {
                ((ObservableEventListener)listener).LogToEmail(Path.GetFileNameWithoutExtension(basefilepath));
            }
            catch
            {
            }
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
        private string Format(string message, Exception exception, string methodName, string sourceFile, int lineNumber, TimeSpan timeSpan, params object[] values)
        {
            var stringBuilder = new StringBuilder();
            try
            {
                stringBuilder.Append(string.Format("CorrelationId: {0} ||", Trace.CorrelationManager.ActivityId));
                stringBuilder.Append(string.Format("ManagedThreadId: {0} ||", Thread.CurrentThread.ManagedThreadId));
                if (exception != null)
                {
                    stringBuilder.AppendLine(string.Format("exception message: {0} ||", exception.ToString()));
                    foreach (DictionaryEntry keyvaluepair in exception.Data)
                    {
                        stringBuilder.Append(keyvaluepair.Key + ":" + keyvaluepair.Value + " ||");
                    }
                }
                stringBuilder.AppendLine();
                foreach (object arg in values)
                {
                    if (values.GetType().Name == typeof(String).Name)
                    {
                        stringBuilder.Append((string)arg + "||");
                    }
                    else if (values.GetType().Name == typeof(DateTime).Name)
                    {
                        var dateArg = (DateTime)arg;
                        stringBuilder.Append(dateArg.ToString() + "||");
                    }
                    else if (arg == null)
                    {
                        stringBuilder.Append(string.Empty);
                    }
                    else
                    {
                        stringBuilder.Append(arg.ToString() + "||");
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
        public void Critical(string message, Exception exception = default(Exception), [CallerMemberName] string methodName = null, [CallerFilePath] string sourceFile = null, [CallerLineNumber] int lineNumber = 0, TimeSpan timeSpan = default(TimeSpan), params object[] values)
        {
            if (LogEventSource.Log.IsLogEnabled)
            {
                //telemetry.TrackException(exception);
                LogEventSource.Log.Critical(message, Format(message, exception, methodName, sourceFile, lineNumber, timeSpan, values));
            }
        }

        public void Error(string message, Exception exception = default(Exception), [CallerMemberName] string methodName = null, [CallerFilePath] string sourceFile = null, [CallerLineNumber] int lineNumber = 0, TimeSpan timeSpan = default(TimeSpan), params object[] values)
        {
            if (LogEventSource.Log.IsLogEnabled)
            {
                //telemetry.TrackException(exception);
                LogEventSource.Log.Error(message, Format(message, exception, methodName, sourceFile, lineNumber, timeSpan, values));
            }
        }

        public void Warning(string message, [CallerMemberName] string methodName = null, [CallerFilePath] string sourceFile = null, [CallerLineNumber] int lineNumber = 0, TimeSpan timeSpan = default(TimeSpan), params object[] values)
        {
            if (LogEventSource.Log.IsLogEnabled)
            {
                LogEventSource.Log.Warning(message, Format(message, default(Exception), methodName, sourceFile, lineNumber, timeSpan, values));
            }
        }

        public void Informational(string message, [CallerMemberName] string methodName = null, [CallerFilePath] string sourceFile = null, [CallerLineNumber] int lineNumber = 0, TimeSpan timeSpan = default(TimeSpan), params object[] values)
        {
            if (LogEventSource.Log.IsLogEnabled)
            {
                LogEventSource.Log.Informational(message, Format(message, default(Exception), methodName, sourceFile, lineNumber, timeSpan, values));
            }
        }

        public void Verbose(string message, [CallerMemberName] string methodName = null, [CallerFilePath] string sourceFile = null, [CallerLineNumber] int lineNumber = 0, TimeSpan timeSpan = default(TimeSpan), params object[] values)
        {
            if (LogEventSource.Log.IsLogEnabled)
            {
                LogEventSource.Log.Verbose(message, Format(message, default(Exception), methodName, sourceFile, lineNumber, timeSpan, values));
            }
        }
        #endregion
    }
}

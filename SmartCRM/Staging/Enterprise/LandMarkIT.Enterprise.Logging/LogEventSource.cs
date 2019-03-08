using System;
using System.Diagnostics.Tracing;

namespace LandmarkIT.Enterprise.Logging
{
    //[EventSource(Name="landmarkit")]
    internal class LogEventSource : EventSource
    {
        private static readonly Lazy<LogEventSource> Instance = new Lazy<LogEventSource>(() => new LogEventSource());

        public static LogEventSource Log
        {
            get { return Instance.Value; }
        }

        public bool IsLogEnabled
        {
            get { return Log.IsEnabled(); }
        }

        [Event(100, Level = EventLevel.Critical, Message = "{0}")]
        public void Critical(string message, string details)
        {
            if (this.IsEnabled())
            {
                WriteEvent(100, message, details);
            }
        }
        [Event(200, Level = EventLevel.Error, Message = "{0}")]
        public void Error(string message, string details)
        {
            if (this.IsEnabled())
            {
                WriteEvent(200, message, details);
            }
        }
        [Event(300, Level = EventLevel.Warning, Message = "{0}")]
        public void Warning(string message, string details)
        {
            if (this.IsEnabled())
            {
                WriteEvent(300, message, details);
            }
        }
        [Event(400, Level = EventLevel.Informational, Message = "{0}")]
        public void Informational(string message, string details)
        {
            if (this.IsEnabled())
            {
                WriteEvent(400, message, details);
            }
        }
        [Event(500, Level = EventLevel.Verbose, Message = "{0}")]
        public void Verbose(string message, string details)
        {
            if (this.IsEnabled())
            {
                WriteEvent(500, message, details);
            }
        }
    }
}

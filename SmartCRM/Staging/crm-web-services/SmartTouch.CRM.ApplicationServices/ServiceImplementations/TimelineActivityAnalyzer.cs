using LandmarkIT.Enterprise.Extensions;
using SmartTouch.CRM.Domain.Contacts;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace SmartTouch.CRM.ApplicationServices.ServiceImplementations
{
    public class TimelineActivityAnalyzer
    {
        IEnumerable<TimeLineContact> timelineActivities;
        string DateFormat;
        public TimelineActivityAnalyzer(IEnumerable<TimeLineContact> activityLogs, string dateFormat)
        {
            this.timelineActivities = activityLogs;
            this.DateFormat = dateFormat;
        }

        public IEnumerable<TimeLineContact> GenerateAnalysis()
        {
            var activity = new TimelineEntryActivity(timelineActivities, DateFormat);
            return activity.Analyze().OrderByDescending(l => l.AuditDate);
        }
    }


    public abstract class TimelineActivity
    {
        protected IEnumerable<TimeLineContact> Logs { get; set; }
        public string Message { get; set; }
        public DateTime ActivityDate { get; set; }
        public abstract IEnumerable<TimeLineContact> Analyze();
    }

    public class TimelineEntryActivity : TimelineActivity
    {
        List<TimeLineContact> analysis = new List<TimeLineContact>();
        string dateFormat;
        public TimelineEntryActivity(IEnumerable<TimeLineContact> logs, string dateFormat)
        {
            this.Logs = logs.ToList();
            this.dateFormat = dateFormat;
        }

        public override IEnumerable<TimeLineContact> Analyze()
        {
            analyzeByActivity(this.Logs);
            return analysis;
        }

        private void analyzeByActivity(IEnumerable<TimeLineContact> timelinecontent)
        {
            DateTime now = DateTime.Now.ToUniversalTime();
            now = now.ToJSDate();

            var lastMinuteTimelines = timelinecontent.Where(l => (now.Date - l.AuditDate.Date).Days == 0 && (now - l.AuditDate).Hours == 0 && (now - l.AuditDate).Minutes < 1);
            foreach (var timeline in lastMinuteTimelines)
            {
                timeline.TimeLineTime = "[|Now|]";
                analysis.Add(timeline);
            }

            var firstHourLogs = timelinecontent.Where(l => (now.Date - l.AuditDate.Date).Days == 0 && (now - l.AuditDate).Hours == 0 && (now - l.AuditDate).Minutes >= 1 && (now - l.AuditDate).Minutes < 60);
            foreach (var timeline in firstHourLogs)
            {
                string message = "";
                if ((now - timeline.AuditDate).Minutes == 1)
                {
                    message = (now - timeline.AuditDate).Minutes + " [|minute ago|]";
                }
                else
                {
                    message = (now - timeline.AuditDate).Minutes + " [|minutes ago|]";
                }
                timeline.TimeLineTime = message;
                analysis.Add(timeline);
            }

            var halfdayLogs = timelinecontent.Where(l => (now.Date - l.AuditDate.Date).Days == 0 && ((now - l.AuditDate).Hours >= 1 && (now - l.AuditDate).Hours <= 12));
            foreach (var timeline in halfdayLogs)
            {
                string message = "";
                if ((now - timeline.AuditDate).Hours == 1)
                {
                    message = (now - timeline.AuditDate).Hours + " [|hour ago|]";
                }
                else
                {
                    message = (now - timeline.AuditDate).Hours + " [|hours ago|]";
                }
                timeline.TimeLineTime = message;
                analysis.Add(timeline);
            }

            var todaylogs = timelinecontent.Where(l => (now.Date - l.AuditDate.Date).Days == 0 && ((now - l.AuditDate).Hours > 12 && (now - l.AuditDate).Hours <= 24));
            foreach (var timeline in todaylogs)
            {
                timeline.TimeLineTime = " [|Today at|] " + timeline.AuditDate.ToString(@"HH\:mm");
                analysis.Add(timeline);
            }

            var yesterdaylogs = timelinecontent.Where(l => (now.Date - l.AuditDate.Date).Days == 1);
            foreach (var timeline in yesterdaylogs)
            {
                timeline.TimeLineTime = " [|Yesterday at|] " + timeline.AuditDate.ToString(@"HH\:mm");
                analysis.Add(timeline);
            }

            var weeklogs = timelinecontent.Where(l => (now.Date - l.AuditDate.Date).Days >= 2 && (now.Date - l.AuditDate.Date).Days < 7);
            foreach (var timeline in weeklogs)
            {
                timeline.TimeLineTime = " [|on|] " + timeline.AuditDate.DayOfWeek.ToString() + " [|at|] " + timeline.AuditDate.ToString(@"HH\:mm");
                analysis.Add(timeline);
            }

            var remaininglogs = timelinecontent.Where(l => (now.Date - l.AuditDate.Date).Days >= 7);
            foreach (var timeline in remaininglogs)
            {
                timeline.TimeLineTime = " [|on|] " + timeline.AuditDate.ToString(dateFormat + @" HH\:mm", CultureInfo.InvariantCulture);
                analysis.Add(timeline);
            }
        }
    }
}

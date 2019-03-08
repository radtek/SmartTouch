using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.Users;
using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace SmartTouch.CRM.ApplicationServices.ServiceImplementations
{
    public enum ActivityType
    {
        Create = 1,
        Read = 2,
        Update = 3,
        Delete = 4,
        ChangeOwner = 5,
        LastRunOn = 6
    }
    public class UserActivityAnalyzer
    {
        IEnumerable<UserActivityLog> activityLogs;
        public UserActivityAnalyzer(IEnumerable<UserActivityLog> activityLogs)
        {
            this.activityLogs = activityLogs;
        }

        public IEnumerable<UserActivityViewModel> GenerateAnalysis(string dateFormat)
        {
            var activity = new DailyActivity(activityLogs);
            return activity.Analyze(dateFormat).OrderByDescending(l => l.LogDate);
        }
    }

    public abstract class Activity
    {
        protected IEnumerable<UserActivityLog> Logs { get; set; }
        public string Message { get; set; }
        public DateTime ActivityDate { get; set; }

        public abstract IEnumerable<UserActivityViewModel> Analyze(string dateFormat);
    }

    public class DailyActivity : Activity
    {
        List<UserActivityViewModel> analysis;
        IDictionary<AppModules, string> modules = new Dictionary<AppModules, string>();
        IDictionary<int, string> activities = new Dictionary<int, string>();
        CultureInfo culture = CultureInfo.CreateSpecificCulture("en-US");
        DateTimeFormatInfo dtfi;
        public DailyActivity(IEnumerable<UserActivityLog> logs)
        {
            this.Logs = logs.ToList();
            analysis = new List<UserActivityViewModel>();
            modules.Add(AppModules.Accounts, "[|Account|]");
            modules.Add(AppModules.Users, "[|User|]");
            modules.Add(AppModules.Contacts, "[|Contact|]");
            modules.Add(AppModules.Campaigns, "[|Campaign|]");
            modules.Add(AppModules.ContactActions, "[|Action|]");
            modules.Add(AppModules.ContactNotes, "[|Note|]");
            modules.Add(AppModules.ContactTours, "[|Tour|]");
            modules.Add(AppModules.ContactRelationships, "[|Relationship|]");
            modules.Add(AppModules.Tags, "[|Tag|]");
            modules.Add(AppModules.Forms, "[|Form|]");
            modules.Add(AppModules.Opportunity, "[|Opportunity|]");
            modules.Add(AppModules.OpportunityActions, "[|Action|]");
            modules.Add(AppModules.OpportunityNotes, "[|Note|]");
            modules.Add(AppModules.AdvancedSearch, "[|Saved Search|]");
            modules.Add(AppModules.Automation, "[|Workflow|]");
            modules.Add(AppModules.Reports, "[|Report|]");

            activities.Add(1, "[|Added|]");
            activities.Add(2, "[|Viewed|]");
            activities.Add(3, "[|Updated|]");
            activities.Add(4, "[|Deleted|]");
            activities.Add(6, "[|last run on|]");
            dtfi = culture.DateTimeFormat;
        }

        public override IEnumerable<UserActivityViewModel> Analyze(string dateFormat)
        {
            var moduleGroupLogs = this.Logs.GroupBy(l => l.ModuleID).Select(g => new { ModuleID = g.Key, Group = g });
            foreach (var group in moduleGroupLogs)
            {
                analyzeByModules(group.Group, group.ModuleID, dateFormat);
            }

            return analysis;
        }

        void analyzeByModules(IEnumerable<UserActivityLog> logs, int moduleId, string dateFormat)
        {
            DateTime now = DateTime.Now.ToUniversalTime();

            var dayOneLog = logs.Where(l => l.ModuleID == moduleId &&
                (now.Date - l.LogDate.Date).Days == 0);
            foreach (var activity in activities)
            {
                analyzeByActivity(dayOneLog, activity.Key, moduleId, dateFormat, true);
            }

            var onADayLogs = logs.Where(l => l.ModuleID == moduleId && (now.Date - l.LogDate.Date).Days > 0)
                .GroupBy(l => l.LogDate.Date).Select(g => new { Date = g.Key, Group = g }).ToList();
            foreach (var day in onADayLogs)
            {
                foreach (var activity in activities)
                {
                    analyzeByActivity(day.Group, activity.Key, moduleId, dateFormat, false);
                }
            }
        }

        void analyzeByActivity(IEnumerable<UserActivityLog> logs, int activity, int moduleId, string dateFormat, bool dayOneLogs)
        {
            AppModules module = (AppModules)moduleId;
            var total = dayOneLogs == true ? logs.Where(l => l.UserActivityID == activity).Count() : logs.Where(l => l.UserActivityID == activity).GroupBy(l => l.EntityID).Count();


            //var total = logs.Where(l => l.UserActivityID == activity).GroupBy(l => l.EntityID).Select(g => g.First()).Count();
            //var entityDetails = logs.Where(l => l.UserActivityID == activity && l.ModuleID == moduleId).Select(l => l.EntityDetail).ToList();
            if (total == 0)
                return;

            string totalInText = string.Empty;
            int[] anEntities = new int[] { 1, 5, 16 };

            if (total == 1)
                totalInText = anEntities.Contains(moduleId) ? "[|an|]" : "[|a|]";
            else
                totalInText = total.ToString();

            //var date = logs.Where(l => l.UserActivityID == activity).First().LogDate;
            //string message = "";
            List<UserActivityViewModel> userActivityModels = new List<UserActivityViewModel>();

            if (dayOneLogs)
            {
                totalInText = anEntities.Contains(moduleId) ? "[|an|]" : "[|a|]";
                var activityLogs = logs.Where(l => l.UserActivityID == activity);
                foreach (var log in activityLogs)
                {
                    UserActivityViewModel userActivityModel = new UserActivityViewModel();
                    userActivityModel.DateFormat = dateFormat;
                    userActivityModel.EntityIds = new List<int>() { log.EntityID };
                    userActivityModel.Message =
                        ((AppModules)moduleId == AppModules.Reports || (AppModules)moduleId == AppModules.AdvancedSearch && activity == (byte)UserActivityType.LastRunOn) ?
                        "[|Ran|] " + totalInText + " " + ((AppModules)moduleId == AppModules.Reports ? " [|Report|]" : " [|Saved Search|]") + " - " + log.EntityName :
                            (activities[activity] + " " + totalInText + " " + modules[module] + " - " + log.EntityName);
                    userActivityModel.LogDate = log.LogDate;
                    userActivityModel.ModuleID = (byte)moduleId;
                    if (dateFormat.Contains('/'))
                        userActivityModel.DateSeperator = dtfi;
                    userActivityModels.Add(userActivityModel);
                }
                analysis.AddRange(userActivityModels);
            }
            else
            {
                var date = logs.Where(l => l.UserActivityID == activity).First().LogDate;
                string message = ((AppModules)moduleId == AppModules.Reports || (AppModules)moduleId == AppModules.AdvancedSearch && activity == (byte)UserActivityType.LastRunOn ?
                    "[|Ran|]" : activities[activity]) + " " + totalInText + " " + (total > 1 ? modules[module] + "[|s|]" : modules[module]) + " "
                               + ("- " + logs.Where(l => l.UserActivityID == activity).GroupBy(l => l.EntityID).Select(g => g.FirstOrDefault().EntityName).Aggregate((a, b) => a + ", " + b));
                UserActivityViewModel model = new UserActivityViewModel();
                model.DateFormat = dateFormat;
                model.Message = message;
                model.EntityIds = logs.Where(l => l.UserActivityID == activity).Select(s => s.EntityID).Distinct().ToList();
                // model.EntityDetails = entityDetails;
                model.LogDate = date;
                // model.EntityId = 
                model.ModuleID = (byte)moduleId;
                if (dateFormat.Contains('/'))
                    model.DateSeperator = dtfi;
                analysis.Add(model);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.ValueObjects
{
    public class NotificationsCount
    {
        public IEnumerable<int> NotificationsByDate { get; set; }
        public IEnumerable<int> AccountNotifications { get; set; }
        public IEnumerable<int> ActionNotifications { get; set; }
        public IEnumerable<int> TourNotifications { get; set; }
        public IEnumerable<int> ContactNotifications { get; set; }
        public IEnumerable<int> OpportunityNotifications { get; set; }
        public IEnumerable<int> LeadAdapterNotifications { get; set; }
        public IEnumerable<int> ImportNotifications { get; set; }
        public IEnumerable<int> CampaignNotifications { get; set; }
        public IEnumerable<int> DownloadNotifications { get; set; }
        public IEnumerable<int> CampaignLitmusNotifications { get; set; }
        public IEnumerable<int> MailTesterNotifications { get; set; }
    }

    public class NotificationResult
    {
        public int ModuleID { get; set; }
        public int NotificationCount { get; set; }
        public bool IsToday { get; set; }
    }
}

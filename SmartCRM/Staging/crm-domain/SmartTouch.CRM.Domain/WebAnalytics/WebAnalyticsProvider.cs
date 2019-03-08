using SmartTouch.CRM.Domain.Accounts;
using SmartTouch.CRM.Domain.Users;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.WebAnalytics
{
    public class WebAnalyticsProvider : EntityBase<short>, IAggregateRoot
    {
        public int AccountID { get; set; }
        public WebAnalyticsStatus StatusID { get; set; }
        public string APIKey { get; set; }
        public string TrackingDomain { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public int LastUpdatedBy { get; set; }
        public DateTime LastUpdatedOn { get; set; }
        public bool NotificationStatus { get; set; }
        public bool DailyStatusEmailOpted { get; set; }
        public short NotificationFrequencyStatus { get; set; }
        public IEnumerable<int> InstantNotificationGroup { get; set; }
        public IEnumerable<int> DailySummaryNotificationGroup { get; set; }

        public DateTime? LastAPICallTimeStamp { get; set; }
        public DateTime? ActivatedOn { get; set; }
        public short RequestInterval { get; set; }
        protected override void Validate()
        {
            throw new NotImplementedException();
        }
    }
}

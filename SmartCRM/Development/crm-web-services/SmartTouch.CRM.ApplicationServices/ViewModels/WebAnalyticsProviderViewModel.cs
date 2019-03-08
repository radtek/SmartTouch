using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    public class WebAnalyticsProviderViewModel
    {
        public short WebAnalyticsProviderID { get; set; }
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
        public IEnumerable<int> InstantNotificationGroup { get; set; }
        public IEnumerable<int> DailySummaryNotificationGroup { get; set; }

        public short NotificationFrequencyStatus { get; set; }
        //public IEnumerable<int> NotificationGroup { get; set; }

        public DateTime? LastAPICallTimeStamp { get; set; }
        public DateTime? ActivatedOn { get; set; }
        public short RequestInterval { get; set; }
    }

    public class UVByIpViewModel
    {
        public string date { get; set; }
        public string ipAddress { get; set; }
        public string identity { get; set; }
        public string pageViews { get; set; }
    }
}

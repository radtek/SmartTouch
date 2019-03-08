using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
    public class WebAnalyticsProvidersDb
    {
        [Key]
        public short WebAnalyticsProviderID { get; set; }

        [ForeignKey("Account")]
        public int AccountID { get; set; }
        public AccountsDb Account { get; set; }

        public WebAnalyticsStatus StatusID { get; set; }
        public string APIKey { get; set; }
        public string TrackingDomain { get; set; }

        [ForeignKey("User")]
        public int CreatedBy { get; set; }
        public UsersDb User { get; set; }
        
        public DateTime CreatedOn { get; set; }

        [ForeignKey("User1")]
        public int LastUpdatedBy { get; set; }
        public UsersDb User1 { get; set; }

        public DateTime LastUpdatedOn { get; set; }
        public bool NotificationStatus { get; set; }
        public bool DailyStatusEmailOpted { get; set; }

        public IEnumerable<WebVisitUserNotificationMapDb> NotificationGroup { get; set; }

        public short NotificationFrequencyStatus { get; set; }

        public DateTime? LastAPICallTimeStamp { get; set; }
        public DateTime? ActivatedOn { get; set; }
        public short RequestInterval { get; set; }
    }
}

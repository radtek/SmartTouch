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
    public class WebVisitUserNotificationMapDb
    {
        [Key]
        public int WebVisitUserNotificationMapID { get; set; }
        
        [ForeignKey("WebAnalyticsProvider")]
        public short WebAnalyticsProviderID { get; set; }
        public WebAnalyticsProvidersDb WebAnalyticsProvider { get; set; }       

        [ForeignKey("User")]
        public int UserID { get; set; }
        public UsersDb User { get; set; }

        [ForeignKey("Account")]
        public int AccountID { get; set; }
        public AccountsDb Account { get; set; }

        public WebVisitEmailNotificationType NotificationType { get; set; }
        public DateTime CreatedOn { get; set; }

        [ForeignKey("User1")]
        public int CreatedBy { get; set; }
        public UsersDb User1 { get; set; }

        public DateTime? LastUpdatedOn { get; set; }

        [ForeignKey("User2")]
        public int? LastUpdatedBy { get; set; }
        public UsersDb User2 { get; set; }

    }
}

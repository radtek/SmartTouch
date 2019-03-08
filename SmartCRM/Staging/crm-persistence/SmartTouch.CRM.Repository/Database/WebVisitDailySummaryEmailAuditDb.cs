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
    public class WebVisitDailySummaryEmailAuditDb
    {
        [Key]
        public int WebVisitDailySummaryEmailAuditID { get; set; }

        [ForeignKey("WebAnalyticsProvider")]
        public short WebAnalyticsProviderID { get; set; }
        public WebAnalyticsProvidersDb WebAnalyticsProvider { get; set; }

        [ForeignKey("User")]
        public int UserID { get; set; }
        public UsersDb User { get; set; }

        public WebVisitEmailStatus StatusID { get; set; }
        public DateTime SentOn { get; set; }
        public string JobID { get; set; }
        public string Recipients { get; set; }
        public string Remarks { get; set; }
    }
}

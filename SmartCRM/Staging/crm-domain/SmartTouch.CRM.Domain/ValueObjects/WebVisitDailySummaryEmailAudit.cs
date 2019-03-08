using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.ValueObjects
{
    public class WebVisitDailySummaryEmailAudit
    {
        public int WebVisitDailySummaryEmailAuditID { get; set; }
        public short WebAnalyticsProviderID { get; set; }
        public int UserID { get; set; }
        public WebVisitEmailStatus StatusID { get; set; }
        public DateTime SentOn { get; set; }
        public string JobID { get; set; }
        public string Recipients { get; set; }
        public string Remarks { get; set; }

    }
}

using SmartTouch.CRM.Domain.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.WebAnalytics
{
    public class GetWebVisitDailySummaryRequest : ServiceRequestBase
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class GetWebVisitDailySummaryResponse : ServiceResponseBase
    {
        public IEnumerable<WebVisitReport> WebVisits { get; set; }
    }
}

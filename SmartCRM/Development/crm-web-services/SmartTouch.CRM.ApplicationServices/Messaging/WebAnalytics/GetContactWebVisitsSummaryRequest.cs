using SmartTouch.CRM.ApplicationServices.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.WebAnalytics
{
    public class GetContactWebVisitsSummaryRequest : ServiceRequestBase
    {
        public int ContactId { get; set; }
        public short PageNumber { get; set; }
        public short PageSize { get; set; }
    }

    public class GetContactWebVisitsSummaryResponse : ServiceResponseBase
    {
        public IEnumerable<ContactWebVisitSummaryViewModel> WebVisits { get; set; }
        public int TotalHits { get; set; }
    }
}

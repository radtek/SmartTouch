using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Domain.WebAnalytics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.WebAnalytics
{
    public class AddContactWebVisitRequest : ServiceRequestBase
    {
        public IList<WebVisit> ContactWebVisits { get; set; }
        public DateTime LastAPICallTimeStamp { get; set; }
        public WebAnalyticsProvider WebAnalyticsProvider { get; set; }
        public short SplitVisitInterval { get; set; }
    }

    public class AddContactWebVisitResponse : ServiceResponseBase
    {
        public List<KeyValuePair<int,short>> AccountWebVisitsCount { get; set; }
    }
}

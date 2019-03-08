using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.WebAnalytics
{
    public class GetWebVisitsByVisitDateRequest : ServiceRequestBase    
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }

    public class GetWebVisitsByVisitDateResponse : ServiceResponseBase
    {
        public IEnumerable<WebVisit> WebVisits { get; set; }
        public IEnumerable<Person> Contacts { get; set; }
    }
}

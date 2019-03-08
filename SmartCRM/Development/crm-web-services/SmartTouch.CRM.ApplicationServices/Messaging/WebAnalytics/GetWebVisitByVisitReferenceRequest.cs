using SmartTouch.CRM.ApplicationServices.ViewModels;
using System.Collections.Generic;

namespace SmartTouch.CRM.ApplicationServices.Messaging.WebAnalytics
{
    public class GetWebVisitByVisitIDRequest : ServiceRequestBase
    {
        public int ContactWebVisitID{ get; set; }
    }

    public class GetWebVisitByVisitIDResponse : ServiceResponseBase
    {
        public IEnumerable<WebVisitViewModel> WebVisits { get; set; }
    }
}

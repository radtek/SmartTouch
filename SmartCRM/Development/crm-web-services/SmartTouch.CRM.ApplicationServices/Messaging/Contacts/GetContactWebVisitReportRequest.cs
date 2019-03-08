using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Contacts
{


    public class GetContactWebVisitReportRequest : ServiceRequestBase
    {
        public int ContactId { get; set; }
        public string Period { get; set; }
        public string VisitReference { get; set; }
    }

    public class GetContactWebVisitReportResponse : ServiceResponseBase
    {
        public IEnumerable<WebVisitViewModel> WebVisits { get; set; }
    }
}

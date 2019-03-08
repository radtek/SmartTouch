using SmartTouch.CRM.ApplicationServices.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Contacts
{
    public class GetContactSummaryRequest : ServiceRequestBase
    {
        public int ContactId { get; set; }
    }

    public class GetContactSummaryResponse : ServiceResponseBase
    {
        public string ContactSummary { get; set; }
        public IEnumerable<ContactSummaryViewModel> ContactSummaryDetails { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Contacts
{
    public class GetUntouchedContactsRequest:ServiceRequestBase
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class GetUntouchedContactsResponse : ServiceResponseBase
    {
       public IEnumerable<int> ContactIds { get; set; }
    }
}

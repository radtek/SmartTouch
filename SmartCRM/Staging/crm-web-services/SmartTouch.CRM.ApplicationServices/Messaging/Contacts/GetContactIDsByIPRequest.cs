using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Contacts
{
    public class GetContactIDsByIPRequest : ServiceRequestBase
    {
        public string IPAddress { get; set; }
    }

    public class GetContactIDsByIPResponse : ServiceResponseBase
    {
        public IEnumerable<int> ContactIDs { get; set; }
    }
}

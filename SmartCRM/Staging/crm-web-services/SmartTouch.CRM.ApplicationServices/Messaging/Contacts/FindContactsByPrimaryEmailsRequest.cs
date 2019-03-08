using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Contacts
{
    public class FindContactsByPrimaryEmailsRequest : ServiceRequestBase
    {
        public IEnumerable<string> Emails { get; set; }
    }

    public class FindContactsByPrimaryEmailsResponse : ServiceResponseBase
    {
        public IEnumerable<int> ContactIDs { get; set; }
    }
}

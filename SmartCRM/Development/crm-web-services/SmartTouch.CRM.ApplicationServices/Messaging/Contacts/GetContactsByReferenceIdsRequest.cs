using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Contacts
{
    public class GetContactsByReferenceIdsRequest : ServiceRequestBase
    {
        public IEnumerable<string> ReferenceIds { get; set; }
    }

    public class GetContactsByReferenceIdsResponse : ServiceResponseBase
    {
        public IEnumerable<int> ContactIDs { get; set; }
    }
}

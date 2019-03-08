using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Contacts
{
    public class ReIndexContactsRequest : ServiceRequestBase
    {
    }

    public class ReIndexContactsResponse : ServiceResponseBase
    {
        public int IndexedContacts { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Contacts
{
    public class DeactivateContactsRequest:ServiceRequestBase
    {
        public int[] ContactIds { get; set; }
    }

    public class DeactivateContactsResponse : ServiceResponseBase
    {
        public int ContactsDeleted { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Contacts
{
    public class GetContactOwnerByContactIDRequest : ServiceRequestBase
    {
        public int ContactID { get; set; }
    }

    public class GetContactOwnerByContactIDResponse : ServiceResponseBase
    {
    }



}

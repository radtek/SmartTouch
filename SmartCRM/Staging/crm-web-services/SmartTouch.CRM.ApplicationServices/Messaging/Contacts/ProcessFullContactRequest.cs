using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Contacts
{
    public class ProcessFullContactRequest : ServiceRequestBase
    {
        public int Limit { get; set; }
    }

    public class ProcessFullContactResponse : ServiceResponseBase
    {

    }
}

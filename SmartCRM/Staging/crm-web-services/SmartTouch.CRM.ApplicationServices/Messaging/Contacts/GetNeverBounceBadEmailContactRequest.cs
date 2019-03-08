using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Contacts
{
    public class GetNeverBounceBadEmailContactRequest:ServiceRequestBase
    {
        public int NeverbounceRequestID { get; set; }
        public byte EmailStatus { get; set; }
    }

    public class GetNeverBounceBadEmailContactResponse : ServiceResponseBase
    {
        public IEnumerable<int> ContactIdList { get; set; }
    }
}

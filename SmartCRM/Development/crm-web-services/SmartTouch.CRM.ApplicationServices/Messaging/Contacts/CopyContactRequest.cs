using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Contacts
{
    public class CopyContactRequest : ServiceRequestBase
    {
        public int contactID { get; set; }
        public int contactType { get; set; }
        public int AccountID { get; set; }
    }
    public class CopyContactResponse : ServiceResponseBase
    { 
    
    }
}

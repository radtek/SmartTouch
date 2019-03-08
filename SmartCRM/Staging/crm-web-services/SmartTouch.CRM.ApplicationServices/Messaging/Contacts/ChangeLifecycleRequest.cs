using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Contacts
{
    public class ChangeLifecycleRequest : ServiceRequestBase
    {
        public int ContactId { get; set; }
        public short dropdownValueId { get; set; }
    }

    public class ChangeLifecycleResponse : ServiceResponseBase
    { 
    
    }
}

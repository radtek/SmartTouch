using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Contacts
{
    public class GetContactEmailIdRequest : ServiceRequestBase
    {
      public string emailID { get; set; }
      public int contactID { get; set; }
    }

    public class GetContactEmailIdResponse : ServiceResponseBase
    {
        public int ContactEmailID { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Contacts
{
    public class GetContactWebVisitsCountRequest : ServiceRequestBase
    {
        public int ContactId { get; set; }
        public DateTime Period { get; set; }
    }

    public class GetContactWebVisitsCountResponse : ServiceResponseBase
    {
        public int WebVisitsCount { get; set; }
    }

}

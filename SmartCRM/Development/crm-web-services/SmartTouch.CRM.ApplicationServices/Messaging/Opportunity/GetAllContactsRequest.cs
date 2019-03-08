using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Opportunity
{
    public class GetAllContactsRequest : ServiceRequestBase
    {
        public int AccountID { get; set; }
    }

    public class GetAllContactsResponse : ServiceResponseBase
    {
        public IEnumerable<dynamic> Contacts { get; set; }
    }
}

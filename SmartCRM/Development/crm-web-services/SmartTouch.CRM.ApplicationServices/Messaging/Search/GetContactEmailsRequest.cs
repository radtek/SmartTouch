using SmartTouch.CRM.Domain.Contacts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Search
{
    public class GetContactEmailsRequest : ServiceRequestBase
    {
        public int SearchDefinitionID { get; set; }
    }

    public class GetContactEmailsResponse : ServiceResponseBase
    {
        public IEnumerable<Contact> Contacts { get; set; }
    }
}

using SmartTouch.CRM.Domain.Contacts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Contacts
{
    public class GetBulkContactsRequest : ServiceRequestBase
    {
        public BulkOperations BulkOperations { get; set; }
        public List<int> ContactIds { get; set; }
    }
    public class GetBulkContactsResponse : ServiceResponseBase
    {
        public int[] ContactIds { get; set; }
        public IEnumerable<Contact> Contacts { get; set; }
    }
}

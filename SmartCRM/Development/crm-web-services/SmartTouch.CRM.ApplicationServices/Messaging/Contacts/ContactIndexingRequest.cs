using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace SmartTouch.CRM.ApplicationServices.Messaging.Contacts
{
    public class ContactIndexingRequest : ServiceRequestBase
    {
        public IList<int> ContactIds { get; set; }
        public ILookup<int, bool> Ids { get; set; }
    }

    public class ContactIndexingResponce : ServiceResponseBase
    {

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Contacts
{
    public class FindContactsOfUserByFirstAndLastNameRequest : ServiceRequestBase
    {
        public IEnumerable<string> ContactNames { get; set; }
    }

    public class FindContactsOfUserByFirstAndLastNameResponse : ServiceResponseBase
    {
        public IEnumerable<int> ContactIds { get; set; }
    }

}

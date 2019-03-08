using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Contacts
{
    public class UpdateLastTouchedRequest : ServiceRequestBase
    {
        public byte ModuleId { get; set; }
        public IEnumerable<KeyValuePair<int, DateTime>> LastTouchedOn { get; set; }
    }
    public class UpdateLastTouchedResponse : ServiceResponseBase
    {

    }
}

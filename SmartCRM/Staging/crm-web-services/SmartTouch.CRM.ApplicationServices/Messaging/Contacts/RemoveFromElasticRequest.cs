using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Contacts
{
    public class RemoveFromElasticRequest : ServiceRequestBase
    {
        public IEnumerable<int> ContactIds { get; set; }
    }

    public class RemoveFromElasticResponse : ServiceResponseBase
    {
    }
}

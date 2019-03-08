using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Tags
{
    public class TagIndexingRequest : ServiceRequestBase
    {
        public IList<int> TagIds { get; set; }
    }
    public class TagIndexingResponce : ServiceResponseBase
    {

    }
}

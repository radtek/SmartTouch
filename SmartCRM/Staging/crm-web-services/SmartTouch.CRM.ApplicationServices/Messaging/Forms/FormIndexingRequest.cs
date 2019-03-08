using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Forms
{
    public class FormIndexingRequest : ServiceRequestBase
    {
        public IList<int> FormIds { get; set; }
    }
    public class FormIndexingResponce : ServiceResponseBase
    {

    }
}

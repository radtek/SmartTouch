using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Forms
{
    public class GetFormFieldIDsRequest: ServiceRequestBase
    {
        public int FormID { get; set; }
    }

    public class GetFormFieldIDsResponse : ServiceResponseBase
    {
        public IEnumerable<int> FieldIDs { get; set; }
    }
}

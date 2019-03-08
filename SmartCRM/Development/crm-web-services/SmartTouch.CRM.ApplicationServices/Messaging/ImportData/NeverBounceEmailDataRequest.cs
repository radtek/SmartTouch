using SmartTouch.CRM.Domain.ImportData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.ImportData
{
    public class NeverBounceEmailDataRequest : ServiceRequestBase
    {
        public int NeverBounceRequestID { get; set; }
    }

    public class NeverBounceEmailDataResponse : ServiceResponseBase
    {
        public NeverBounceEmailData Data { get; set; }
    }
}

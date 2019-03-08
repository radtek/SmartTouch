using SmartTouch.CRM.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Forms
{
    public class GetFailedFormsResultsRequest : ServiceRequestBase
    {
        public IEnumerable<ApproveLeadsQueue> Queue { get; set; }
    }

    public class GetFailedFormsResultsResponse : ServiceResponseBase
    {
        public byte[] Result { get; set; }
    }
}

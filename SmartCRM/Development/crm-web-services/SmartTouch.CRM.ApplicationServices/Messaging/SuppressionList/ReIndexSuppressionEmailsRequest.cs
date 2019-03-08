using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.SuppressionList
{
    public class ReIndexSuppressionEmailsRequest : ServiceRequestBase
    {
        public int SuppressionEmailsBatchCount { get; set; }
    }

    public class ReIndexSuppressionEmailsResponse : ServiceResponseBase
    {
        public int IndexedEmailsCount { get; set; }
    }
}

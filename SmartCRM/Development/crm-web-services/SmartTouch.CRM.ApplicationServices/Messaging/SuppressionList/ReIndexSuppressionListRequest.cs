using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.SuppressionList
{
    public class ReIndexSuppressionListRequest : ServiceRequestBase
    {
        public byte IndexType { get; set; }
        public int SuppressionListBatchCount { get; set; }
    }

    public class ReIndexSuppressionListResponse : ServiceResponseBase
    {
        public int IndexedListCount { get; set; }
    }
}

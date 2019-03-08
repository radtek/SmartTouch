using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Forms
{
    public class GetApproveLeadsRequest : ServiceRequestBase
    {
        public int PageNumber { get; set; }
        public int Limit { get; set; }
        public FailedFormsDateType DateType { get; set; }
    }

    public class GetApproveLeadsResponse : ServiceResponseBase
    {
        public IEnumerable<ApproveLeadsQueue> Queue { get; set; }
    }
}

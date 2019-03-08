using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.WorkFlow
{
    public class IsEnrolledToRemoveRequest : ServiceRequestBase
    {
        public int WorkflowId { get; set; }
    }

    public class IsEnrolledToRemoveResponse : ServiceResponseBase
    {
        public IEnumerable<short> WorkflowIds { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.WorkFlow
{
    public class RemoveFromOtherWorkflowsRequest : ServiceRequestBase
    {
        public int ContactId { get; set; }
        public string WorkflowIds { get; set; }
        public byte AllowParallelWorkflows { get; set; }
    }

    public class RemoveFromOtherWorkflowsResponse : ServiceResponseBase
    {

    }
}

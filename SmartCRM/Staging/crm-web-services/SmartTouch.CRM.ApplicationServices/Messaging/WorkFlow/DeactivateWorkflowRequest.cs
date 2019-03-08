using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.WorkFlow
{
    public class DeactivateWorkflowRequest : ServiceRequestBase
    {
        public int WorkflowId { get; set; }
    }

    public class DeactivateWorkflowResponse : ServiceResponseBase
    {

    }
}

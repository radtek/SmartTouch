using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.WorkFlow
{
    public class CanContactReenterWorkflowRequest : ServiceRequestBase
    {
        public int WorkflowId { get; set; }
    }

    public class CanContactReenterWorkflowResponse : ServiceResponseBase
    {
        public bool CanReenter { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.WorkFlow
{
    public class DeleteWorkflowRequest:ServiceRequestBase
    {
        public int[] WorkflowIDs { get; set; }
    }

    public class DeleteWorkflowResponse : ServiceResponseBase
    {
       
    }
}

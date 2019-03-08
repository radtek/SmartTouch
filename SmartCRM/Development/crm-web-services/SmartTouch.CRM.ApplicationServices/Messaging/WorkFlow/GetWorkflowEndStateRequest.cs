using SmartTouch.CRM.ApplicationServices.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.WorkFlow
{
    public class GetWorkflowEndStateRequest : ServiceRequestBase
    {
        public int WorkflowId { get; set; }
    }

    public class GetWorkflowEndStateResponse : ServiceResponseBase
    {
        public WorkflowActionViewModel WorkflowActionViewModel { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.ApplicationServices.ViewModels;

namespace SmartTouch.CRM.ApplicationServices.Messaging.WorkFlow
{
    public class InsertWorkflowRequest:ServiceRequestBase
    {
        public WorkFlowViewModel WorkflowViewModel { get; set; }
    }

    public class InsertWorkflowResponse : ServiceResponseBase
    {
        public WorkFlowViewModel WorkflowViewModel { get; set; }
    }
}

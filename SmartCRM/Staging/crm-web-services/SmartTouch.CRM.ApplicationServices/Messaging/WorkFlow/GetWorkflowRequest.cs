using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.ApplicationServices.ViewModels;

namespace SmartTouch.CRM.ApplicationServices.Messaging.WorkFlow
{
    public class GetWorkflowRequest:ServiceRequestBase
    {
        public short WorkflowID { get; set; }
        public bool IsNewWorkflow { get; set; }
        public bool RequestFromAutomationService { get; set; }
        public int AccountID { get; set; }
    }

    public class GetWorkflowResponse : ServiceResponseBase
    {
        public WorkFlowViewModel WorkflowViewModel { get; set; }
        public IEnumerable<ParentWorkflowViewModel> ParentWorkflows { get; set; }

    }
}

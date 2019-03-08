using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.ApplicationServices.ViewModels;

namespace SmartTouch.CRM.ApplicationServices.Messaging.WorkFlow
{
    public class GetWorkflowsRequest:ServiceRequestBase
    {
        public short WorkflowID { get; set; }
    }

    public class GetWorkflowsResponse : ServiceResponseBase
    {
        public IEnumerable<WorkFlowViewModel> Workflows { get; set; }
    }
}

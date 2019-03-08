using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.ApplicationServices.ViewModels;

namespace SmartTouch.CRM.ApplicationServices.Messaging.WorkFlow
{
    public class GetContactWorkflowsRequest : ServiceRequestBase
    {
        public int ContactId { get; set; }
    }

    public class GetContactWorkflowsResponse : ServiceResponseBase
    {
        public IEnumerable<WorkFlowViewModel> WorkflowViewModel { get; set; }
    }
}

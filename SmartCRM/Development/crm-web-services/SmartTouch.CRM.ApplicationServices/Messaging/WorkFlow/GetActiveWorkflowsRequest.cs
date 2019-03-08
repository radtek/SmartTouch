using SmartTouch.CRM.ApplicationServices.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.WorkFlow
{
    public class GetActiveWorkflowsRequest : ServiceRequestBase
    {
        public int Limit { get; set; }
        public int PageNumber { get; set; }
    }

    public class GetActiveWorkflowsResponse : ServiceResponseBase
    {
        public int TotalHits { get; set; }
        public IEnumerable<WorkFlowViewModel> Workflows { get; set; }
    }
}

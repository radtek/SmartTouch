using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Reports
{
    public class GetWorkflowsForCampaignReportRequest : ServiceRequestBase
    {
        public int CampaignID { get; set; }
    }

    public class GetWorkflowsForCampaignReportResponse : ServiceResponseBase
    {
        public IEnumerable<string> WorkflowNames { get; set; }
    }
}

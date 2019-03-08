using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Workflows
{
    public class WorkflowCampaignActionLink
    {
        public int WorkflowCampaignLinkID { get; set; }
        public int LinkID { get; set; }
        public int ParentWorkflowActionID { get; set; }
        public int LinkActionID { get; set; }
        public int Order { get; set; }

        public IEnumerable<WorkflowAction> Actions { get; set; }
    }
}

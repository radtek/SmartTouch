using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Workflows
{
    [AttributeUsage(AttributeTargets.Property)]
    public class KeyAttribute : Attribute
    {
        public string Name { get; set; }
    }
    public class WorkflowCampaignAction : WorkflowAction
    {
        [Key]
        public int WorkflowCampaignActionID { get; set; }
        public int CampaignID { get; set; }
        public string CampaignName { get; set; }
        public IEnumerable<WorkflowCampaignActionLink> Links { get; set; }
        protected override void Validate()
        {
            base.Validate();
        }
    }
}

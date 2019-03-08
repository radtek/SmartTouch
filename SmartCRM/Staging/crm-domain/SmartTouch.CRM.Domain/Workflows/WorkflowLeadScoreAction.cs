using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Workflows
{
    public class WorkflowLeadScoreAction : WorkflowAction
    {
        [Key]
        public int WorkflowLeadScoreActionID { get; set; }
        public int LeadScoreValue { get; set; }

        protected override void Validate()
        {
            base.Validate();
            if (this.LeadScoreValue <= 0)
                AddBrokenRule(WorkflowBusinessRules.LeadScoreValueMustbePositive);          
        }
    }
}

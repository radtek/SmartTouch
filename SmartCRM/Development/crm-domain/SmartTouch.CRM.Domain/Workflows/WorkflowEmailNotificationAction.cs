using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Workflows
{
    public class WorkflowEmailNotificationAction : WorkflowAction
    {
        [Key]
        public int WorkFlowEmailNotificationActionID { get; set; }
        public int FromEmailID { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }

        protected override void Validate()
        {
            base.Validate();
            if (FromEmailID <= 0)
                AddBrokenRule(WorkflowBusinessRules.FromEmailRequired);
            else if (!string.IsNullOrEmpty(Subject))
                AddBrokenRule(WorkflowBusinessRules.SubjectRequired);
            else if (!string.IsNullOrEmpty(Body))
                AddBrokenRule(WorkflowBusinessRules.BodyRequired);           
        }
    }
}

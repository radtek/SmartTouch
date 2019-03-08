using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Workflows
{
    public class WorkflowUpdateFieldAction : BaseWorkflowAction
    {
        public int WorkflowUpdateFieldActionID { get; set; }
        public int FieldID { get; set; }
        public string FieldValue { get; set; }

        protected override void Validate()
        {
            base.Validate();
            if (string.IsNullOrEmpty(this.FieldValue))
                AddBrokenRule(WorkflowBusinessRules.FieldValueRequired);
            if (FieldID == 0)
                AddBrokenRule(WorkflowBusinessRules.FieldRequired);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Workflows
{
    public class WorkflowTagAction : WorkflowAction
    {
        [Key]
        public int WorkflowTagActionID { get; set; }
        public int TagID { get; set; }
        public byte ActionType { get; set; }
        public string TagName { get; set; }

        protected override void Validate()
        {
            base.Validate();

            if (this.TagID == 0)
                AddBrokenRule(WorkflowBusinessRules.TagRequired);            
        }


    }
}

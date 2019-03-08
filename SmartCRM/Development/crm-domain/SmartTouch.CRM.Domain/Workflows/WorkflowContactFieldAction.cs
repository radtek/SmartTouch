using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Workflows
{
    public class WorkflowContactFieldAction : WorkflowAction
    {
        [Key]
        public int WorkflowContactFieldActionID { get; set; }
        public int FieldID { get; set; }
        public string FieldValue { get; set; }
        public string FieldName { get; set; }
        public int DropdownValueID { get; set; }
        public bool IsDropdownField { get; set; }
        public FieldType FieldInputTypeId { get; set; }

        protected override void Validate()
        {
            base.Validate();
        }
    }
}

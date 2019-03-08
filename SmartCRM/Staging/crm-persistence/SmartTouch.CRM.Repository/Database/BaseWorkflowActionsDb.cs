using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Entities;

namespace SmartTouch.CRM.Repository.Database
{
    public abstract class BaseWorkflowActionsDb
    {
        [ForeignKey("WorkflowAction")]
        public int WorkflowActionID { get; set; }
        public WorkflowActionsDb WorkflowAction { get; set; }
      
        [NotMapped]
        public WorkflowActionType WorkflowActionTypeID { get; set; }
        [NotMapped]
        public short WorkflowID { get; set; }

        public abstract void Save(CRMDb db);
        public abstract BaseWorkflowActionsDb Get(int workflowActionId, CRMDb db);
    }
}

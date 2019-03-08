using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;

namespace SmartTouch.CRM.Repository.Database
{
    public class TriggerWorkflowActionsDb : BaseWorkflowActionsDb
    {
        [Key]
        public int TriggerWorkflowActionID { get; set; }

        [ForeignKey("Workflow")]
        public short SiblingWorkflowID { get; set; }
        public virtual WorkflowsDb Workflow { get; set; }

        public override void Save(CRMDb db)
        {
            if (TriggerWorkflowActionID == 0)
            {
                var triggerWorkflowDb = this;
                db.TriggerWorkflowActions.Add(triggerWorkflowDb);
            }
            else
            {
                db.Entry(this).State = EntityState.Modified;
            }
        }

        public override BaseWorkflowActionsDb Get(int workflowActionId, CRMDb db)
        {
            return db.TriggerWorkflowActions.Where(lc => lc.WorkflowActionID == workflowActionId).Include(i => i.Workflow).FirstOrDefault();
        }
    }
}

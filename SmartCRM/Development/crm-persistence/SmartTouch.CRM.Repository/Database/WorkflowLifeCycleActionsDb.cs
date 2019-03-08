using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;

namespace SmartTouch.CRM.Repository.Database
{
    public class WorkflowLifeCycleActionsDb : BaseWorkflowActionsDb
    {
        [Key]
        public int WorkflowLifeCycleActionID { get; set; }

        [ForeignKey("DropdownValues")]
        public short LifecycleDropdownValueID { get; set; }
        public DropdownValueDb DropdownValues { get; set; }

        public override void Save(CRMDb db)
        {
            if (WorkflowLifeCycleActionID == 0)
            {
                if(this.LifecycleDropdownValueID > 0)
                  db.WorkflowLifeCycleActions.Add(this);

            }
            else
            {
                db.Entry(this).State = EntityState.Modified;
            }
            
        }

        public override BaseWorkflowActionsDb Get(int workflowActionId, CRMDb db)
        {
            return db.WorkflowLifeCycleActions.Include(c=>c.DropdownValues).Where(lc => lc.WorkflowActionID == workflowActionId).FirstOrDefault();
        }
    }
}

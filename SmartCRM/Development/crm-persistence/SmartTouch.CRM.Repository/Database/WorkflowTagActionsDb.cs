using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;

namespace SmartTouch.CRM.Repository.Database
{
    public class WorkflowTagActionsDb : BaseWorkflowActionsDb
    {
        [Key]
        public int WorkflowTagActionID { get; set; }

        [ForeignKey("Tags")]
        public int TagID { get; set; }
        public TagsDb Tags { get; set; }

        public byte ActionType { get; set; }

        public override void Save(CRMDb db)
        {
            
            if(WorkflowTagActionID ==0)
            {
                if(this.TagID > 0)
                  db.WorkflowTagActions.Add(this);

            }
            else 
            {
                db.Entry(this).State = EntityState.Modified;
            }
        }


        public override BaseWorkflowActionsDb Get(int workflowActionId, CRMDb db)
        {
            return db.WorkflowTagActions.Include(g=>g.Tags).Where(t => t.WorkflowActionID == workflowActionId).FirstOrDefault();
        }
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;

namespace SmartTouch.CRM.Repository.Database
{
    public class WorkflowContactFieldActionsDb : BaseWorkflowActionsDb
    {
        [Key]
        public int WorkflowContactFieldActionID { get; set; }

        [ForeignKey("Fields")]
        public int? FieldID { get; set; }
        public FieldsDb Fields { get; set; }

        public string FieldValue { get; set; }

        [ForeignKey("DropdownValue")]
        public short? DropdownValueID { get; set; }
        public DropdownValueDb DropdownValue { get; set; }

        public override void Save(CRMDb db)
        {
            if(WorkflowContactFieldActionID == 0)
            {
                if((this.FieldID.HasValue && this.FieldID.Value > 0) || (this.DropdownValueID.HasValue && this.DropdownValueID.Value > 0))
                  db.WorkflowContactFieldActions.Add(this);

            }
            else
            {
                db.Entry(this).State = EntityState.Modified;
            }
            
        }

        public override BaseWorkflowActionsDb Get(int workflowActionId, CRMDb db)
        {
            return db.WorkflowContactFieldActions.Include(c=>c.Fields).Where(f => f.WorkflowActionID == workflowActionId).FirstOrDefault();
        }
    }
}

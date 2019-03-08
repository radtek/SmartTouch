using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;

namespace SmartTouch.CRM.Repository.Database
{
    public class WorkFlowTextNotificationActionsDb : BaseWorkflowActionsDb
    {
        [Key]
        public int WorkFlowTextNotificationActionID { get; set; }

        [ForeignKey("ContactPhoneNumbers")]
        public int FromMobileID { get; set; }
        public ContactPhoneNumbersDb ContactPhoneNumbers { get; set; }

        public string Message { get; set; }

        public override void Save(CRMDb db)
        {
            if(WorkFlowTextNotificationActionID == 0)
            {
                db.WorkflowTextNotificationActions.Add(this);
            }
            else
            {
                db.Entry(this).State = EntityState.Modified;
            }
            
        }

        public override BaseWorkflowActionsDb Get(int workflowActionId, CRMDb db)
        {
            return db.WorkflowTextNotificationActions.Where(tn => tn.WorkflowActionID == workflowActionId).FirstOrDefault();
        }
    }
}

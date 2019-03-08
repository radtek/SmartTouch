using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
    public class WorkflowEmailNotificationActionDb : BaseWorkflowActionsDb
    {
        [Key]
        public int WorkFlowEmailNotificationActionID { get; set; }

        [ForeignKey("AccountEmails")]        
        public int FromEmailID { get; set; }
        public AccountEmailsDb AccountEmails { get; set; }

        public string Subject { get; set; }
        public string Body { get; set; }

        public override void Save(CRMDb db)
        {
            if (WorkFlowEmailNotificationActionID == 0) {
                if(this.FromEmailID > 0)
                    db.WorkflowEmailNotificationAction.Add(this);
            }          
                          
            else            
                db.Entry(this).State = EntityState.Modified;
            
        }

        public override BaseWorkflowActionsDb Get(int workflowActionId, CRMDb db)
        {
            return db.WorkflowEmailNotificationAction.Where(cl => cl.WorkflowActionID == workflowActionId).FirstOrDefault();
        }
    }
}

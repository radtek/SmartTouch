using System;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;

namespace SmartTouch.CRM.Repository.Database
{
    public class WorkflowLeadScoreActionsDb : BaseWorkflowActionsDb
    {
        [Key]
        public int WorkflowLeadScoreActionID { get; set; }
        public int LeadScoreValue { get; set; }
        
        public override void Save(CRMDb db)
        {
            if(WorkflowLeadScoreActionID == 0)
            {
                db.WorkflowLeadScoreActions.Add(this);
            }
            else
            {
                db.Entry(this).State = EntityState.Modified;
            }
            
        }

        private void InsertLeadScoreRule(int accountId, int by, CRMDb db)
        {
            var rule = new LeadScoreRulesDb()
            {
                AccountID = accountId,
                ConditionDescription = "Update lead score through automation",
                ConditionID = 10,
                ConditionValue = this.WorkflowActionID.ToString(),
                Score = this.LeadScoreValue,
                IsActive = true,
                CreatedBy = by,
                ModifiedBy = by,
                CreatedOn = DateTime.Now.ToUniversalTime(),
                ModifiedOn = DateTime.Now.ToUniversalTime()
            };

            db.LeadScoreRules.Add(rule);
        }

        public void UpdateLeadScoreRule(int accountId, int by)
        {
            using(CRMDb db = new CRMDb())
            {
                var rule = db.LeadScoreRules.Where(r => r.ConditionValue == this.WorkflowActionID.ToString()).FirstOrDefault();

                if (rule != null)
                {
                    var isActionDeleted = db.WorkflowActions.Where(a => a.WorkflowActionID == this.WorkflowActionID).Select(a => a.IsDeleted).FirstOrDefault();
                    if (isActionDeleted)
                    {
                        db.LeadScoreRules.Remove(rule);
                    }
                    else
                    {
                        rule.Score = this.LeadScoreValue;
                        rule.ModifiedBy = by;
                        rule.ModifiedOn = DateTime.Now.ToUniversalTime();
                    }
                }
                else
                {
                    InsertLeadScoreRule(accountId, by, db);
                }
                db.SaveChanges();
            }
            
        }

        public override BaseWorkflowActionsDb Get(int workflowActionId, CRMDb db)
        {
            return db.WorkflowLeadScoreActions.Where(cl=>cl.WorkflowActionID == workflowActionId).FirstOrDefault();
        }
    }
}

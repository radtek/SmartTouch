using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;

namespace SmartTouch.CRM.Repository.Database
{
    public class WorkflowCampaignActionLinksDb
    {
        [Key]
        public int WorkflowCampaignLinkID { get; set; }

        [ForeignKey("CampaignLinks")]
        public int LinkID { get; set; }
        public CampaignLinksDb CampaignLinks { get; set; }

        [ForeignKey("ParentWorkflowAction")]
        public int ParentWorkflowActionID { get; set; }
        public WorkflowCampaignActionsDb ParentWorkflowAction { get; set; }

        [NotMapped]
        public int Order { get; set; }

        public int LinkActionID { get; set; }
        [NotMapped]
        public virtual IEnumerable<WorkflowActionsDb> LinkActions { get; set; }

        
        [NotMapped]
        public int WorkflowID { get; set; }

        public void Save(CRMDb db)
        {
            var index = 1;
            if (LinkActions != null)
            {
                LinkActions.Each(la =>
                    {
                        index = 1;
                        if(la.Action != null)
                        {
                            if (la.WorkflowActionID == 0)
                            {
                                var wa = new WorkflowActionsDb()
                                {
                                    OrderNumber = index,
                                    WorkflowActionTypeID = la.WorkflowActionTypeID,
                                    IsSubAction = true,
                                    WorkflowID = (short)WorkflowID
                                };
                                wa.Save(db);
                            }
                            else
                            {
                                la.OrderNumber = index;
                                la.IsSubAction = true;
                                la.WorkflowID = (short)WorkflowID;
                                la.Save(db);
                            }
                            la.Action.Save(db);
                            db.SaveChanges();
                            if(!la.IsDeleted)
                            {
                                LinkActionID = la.Action.WorkflowActionID;
                                SaveActionLink(db);
                                db.SaveChanges();
                            }
                            index++;
                        }
                    });
            }
        }

        public void SaveActionLink(CRMDb db)
        {
            db.WorkflowCampaignActionLinks.Add(this);
        }
    }
}

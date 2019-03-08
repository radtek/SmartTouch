using SmartTouch.CRM.Entities;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using LandmarkIT.Enterprise.Extensions;
using System;
using LandmarkIT.Enterprise.Utilities.Logging;

namespace SmartTouch.CRM.Repository.Database
{
    public class WorkflowCampaignActionsDb : BaseWorkflowActionsDb
    {
        [Key]
        public int WorkflowCampaignActionID { get; set; }

        [ForeignKey("Campaigns")]
        public int CampaignID { get; set; }
        public CampaignsDb Campaigns { get; set; }

        [NotMapped]
        public ICollection<WorkflowCampaignActionLinksDb> Links { get; set; }

        public override void Save(CRMDb db)
        {
            if (WorkflowCampaignActionID == 0)
            {
                if(this.CampaignID > 0)
                   db.WorkflowCampaignActions.Add(this);

            }
            else
            {
                db.Entry(this).State = EntityState.Modified;
            }
        }

        public override BaseWorkflowActionsDb Get(int workflowActionId, CRMDb db)
        {
            var action = db.WorkflowCampaignActions.Include(x => x.Campaigns).Include(i => i.WorkflowAction).Where(c => c.WorkflowActionID == workflowActionId).FirstOrDefault();
            try
            {
                action.Links = db.WorkflowCampaignActionLinks.Where(cl => cl.ParentWorkflowActionID == action.WorkflowCampaignActionID).ToList();
                var distinctLinks = action.Links.Select(l => l.LinkID).Distinct();
                var campaignLinkActions = new List<WorkflowCampaignActionLinksDb>();
                distinctLinks.Each(dl =>
                {
                    var cl = new WorkflowCampaignActionLinksDb();
                    cl.LinkID = dl;
                    var actions = action.Links.Where(al => al.LinkID == dl);
                    var linkActions = new List<WorkflowActionsDb>();
                    actions.Each(a =>
                    {
                        cl.Order = a.Order;
                        cl.LinkActionID = a.LinkActionID;
                        cl.ParentWorkflowActionID = a.ParentWorkflowActionID;
                        if (db.WorkflowActions.Where(aid => aid.WorkflowActionID == a.LinkActionID && !aid.IsDeleted).IsAny())
                        {
                            var linkAction = db.WorkflowActions.Where(aid => aid.WorkflowActionID == a.LinkActionID && !aid.IsDeleted).FirstOrDefault();
                            linkAction.Action = linkAction.GetAction(db);
                            linkActions.Add(linkAction);
                        }
                    });
                    cl.LinkActions = linkActions;
                    campaignLinkActions.Add(cl);
                });
                action.Links = campaignLinkActions;
            }
            catch(Exception ex)
            {
                Logger.Current.Error("Error while getting link actions", ex);
                ex.Data.Clear();
                ex.Data.Add("Workflow Action ID", workflowActionId);
            }
            
            return action;
        }
    }
}

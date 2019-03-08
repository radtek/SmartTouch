using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.Domain;

namespace SmartTouch.CRM.Domain.Workflows
{
    public class Workflow : EntityBase<short>, IAggregateRoot
    {
        public int WorkflowID { get; set; }
        public string WorkflowName { get; set; }
        public int AccountID { get; set; }
        public WorkflowStatus StatusID { get; set; }
        public string StatusName { get; set; }
        public DateTime? DeactivatedOn { get; set; }
        public bool? IsWorkflowAllowedMoreThanOnce { get; set; }
        public byte? AllowParallelWorkflows { get; set; }
        public string RemovefromWorkflows { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public IEnumerable<WorkflowTrigger> Triggers { get; set; }
        public IEnumerable<WorkflowAction> WorkflowActions { get; set; }
        public int ContactsStarted { get; set; }
        public int ContactsInProgress { get; set; }
        public int ContactsFinished { get; set; }
        public int ContactsOptedOut { get; set; }
        public int ParentWorkflowID { get; set; }
        public int TotalWorkflowCount { get; set; }

        protected override void Validate()
        {
            if (string.IsNullOrEmpty(WorkflowName))
                AddBrokenRule(WorkflowBusinessRules.WorkflowNameRequired);


            if (!string.IsNullOrEmpty(WorkflowName) && WorkflowName.Length > 75)
                AddBrokenRule(WorkflowBusinessRules.WorkflowNameMaxLength);


            this.validateTriggers();
            this.validateActions();
        }

        void validateTriggers()
        {
            foreach (WorkflowTrigger trigger in this.Triggers)
            {
                if (trigger.TriggerTypeID == 0 && trigger.IsStartTrigger == true)
                    AddBrokenRule(WorkflowBusinessRules.StartTriggerTypeRequired);
                else if (trigger.TriggerTypeID == 0 && trigger.IsStartTrigger == false)
                    AddBrokenRule(WorkflowBusinessRules.StopTriggerTypeRequired);
                else if (trigger.TriggerTypeID == WorkflowTriggerType.FormSubmitted && trigger.FormID == null)
                    AddBrokenRule(WorkflowBusinessRules.FormRequired);
                else if (trigger.TriggerTypeID == WorkflowTriggerType.Campaign && trigger.CampaignID == null)
                    AddBrokenRule(WorkflowBusinessRules.CampaignRequired);
                else if (trigger.TriggerTypeID == WorkflowTriggerType.LifecycleChanged && trigger.LifecycleDropdownValueID == null)
                    AddBrokenRule(WorkflowBusinessRules.LifeCycleValueRequired);
                else if (trigger.TriggerTypeID == WorkflowTriggerType.TagApplied && trigger.TagID == null)
                    AddBrokenRule(WorkflowBusinessRules.TagRequired);
                else if (trigger.TriggerTypeID == WorkflowTriggerType.SmartSearch && trigger.SearchDefinitionID == null)
                    AddBrokenRule(WorkflowBusinessRules.SmartSearchRequired);
                else if (trigger.TriggerTypeID == WorkflowTriggerType.OpportunityStatusChanged && trigger.OpportunityStageID == null)
                    AddBrokenRule(WorkflowBusinessRules.OpportunityStageRequired);
                else if (trigger.TriggerTypeID == WorkflowTriggerType.LeadAdapterSubmitted && trigger.LeadAdapterID == null)
                    AddBrokenRule(WorkflowBusinessRules.LeadAdapterRequired);
            }
        }

        void validateActions()
        {
            foreach (WorkflowAction action in this.WorkflowActions)
            {
                if (action.IsDeleted != true && action.WorkflowActionTypeID == 0)
                    AddBrokenRule(WorkflowBusinessRules.ActionTypeRequired);

            }

            if (!this.WorkflowActions.Where(i => !i.IsDeleted).Any())
            {
                AddBrokenRule(WorkflowBusinessRules.MinOneActionIsRequired);
            }
            else
            {
                foreach (WorkflowAction action in this.WorkflowActions)
                {
                    if (action.IsDeleted != true && action.WorkflowActionTypeID == WorkflowActionType.LinkActions)
                    {
                        var wal = new List<WorkflowAction>();
                        var campaignAction = action.Action as WorkflowCampaignAction;
                        if (campaignAction != null && campaignAction.Links != null && campaignAction.Links.Any())
                        {
                            campaignAction.Links.ToList().ForEach(l =>
                                {
                                    if (l.Actions != null && l.Actions.Where(a => a.Action != null).Any())
                                    {
                                        l.Actions.ToList().ForEach(a =>
                                        {
                                            if (a.Action != null && !a.IsDeleted)
                                                wal.Add(a);
                                        });
                                    }
                                });
                        }
                        if (!wal.Any())
                            AddBrokenRule(WorkflowBusinessRules.ActionTypeRequired);
                    }
                }
            }
        }
    }
}

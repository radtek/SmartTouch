using Newtonsoft.Json;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Domain.Workflows;
using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    public class WorkFlowViewModel
    {
        public short WorkflowID { get; set; }
        public string WorkflowName { get; set; }
        public int AccountID { get; set; }
        public short StatusID { get; set; }
        public string Status { get; set; }
        public DateTime? DeactivatedOn { get; set; }
        public bool? IsWorkflowAllowedMoreThanOnce { get; set; }
        public byte? AllowParallelWorkflows { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public IEnumerable<short> RemoveFromWorkflows { get; set; }
        public IEnumerable<WorkflowTriggerViewModel> Triggers { get; set; }
        public IEnumerable<WorkflowActionViewModel> WorkflowActions { get; set; }
        public int ContactsStarted { get; set; }
        public int ContactsInProgress { get; set; }
        public int ContactsFinished { get; set; }
        public int ContactsOptedOut { get; set; }
        public int ParentWorkflowID { get; set; }

        public IEnumerable<TagViewModel> PopularTags { get; set; }
        public IEnumerable<TagViewModel> RecentTags { get; set; }
        public int TotalWorkflowCount { get; set; }
    }
    public class WorkflowTriggerViewModel
    {
        public int WorkflowTriggerID { get; set; }
        public int WorkflowID { get; set; }
        
        public int? CampaignID { get; set; }
        public string CampaignName { get; set; }
        public int? FormID { get; set; }
        public string FormName { get; set; }
        public int? LifecycleDropdownValueID { get; set; }
        public string LifecycleName { get; set; }
        public int? TagID { get; set; }
        public string TagName { get; set; }
        public int? SearchDefinitionID { get; set; }       
        public string SearchDefinitionName { get; set; }
        public int? LeadAdapterID { get; set; }
        public string LeadAdapterName { get; set; }
        public WorkflowTriggerType TriggerTypeID { get; set; }
        public bool IsStartTrigger { get; set; }
        public short? OpportunityStageID { get; set; }
        public string OpportunityStageName { get; set; }
        public IEnumerable<int> SelectedLinks { get; set; }
        public string SelectedURLs { get; set; }
        public int? LeadScore { get; set; }
        public string WebPage { get; set; }
        public int? Duration { get; set; }
        public WebPageDurationOperator? Operator { get; set; }
        public bool IsAnyWebPage { get; set; }
        public short? ActionType { get; set; }
        public string ActionTypeName { get; set; }
        public short? TourType { get; set; }
        public string TourTypeName { get; set; }

        public IEnumerable<int?> FormIDs { get; set; }
        public IEnumerable<string> FormNames { get; set; }
        public IEnumerable<int?> TagIDs { get; set; }
        public IEnumerable<string> TagNames { get; set; }
        public IEnumerable<int?> SearchDefinitionIDs { get; set; }
        public IEnumerable<string> SearchDefinitionNames { get; set; }
        public IEnumerable<int?> LeadAdapterIDs { get; set; }
        public IEnumerable<string> LeadAdapterNames { get; set; }
    }

    public abstract class BaseWorkflowActionViewModel
    {
        public int WorkflowActionID { get; set; }
        public WorkflowActionType WorkflowActionTypeID { get; set; }
    }

    public class WorkflowActionViewModel
    {
        public short WorkflowID { get; set; }
        public int WorkflowActionID { get; set; }
        public int OrderNumber { get; set; }
        public WorkflowActionType WorkflowActionTypeID { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsSubAction { get; set; }
        [JsonConverter(typeof(BaseWorkflowActionConverter))]
        public BaseWorkflowActionViewModel Action { get; set; }
    }

    public class WorkflowCampaignActionViewModel : BaseWorkflowActionViewModel
    {      
        public int WorkflowCampaignActionID { get; set; }
        public int CampaignID { get; set; }
        public string CampaignName { get; set; }        
        public int FromUserMailID { get; set; }

        public IEnumerable<WorkflowCampaignActionLinkViewModel> CampaignLinks { get; set; }
    }

    public class WorkflowCampaignActionLinkViewModel
    {
        public WorkflowCampaignActionLinkViewModel()
        {
            Actions = new List<WorkflowActionViewModel>();
        }
        public int WorkflowCampaignLinkID { get; set; }
        public int CampaignLinkId { get; set; }
        public int ParentWorkflowActionID { get; set; }
        public int LinkActionID { get; set; }
        public int Order { get; set; }

        public IEnumerable<WorkflowActionViewModel> Actions { get; set; }
    }
    public class WorkflowLeadScoreActionViewModel : BaseWorkflowActionViewModel
    {
        public int WorkflowLeadScoreActionID { get; set; }
        public int LeadScoreValue { get; set; }
    }

    public class WorkflowEmailNotifyActionViewModel : BaseWorkflowActionViewModel
    {
        public int WorkFlowEmailNotificationActionID { get; set; }
        public int FromEmailID { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }

    public class WorkflowTimerActionViewModel : BaseWorkflowActionViewModel
    {
        public int WorkflowTimerActionID { get; set; }
        public TimerType TimerType { get; set; }
        public int? DelayPeriod { get; set; }
        public DateInterval? DelayUnit { get; set; }
        // 1 for anyday, 2 for weekdays
        public RunOn? RunOn { get; set; }
        public DateTime? RunAt { get; set; }
        public DateTime? RunAtDateTime { get; set; }
        public RunType? RunType { get; set; }        
        // for timer type A date
        public DateTime? RunOnDate { get; set; }
        public DateTime? RunAtTime { get; set; }
        public DateTime? RunAtTimeDateTime { get; set; }
        // for timer type between dates
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public IEnumerable<DayOfWeek> DaysOfWeek { get; set; }
    }
    public class WorkflowLifeCycleActionViewModel : BaseWorkflowActionViewModel
    {
        public int WorkflowLifeCycleActionID { get; set; }
        public short LifeCycleDropdownValueID { get; set; }
        public string LifecycleName { get; set; }
    }

    public class TriggerCategoryTypeViewModel
    {
        public byte TriggerTypeID { get; set; }
        public string TriggerName { get; set; }
        public TriggerCategory TriggerCategory { get; set; }
    }
    public class WorkflowTagActionViewModel : BaseWorkflowActionViewModel
    {
        public int WorkflowTagActionID { get; set; }
        public int TagID { get; set; }
        public int Order { get; set; }
        //1 if add 0 for remove
        public int ActionType { get; set; }
        public string TagName { get; set; }
    }
    public class WorkflowUserAssignmentActionViewModel : BaseWorkflowActionViewModel
    {
        public int WorkflowUserAssignmentActionID { get; set; }
        public int ScheduledID { get; set; }
        public string UserName { get; set; }

        public IEnumerable<RoundRobinContactAssignmentViewModel> RoundRobinContactAssignments { get; set; }
    }

    public class RoundRobinContactAssignmentViewModel
    {
        public int RoundRobinContactAssignmentID { get; set; }
        public byte DayOfWeek { get; set; }
        public string UserID { get; set; }
        public IEnumerable<int> UserIds { get; set; }
        public IEnumerable<string> UserNames { get; set; }
        public string IsRoundRobinAssignment { get; set; }

        public int WorkFlowUserAssignmentActionID { get; set; }
        public WorkflowUserAssignmentActionViewModel UserAction { get; set; }
    }

    public class WorkflowNotifyUserActionViewModel : BaseWorkflowActionViewModel
    {
        public int WorkflowNotifyUserActionID { get; set; }
        //public string UserID { get; set; }
        public IEnumerable<int> UserIds { get; set; }
        public int NotifyType { get; set; }
        public string MessageBody { get; set; }
        public string UserName { get; set; }
        public int Order { get; set; }
        public IEnumerable<int> NotificationFieldIds { get; set; }
    }
    public class WorkflowContactFieldActionViewModel : BaseWorkflowActionViewModel
    {
        public int WorkflowContactFieldActionID { get; set; }
        public int FieldID { get; set; }
        public string FieldValue { get; set; }
        public string FieldName { get; set; }
        public int DropdownValueID { get; set; }
        public bool IsDropdownField { get; set; }
        public IEnumerable<FieldValueOption> ValueOptions { get; set; }
        public FieldType FieldInputTypeId { get; set; }
        public string Name { get; set; } // Used in report screen to display the exact value name
    }
    public class WorkflowTextNotificationActionViewModel : BaseWorkflowActionViewModel
    {
        public int WorkflowTextNotificationActionID { get; set; }
        public string Message { get; set; }
        public string FromMobileID { get; set; }
    }
    public class TriggerWorkflowActionViewModel : BaseWorkflowActionViewModel
    {
        public int TriggerWorkflowActionID { get; set; }
        public int SiblingWorkflowID { get; set; }
        public string WorkflowName { get; set; }
    }
}

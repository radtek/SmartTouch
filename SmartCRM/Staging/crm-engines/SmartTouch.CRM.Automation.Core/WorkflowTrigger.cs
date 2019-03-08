using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.Campaigns;

namespace SmartTouch.CRM.Automation.Core
{
    public enum AutomationTrigger
    {
        BeginWorkflow,
        CampaignSent,        
        CampaignLinkClicked,
        NotifyUser,
        FormSubmitted,
        LifecycleChanged,
        TagAdded,
        TagRemoved,
        EndWorkflow
    }

    public enum AutomationState
    {
        WorkflowStarted,
        CampaignLinkClicked,
        ContactInSavedSearch,
        FormSubmitted,
        LifeCycleChanged,
        CampaignDelivered,
        TagAdded,
        NotifiedUser,
        SubWorkflowEnd,
        WorkflowEnded
    }
}

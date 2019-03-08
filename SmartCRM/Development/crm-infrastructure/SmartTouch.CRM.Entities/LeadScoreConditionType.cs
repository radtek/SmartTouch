using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Entities
{
    [AttributeUsage(AttributeTargets.All)]
    public sealed class MessageModuleAttribute:Attribute
    {
        public int[] Modules { get; set; }
        public MessageModuleAttribute(params int[] modules)
        {
            this.Modules = modules;
        }
    }
    // modules : 1 : lead score, 2: automation
    public enum LeadScoreConditionType : byte
    {
        [MessageModule(1)]
        ContactOpensEmail = 1, //leadscore
        [MessageModule(1,2)]
        ContactClicksLink = 2, //leadscore, automation
        [MessageModule(1,2)]
        ContactSubmitsForm = 3, //leadscore, automation
        [MessageModule(1)]
        ContactVisitsWebsite = 4, //leadscore, not being used
        [MessageModule(1,2)]
        ContactVisitsWebPage = 5, //leadscore, automation
        [MessageModule(1)]
        ContactActionTagAdded = 6, //leadscore
        [MessageModule(1)]
        ContactNoteTagAdded = 7, //leadscore
        [MessageModule(1)]
        ContactLeadSource = 8, //leadscore
        [MessageModule(1)]
        ContactTourType = 9, //leadscore
        [MessageModule(2)]
        ContactTagAdded = 10, //automation
        [MessageModule(2)]
        ContactTagRemoved = 11, //automation
        [MessageModule(2)]
        ContactLifecycleChange = 12, //automation
        [MessageModule(1)]
        ContactOwnerChange = 13, //leadscore
        [MessageModule(2)]
        ContactMatchesSavedSearch = 14, //automation
        [MessageModule(2)]
        WorkflowActivated = 15, //automation
        [MessageModule(2)]
        OpportunityStatusChanged = 16, //automation
        [MessageModule(2)]
        ContactWaitPeriodEnded = 17, //automation
        [MessageModule(2)]
        CampaignSent = 18, //automation
        [MessageModule(2)]
        WorkflowInactive = 19, //automation
        [MessageModule(2)]
        WorkflowPaused = 20, //automation
        [MessageModule(2)]
        UnsubscribeEmails = 21,
        [MessageModule(2)]
        LeadAdapterSubmitted = 22, //automation
        [MessageModule(1,2)]
        AnEmailSent = 23, //leadscore, automation
        [MessageModule(2)]
        LeadscoreReached = 24, //automation
        [MessageModule(2)]
        TriggerWorkflow = 25,
        [MessageModule(1,2)] //leadscore, automation
        PageDuration = 26,
        [MessageModule(1)] //LeadScore
        ContactNoteCategoryAdded = 27,
        [MessageModule(1)] // For Campaign Litmus Results
        CampaignLitmusResults = 28,
        [MessageModule(1,2)]//leadscore, automation
        ContactActionCompleted = 29,
        [MessageModule(1)]//leadscore
        ContactSendText =30,
        [MessageModule(1)]//leadscore
        ContactEmailReceived =31,
        [MessageModule(2)]//Automation
        ContactTourCompleted = 32
    }
}

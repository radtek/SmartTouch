using SmartTouch.CRM.Infrastructure.Domain;

namespace SmartTouch.CRM.Domain.Workflows
{
    public static class WorkflowBusinessRules
    {
        public static readonly BusinessRule WorkflowNameRequired = new BusinessRule("[|Workflow Name is required|]");
        public static readonly BusinessRule WorkflowNameMaxLength = new BusinessRule("[|You are allowed to enter only 75 characters for Workflow Name|]");
        public static readonly BusinessRule StartTriggerTypeRequired = new BusinessRule("[|Start Trigger is required|]");
        public static readonly BusinessRule StopTriggerTypeRequired = new BusinessRule("[|Stop Trigger is required|]");
        public static readonly BusinessRule ActionTypeRequired = new BusinessRule("[|Action type is required|]");
        public static readonly BusinessRule TagRequired = new BusinessRule("[|Tag is required|]");
        public static readonly BusinessRule LeadScoreValueRequired = new BusinessRule("[|Leadscore Value is required|]");
        public static readonly BusinessRule LeadScoreValueMustbePositive = new BusinessRule("[|Leadscore Value should be positive|]");
        public static readonly BusinessRule FromMobileRequried = new BusinessRule("[|From Mobile is required|]");
        public static readonly BusinessRule MessageRequried = new BusinessRule("[|Message is required|]");
        public static readonly BusinessRule LifeCycleValueRequired = new BusinessRule("[|Lifecycle value is required|]");
        public static readonly BusinessRule FieldRequired = new BusinessRule("[|Field is required|]");
        public static readonly BusinessRule FieldValueRequired = new BusinessRule("[|Field value is required|]");
        public static readonly BusinessRule UserRequired = new BusinessRule("[|User is required|]");
        public static readonly BusinessRule NotifybyRequired = new BusinessRule("[|Notify by is required|]");
        public static readonly BusinessRule MinOneActionIsRequired = new BusinessRule("[|Minimum one action is required|]");
        public static readonly BusinessRule FormRequired = new BusinessRule("[|Form is required|]");
        public static readonly BusinessRule CampaignRequired = new BusinessRule("[|Campaign is required|]");
        public static readonly BusinessRule SmartSearchRequired = new BusinessRule("[|Smart Search is required|]");
        public static readonly BusinessRule OpportunityStageRequired = new BusinessRule("[|Opportunity stage is required|]");
        public static readonly BusinessRule CampaignLinksRequired = new BusinessRule("[|Campaign links required|]");
        public static readonly BusinessRule FromEmailRequired = new BusinessRule("[|From Email is requierd|]");
        public static readonly BusinessRule SubjectRequired = new BusinessRule("[|Email Subject is requierd|]");
        public static readonly BusinessRule BodyRequired = new BusinessRule("[|Email Body is requierd|]");
        public static readonly BusinessRule LeadAdapterRequired = new BusinessRule("[|Lead Adapter required|]");
        public static readonly BusinessRule MaxLength160 = new BusinessRule("[|Please Enter Only 160 characters|]");
        public static readonly BusinessRule MaxLength512 = new BusinessRule("[|Please Enter Only 512 characters|]");
    }
}

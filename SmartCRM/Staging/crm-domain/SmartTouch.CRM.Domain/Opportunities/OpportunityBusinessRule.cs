using SmartTouch.CRM.Infrastructure.Domain;

namespace SmartTouch.CRM.Domain.Opportunities
{
    public static class OpportunityBusinessRule
    {
        public static readonly BusinessRule OpportunityRequired = new BusinessRule("[|Opportunity is required|]");
        public static readonly BusinessRule OpportunityNameMaxLength = new BusinessRule("[|You are allowed to enter only 75 characters for Opportunity|]");
        public static readonly BusinessRule ContactsRequired = new BusinessRule("[|Contacts required|]");
        public static readonly BusinessRule StageRequired = new BusinessRule("[|Stage is required|]");
        public static readonly BusinessRule PotentialRequired = new BusinessRule("[|Potential required|]");
        public static readonly BusinessRule ExpectedCloseDateShouldBeGreaterThanToday = new BusinessRule("[|Expected Close Should be future date|]");
        public static readonly BusinessRule OpportunityDescriptionMaxLength = new BusinessRule("[|You are allowed to enter only 1000 characters for Description|]");
        public static readonly BusinessRule OwnerRequired = new BusinessRule("[|Owner is required|]");
        public static readonly BusinessRule PotentialMaxValueValidation = new BusinessRule("[|Potential can not be more than 922337203685477|]");
        public static readonly BusinessRule PotentialMinValueValidation = new BusinessRule("[|Potential should be a positive value|]");
    }
}

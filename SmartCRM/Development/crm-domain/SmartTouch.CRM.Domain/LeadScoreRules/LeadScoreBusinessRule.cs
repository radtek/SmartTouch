using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.LeadScoreRules
{
    public static class LeadScoreBusinessRule
    {
        public static readonly BusinessRule DescriptionRequired = new BusinessRule("[|Description is required|]");
        public static readonly BusinessRule ConditionRequired = new BusinessRule("[|Condition is required|]");
        public static readonly BusinessRule ScoreRequired = new BusinessRule("[|Score is required.|]");
        public static readonly BusinessRule Scoremustbepositivenumber = new BusinessRule("[|Score must be positive number only|]");
        public static readonly BusinessRule WebsiteRequired = new BusinessRule("[|Website is required.|]");
        public static readonly BusinessRule WebpageRequired = new BusinessRule("[|Webpage is required.|]");
        public static readonly BusinessRule ConditionValueRequired = new BusinessRule("[|Please enter Condition Value|]");
        public static readonly BusinessRule ValidUrl = new BusinessRule("[|Please enter valid URL|]");
        public static readonly BusinessRule Tags = new BusinessRule("[|Include one tag only|]");
        public static readonly BusinessRule CampaignsRequired = new BusinessRule("[|Campaigns is required|]");
        public static readonly BusinessRule LeadSourceRequired = new BusinessRule("[|Lead Source is required.|]");
        public static readonly BusinessRule TourTypeRequired = new BusinessRule("[|Tour type is required.|]");
        public static readonly BusinessRule ContactID = new BusinessRule("[|ContactID is required.|]");
        public static readonly BusinessRule LeadScoreRuleID = new BusinessRule("[|LeadScoreRuleID  is required.|]");
        public static readonly BusinessRule LinksRequried = new BusinessRule("[|Links are requried|]");
        public static readonly BusinessRule DurationShouldBeAtleastOneSecond = new BusinessRule("[|Duration should be at least one second|]");

    }
}

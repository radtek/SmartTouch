using SmartTouch.CRM.Domain.Tags;
using SmartTouch.CRM.Domain.Users;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.LeadScoreRules
{
    public class LeadScoreRule : EntityBase<int>, IAggregateRoot
    {
        public int AccountID { get; set; }
        public string ConditionDescription { get; set; }
        public virtual ScoreCategories Category { get; set; }
        public Condition Condition { get; set; }
        public ScoreCategory CategoryID { get; set; }
        public LeadScoreConditionType ConditionID { get; set; }
        public int? Score { get; set; }
        public string ConditionValue { get; set; }
        public bool AppliedToPreviousActions { get; set; }
        public IEnumerable<Tag> Tags { get; set; }

        public virtual LeadSource LeadSource { get; set; }
        public virtual TourType TourType { get; set; }
        public virtual Campaigns.Campaign Campaign { get; set; }

        public int LeadScoreRuleMapID { get; set; }
        public bool IsActive { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public int ModifiedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
        public virtual User User { get; set; }

        public IEnumerable<LeadScoreConditionValue> LeadScoreConditionValues { get; set; }

        public int[] SelectedFormID { get; set; }
        public int[] SelectedCampaignID { get; set; }
        public int[] SelectedCampaignLinkID { get; set; }
        public int[] SelectedLeadSourceID { get; set; }
        public int[] SelectedTourTypeID { get; set; }
        public int[] SelectedTagID { get; set; }
        public int[] SelectedNoteCategoryID { get; set; }
        public int[] SelectedActionTypeID { get; set; }


        protected override void Validate()
        {
            if (ConditionDescription == null || ConditionDescription == "") AddBrokenRule(LeadScoreBusinessRule.DescriptionRequired);
            if (Score == null) AddBrokenRule(LeadScoreBusinessRule.ScoreRequired);
            if (Score <= 0) AddBrokenRule(LeadScoreBusinessRule.Scoremustbepositivenumber);
            if (ConditionID == 0) AddBrokenRule(LeadScoreBusinessRule.ConditionRequired);

            if ((ConditionID == LeadScoreConditionType.ContactVisitsWebPage ||ConditionID == LeadScoreConditionType.ContactVisitsWebsite
                || ConditionID == LeadScoreConditionType.PageDuration) && string.IsNullOrEmpty(ConditionValue))
                AddBrokenRule(LeadScoreBusinessRule.WebpageRequired);
            
            if (ConditionID == LeadScoreConditionType.ContactClicksLink && SelectedCampaignID != null && SelectedCampaignID.Any()
                    && SelectedCampaignLinkID == null && !SelectedCampaignLinkID.Any())
            {
                AddBrokenRule(LeadScoreBusinessRule.LinksRequried);
            }
            if (ConditionID == LeadScoreConditionType.PageDuration)
            {
                var duration = LeadScoreConditionValues.Where(c => c.ValueType == LeadScoreValueType.PageDuration).Select(c => c.Value).FirstOrDefault();
                if (duration == null || int.Parse(duration) == 0)
                    AddBrokenRule(LeadScoreBusinessRule.DurationShouldBeAtleastOneSecond);

            }
        }

        public bool IsValidUrl(string url)
        {
            string pattern = @"(http(s)?:\\)?([\w-]+\.)+[\w-]+[.com|.in|.org]+(\[\?%&=]*)?";
            Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
            return regex.IsMatch(url);
        }
    }
}
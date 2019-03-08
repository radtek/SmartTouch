using SmartTouch.CRM.Domain.Campaigns;
using SmartTouch.CRM.Domain.LeadScoreRules;
using SmartTouch.CRM.Domain.Tags;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    public interface ILeadScoreRuleViewModel
    {
        int LeadScoreRuleID { get; set; }
        int AccountID { get; set; }
        IEnumerable<dynamic> Conditions { get; set; }
        IEnumerable<dynamic> Categories { get; set; }

        Condition Condition { get; set; }
        string DateFormat { get; set; }

        string ConditionDescription { get; set; }
        int? Score { get; set; }
        string ConditionValue { get; set; }
        bool AppliedToPreviousActions { get; set; }
    }

    public class LeadScoreRuleViewModel : ILeadScoreRuleViewModel
    {
        public int LeadScoreRuleID { get; set; }
        public int AccountID { get; set; }
        public IEnumerable<dynamic> Conditions { get; set; }
        public IEnumerable<dynamic> Categories { get; set; }
        public IEnumerable<dynamic> TourTypes { get; set; }
        public IEnumerable<dynamic> LeadsourceTypes { get; set; }
        public IEnumerable<dynamic> NoteCategories { get; set; }
        public ScoreCategories Category { get; set; }
        public string CategoryName { get; set; }
        public Condition Condition { get; set; }
        public string ConditionName { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public int ModifiedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
        public string ConditionDescription { get; set; }
        public int? Score { get; set; }
        public string ConditionValue { get; set; }
        public IEnumerable<TagViewModel> TagsList { get; set; }
        public CampaignEntryViewModel Campaigns { get; set; }
        public IEnumerable<CampaignLinkViewModel> CampaignLinks { get; set; }
        public IEnumerable<LeadScoreConditionValueViewModel> LeadScoreConditionValues { get; set; }
        public IEnumerable<dynamic> ActionTypes { get; set; }

        public FormEntryViewModel Forms { get; set; }
        public bool AppliedToPreviousActions { get; set; }
        public string SelectedCampaignText { get; set; }
        public int LeadScoreRuleMapID { get; set; }
        public string DateFormat { get; set; }

        public int[] SelectedCampaignID { get; set; }
        public int[] SelectedCampaignLinkID { get; set; }
        public int[] SelectedFormID { get; set; }
        public int[] SelectedTagID { get; set; }
        public int[] SelectedLeadSourceID { get; set; }
        public int[] SelectedTourTypeID { get; set; }
        public int[] SelectedNoteCategoryID { get; set; }
        public int[] SelectedActionTypeID { get; set; }
        public ScoreCategory CategoryID { get; set; }
        public LeadScoreConditionType ConditionID { get; set; }
        public IEnumerable<string> ConditionValues { get; set; }
    }
}

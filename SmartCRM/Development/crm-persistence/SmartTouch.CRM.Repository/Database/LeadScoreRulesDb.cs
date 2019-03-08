using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
    public class LeadScoreRulesDb
    {
        [Key]
        public int LeadScoreRuleID { get; set; }

        [ForeignKey("Condition")]
        public byte ConditionID { get; set; }
        public virtual ConditionDb Condition { get; set; }
        
        public string ConditionDescription { get; set; }
        public int Score { get; set; }        
        public DateTime CreatedOn { get; set; }        
        public DateTime ModifiedOn { get; set; }
        
        [ForeignKey("User")]
        public virtual int CreatedBy { get; set; }
        public virtual UsersDb User { get; set; }
        
        [ForeignKey("UserDb")]
        public virtual int ModifiedBy { get; set; }
        public virtual UsersDb UserDb { get; set; }
        
        [ForeignKey("Account")]
        public virtual int AccountID { get; set; }
        public virtual AccountsDb Account { get; set; }
        
        public string ConditionValue { get; set; }
        public bool? AppliedToPreviousActions { get; set; }
        public bool? IsActive { get; set; }
        public int LeadScoreRuleMapID { get; set; }
        public string SelectedCampaignLinks { get; set; }
        [NotMapped]
        public IEnumerable<int> SelectedIDs { get; set; }
        public IEnumerable<LeadScoreConditionValuesDb> LeadScoreConditionValues { get; set; }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Domain.ValueObjects;

namespace SmartTouch.CRM.Repository.Database
{
    public class LeadScoreDb
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LeadScoreID { get; set; }
        [ForeignKey("Contacts")]
        public int ContactID { get; set; }
        [ForeignKey("LeadscoreRules")]
        public int LeadScoreRuleID { get; set; }
        public int Score { get; set; }
        public DateTime AddedOn { get; set; }
        public int EntityID { get; set; }

        public LeadScoreRulesDb LeadscoreRules { get; set; }
        public ContactsDb Contacts { get; set; }
    }
}

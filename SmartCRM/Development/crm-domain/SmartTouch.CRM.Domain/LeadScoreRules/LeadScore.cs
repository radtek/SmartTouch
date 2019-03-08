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
    public class LeadScore : EntityBase<int>, IAggregateRoot
    {
        public int? ContactID { get; set; }
        public int? LeadScoreRuleID { get; set; }
        public int Score { get; set; }
        public DateTime AddedOn { get; set; }
        public int EntityID { get; set; }

        protected override void Validate()
        {
        }
    }
}

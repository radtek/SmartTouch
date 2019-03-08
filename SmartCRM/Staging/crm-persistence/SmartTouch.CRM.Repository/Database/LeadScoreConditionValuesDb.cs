using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace SmartTouch.CRM.Repository.Database
{
    public class LeadScoreConditionValuesDb
    {
        [Key]
        public int LeadScoreConditionValueID { get; set; }
        public int LeadScoreRuleID { get; set; }
        public short ValueType { get; set; }
        public string Value { get; set; }
    }
}

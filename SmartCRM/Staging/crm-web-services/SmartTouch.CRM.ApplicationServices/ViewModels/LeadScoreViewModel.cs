using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    public interface ILeadScoreViewModel
    {
        int LeadScoreID { get; set; }
        int ContactID { get; set; }
        int LeadScoreRuleID { get; set; }
        int Score { get; set; }
        DateTime AddedOn { get; set; }

    }

    public class LeadScoreViewModel : ILeadScoreViewModel
    {
        public int LeadScoreID { get; set; }
        public int ContactID { get; set; }
        public int LeadScoreRuleID { get; set; }
        public int Score { get; set; }
        public DateTime AddedOn { get; set; }
    }
}

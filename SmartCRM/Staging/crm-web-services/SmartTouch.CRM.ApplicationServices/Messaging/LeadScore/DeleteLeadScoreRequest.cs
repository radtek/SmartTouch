using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.LeadScore
{
    public class DeleteLeadScoreRequest : ServiceRequestBase
    {
        public int[] RuleID { get; set; }
        public byte? Status { get; set; }
        //public int tagId { get; set; }
        public int conditionId { get; set; }
        public int accountID { get; set; }
    }

    public class DeleteLeadScoreResponse : ServiceResponseBase
    {
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Enterprises
{
   public class ReportedCoupons
    {
        public string Coupon { get; set; }
        public string Issue { get; set; }
        public DateTime SubmittedOn { get; set; }
        public int ContactId { get; set; }
        public string FormSubmissionID { get; set; }
        public int TotalCount { get; set; }
    }
}

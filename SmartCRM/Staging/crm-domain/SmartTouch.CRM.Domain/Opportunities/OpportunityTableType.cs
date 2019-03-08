using SmartTouch.CRM.Domain.Images;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Opportunities
{
    public class OpportunityTableType
    {
        public int OpportunityID { get; set; }
        public string OpportunityName { get; set; }
        public decimal Potential { get; set; }
        public int StageID { get; set; }
        public DateTime? ExpectedClosingDate { get; set; }
        public string Description { get; set; }
        public int Owner { get; set; }
        public int AccountID { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? LastModifiedBy { get; set; }
        public DateTime? LastModifiedOn { get; set; }
        public bool IsDeleted { get; set; }
        public string OpportunityType { get; set; }
        public string ProductType { get; set; }
        public string Address { get; set; }
        public int? ImageID { get; set; }
        public Image OpportunityImage { get; set; }
    }
}

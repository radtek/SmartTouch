using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Opportunities
{
    public class OpportunityContactMapTableType
    {
        public int OpportunityContactMapID { get; set; }
        public int OpportunityID { get; set; }
        public int ContactID { get; set; }
        public decimal Potential { get; set; }
        public DateTime? ExpectedToClose { get; set; }
        public string Comments { get; set; }
        public int Owner { get; set; }
        public int StageID { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedOn { get; set; }
        public int CreatedBy { get; set; }
    }
}

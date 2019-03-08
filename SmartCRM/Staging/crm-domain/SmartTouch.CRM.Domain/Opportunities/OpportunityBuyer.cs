using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Opportunities
{
    public class OpportunityBuyer
    {
        public int OpportunityContactMapID { get; set; }
        public string Name { get; set; }
        public decimal Potential { get; set; }
        public DateTime ExpectedToClose { get; set; }
        public string Comments { get; set; }
        public string PreviousComments { get; set; }
        public string OwnerName { get; set; }
        public int Owner { get; set; }
        public int StageID { get; set; }
        public int ContactID { get; set; }
        public byte ContactType { get; set; }
        public int OpportunityID { get; set; }
        public DateTime CreatedOn { get; set; }
        public int RowNumber { get; set; }
        public string Stage { get; set; }
        public int TotalCount { get; set; }
        public string Description { get; set; }
    }
}

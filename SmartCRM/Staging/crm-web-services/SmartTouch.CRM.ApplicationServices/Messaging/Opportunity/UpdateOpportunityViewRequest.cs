using SmartTouch.CRM.ApplicationServices.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Opportunity
{
    public class UpdateOpportunityViewRequest: ServiceRequestBase
    {
        public int OpportunityID { get; set; }
        public string OpportunityName { get; set; }
        public int StageID { get; set; }
        public int OwnerId { get; set; }
        public decimal Potential { get; set; }
        public DateTime ExpectedCloseDate { get; set; }
        public string Description { get; set; }
        public string OpportunityType { get; set; }
        public string ProductType { get; set; }
        public string Address { get; set; }
        public ImageViewModel image { get; set; }
    }

    public class UpdateOpportunityViewResponse: ServiceRequestBase
    {
        public OpportunityViewModel opportunityViewModel { get; set; }
    }
}

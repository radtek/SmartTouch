using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.ApplicationServices.ViewModels;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Opportunity
{
    public class GetOpportunityListRequest: ServiceRequestBase
    {
        public int Limit { get; set; }
        public string Query { get; set; }
        public int PageNumber { get; set; }       
        public int AccountID { get; set; }        
        public string TimeZone { get; set; }
    }

    public class GetOpportunityListResponse : ServiceResponseBase
    {
        public int TotalHits { get; set; }
        public IEnumerable<OpportunityViewModel> Opportunities { get; set; }
    }
}

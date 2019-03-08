using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.Opportunities;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Opportunity
{
    public class GetOpportunityListByContactRequest : ServiceRequestBase
    {
        public int ContactID { get; set; }
        public int PageSize { get; set; }
        public int PageNumber { get; set; }
    }

    public class GetOpportunityListByContactResponse : ServiceResponseBase
    {
        public IEnumerable<OpportunityViewModel> OpportunitiesList { get; set; }
        public IEnumerable<OpportunityBuyer> Opportunities { get; set; }
    }
}

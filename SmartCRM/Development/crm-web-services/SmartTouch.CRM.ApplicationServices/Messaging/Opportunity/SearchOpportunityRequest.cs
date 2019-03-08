using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Opportunity
{
    public class SearchOpportunityRequest : ServiceRequestBase
    {
        public int Limit { get; set; }
        public string Query { get; set; }
        public int PageNumber { get; set; }
        public IEnumerable<string> OpportunityIDs { get; set; }
        public byte ShowingFieldType { get; set; }
        public string TimeZone { get; set; }
        public string SortField { get; set; }
        public ListSortDirection SortDirection { get; set; }
        public ContactSortFieldType SortFieldType { get; set; }
        public int? UserID { get; set; }
        public int[] UserIDs { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class SearchOpportunityResponse : ServiceResponseBase
    {
        public long TotalHits { get; set; }
        public IEnumerable<OpportunityViewModel> Opportunities { get; set; }
    }
}

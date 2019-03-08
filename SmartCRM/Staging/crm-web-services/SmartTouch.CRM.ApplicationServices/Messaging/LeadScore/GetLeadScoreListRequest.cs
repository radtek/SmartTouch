using SmartTouch.CRM.ApplicationServices.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.LeadScore
{
    public class GetLeadScoreListRequest : ServiceRequestBase
    {
        public int Limit { get; set; }
        public string Query { get; set; }
        public int PageNumber { get; set; }
        public int AccountID { get; set; }
        public string SortField { get; set; }
        public ListSortDirection SortDirection { get; set; }
        public IEnumerable<byte> Modules { get; set; }
    }

    public class GetLeadScoreListResponse : ServiceResponseBase
    {
        public int TotalHits { get; set; }
        public IEnumerable<LeadScoreRuleViewModel> LeadScoreViewModel { get; set; }
    }

    public class GetLeadScoreRequest : IntegerIdRequest
    {
        public GetLeadScoreRequest(int id) : base(id) { }
        public int[] TagValues { get; set; }
    }

    public class GetLeadScoreResponse : ServiceResponseBase
    {
        public LeadScoreRuleViewModel LeadScoreViewModel { get; set; }

    }
}

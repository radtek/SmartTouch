using SmartTouch.CRM.ApplicationServices.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.LeadAdapters
{
    public class GetViewLeadAdapterListRequest : ServiceRequestBase
    {
        public int LeadAdapterID { get; set; }
        public int Limit { get; set; }
        public string Query { get; set; }
        public int PageNumber { get; set; }
    }
    public class GetViewLeadAdapterListResponse : ServiceResponseBase
    {
        public IEnumerable<ViewLeadAdapterViewModel> ViewLeadAdapters { get; set; }
        public int TotalHits { get; set; }
    }
}

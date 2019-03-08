using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SmartTouch.CRM.ApplicationServices.ViewModels;

namespace SmartTouch.CRM.ApplicationServices.Messaging.LeadAdapters
{
    public class GetLeadAdapterListRequest : ServiceRequestBase
    {
        public int AccountID { get; set; }
        public int Limit { get; set; }
        public string Query { get; set; }
        public int PageNumber { get; set; }
       
    }
    public class GetLeadAdapterListResponse : ServiceResponseBase
    {
        public IEnumerable<LeadAdapterViewModel> LeadAdapters { get; set; }
        public int TotalHits { get; set; }
    }
}

using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.LeadAdapters
{
    public class GetLeadAdapterJobLogDetailsListRequest : ServiceRequestBase
    {
        public int LeadAdapterJobLogID { get; set; }
        public bool LeadAdapterRecordStatus { get; set; }
        public int Limit { get; set; }
        public string Query { get; set; }
        public int PageNumber { get; set; }
    }
    public class GetLeadAdapterJobLogDetailsListResponse : ServiceResponseBase
    {
        public IEnumerable<LeadAdapterJobLogDeailsViewModel> LeadAdapterErrorDeailsViewModel { get; set; }
        public int TotalHits { get; set; }
    }
}

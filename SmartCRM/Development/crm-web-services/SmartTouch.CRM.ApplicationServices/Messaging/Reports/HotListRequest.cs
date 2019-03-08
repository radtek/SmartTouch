using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.SearchEngine.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Reports
{
    public class HotListRequest : ServiceRequestBase
    {
        public ReportViewModel HotlistViewModel { get; set; }
        public int Limit { get; set; }
        public int PageNumber { get; set; }
        public IEnumerable<ContactFields> Fields { get; set; }
     //   public bool IsAdvancedSearch { get; set; }

    }

    public class HotListResponse : ServiceResponseBase
    {
        public SearchResult<ContactReportEntry> RunListResult { get; set; }
        public int Totalhits { get; set; }
       // public List<dynamic> TopFiveLead
        
    }
}
 
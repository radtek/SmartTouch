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
   public class StandardReportRequest:ServiceRequestBase
    {
       public ReportViewModel ReportViewModel { get; set; }
       public IEnumerable<ContactFields> Fields { get; set; }
       public int UserID { get; set; }
       public bool IsSTadmin { get; set; }
       public bool isReputationReport { get; set; }
       public IEnumerable<Type> Types { get; set; } 
    }
    public class StandardReportResponse:ServiceResponseBase
    {
       // public SearchResult<ContactListEntry> RunListResult { get; set; }
        public IEnumerable<ContactReportEntry> ReportContacts { get; set; }
        public SearchResult<ContactListEntry> ContactSearchResult { get; set; }
        public AdvancedSearchViewModel  AdvancedSearchViewModel { get; set; }
        public IEnumerable<dynamic> ReportList { get; set; }
        public long TotalHits { get; set; }
        public long PreviousLeads { get; set; }
        public List<ChartData> TopFive { get; set; }
        public List<ChartData> TopLeads { get; set; }
        public List<ChartData> TopPreviousLeads { get; set; }
        public long NewLeads { get; set; }
        public int? ReportId { get; set; }
        public List<int> ContactIds { get; set; }
        public string ContactsGuid { get; set; }
       

    }

    public class ChartData
    {
        public short DropdownValueID { get; set; }
        public string Name { get; set; }
        public long? Value { get; set; }
        public long? PreviousValue { get; set; }
    }
}

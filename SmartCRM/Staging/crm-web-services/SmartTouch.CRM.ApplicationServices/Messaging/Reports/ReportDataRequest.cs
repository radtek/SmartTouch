using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Domain.Opportunities;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.Reports;
using SmartTouch.CRM.Domain.Contacts;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Reports
{
    public class ReportDataRequest : ServiceRequestBase
    {
        public ReportViewModel ReportViewModel { get; set; }
        public bool IsSTAdmin { get; set; }
        public int UserId { get; set; }
        public bool IsDasboardView { get; set; }
        public byte ModuleId { get; set; }
    }
    public class ReportDataResponse : ServiceResponseBase
    {
        public ReportResult ReportData { get; set; }
        public IEnumerable<ReportContact> ContactsData { get; set; }
        public List<int> ContactIds { get; set; }
        public HotlistGridData HotlistGridData { get; set; }
        public int TotalHits { get; set; }
        public int? ReportId { get; set; }
        public IList<WebVisitReport> WebVisits { get; set; }
    }
}

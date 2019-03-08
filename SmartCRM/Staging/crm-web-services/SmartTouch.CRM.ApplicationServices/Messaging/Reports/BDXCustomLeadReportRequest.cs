using SmartTouch.CRM.ApplicationServices.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Reports
{
    public class BDXCustomLeadReportRequest : ServiceRequestBase
    {
        public ReportViewModel ReportViewModel { get; set; }
        public bool IsSTadmin { get; set; }
        public int UserID { get; set; }
    }

    public class BDXCustomLeadReportResponse : ServiceResponseBase
    {
        public IEnumerable<BDXLeadReportEntry> Contacts { get; set; }
        public IEnumerable<int> ContactIds { get; set; }
        public int TotalHits { get; set; }
        public byte[] byteArray { get; set; }
        public string FileName { get; set; }
    }
}

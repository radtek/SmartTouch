using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    public class ReportListEntry
    {
        public int ReportID { get; set; }
        public string ReportName { get; set; }
        public byte ReportType { get; set; }
        public int? AccountID { get; set; }
        public DateTime? LastRunOn { get; set; }
    }
    public interface IReportListViewModel
    {
        IEnumerable<ReportListEntry> Reports { get; set; }
    }
    public class ReportListViewModel : IReportListViewModel
    {
        public IEnumerable<ReportListEntry> Reports { get; set; }
        
    }
}

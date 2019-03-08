using SmartTouch.CRM.ApplicationServices.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RE = SmartTouch.CRM.Entities;
namespace SmartTouch.CRM.ApplicationServices.Messaging.Reports
{
    public class GetReportsRequest : ServiceRequestBase
    {
       // public object SortField;
        public string Name { get; set; }
        public string Filter { get; set; }
        public RE.Reports  ReportType { get; set; }
        public int PageNumber { get; set; }
        public int pageSize { get; set; }
        public string SortField { get; set; }
        public ListSortDirection SortDirection { get; set; }

       
    }

    public class GetReportsResponse : ServiceResponseBase
    {
        public ReportListViewModel ReportListViewModel { get; set; }
        public ReportListEntry Report { get; set; }
        public int TotalRecordsCount { get; set; }

    }
}

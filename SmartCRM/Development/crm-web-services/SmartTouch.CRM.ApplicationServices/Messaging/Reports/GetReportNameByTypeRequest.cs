using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Reports
{
    public class GetReportNameByTypeRequest : ServiceRequestBase
    {
        public byte ReportType { get; set; }
    }

    public class GetReportNameByTypeResponse : ServiceResponseBase
    {
        public string ReportName { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Reports
{
    public class ReportExportRequest : ServiceRequestBase
    {
        public DateTime CustomStartDate { get; set; }
        public DateTime CustomEndDate { get; set; }
        public bool IsSTadmin { get; set; }
        public bool isReputationReport { get; set; }
    }
    public class ReportExportResponce : ServiceResponseBase
    {
        public string fileKey { get; set; }
        public string fileName { get; set; }
        public byte[] fileContent { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Reports
{
    public class CustomReportDataRequest : ServiceRequestBase
    {
    }
    public class CustomReportDataResponse : ServiceResponseBase
    {
        public bool hasCustomReports { get; set; }
    }

}

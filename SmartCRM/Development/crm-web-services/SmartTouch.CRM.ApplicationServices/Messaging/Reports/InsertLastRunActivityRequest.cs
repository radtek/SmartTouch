using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Reports
{
    public class InsertLastRunActivityRequest : ServiceRequestBase
    {
        public int ReportId { get; set; }
        public string ReportName { get; set; }
    }

    public class InsertLastRunActivityResponse : ServiceResponseBase
    { 
    
    }
}

using SmartTouch.CRM.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.WebAnalytics
{
    public class InsertWebVisitNotificationAuditRequest : ServiceRequestBase
    {
        public WebVisitEmailAudit EmailAudit { get; set; }
    }

    public class InsertWebVisitNotificationAuditResponse : ServiceResponseBase
    {

    }
}

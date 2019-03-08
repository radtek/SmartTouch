using SmartTouch.CRM.Domain.WebAnalytics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.WebAnalytics
{
    public class GetWebVisitProviderByAccountIdRequest : ServiceRequestBase
    {

    }
    public class GetWebVisitProviderByAccountIdResponse : ServiceResponseBase
    {
        public WebAnalyticsProvider Provider { get; set; }
    }
}

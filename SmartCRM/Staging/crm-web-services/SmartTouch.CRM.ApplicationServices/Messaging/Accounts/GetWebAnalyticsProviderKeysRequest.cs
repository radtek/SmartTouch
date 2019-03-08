using SmartTouch.CRM.Domain.WebAnalytics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Accounts
{
    public class GetWebAnalyticsProvidersRequest : ServiceRequestBase
    {
    }

    public class GetWebAnalyticsProvidersResponse : ServiceResponseBase
    {
        public IList<WebAnalyticsProvider> WebAnalyticsProviders { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.WebAnalytics
{
    public class UpdateWebVisitNotificationsRequest : ServiceRequestBase
    {
        public IEnumerable<KeyValuePair<IEnumerable<string>, string>> VisitReferences { get; set; }

    }

    public class UpdateWebVisitNotificationsResponse : ServiceResponseBase
    {
 
    }
}

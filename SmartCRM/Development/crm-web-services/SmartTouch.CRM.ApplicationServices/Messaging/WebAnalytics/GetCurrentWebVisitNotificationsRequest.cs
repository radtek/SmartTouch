using SmartTouch.CRM.Domain;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.Reports;
using SmartTouch.CRM.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.WebAnalytics
{
    public class GetCurrentWebVisitNotificationsRequest : ServiceRequestBase
    {
    }

    public class GetCurrentWebVisitNotificationsResponse : ServiceResponseBase
    {
        public IEnumerable<WebVisitReport> CurrentVisits { get; set; }
    }
}

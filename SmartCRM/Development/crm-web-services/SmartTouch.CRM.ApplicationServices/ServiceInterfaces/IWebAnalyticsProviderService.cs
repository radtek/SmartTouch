using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.ApplicationServices.Messaging.WebAnalytics;
using SmartTouch.CRM.ApplicationServices.Messaging;

namespace SmartTouch.CRM.ApplicationServices.ServiceInterfaces
{
    public interface IWebAnalyticsProviderService
    {
        CompareKnownContactIdentitiesResponse CompareKnownIps(CompareKnownContactIdentitiesRequest request);
        AddContactWebVisitResponse AddContactWebVisits(AddContactWebVisitRequest request);
        ValidateVisiStatKeyResponse ValidateVisiStatKey(ValidateVisiStatKeyRequest request);
        ReIndexWebVisitsResponse ReIndexWebVisits(ReIndexWebVisitsRequest request);
        GetCurrentWebVisitNotificationsResponse GetCurrentWebVistNotifications(GetCurrentWebVisitNotificationsRequest request);
        UpdateWebVisitNotificationsResponse UpdateWebVisitNotifications(UpdateWebVisitNotificationsRequest request);
        GetAccountIdsToSendWebVisitDailySummaryResponse GetAccountIdsToSendWebVisitDailySummary(GetAccountIdsToSendWebVisitDailySummaryRequest request);
        GetWebVisitDailySummaryResponse GetWebVisitDailySummary(GetWebVisitDailySummaryRequest request);
    }
}
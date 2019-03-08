using SmartTouch.CRM.Domain.Accounts;
using SmartTouch.CRM.Domain.Reports;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.WebAnalytics
{
    public interface IWebAnalyticsProviderRepository : IRepository<WebAnalyticsProvider, int>
    {

        List<WebVisit> AddWebVisists(IList<WebVisit> contactWebVisits, WebAnalyticsProvider webAnalyticsProviderId, DateTime lastAPICallTimeStamp, short splitVisitInterval);
        IEnumerable<WebVisit> FindWebvisitsByAccount(int accountId);
        IEnumerable<WebVisit> FindAllWebVisits(int pageNumber, int limit, int accountId);
        IEnumerable<Notification> FindWebVisitsByOwner(int pageNumber, int limit, int accountId, int ownerId, bool FindWebVisitsByOwner);
        IEnumerable<WebVisit> GetWebVisitToBeEmailed();
        void UpdateWebVisitNotifications(IEnumerable<KeyValuePair<IEnumerable<string>, string>> VisitReferences);
        IEnumerable<AccountBasicInfo> GetAccountIdsToSendWebVisitDailySummary();
        IEnumerable<WebVisitReport> GetWebVisitDailySummary(int accountId, DateTime startDate, DateTime endDate);
        IEnumerable<WebVisitReport> GetCurrentWebVisits();
        WebVisitReport GetWebVisitreportByWebVisitId(int webVisitId);
    }
}

using System;
using System.Collections.Generic;
using SmartTouch.CRM.JobProcessor.Services.Implementation;

namespace SmartTouch.CRM.JobProcessor.Services
{
    public interface IKickfireService
    {
        IEnumerable<PartnerInfo> GetPartnerInfos();
        IEnumerable<UniqueVisitor> GetUniqueVisitors(string kickFireClientApiKey, DateTime startDate, DateTime endDate);
        IEnumerable<PageVisitInfo> GetPageVisitors(string kickFireClientApiKey, string identity, DateTime startDate, DateTime endDate);
        IEnumerable<PageVisitInfo> GetPageVisitors(string kickFireClientApiKey, string[] identities, DateTime startDate, DateTime endDate);
    }
}

using SmartTouch.CRM.Domain.Accounts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.WebAnalytics
{
    public class GetAccountIdsToSendWebVisitDailySummaryRequest : ServiceRequestBase
    {
    }

    public class GetAccountIdsToSendWebVisitDailySummaryResponse : ServiceResponseBase
    {
        public IEnumerable<AccountBasicInfo> AccountsBasicInfo { get; set; }
    }
}

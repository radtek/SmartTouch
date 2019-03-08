using SmartTouch.CRM.Domain.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.WebAnalytics
{
    public class GetUsersOptedWebVisitSummaryEmailRequest : ServiceRequestBase
    {
    }

    public class GetUsersOptedWebVisitSummaryEmailResponse : ServiceResponseBase
    {
        public IEnumerable<UserBasicInfo> UsersOpted { get; set; }
        public IEnumerable<UserBasicInfo> AllUsers { get; set; }
    }
}

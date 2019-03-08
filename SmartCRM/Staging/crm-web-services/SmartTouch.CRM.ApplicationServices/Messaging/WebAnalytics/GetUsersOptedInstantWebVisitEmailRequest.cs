using SmartTouch.CRM.Domain.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.WebAnalytics
{
    public class GetUsersOptedInstantWebVisitEmailRequest : ServiceRequestBase
    {
    }

    public class GetUsersOptedInstantWebVisitEmailResponse : ServiceResponseBase
    {
        public IEnumerable<UserBasicInfo> Users { get; set; }
    }
}

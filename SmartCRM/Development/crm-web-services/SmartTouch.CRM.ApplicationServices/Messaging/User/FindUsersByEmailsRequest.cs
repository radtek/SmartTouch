using SmartTouch.CRM.Domain.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.User
{
    public class FindUsersByEmailsRequest : ServiceRequestBase
    {
        public IEnumerable<string> UserEmails { get; set; }
    }

    public class FindUsersByEmailsResponse : ServiceResponseBase
    {
        public IEnumerable<UserBasicInfo> Users { get; set; }
    }
}

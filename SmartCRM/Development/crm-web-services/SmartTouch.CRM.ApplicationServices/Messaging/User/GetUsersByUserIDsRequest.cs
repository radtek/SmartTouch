using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using U = SmartTouch.CRM.Domain.Users;

namespace SmartTouch.CRM.ApplicationServices.Messaging.User
{
    public class GetUsersByUserIDsRequest : ServiceRequestBase
    {
        public IEnumerable<int?> UserIDs { get; set; } // Need to change it to int from int?
    }

    public class GetUsersByUserIDsResponse : ServiceResponseBase
    {
        public IEnumerable<U.UserBasicInfo> Users { get; set; }
    }
}

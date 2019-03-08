using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SmartTouch.CRM.Infrastructure;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Accounts
{
    public class GetUsersRequest : ServiceRequestBase
    {
        public string Query { get; set; }
    }

    public class GetUsersResponse : ServiceResponseBase
    {
        public IEnumerable<dynamic> Users { get; set; }
    }
}

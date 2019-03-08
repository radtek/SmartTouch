using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Accounts
{
    public class GetAccountPermissionsRequest : ServiceRequestBase
    {

    }

    public class GetAccountPermissionsResponse : ServiceResponseBase
    {
        public IEnumerable<byte> ModuleIds { get; set; }
    }
}

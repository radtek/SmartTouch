using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Accounts
{
    public class GetGoogleDriveAPIKeyRequest : ServiceRequestBase
    {
    }

    public class GetGoogleDriveAPIKeyResponse : ServiceResponseBase
    {
        public string ClientID { get; set; }
        public string APIKey { get; set; }
    }
}

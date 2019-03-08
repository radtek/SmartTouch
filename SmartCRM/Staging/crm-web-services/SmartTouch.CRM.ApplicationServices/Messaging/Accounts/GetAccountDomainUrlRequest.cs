using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Accounts
{
    public class GetAccountDomainUrlRequest:ServiceRequestBase
    {

    }
    public class GetAccountDomainUrlResponse : ServiceResponseBase
    {
        public string DomainUrl { get; set; }
    }

    
}

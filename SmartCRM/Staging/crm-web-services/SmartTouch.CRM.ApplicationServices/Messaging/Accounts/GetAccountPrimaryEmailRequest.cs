using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Accounts
{
    public class GetAccountPrimaryEmailRequest:ServiceRequestBase
    {
    }

    public class GetAccountPrimaryEmailResponse: ServiceResponseBase
    {
        public string PrimaryEmail { get; set; }
    }
}

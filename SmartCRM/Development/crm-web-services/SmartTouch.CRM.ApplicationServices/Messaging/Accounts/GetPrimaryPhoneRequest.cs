using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Accounts
{
    public class GetPrimaryPhoneRequest : ServiceRequestBase
    {

    }

    public class GetPrimaryPhoneResponse : ServiceResponseBase
    {
        public string PrimaryPhone { get; set; } 
    }
}

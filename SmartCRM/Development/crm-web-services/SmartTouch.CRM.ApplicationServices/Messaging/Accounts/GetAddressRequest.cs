using SmartTouch.CRM.ApplicationServices.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Accounts
{
    public class GetAddressRequest : ServiceRequestBase
    {

    }

    public class GetAddressResponse : ServiceResponseBase
    {
        public string Address { get; set; }
        public string  Location { get; set; }
    }
}

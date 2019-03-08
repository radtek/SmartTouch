using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Domain.ValueObjects;

namespace SmartTouch.CRM.ApplicationServices.Messaging.User
{
    public class GetAccountAddressRequest : ServiceRequestBase
    {

    }

    public class GetAccountAddressResponse : ServiceRequestBase
    {
        public Address Address { get; set; }
    }
}

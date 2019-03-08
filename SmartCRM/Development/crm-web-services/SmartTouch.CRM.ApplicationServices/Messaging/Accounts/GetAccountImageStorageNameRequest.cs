using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Domain.ValueObjects;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Accounts
{
    public class GetAccountImageStorageNameRequest : ServiceRequestBase
    {
    }

    public class GetAccountImageStorageNameResponse : ServiceResponseBase
    {

        public AccountLogoInfo AccountLogoInfo { get; set; }  
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SmartTouch.CRM.ApplicationServices.Messaging;
using SmartTouch.CRM.ApplicationServices.ViewModels;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Accounts
{
    public class InsertAccountRequest : ServiceRequestBase
    {
        public AccountViewModel AccountViewModel { get; set; }
        public string SSLKey { get; set; }
    }

    public class InsertAccountResponse : ServiceResponseBase
    {
        public virtual AccountViewModel AccountViewModel { get; set; }
    }
}

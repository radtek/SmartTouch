using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SmartTouch.CRM.ApplicationServices.Messaging;
using SmartTouch.CRM.ApplicationServices.ViewModels;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Accounts
{
    public class UpdateAccountRequest : ServiceRequestBase
    {
        public AccountViewModel AccountViewModel { get; set; }
        public bool IsSettingsUpdate { get; set; }
    }

    public class UpdateAccountResponse : ServiceResponseBase
    {
        public AccountViewModel AccountViewModel { get; set; }
    }
}

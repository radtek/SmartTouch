using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SmartTouch.CRM.ApplicationServices.ViewModels;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Accounts
{
    public class GetAccountsListRequest : ServiceRequestBase
    {
        public string Name { get; set; }
    }

    public class GetAccountsListResponse : ServiceResponseBase
    {
        public IEnumerable<AccountViewModel> Accounts { get; set; }
    }
}

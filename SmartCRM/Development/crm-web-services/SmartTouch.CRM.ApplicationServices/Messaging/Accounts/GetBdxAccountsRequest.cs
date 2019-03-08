using SmartTouch.CRM.Domain.Accounts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Accounts
{
    public class GetBdxAccountsRequest : ServiceRequestBase
    {
        public string AccountName { get; set; }
    }

    public class GetBdxAccountsResponse : ServiceResponseBase
    {
        public List<BdxAccounts> BdxAccounts { get; set; }
      
    }
}

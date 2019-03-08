using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Accounts
{
    public class AccountSubscriptionData
    {
        public int ModuleID { get; set; }
        public int AccountID { get; set; }
        public int? Limit { get; set; }
        public int SubscriptionID { get; set; }
        public string AccountUrl { get; set; }
    }
}

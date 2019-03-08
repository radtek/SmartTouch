using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Accounts
{
    public class SubscriptionSettings
    {
        public byte SubscriptionId { get; set; }
        public string Value { get; set; }
        public SubscriptionSettingTypes SubscriptionSettingType { get; set; }

    }
}

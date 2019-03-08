using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
    public class SubscriptionModuleMapDb
    {
        [Key]
        public int SubscriptionModuleMapID { get; set; }

        [ForeignKey("Subscription")]
        public byte SubscriptionID { get; set; }
        public SubscriptionsDb Subscription { get; set; }

        [ForeignKey("Module")]
        public byte ModuleID { get; set; }
        public ModulesDb Module { get; set; }

        [ForeignKey("Account")]
        public int? AccountID { get; set; }
        public AccountsDb Account { get; set; }

        public int? Limit { get; set; }
        public string ExcludedRoles { get; set; }
    }
}

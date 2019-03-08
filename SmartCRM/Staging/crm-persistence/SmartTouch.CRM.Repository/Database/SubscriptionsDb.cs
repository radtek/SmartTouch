using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
    public class SubscriptionsDb
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public byte SubscriptionID { get; set; }
        public string SubscriptionName { get; set; }

        public virtual ICollection<SubscriptionModuleMapDb> SubscriptionModules { get; set; }        
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTouch.CRM.Repository.Database
{
    public class RolesDb
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public short RoleID { get; set; }
        public string RoleName { get; set; }
        public int? AccountID { get; set; }
        [ForeignKey("Subscriptions")]
        public byte? SubscriptionID { get; set; }
        public SubscriptionsDb Subscriptions { get; set; }
        public bool IsDeleted { get; set; }        
    }
}

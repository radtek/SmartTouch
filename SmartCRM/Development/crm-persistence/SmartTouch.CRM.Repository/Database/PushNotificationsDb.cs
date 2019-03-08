using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
    public class PushNotificationsBb
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PushNotificationID { get; set; }
        public string Device { get; set; }
        public string SubscriptionID { get; set; }
        public int AccountID { get; set; }
        public int UserId { get; set; }
        public bool Allow { get; set; }
        public DateTime CreatedDate { get; set; }


    }
}

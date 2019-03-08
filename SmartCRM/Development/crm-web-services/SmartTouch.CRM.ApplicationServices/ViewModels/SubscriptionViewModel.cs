using SmartTouch.CRM.Domain.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
   public class SubscriptionViewModel
    {
        public int SubscriptionID { get; set; }
        public string SubscriptionName { get; set; }
        public ICollection<Module> Modules { get; set; }
        
    }
}

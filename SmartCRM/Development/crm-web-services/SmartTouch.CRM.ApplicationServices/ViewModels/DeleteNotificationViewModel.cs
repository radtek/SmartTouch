using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    public class DeleteNotificationViewModel 
    {
       //public bool IsActionType {get;set;}
       //public bool IsTourType {get;set;}
       public IEnumerable<int> NotificationIds { get; set; }
       public bool IsBulkDelete { get; set; }

       //In case of bulk remove
       public byte ModuleId { get; set; }
       public bool ArePreviousNotifications { get; set; }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.ApplicationServices.ViewModels;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Contacts
{
   public class SendEmailRequest: ServiceRequestBase
    {
       public int UserID { get; set; }
    }
   public class SendEmailResponse : ServiceResponseBase
   {
       public IEnumerable<SendMailViewModel> SendMailViewModels { get; set; } 
   }
}

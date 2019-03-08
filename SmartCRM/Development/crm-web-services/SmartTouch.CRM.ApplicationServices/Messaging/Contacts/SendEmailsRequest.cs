using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.ApplicationServices.ViewModels;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Contacts
{
   public class SendEmailsRequest : ServiceRequestBase
    {
       public SendMailViewModel SendMailsViewModel { get; set; }
    }
   public class SendEmailsRespone : ServiceRequestBase
   {
       public SendMailViewModel SendMailsViewModel { get; set; }
   }
}

using SmartTouch.CRM.ApplicationServices.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Communication
{
    public class SendTextRequest:ServiceRequestBase
    {
        public int UserId { get; set; }
        public SendTextViewModel SendTextViewModel { get; set; }
    }
   public class SendTextResponse : ServiceResponseBase
   {
       public SendTextViewModel SendTextViewModel { get; set; }
       public string SMSStatus { get; set; }
       public string Message { get; set; }
   }
}

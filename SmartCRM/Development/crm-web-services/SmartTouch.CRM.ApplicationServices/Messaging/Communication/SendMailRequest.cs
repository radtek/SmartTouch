using SmartTouch.CRM.ApplicationServices.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Entities;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Communication
{
    public  class SendMailRequest:ServiceRequestBase
    {
        public SendMailViewModel SendMailViewModel { get; set; }
        public string UserName { get; set; }
        public string AccountDomain { get; set; }
    }
    public class SendMailResponse : ServiceResponseBase
    {
        public CommunicationStatus ResponseStatus { get; set; }    //Need to rename this to ResponseStatus
        public string ResponseMessage { get; set; }             //Need to rename this to ResponseMessage
        public SendMailViewModel SendMailViewModel { get; set; }        
    }
}

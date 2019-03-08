using SmartTouch.CRM.ApplicationServices.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Communication
{
    public class ScheduleMailRequest : ServiceRequestBase
    {
        public int ContactId { get; set; }
        public SendMailViewModel SendMailViewModel { get; set; }
    }

    public class ScheduleMailResponse : ServiceResponseBase
    {

    }
}

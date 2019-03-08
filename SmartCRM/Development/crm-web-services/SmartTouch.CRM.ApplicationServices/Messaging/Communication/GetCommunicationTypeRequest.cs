using SmartTouch.CRM.ApplicationServices.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Communication
{
   public class GetCommunicationTypeRequest:ServiceRequestBase
    {

    }
   public class GetCommunicationTypeResponse : ServiceResponseBase
   {
       public IEnumerable<CommunicationTypeViewModel> CommunicationTypeViewModel { get; set; }
   }
}

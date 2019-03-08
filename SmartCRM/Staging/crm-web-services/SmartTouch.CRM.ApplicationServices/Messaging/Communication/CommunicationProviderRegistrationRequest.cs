using SmartTouch.CRM.ApplicationServices.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Communication
{
   public class CommunicationProviderRegistrationRequest:ServiceRequestBase
    {
    
       public IEnumerable<ProviderRegistrationViewModel> ProviderRegistrationViewModels { get; set; } 
    }
    public class CommunicationProviderRegistrationResponse:ServiceResponseBase
    {
        public string Message { get; set; }
    }
}

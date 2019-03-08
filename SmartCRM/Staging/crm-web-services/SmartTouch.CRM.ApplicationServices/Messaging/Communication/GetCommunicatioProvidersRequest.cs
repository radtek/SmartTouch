using SmartTouch.CRM.ApplicationServices.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Communication
{
   public class GetCommunicatioProvidersRequest:ServiceRequestBase
    {
       public bool FromCache { get; set; }
    }
   public class GetCommunicatioProvidersResponse:ServiceResponseBase
   {
       public IEnumerable<ProviderRegistrationViewModel> RegistrationListViewModel { get; set; }
       public IEnumerable<ProviderViewModel> campaignProviderViewModel { get; set; }
   }
}

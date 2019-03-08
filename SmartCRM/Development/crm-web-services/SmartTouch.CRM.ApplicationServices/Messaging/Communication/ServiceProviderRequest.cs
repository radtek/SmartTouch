using SmartTouch.CRM.ApplicationServices.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Communication
{
   public class ServiceProviderRequest:ServiceRequestBase
   {
       public ServiceProviderViewModel ServiceProviderViewModel { get; set; }
   }
   public class ServiceProviderResponse : ServiceResponseBase
   {
       public ServiceProviderViewModel ServiceProviderViewModel { get; set; }
   }
}

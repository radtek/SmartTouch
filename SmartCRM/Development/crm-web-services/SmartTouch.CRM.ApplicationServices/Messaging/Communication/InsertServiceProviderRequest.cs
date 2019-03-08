using SmartTouch.CRM.ApplicationServices.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Communication
{
    public class InsertServiceProviderRequest : ServiceRequestBase
    {
        public ProviderRegistrationViewModel ProviderViewModel { get; set; }
        public string Url { get; set; }
    }
    public class InsertServiceProviderResponse: ServiceResponseBase
    {
        public ProviderRegistrationViewModel ProviderViewModel { get; set; }
    }
}

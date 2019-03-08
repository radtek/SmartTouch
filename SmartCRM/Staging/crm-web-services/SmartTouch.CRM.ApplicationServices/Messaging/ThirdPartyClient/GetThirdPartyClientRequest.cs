using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.ApplicationServices.Messaging;

namespace SmartTouch.CRM.ApplicationServices.Messaging.ThirdPartyClient
{
    public class GetThirdPartyClientRequest : ServiceRequestBase
    {
        public string Filter { get; set; }
        public string Name { get; set; }
    }

    public class GetThirdPartyClientResponse : ServiceResponseBase
    {
        public IEnumerable<ThirdPartyClientViewModel> ThirdPartyClientViewModel { get; set; }
    }

   
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.ApplicationServices.ViewModels;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Communication
{
    public class GetDefaultCampaignEmailProviderRequest : ServiceRequestBase
    {
    }

    public class GetDefaultCampaignEmailProviderResponse : ServiceResponseBase
    {
        public ServiceProviderViewModel CampaignEmailProvider { get; set; }
    }
}

﻿using SmartTouch.CRM.ApplicationServices.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Communication
{
    public class GetServiceProviderByIdRequest : ServiceRequestBase
    {
        public int ServiceProviderId { get; set; }
    }

    public class GetServiceProviderByIdResponse : ServiceResponseBase
    {
        public ServiceProviderViewModel CampaignEmailProvider { get; set; }
    }
}

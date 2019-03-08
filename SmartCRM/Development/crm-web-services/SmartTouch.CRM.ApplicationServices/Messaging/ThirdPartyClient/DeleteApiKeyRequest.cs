﻿using SmartTouch.CRM.ApplicationServices.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.ThirdPartyClient
{
   public class DeleteApiKeyRequest: ServiceRequestBase
    {
       public ThirdPartyClientViewModel ThirdPartyClientViewModel { get; set; }
    }

    public class DeleteApiKeyResponse: ServiceResponseBase
    {

    }
}

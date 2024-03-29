﻿using SmartTouch.CRM.ApplicationServices.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Campaigns
{
    public class InsertCampaignRequest : ServiceRequestBase
    {
        public CampaignViewModel CampaignViewModel { get; set; }
    }

    public class InsertCampaignResponse : ServiceResponseBase
    {
        public CampaignViewModel CampaignViewModel { get; set; }
    }
}

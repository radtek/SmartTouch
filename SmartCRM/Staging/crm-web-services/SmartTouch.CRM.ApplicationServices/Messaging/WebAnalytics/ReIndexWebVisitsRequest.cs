﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.WebAnalytics
{
    public class ReIndexWebVisitsRequest : ServiceRequestBase
    {
    }

    public class ReIndexWebVisitsResponse : ServiceResponseBase
    {
        public int IndexedWebVisits { get; set; }
    }
}

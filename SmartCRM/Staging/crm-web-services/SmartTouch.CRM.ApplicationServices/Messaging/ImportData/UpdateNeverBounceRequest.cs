﻿using SmartTouch.CRM.Domain.ImportData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.ImportData
{
    public class UpdateNeverBounceRequest : ServiceRequestBase
    {
        public NeverBounceRequest Request { get; set; }
    }

    public class UpdateNeverBounceResponse : ServiceResponseBase
    {

    }
}
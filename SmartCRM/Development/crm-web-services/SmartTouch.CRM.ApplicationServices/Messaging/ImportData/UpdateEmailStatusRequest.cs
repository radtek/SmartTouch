﻿using SmartTouch.CRM.Domain.ImportData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.ImportData
{
    public class UpdateEmailStatusRequest : ServiceRequestBase
    {
        public IEnumerable<NeverBounceResult> Results { get; set; }
    }

    public class UpdateEmailStatusResponse : ServiceResponseBase
    {

    }
}

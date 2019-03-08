﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.ApplicationServices.ViewModels;

namespace SmartTouch.CRM.ApplicationServices.Messaging.CustomFields
{
    public class GetCustomFieldsRequest : IntegerIdRequest
    {
        public GetCustomFieldsRequest(int id) : base(id) { }
    }

    public class GetCustomFieldsResponse : ServiceResponseBase
    {
        public CustomFieldTabsViewModel FormViewModel { get; set; }
    }
}

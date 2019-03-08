﻿using SmartTouch.CRM.ApplicationServices.Messaging.Subscriptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ServiceInterfaces
{
    public interface ISubscriptionService
    {
        GetAllAccountSubscriptionTypesResponse GetAllAccountsSubscriptionTypes(GetAllAccountSubscriptionTypesRequest request);
    }
}

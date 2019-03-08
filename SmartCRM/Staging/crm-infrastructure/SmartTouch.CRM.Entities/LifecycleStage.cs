﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Entities
{
    public enum LifecycleStage : short
    {
        Lead = 1,
        Subscriber = 2,
        Prospect = 3,
        Customer = 4,
        Partner = 5
    }    
}
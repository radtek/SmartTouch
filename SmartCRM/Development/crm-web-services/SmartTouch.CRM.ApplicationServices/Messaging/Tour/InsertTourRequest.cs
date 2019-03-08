﻿using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Tour
{
    public class InsertTourRequest : ServiceRequestBase
    {
        public TourViewModel TourViewModel { get; set; }
        public string AccountPrimaryEmail { get; set; }
        public string AccountAddress { get; set; }
        public string Location { get; set; }
        public string AccountPhoneNumber { get; set; }
        public string AccountDomain { get; set; }     
    }

    public class InsertTourResponse : ServiceResponseBase
    {
        //why do we have virtual. Check out
        public virtual TourViewModel TourViewModel { get; set; }
    }
}
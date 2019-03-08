using SmartTouch.CRM.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.LeadAdapters
{
    public class InsertFacebookLeadGenRequest : ServiceRequestBase
    {
        public FacebookLeadGen FacebookLeadGen { get; set; } 
    }

    public class InsertFacebookLeadGenResponse : ServiceResponseBase
    {

    }
}

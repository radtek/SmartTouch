using SmartTouch.CRM.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.LeadAdapters
{
    public class GetFacebookLeadGensRequest : ServiceRequestBase
    {
        public int AccountMapID { get; set; }
    }

    public class GetFacebookLeadGensResponse : ServiceResponseBase
    {
        public IEnumerable<FacebookLeadGen> FacebookLeadGens { get; set; }
    }
}

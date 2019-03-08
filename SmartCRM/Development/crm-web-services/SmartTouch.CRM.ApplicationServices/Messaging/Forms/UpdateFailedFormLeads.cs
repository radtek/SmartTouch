using SmartTouch.CRM.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Forms
{
    public class UpdateFailedFormLeadsRequest : ServiceRequestBase
    {
        public ApproveLeadsQueue Queue { get; set; }
    }

    public class UpdateFailedFromLeadsResponse : ServiceResponseBase
    { 
    
    }
}

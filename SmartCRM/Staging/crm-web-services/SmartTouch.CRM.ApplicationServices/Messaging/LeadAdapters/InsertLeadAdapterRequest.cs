using SmartTouch.CRM.ApplicationServices.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.LeadAdapters
{
    public class InsertLeadAdapterRequest : ServiceRequestBase
    {
        public LeadAdapterViewModel LeadAdapterViewModel { get; set; }
    }

    public class InsertLeadAdapterResponse : ServiceResponseBase
    {
        public virtual LeadAdapterViewModel LeadAdapterViewModel { get; set; }
    }
}

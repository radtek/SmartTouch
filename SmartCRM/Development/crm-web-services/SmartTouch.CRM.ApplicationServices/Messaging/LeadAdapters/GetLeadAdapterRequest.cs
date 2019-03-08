using SmartTouch.CRM.ApplicationServices.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.LeadAdapters
{
    public class GetLeadAdapterRequest : IntegerIdRequest
    {
        public GetLeadAdapterRequest(int id) : base(id) { }
    }
    public class GetLeadAdapterResponse : ServiceResponseBase
    {
        public LeadAdapterViewModel LeadAdapterViewModel { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.LeadAdapters
{
    public class DeleteLeadAdapterRequest : IntegerIdRequest
    {
        public DeleteLeadAdapterRequest(int id) : base(id) { }
    }
    public class DeleteLeadAdapterResponse : ServiceResponseBase
    {

    }
}

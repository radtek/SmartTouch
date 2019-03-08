using SmartTouch.CRM.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.User
{
    public class GetUserActivityDetailRequest : ServiceRequestBase
    {
        public byte ModuleId { get; set; }
        public IEnumerable<int> EntityIds { get; set; }
    }
    public class GetUserActivityDetailResponse : ServiceResponseBase
    {
        public IEnumerable<UserActivityEntityDetail> EntityDetails { get; set; }
    }
}

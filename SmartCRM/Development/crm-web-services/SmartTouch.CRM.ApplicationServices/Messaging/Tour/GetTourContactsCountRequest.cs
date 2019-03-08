using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Tour
{
    public class GetTourContactsCountRequest : ServiceRequestBase
    {
        public int TourId { get; set; }
    }

    public class GetTourContactsCountResponse : ServiceResponseBase
    {
        public int Count { get; set; }
        public bool SelectAll { get; set; }
    }

    public class GetContactTourIsCreatedRequest : ServiceRequestBase
    {
        public int ContactId { get; set; }
    }

    public class GetContactTourIsCreatedResponse : ServiceResponseBase
    {
        public short[] ContactCommunities { get; set; }
    }
}

using SmartTouch.CRM.ApplicationServices.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Tour
{
    public class GetTourRequest : IntegerIdRequest
    {
        public GetTourRequest(int id) : base(id) { }
        public int LoginUserId { get; set; }
    }

    public class GetTourResponse : ServiceResponseBase
    {
        public TourViewModel TourViewModel { get; set; }
    }

    public class GetContactTourMapRequest
    {
        public GetContactTourMapRequest(int tourId, int contactId) { }
    }

    public class GetContactTourMapResponse : ServiceResponseBase
    {
        public int ContactTourMapResponseId { get; set; }
    }

}

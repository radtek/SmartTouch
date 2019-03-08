using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.User
{
    public class GetUserCalenderRequest : ServiceRequestBase
    {
        public string UserTimeZone { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class GetUserCalenderResponse : ServiceResponseBase
    {
        public IEnumerable<CalenderTimeSlotViewModel> CalenderSlots { get; set; }

    }
}

using SmartTouch.CRM.ApplicationServices.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Tour
{
    public class DeleteTourRequest : ServiceRequestBase
    {
        public int TourId { get; set; }
    }
    public class DeleteTourResponse : ServiceResponseBase
    {
    }
}
 
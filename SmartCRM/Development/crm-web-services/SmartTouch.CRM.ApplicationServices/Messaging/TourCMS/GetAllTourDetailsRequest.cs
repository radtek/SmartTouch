using SmartTouch.CRM.ApplicationServices.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.TourCMS
{
    public class GetAllTourDetailsRequest : ServiceRequestBase
    {

    }

    public class GetAllTourDetailsResponse : ServiceResponseBase
    {
        public IEnumerable<ApplicationTourViewModel> ApplicationTours { get; set; }
    }
}

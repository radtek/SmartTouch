using SmartTouch.CRM.ApplicationServices.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.TourCMS
{
    public class UpdateTourCMSRequest : ServiceRequestBase
    {
        public ApplicationTourViewModel ViewModel { get; set; }
    }

    public class UpdateTourCMSResponse : ServiceResponseBase
    {

    }
}

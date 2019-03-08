using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.TourCMS
{
    public class UpdateApplicationTourRequest : ServiceRequestBase
    {
        public int ApplicationTourId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
    }

    public class UpdateApplicationTourResponse : ServiceResponseBase
    {

    }
}

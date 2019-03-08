using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.TourCMS
{
    public class UpdateTourVisitRequest : ServiceRequestBase
    {
        public int UserId { get; set; }
    }

    public class UpdateTourVisitResponse : ServiceResponseBase
    {

    }
}

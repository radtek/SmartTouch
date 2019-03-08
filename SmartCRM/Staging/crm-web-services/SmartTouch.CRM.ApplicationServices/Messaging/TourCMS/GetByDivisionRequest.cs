using SmartTouch.CRM.ApplicationServices.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.TourCMS
{
    public class GetByDivisionRequest : ServiceRequestBase
    {
        public int DivisionId { get; set; }
    }

    public class GetByDivisionResponse : ServiceResponseBase
    {
        public IEnumerable<ApplicationTourViewModel> AppTourViewModel { get; set; }
    }
}

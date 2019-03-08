using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.ApplicationServices.ViewModels;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Tour
{
    public class GetTourListRequest : ServiceRequestBase
    {
        public int Id { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }

    public class GetTourListResponse : ServiceResponseBase
    {
        public IEnumerable<TourViewModel> ToursListViewModel { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.ApplicationServices.ViewModels;

namespace SmartTouch.CRM.ApplicationServices.Messaging.User
{
    public class GetUserActivitiesRequest : ServiceRequestBase
    {
        public int UserId { get; set; }
        public int PageNumber { get; set; }
        public string DateFormat { get; set; }
        public int[] ModuleIds { get; set; }
    }

    public class GetUserActivitiesResponse : ServiceResponseBase
    {
        public UserActivitiesListViewModel UserActivities { get; set; }
        public IEnumerable<byte> UserModules { get; set; }
    }

    public class GetUserActivitiesListRequest : ServiceRequestBase
    {
        public HotListViewModel HotlistViewModel { get; set; }
    }

   
}

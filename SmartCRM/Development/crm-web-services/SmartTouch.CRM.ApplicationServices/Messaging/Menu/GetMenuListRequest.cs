using SmartTouch.CRM.ApplicationServices.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Menu
{
    public class GetMenuListRequest : ServiceRequestBase
    {
        public string Name { get; set; }
    }

    public class GetMenuListResponse : ServiceResponseBase
    {
        public IMenuListViewModel MenuListViewModel { get; set; }
    }
}

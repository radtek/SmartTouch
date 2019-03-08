using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Entities;

namespace SmartTouch.CRM.ApplicationServices.Messaging.User
{
    public class GetRecentViwedContactsRequest : ServiceRequestBase
    {
        public int UserId { get; set; }
        public AppModules ModuleName { get; set; }
        public UserActivityType ActivityName { get; set; }
        public string sort { get; set; }
        public int[] ContactIDs { get; set; }
    }

    public class GetRecentViwedContactsResponse : ServiceResponseBase
    {
        public IList<int> ContactIdList { get; set; }
        public string SortedValue { get; set; }
    }
}

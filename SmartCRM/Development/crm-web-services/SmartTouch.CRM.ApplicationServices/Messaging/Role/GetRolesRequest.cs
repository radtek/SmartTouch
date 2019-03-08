using SmartTouch.CRM.ApplicationServices.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Role
{
    public class GetRolesRequest : ServiceRequestBase
    {
        public int Limit { get; set; }
        public string Query { get; set; }
        public int PageNumber { get; set; }
        public int AccountID { get; set; }
    }

    public class GetRolesResponse : ServiceResponseBase
    {
        public int TotalHits { get; set; }
        public IEnumerable<RoleViewModel> RoleViewModel { get; set; }
        public byte SubscriptionId { get; set; }
    }
}

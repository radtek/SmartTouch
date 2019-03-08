using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Entities;

namespace SmartTouch.CRM.ApplicationServices.Messaging.User
{
    public class UserReadActivityRequest : ServiceRequestBase
    {
        public int EntityId { get; set; }
        public string EntityName { get; set; }
        public int UserId { get; set; }
        public UserActivityType ActivityName { get; set; }
        public AppModules ModuleName { get; set; }
    }

    public class UserReadActivityResponse : ServiceResponseBase
    {

    }
}

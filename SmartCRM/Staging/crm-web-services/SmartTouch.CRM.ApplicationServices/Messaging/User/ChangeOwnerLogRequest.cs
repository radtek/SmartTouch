using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.User
{
    public class ChangeOwnerLogRequest : ServiceRequestBase
    {
        public int EntityId { get; set; }
        public int? UserId { get; set; }
        public UserActivityType ActivityName { get; set; }
        public AppModules ModuleName { get; set; }
    }

    public class ChangeOwnerLogResponse : ServiceResponseBase
    { 
    
    }
}

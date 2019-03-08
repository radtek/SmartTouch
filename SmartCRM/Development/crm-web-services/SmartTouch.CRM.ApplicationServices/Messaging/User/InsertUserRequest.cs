using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.ApplicationServices.ViewModels;

namespace SmartTouch.CRM.ApplicationServices.Messaging.User
{
    public class InsertUserRequest : ServiceRequestBase
    {
        public UserViewModel UserViewModel { get; set; }
    }

    public class InsertUserResponse : ServiceResponseBase
    {
        public virtual UserViewModel UserViewModel { get; set; }
    }

    public class InsertUserPasswordRequest : ServiceRequestBase
    {
        public int[] UserIDs { get; set; }
        public string Password { get; set; }
    }

    public class InsertUserPasswordResponse : ServiceResponseBase
    {

    }
}

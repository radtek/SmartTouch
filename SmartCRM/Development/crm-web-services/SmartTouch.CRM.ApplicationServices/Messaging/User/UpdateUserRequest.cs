using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.ApplicationServices.ViewModels;

namespace SmartTouch.CRM.ApplicationServices.Messaging.User
{
    public class UpdateUserRequest : ServiceRequestBase
    {
        public UserViewModel UserViewModel { get; set; }
    }

    public class UpdateUserResponse : ServiceResponseBase
    {
        public virtual UserViewModel UserViewModel { get; set; }
        public string ErrorMessage { get; set; }
    }
}

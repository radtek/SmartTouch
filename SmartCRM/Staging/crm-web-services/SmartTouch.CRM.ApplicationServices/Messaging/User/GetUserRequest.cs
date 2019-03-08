using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.ApplicationServices.ViewModels;

namespace SmartTouch.CRM.ApplicationServices.Messaging.User
{
    public class GetUserRequest : IntegerIdRequest
    {
        public GetUserRequest(int id) : base(id) { }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string DomainUrl { get; set; }
    }

    public class GetUserResponse : ServiceResponseBase
    {
        public UserViewModel User { get; set; }
        public IEnumerable<DropdownViewModel> DropdownValues { get; set; }
    }
}

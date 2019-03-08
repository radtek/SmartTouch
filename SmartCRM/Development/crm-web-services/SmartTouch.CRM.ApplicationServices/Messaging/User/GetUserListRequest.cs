using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.ApplicationServices.ViewModels;

namespace SmartTouch.CRM.ApplicationServices.Messaging.User
{
    public class GetUserListRequest : ServiceRequestBase
    {
        public int Limit { get; set; }
        public string Query { get; set; }
        public int PageNumber { get; set; }
        public byte Status { get; set; }
        public short Role { get; set; }
        public int AccountID { get; set; }
        public bool IsSTAdmin { get; set; }
    }

    public class GetUserListResponse : ServiceResponseBase
    {
        public int TotalHits { get; set; }
        public IEnumerable<UserViewModel> Users { get; set; }
        public bool IsLimitExceeded { get; set; }
    }

    public class GetRequestUserResponse : ServiceResponseBase
    {
        public IEnumerable<UserEntryViewModel> Users { get; set; }
    }
}

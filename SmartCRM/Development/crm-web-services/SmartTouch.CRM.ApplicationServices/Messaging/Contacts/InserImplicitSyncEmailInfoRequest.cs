using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Domain.ImplicitSync;
using SmartTouch.CRM.Domain.Users;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Contacts
{
    public class InserImplicitSyncEmailInfoRequest : ServiceRequestBase
    {
        public EmailInfo EmailInfo { get; set; }
        public int SentByContactID { get; set; }
        public IEnumerable<UserBasicInfo> Users { get; set; }
    }

    public class InserImplicitSyncEmailInfoResponse : ServiceResponseBase
    {
        public Guid ResponseGuid { get; set; }
    }
}

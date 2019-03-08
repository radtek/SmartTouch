using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Entities;

namespace SmartTouch.CRM.ApplicationServices.Messaging.User
{
    public class InsertProfileAuditRequest : ServiceRequestBase
    {
        public IEnumerable<int> UserId { get; set; }
        public UserAuditType AuditType { get; set; }
        public string Password { get; set; }
        public int AuditedBy { get; set; }
    }

    public class InsertProfileAuditResponse : ServiceResponseBase
    { }
}

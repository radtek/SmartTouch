using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging
{
    public abstract class ServiceRequestBase
    {
        public int AccountId { get; set; }
        public int? RequestedBy { get; set; }
        public short RoleId { get; set; }
        public RequestOrigin? RequestedFrom { get; set; }
    }
}

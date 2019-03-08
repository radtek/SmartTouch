using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.ValueObjects
{
    public class UserActivityEntityDetail
    {
        public int EntityId { get; set; }
        public byte? ContactType { get; set; }
        public string EntityName { get; set; }
        public byte? ReportType { get; set; }
    }
}

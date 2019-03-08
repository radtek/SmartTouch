using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.ValueObjects
{
    public class ReceivedMailAudit : ValueObjectBase
    {
        public int ReceivedMailAuditID { get; set; }
        public int UserID { get; set; }
        public int SentByContactID { get; set; }
        public DateTime ReceivedOn { get; set; }
        public Guid ReferenceID { get; set; }

        protected override void Validate()
        {
            throw new NotImplementedException();
        }

    }
}

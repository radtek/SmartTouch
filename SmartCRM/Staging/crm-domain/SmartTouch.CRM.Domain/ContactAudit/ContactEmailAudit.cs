using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.ContactAudit
{
    public class ContactEmailAudit : EntityBase<int>, IAggregateRoot, IEquatable<ContactEmailAudit>
    {
        
        public int ContactEmailID { get; set; }
        public int SentBy { get; set; }
        public DateTime SentOn { get; set; }
        public byte Status { get; set; }
        public Guid RequestGuid { get; set; }

        protected override void Validate()
        {
        }
        public override int GetHashCode()
        {
            int result = 29;
            result = result * 13 + ContactEmailID.GetHashCode();
            return result;
        }
        public override bool Equals(object entity)
        {
            return Equals(entity as ContactEmailAudit);
        }
        public bool Equals(ContactEmailAudit other)
        {
            if (other == null)
                return false;
            if (other.ContactEmailID == this.ContactEmailID && other.Id == this.Id)
            {
                    return true;
            }
            else
                return false;
        }
    }
}

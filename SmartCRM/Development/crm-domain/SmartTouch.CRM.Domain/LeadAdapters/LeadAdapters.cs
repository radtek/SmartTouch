using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SmartTouch.CRM.Infrastructure.Domain;

namespace SmartTouch.CRM.Domain.LeadAdapters
{
    public class LeadAdapter : EntityBase<int>, IAggregateRoot
    {
        public string Name { get; set; }
        public string BuilderNumber { get; set; }
        public string LeadAdapterType { get; set; }
        public string FTPHost { get; set; }
        public string FTPUserName { get; set; }
        public string FTPPassword { get; set; }

        public int AccountID { get; set; }

        public ICollection<Contact.Contact> Contact { get; set; }

        protected override void Validate()
        {
            throw new NotImplementedException();
        }
    }
}

using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.SuppressedEmails
{
    public class SuppressedDomain: EntityBase<int>, IAggregateRoot
    {
        public int SuppressedDomainID { get; set; }
        public string Domain { get; set; }
        public int AccountID { get; set; }

        protected override void Validate()
        {
            throw new NotImplementedException();
        }
    }
}

using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.SuppressedEmails
{
    public class SuppressedEmail: EntityBase<int>, IAggregateRoot
    {
        public int SuppressedEmailID { get; set; }
        public string Email { get; set; }
        public int AccountID { get; set; }

        protected override void Validate()
        {
            throw new NotImplementedException();
        }
    }
}

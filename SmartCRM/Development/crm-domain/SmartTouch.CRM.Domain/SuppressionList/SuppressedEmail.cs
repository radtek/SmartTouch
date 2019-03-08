using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.SuppressedEmails
{
    public class SuppressionList : EntityBase<int>, IAggregateRoot
    {
        public int AccountID { get; set; }

        protected override void Validate()
        {
            throw new NotImplementedException();
        }
    }

    public class SuppressedEmail : SuppressionList
    {
        public string Email { get; set; }

        protected override void Validate()
        {
            throw new NotImplementedException();
        }
    }
}

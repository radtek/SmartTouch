using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.ValueObjects
{
    public class ActiveContacts : ValueObjectBase
    {
        public int ContactID { get; set; }
        public bool IsActive { get; set; }

        protected override void Validate()
        {
            throw new NotImplementedException();
        }
    }
}

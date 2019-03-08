using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.ValueObjects
{
    public class LoginFrequencyReport : ValueObjectBase
    {
        public string AccountName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public DateTime RecentLoginDate { get; set; }
        public int LoggedInCount { get; set; }

        protected override void Validate()
        {
            throw new NotImplementedException();
        }
    }
}

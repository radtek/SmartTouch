using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Users
{
    public class UserActivity : EntityBase<int>, IAggregateRoot
    {
        public int UserActivityID { get; set; }
        public string ActivityName { get; set; }

        protected override void Validate()
        {
            throw new NotImplementedException();
        }
    }
}

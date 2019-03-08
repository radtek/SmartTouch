using SmartTouch.CRM.Domain.Modules;
using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Roles
{
    public class Role : EntityBase<short>, IAggregateRoot
    {
        public string RoleName { get; set; }
        public int AccountId { get; set; }
        public ICollection<Module> Modules { get; set; }

        protected override void Validate()
        {
            throw new NotImplementedException();
        }
    }
}


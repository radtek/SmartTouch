using SmartTouch.CRM.Domain.Modules;
using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.SeedList
{
   public class SeedEmail : EntityBase<int>, IAggregateRoot
    {
        public string Email { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }

        protected override void Validate()
        {
            throw new NotImplementedException();
        }
    }
}

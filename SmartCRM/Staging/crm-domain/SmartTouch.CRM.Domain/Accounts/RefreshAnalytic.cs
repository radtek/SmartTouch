using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Accounts
{
   public class RefreshAnalytic : EntityBase<int>, IAggregateRoot
    {
        public int EntityID { get; set; }
        public byte EntityType { get; set; }
        public byte Status { get; set; }
        public DateTime?  LastModifiedOn { get; set; }

        protected override void Validate()
        {
        }
    }
}

using SmartTouch.CRM.Domain.Modules;
using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Subscriptions
{
    public class Subscription : EntityBase<byte>, IAggregateRoot
    {
        public int SubscriptionID { get; set; }
        public string SubscriptionName { get; set; }
        public ICollection<Module> Modules { get; set; }

        protected override void Validate()
        {
            throw new NotImplementedException();
        }
    }
}

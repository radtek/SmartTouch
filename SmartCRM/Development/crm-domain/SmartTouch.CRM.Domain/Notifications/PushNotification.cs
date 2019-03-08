using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Notifications
{
    public class PushNotification: EntityBase<int>, IAggregateRoot
    {
        public int AccountId { get; set; }
        public string SubscriptionID { get; set; }
        public string Device { get; set; }
        public bool Allow { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedDate { get; set; }

        protected override void Validate()
        {
            throw new NotImplementedException();
        }
    }
}

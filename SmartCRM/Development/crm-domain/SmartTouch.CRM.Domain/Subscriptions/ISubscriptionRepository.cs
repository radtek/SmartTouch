using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Subscriptions
{
    public interface ISubscriptionRepository : IRepository<Subscription, byte>
    {
        void InsertSubscriptionPermissions(byte subscriptionId, List<byte> moduleIds, int accountId);
        string FindSubscriptionForDomainurl(string domainUrl);
        IEnumerable<Subscription> GetAllSubscriptions();
    }
}

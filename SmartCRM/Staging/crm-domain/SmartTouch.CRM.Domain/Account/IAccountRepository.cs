using System.Collections.Generic;

using SmartTouch.CRM.Infrastructure.Domain;

namespace SmartTouch.CRM.Domain.Account
{
    public interface IAccountRepository : IRepository<Account, int>
    {
        IEnumerable<Account> FindAll(string name, int limit, int pageNumber);
        void UpdateAccountStatus(int[] accountId, string StatusID);
        //IEnumerable<User> GetUsers(string name);
    }
}

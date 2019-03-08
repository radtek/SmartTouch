using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.AccountSettings
{
    public interface IAccountSettingsRepository : IRepository<AccountSettings, string>
    {
        AccountSettings GetAccountUnsubcribeViewByAccountId(int accountId);
    }
}

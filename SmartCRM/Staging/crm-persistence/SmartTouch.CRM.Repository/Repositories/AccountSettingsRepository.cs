using AutoMapper;
using SmartTouch.CRM.Domain.AccountSettings;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using SmartTouch.CRM.Repository.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Repositories
{
    public class AccountSettingsRepository : Repository<AccountSettings, string, AccountSettingsDb>, IAccountSettingsRepository
    {
        public AccountSettingsRepository(IUnitOfWork unitOfWork, IObjectContextFactory objectContextFactory)
            : base(unitOfWork, objectContextFactory)
        {

        }

        public AccountSettings GetAccountUnsubcribeViewByAccountId(int accountId)
        {
            var db = ObjectContextFactory.Create();
            var accountUnsubscribeView = db.accountSettings.Where(s => s.AccountID == accountId).FirstOrDefault();
            return Mapper.Map<AccountSettingsDb,AccountSettings>(accountUnsubscribeView);
           
        }

        public override AccountSettings FindBy(string id)
        {
            throw new NotImplementedException();
        }

        public override AccountSettingsDb ConvertToDatabaseType(AccountSettings domainType, CRMDb context)
        {
            throw new NotImplementedException();
        }

        public override AccountSettings ConvertToDomain(AccountSettingsDb databaseType)
        {
            throw new NotImplementedException();
        }

        public override void PersistValueObjects(AccountSettings domainType, AccountSettingsDb dbType, CRMDb context)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<AccountSettings> FindAll()
        {
            throw new NotImplementedException();
        }
    }
}

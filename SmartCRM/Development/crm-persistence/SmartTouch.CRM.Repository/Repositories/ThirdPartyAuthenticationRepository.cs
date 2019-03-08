using AutoMapper;
using SmartTouch.CRM.Domain.Login;
using SmartTouch.CRM.Domain.ThirdPartyAuthentication;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using SmartTouch.CRM.Repository.Database;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace SmartTouch.CRM.Repository.Repositories
{
    /// <summary>
    /// 
    /// </summary>
    public class ThirdPartyAuthenticationRepository : Repository<ThirdPartyClient, string, ThirdPartyClientsDb>, IThirdPartyAuthenticationRepository
    {
        public ThirdPartyAuthenticationRepository(IUnitOfWork unitOfWork, IObjectContextFactory objectContextFactory)
            : base(unitOfWork, objectContextFactory)
        {

        }

        /// <summary>
        /// Finds the by.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public override ThirdPartyClient FindBy(string id)
        {
            var db = ObjectContextFactory.Create();
            var client = db.ThirdPartyClients.Where(c => c.ID == id).FirstOrDefault();
            return ConvertToDomain(client);
        }

        /// <summary>
        /// Gets the refresh token.
        /// </summary>
        /// <param name="Id">The identifier.</param>
        /// <returns></returns>
        public ClientRefreshToken GetRefreshToken(string Id)
        {
            var db = ObjectContextFactory.Create();
            var refreshToken = db.ClientRefreshTokens.Find(Id);
            return Mapper.Map<ClientRefreshToken>(refreshToken);
        }

        /// <summary>
        /// Adds the refresh token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        public bool AddRefreshToken(ClientRefreshToken token)
        {
            var dbType = Mapper.Map<ClientRefreshTokensDb>(token);
            using(var db = new CRMDb())
            {
                db.ClientRefreshTokens.Add(dbType);
                db.SaveChanges();
            }
            return true;
        }

        /// <summary>
        /// Converts the type of to database.
        /// </summary>
        /// <param name="domainType">Type of the domain.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public override ThirdPartyClientsDb ConvertToDatabaseType(ThirdPartyClient domainType, CRMDb context)
        {
            return Mapper.Map<ThirdPartyClientsDb>(domainType);
        }

        /// <summary>
        /// Converts to domain.
        /// </summary>
        /// <param name="databaseType">Type of the database.</param>
        /// <returns></returns>
        public override ThirdPartyClient ConvertToDomain(ThirdPartyClientsDb databaseType)
        {
            return Mapper.Map<ThirdPartyClient>(databaseType);
        }

        /// <summary>
        /// Persists the value objects.
        /// </summary>
        /// <param name="domainType">Type of the domain.</param>
        /// <param name="dbType">Type of the database.</param>
        /// <param name="context">The context.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public override void PersistValueObjects(ThirdPartyClient domainType, ThirdPartyClientsDb dbType, CRMDb context)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Finds all.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public IEnumerable<ThirdPartyClient> FindAll()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Finds the refresh token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        public ClientRefreshToken FindRefreshToken(string token)
        {
            var rtoken = GetRefreshToken(token);
            return (rtoken != null && rtoken.ExpiresOn > DateTime.UtcNow) ? rtoken : null;
        }

        /// <summary>
        /// Removes the refresh token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        public bool RemoveRefreshToken(string token)
        {
            var db = ObjectContextFactory.Create();
            var c_token = db.ClientRefreshTokens.Find(token);
            db.ClientRefreshTokens.Remove(c_token);
            db.SaveChanges();
            return true;
        }

        /// <summary>
        /// Gets all third party clients.
        /// </summary>
        /// <param name="Name">The name.</param>
        /// <param name="Filter">The filter.</param>
        /// <returns></returns>
        public IEnumerable<ThirdPartyClient> GetAllThirdPartyClients(string Name ,string Filter)
        {
            var db = ObjectContextFactory.Create();
            IEnumerable<ThirdPartyClientsDb> thirdPartyClientDb = Enumerable.Empty<ThirdPartyClientsDb>();
            if(Filter == "" || Filter == "1")
            {
                 thirdPartyClientDb = db.ThirdPartyClients.Include("Account").OrderByDescending(c => c.LastUpdatedOn).Where(s => s.IsActive == true && (s.Account.AccountName.Contains(Name) || s.Name.Contains(Name))).ToList();                 
            }
            else if(Filter == "0")
            {
                 thirdPartyClientDb = db.ThirdPartyClients.Include("Account").OrderByDescending(c => c.LastUpdatedOn).Where(s => (s.Account.AccountName.Contains(Name) || s.Name.Contains(Name))).ToList();
            }            
            else if(Filter == "2")
            {
                thirdPartyClientDb = db.ThirdPartyClients.Include("Account").OrderByDescending(c => c.LastUpdatedOn).Where(s => s.IsActive == false && (s.Account.AccountName.Contains(Name) || s.Name.Contains(Name))).ToList();
            }

            return Mapper.Map<IEnumerable<ThirdPartyClientsDb>, IEnumerable<ThirdPartyClient>>(thirdPartyClientDb);
        }

        /// <summary>
        /// Adds the third party client.
        /// </summary>
        /// <param name="thirdPartyClient">The third party client.</param>
        public void AddThirdPartyClient(ThirdPartyClient thirdPartyClient)
        {
            var db = ObjectContextFactory.Create();
            ThirdPartyClientsDb thirdPartyClientDB = Mapper.Map<ThirdPartyClient, ThirdPartyClientsDb>(thirdPartyClient);
            db.ThirdPartyClients.Add(thirdPartyClientDB);
            db.SaveChanges();
        }

        /// <summary>
        /// Gets the API key by identifier.
        /// </summary>
        /// <param name="apiKeyID">The API key identifier.</param>
        /// <returns></returns>
        public ThirdPartyClient GetApiKeyByID(string apiKeyID)
        {
            var db = ObjectContextFactory.Create();
            var apikeyDB = db.ThirdPartyClients.FirstOrDefault(l => l.ID == apiKeyID);
            return Mapper.Map<ThirdPartyClientsDb, ThirdPartyClient>(apikeyDB);
        }

        /// <summary>
        /// Updates the third party client.
        /// </summary>
        /// <param name="thirdPartyClient">The third party client.</param>
        public  void UpdateThirdPartyClient(ThirdPartyClient thirdPartyClient)
        {
            var db = ObjectContextFactory.Create();
            ThirdPartyClientsDb thirdPartyClientDB = Mapper.Map<ThirdPartyClient, ThirdPartyClientsDb>(thirdPartyClient);
            db.Entry(thirdPartyClientDB).State = EntityState.Modified;
            db.SaveChanges();

        }

        /// <summary>
        /// Deletes the third party client.
        /// </summary>
        /// <param name="thirdPartyClient">The third party client.</param>
        public void DeleteThirdPartyClient(ThirdPartyClient thirdPartyClient)
        {
            var db = ObjectContextFactory.Create();
            ThirdPartyClientsDb thirdpartyclient = db.ThirdPartyClients.Find(thirdPartyClient.ID);
            thirdpartyclient.IsActive = false;
            db.Entry(thirdpartyclient).State = EntityState.Modified;
            db.SaveChanges();
        }

        public ThirdPartyClient GetCLientSecretKeyByAccountName(string accountName)
        {
            using (var db = ObjectContextFactory.Create())
            {
                var sql = @"SELECT TPC.* FROM Accounts (NOLOCK) A 
                            JOIN ThirdPartyClients (NOLOCK) TPC ON TPC.AccountID=A.AccountID
                            WHERE A.AccountName=@AccountName";
                return db.Get<ThirdPartyClient>(sql, new { AccountName = accountName }).FirstOrDefault();
            }
        }
        public ThirdPartyClient GetCLientSecretKeyByDomainName(string accountName)
        {
            using (var db = ObjectContextFactory.Create())
            {
                var sql = @"SELECT TPC.* FROM Accounts (NOLOCK) A 
                            JOIN ThirdPartyClients (NOLOCK) TPC ON TPC.AccountID=A.AccountID
                            WHERE A.DomainURL=@DomainName";
                return db.Get<ThirdPartyClient>(sql, new { DomainName = accountName }).FirstOrDefault();
            }
        }
    }
}

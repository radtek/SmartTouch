using AutoMapper;
using LandmarkIT.Enterprise.Utilities.Logging;
using SmartTouch.CRM.Domain.Communication;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using SmartTouch.CRM.Repository.Database;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartTouch.CRM.Repository.Repositories
{
    public class ServiceProviderRepository : Repository<ServiceProvider, int, ServiceProvidersDb>, IServiceProviderRepository
    {
        public ServiceProviderRepository(IUnitOfWork unitOfWork, IObjectContextFactory objectContextFactory)
            : base(unitOfWork, objectContextFactory) { }

        /// <summary>
        /// Finds all.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ServiceProvider> FindAll()
        {
            var db = ObjectContextFactory.Create();
            var varCommunicationtracker = db.ServiceProviders.ToList();
            foreach (ServiceProvidersDb dc in varCommunicationtracker)
            {
                yield return Mapper.Map<ServiceProvidersDb, ServiceProvider>(dc);
            }
        }

        /// <summary>
        /// Finds the by.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public override ServiceProvider FindBy(int id)
        {
            var target = default(ServiceProvider);
            try
            {
                var db = ObjectContextFactory.Create();
                ServiceProvidersDb serviceProviders = db.ServiceProviders.SingleOrDefault(c => c.ServiceProviderID == id);
                if (serviceProviders != null)
                    target = ConvertToDomain(serviceProviders);
            }
            catch (Exception ex)
            {
                Logger.Current.Error("Exception occurred while getting Communication LogInDetails by using id", ex);
            }
            return target;

        }

        /// <summary>
        /// Finds the name of the by.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        public int FindByName(string name, int accountId)
        {
            var db = ObjectContextFactory.Create();
            int total = db.ServiceProviders.Where(c => c.ProviderName.Contains(name.ToLower()) && c.AccountID == accountId).Count();
            return total;
        }

        /// <summary>
        /// Accounts the service providers.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        public IEnumerable<ServiceProvider> AccountServiceProviders(int accountId)
        {
            try
            {
                var db = ObjectContextFactory.Create();
                var serviceProvidersdb = db.ServiceProviders.Include("ImageDomain").Where(Id => Id.AccountID == accountId).ToList();
                List<ServiceProvider> serviceProviderlist = new List<ServiceProvider>();
                foreach (ServiceProvidersDb providerdb in serviceProvidersdb)
                {
                    serviceProviderlist.Add(ConvertToDomain(providerdb));
                }
                // ServiceProviders serviceProvider = ConvertToDomain(serviceProvidersdb);
                return serviceProviderlist;
            }
            catch (Exception ex)
            {
                Logger.Current.Error("Exception occurred while getting Service Provider details by specific Account", ex);
                return null;
            }
        }

        /// <summary>
        /// Accounts the service providers.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <param name="fromCache">if set to <c>true</c> [from cache].</param>
        /// <returns></returns>
        public IEnumerable<ServiceProvider> AccountServiceProviders(int accountId, bool fromCache)
        {
            try
            {
                var db = ObjectContextFactory.Create();
                var sqlQuery = @" select * from serviceproviders where accountid = @accountId and emailtype = @emailType";

                IEnumerable<ServiceProvidersDb> serviceProvidersdb =
                    db.Get<ServiceProvidersDb>(sqlQuery, new { AccountId = accountId, emailType = MailType.BulkEmail }, fromCache);
                IEnumerable<ServiceProvider> serviceProviderlist = Mapper.Map<IEnumerable<ServiceProvidersDb>, IEnumerable<ServiceProvider>>(serviceProvidersdb);
                return serviceProviderlist;
            }
            catch (Exception ex)
            {
                Logger.Current.Error("Exception occurred while getting Service Provider details by Account: " + accountId, ex);
                return null;
            }
        }

        /// <summary>
        /// Gets the service providers.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <param name="communicationType">Type of the communication.</param>
        /// <param name="mailprovider">The mailprovider.</param>
        /// <returns></returns>
        public ServiceProvider GetServiceProviders(int accountId, CommunicationType communicationType, MailType mailprovider)
        {
            try
            {
                var db = ObjectContextFactory.Create();
                var serviceProviderDb = db.ServiceProviders
                    .Include("ImageDomain")
                    .Where(Id => Id.CommunicationTypeID == communicationType && Id.AccountID == accountId
                        && Id.EmailType == (byte)mailprovider && Id.IsDefault == true).FirstOrDefault();
                ServiceProvider serviceProvider = ConvertToDomain(serviceProviderDb);
                return serviceProvider;
            }
            catch (Exception ex)
            {
                Logger.Current.Error("Exception occurred while getting ServiceProvider details", ex);
                return null;
            }
        }

        /// <summary>
        /// Gets the send text service providers.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <param name="communicationType">Type of the communication.</param>
        /// <returns></returns>
        public ServiceProvider GetSendTextServiceProviders(int accountId, CommunicationType communicationType)
        {
            var db = ObjectContextFactory.Create();
            var sql = @"SELECT TOP 1 * FROM ServiceProviders (NOLOCK) WHERE CommunicationTypeID = @CommunicationTypeId AND AccountID=@AccountId";
            var serviceProvidersdb = db.Get<ServiceProvidersDb>(sql, new { CommunicationTypeId = communicationType, AccountId = accountId }).FirstOrDefault();
            ServiceProvider serviceProviders = ConvertToDomain(serviceProvidersdb);
            return serviceProviders;
        }

        /// <summary>
        /// Gets the account communication providers.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <param name="communicationType">Type of the communication.</param>
        /// <param name="mailProvider">The mail provider.</param>
        /// <returns></returns>
        public IEnumerable<ServiceProvider> GetAccountCommunicationProviders(int accountId, CommunicationType communicationType, MailType mailProvider)
        {
            var db = ObjectContextFactory.Create();
            var ServiceProviders = db.ServiceProviders.Include("ImageDomain").Where(Id => Id.CommunicationTypeID == communicationType && Id.AccountID == accountId && Id.EmailType == (byte)mailProvider && Id.IsDefault == true).ToList();

            foreach (ServiceProvidersDb cld in ServiceProviders)
            {
                yield return Mapper.Map<ServiceProvidersDb, ServiceProvider>(cld);
            }
        }

        /// <summary>
        /// Gets the email provider tokens.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        public IEnumerable<Guid> GetEmailProviderTokens(int accountId)
        {
            var db = ObjectContextFactory.Create();
            var serviceProviders = db.ServiceProviders.Where(ep => ep.AccountID == accountId && ep.CommunicationTypeID == CommunicationType.Mail && ep.EmailType == (byte)MailType.BulkEmail).
                                   Select(s => s.LoginToken).ToList();
            return serviceProviders;
        }

        /// <summary>
        /// Converts to domain.
        /// </summary>
        /// <param name="ServiceProviders">The service providers.</param>
        /// <returns></returns>
        public override ServiceProvider ConvertToDomain(ServiceProvidersDb ServiceProviders)
        {
            ServiceProvider serviceProviders = new ServiceProvider();
            try
            {
                if (serviceProviders != null)
                    Mapper.Map<ServiceProvidersDb, ServiceProvider>(ServiceProviders, serviceProviders);
            }
            catch (Exception ex)
            {
                Logger.Current.Error("Exception occurred while converting DbObject(ServiceProviders) to DomainObject(ServiceProviders)", ex);
            }
            return serviceProviders;

        }

        /// <summary>
        /// Converts the type of to database.
        /// </summary>
        /// <param name="domainType">Type of the domain.</param>
        /// <param name="db">The database.</param>
        /// <returns></returns>
        public override ServiceProvidersDb ConvertToDatabaseType(ServiceProvider domainType, CRMDb db)
        {
            ServiceProvidersDb serviceProviders = null;
            try
            {
                serviceProviders = Mapper.Map<ServiceProvider, ServiceProvidersDb>(domainType as ServiceProvider);
            }
            catch (Exception ex)
            {
                Logger.Current.Error("Exception occurred while converting DomainObject(ServiceProviders) to DbObject(ServiceProviders)", ex);
            }

            return serviceProviders;
        }

        /// <summary>
        /// Persists the value objects.
        /// </summary>
        /// <param name="domainType">Type of the domain.</param>
        /// <param name="ServiceProviders">The service providers.</param>
        /// <param name="db">The database.</param>
        public override void PersistValueObjects(ServiceProvider domainType, ServiceProvidersDb ServiceProviders, CRMDb db)
        {
        }

        /// <summary>
        /// Gets the service provider email.
        /// </summary>
        /// <param name="serviceProviderId">The service provider identifier.</param>
        /// <returns></returns>
        public Email GetServiceProviderEmail(int serviceProviderId)
        {
            try
            {
                if (serviceProviderId > 0)
                {
                    var db = ObjectContextFactory.Create();
                    AccountEmailsDb accountEmailsDb = db.AccountEmails.Where(ae => ae.ServiceProviderID == serviceProviderId).FirstOrDefault();
                    if (accountEmailsDb != null)
                        return Mapper.Map<AccountEmailsDb, Email>(accountEmailsDb);
                    else
                        return null;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                Logger.Current.Error("Exception occurred while fetching the ServiceProvider Email details", ex);
                return null;
            }
        }

        /// <summary>
        /// Inserts the serviceprovider email.
        /// </summary>
        /// <param name="serviceprovider">The serviceprovider.</param>
        /// <param name="email">The email.</param>
        /// <returns></returns>
        public Email InsertServiceproviderEmail(ServiceProvider serviceprovider, String email)
        {
            try
            {
                if (email != null && email != "")
                {
                    var AccountEmailDb = new AccountEmailsDb
                    {
                        Email = email,
                        AccountID = serviceprovider.AccountId,
                        ServiceProviderID = serviceprovider.Id,

                    };
                    var db = ObjectContextFactory.Create();
                    db.AccountEmails.Add(AccountEmailDb);
                    db.SaveChanges();
                    return Mapper.Map<AccountEmailsDb, Email>(AccountEmailDb);
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                Logger.Current.Error("Exception occurred while Inserting the ServiceProvider Email details", ex);
                return null;
            }

        }

        /// <summary>
        /// Updates the serviceprovider email.
        /// </summary>
        /// <param name="serviceprovider">The serviceprovider.</param>
        /// <param name="email">The email.</param>
        /// <returns></returns>
        public Email UpdateServiceproviderEmail(ServiceProvider serviceprovider, String email)
        {
            try
            {
                var db = ObjectContextFactory.Create();
                AccountEmailsDb accountEmailsDb = db.AccountEmails.Where(ae => ae.ServiceProviderID == serviceprovider.Id).FirstOrDefault();
                if (accountEmailsDb == null)
                {
                    return InsertServiceproviderEmail(serviceprovider, email);
                }
                if (accountEmailsDb != null && email != null)
                {
                    accountEmailsDb.Email = email;
                    accountEmailsDb.UserID = null;
                    db.SaveChanges();
                    return Mapper.Map<AccountEmailsDb, Email>(accountEmailsDb);
                }
                else
                    return null;

            }
            catch (Exception ex)
            {
                Logger.Current.Error("Exception occurred while Inserting the ServiceProvider Email details", ex);
                return null;
            }

        }

        /// <summary>
        /// Gets the default campaign provider.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        public ServiceProvider GetDefaultCampaignProvider(int accountId)
        {
            try
            {
                var db = ObjectContextFactory.Create();
                var serviceProvidersdb = db.ServiceProviders.Include("Accounts").Include("ImageDomain").Include("Accounts.Addresses").Where(sp => sp.AccountID == accountId
                    && sp.EmailType == (byte)MailType.BulkEmail && sp.IsDefault).FirstOrDefault();

                ServiceProvider serviceProviders = ConvertToDomain(serviceProvidersdb);
                return serviceProviders;
            }
            catch (Exception ex)
            {
                Logger.Current.Error("Exception occurred while getting default campaign service provider details", ex);
                return null;
            }
        }

        /// <summary>
        /// Gets the campaign provider.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        public ServiceProvider GetCampaignProvider(int accountId)
        {
            try
            {
                var db = ObjectContextFactory.Create();
                var serviceProviderDb = db.ServiceProviders.Include("ImageDomain").Where(sp => sp.AccountID == accountId && sp.EmailType == (byte)MailType.BulkEmail && sp.IsDefault).FirstOrDefault();
                ServiceProvider serviceProviders = ConvertToDomain(serviceProviderDb);
                return serviceProviders;
            }
            catch (Exception ex)
            {
                Logger.Current.Error("Exception occurred while getting default campaign service provider details", ex);
                return null;
            }

        }

        /// <summary>
        /// Gets the campaign provider.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        public ServiceProvider GetAutomationCampaignProvider(Guid guid)
        {
            try
            {
                var db = ObjectContextFactory.Create();
                var serviceProviderDb = db.ServiceProviders.Include("ImageDomain").Where(sp => sp.LoginToken == guid).FirstOrDefault();
                ServiceProvider serviceProviders = ConvertToDomain(serviceProviderDb);
                return serviceProviders;
            }
            catch (Exception ex)
            {
                Logger.Current.Error("Exception occurred while getting vmta campaign service provider for guid: " + guid, ex);
                return null;
            }

        }

        /// <summary>
        /// Gets the transactional provider details.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        public Dictionary<Guid, string> GetTransactionalProviderDetails(int accountId)
        {
            string email = string.Empty;
            Dictionary<Guid, string> details = new Dictionary<Guid, string>();
            if (accountId != 0)
            {
                var db = ObjectContextFactory.Create();
                var serviceProvider = db.ServiceProviders.Where(sp => sp.AccountID == accountId && sp.CommunicationTypeID == CommunicationType.Mail
                    && sp.EmailType == (byte)MailType.TransactionalEmail && sp.IsDefault).FirstOrDefault();
                if (serviceProvider != null)
                {
                    email = db.AccountEmails.Where(ae => ae.ServiceProviderID == serviceProvider.ServiceProviderID).Select(s => s.Email).FirstOrDefault();
                    details.Add(serviceProvider.LoginToken, email);
                }
            }
            return details;
        }

        /// <summary>
        /// Gets the service provider by identifier.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <param name="serviceProviderId">The service provider identifier.</param>
        /// <returns></returns>
        public ServiceProvider GetServiceProviderById(int accountId, int serviceProviderId)
        {
            try
            {
                var db = ObjectContextFactory.Create();
                var serviceProvidersdb = db.ServiceProviders.Include("Accounts").Include("ImageDomain").Include("Accounts.Addresses").Where(sp => sp.AccountID == accountId
                    && sp.EmailType == (byte)MailType.BulkEmail && sp.ServiceProviderID == serviceProviderId).FirstOrDefault();

                ServiceProvider serviceProviders = ConvertToDomain(serviceProvidersdb);
                return serviceProviders;
            }
            catch (Exception ex)
            {
                Logger.Current.Error("Exception occurred while getting default campaign service provider details", ex);
                return null;
            }
        }
    }
}

using AutoMapper;
using LandmarkIT.Enterprise.Utilities.Logging;
using LinqKit;
using SmartTouch.CRM.Domain.Accounts;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.Images;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Domain.WebAnalytics;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using SmartTouch.CRM.Repository.Database;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using SmartTouch.CRM.Repository;
using LandmarkIT.Enterprise.Extensions;
using SmartTouch.CRM.Domain.Search;

namespace SmartTouch.CRM.Repository.Repositories
{
    public class AccountRepository : Repository<Account, int, AccountsDb>, IAccountRepository
    {

        public AccountRepository(IUnitOfWork unitOfWork, IObjectContextFactory objectContextFactory)
            : base(unitOfWork, objectContextFactory)
        {

        }

        /// <summary>
        /// Finds all.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Account> FindAll()
        {
            PredicateBuilder.True<AccountsDb>();

            IEnumerable<AccountsDb> accounts = ObjectContextFactory.Create().Accounts
                .Include(c => c.Addresses).Include(c => c.Communication);
            foreach (AccountsDb dc in accounts)
            {
                yield return Mapper.Map<AccountsDb, Account>(dc);
            }
        }

        /// <summary>
        /// Finds the by.
        /// </summary>
        /// <param name="id">accountId.</param>
        /// <returns></returns>
        public override Account FindBy(int id)
        {
            var account = getAccountDb(id, true);

            if (account != null)
                return ConvertToDomain(account);
            return null;
        }

        /// <summary>
        /// Gets the account.
        /// </summary>
        /// <param name="accountId">accountId.</param>
        /// <param name="isSTAdmin">if set to <c>true</c> [is st admin].</param>
        /// <returns></returns>
        public Account GetAccount(int accountId, bool isSTAdmin)
        {
            var account = getAccountDb(accountId, isSTAdmin);

            if (account != null)
                return ConvertToDomain(account);
            return null;
        }

        public IEnumerable<SubscriptionSettings> GetSubscriptionSettings(int subscriptionId, string doaminUrl)
        {
            var db = ObjectContextFactory.Create();
            IEnumerable<SubscriptionSettings> settings = db.SubscriptionSettings.Where(p => (int)p.SubscriptionID == subscriptionId).Select(p => new SubscriptionSettings()
            {
                SubscriptionId = p.SubscriptionID,
                SubscriptionSettingType = (SubscriptionSettingTypes)p.Name,
                Value = p.Value
            }).ToArray();

            return settings;
        }
        /// <summary>
        /// Saves the account logo.
        /// </summary>
        /// <param name="image">The image.</param>
        public void SaveAccountLogo(Image image)
        {
            var db = ObjectContextFactory.Create();
            ImagesDb imagesdb = Mapper.Map<Image, ImagesDb>(image);
            if (imagesdb != null)
            {
                var varImage = db.Images.Where(Id => Id.ImageID == imagesdb.ImageID).FirstOrDefault();
                if (varImage != null)
                {
                    varImage.StorageName = image.StorageName;
                    varImage.FriendlyName = image.FriendlyName;
                    varImage.OriginalName = image.OriginalName;
                }
                else
                {
                    db.Images.Add(imagesdb);
                }

                Image Image = Mapper.Map<ImagesDb, Image>(imagesdb);
                AccountsDb accountDatabase = db.Accounts.Where(p => p.AccountID == image.AccountID && p.IsDeleted == false).FirstOrDefault();
                if (accountDatabase != null)
                    accountDatabase.LogoImageID = Image.Id;
                db.SaveChanges();
            }

        }

        /// <summary>
        /// Gets the account database.
        /// </summary>
        /// <param name="id">accountId.</param>
        /// <returns></returns>
        AccountsDb getAccountDb(int accountId, bool isSTAdmin)
        {
            var db = ObjectContextFactory.Create();
            var accountSql = @"SELECT * FROM Accounts(NOLOCK) A
                             INNER JOIN Subscriptions (NOLOCK) S ON S.SubscriptionID = A.SubscriptionID
                             INNER JOIN Communications (NOLOCK) C ON C.CommunicationID = A.CommunicationID
                             WHERE A.AccountID = @accountId
                             SELECT A.* FROM Addresses (NOLOCK) A
                             INNER JOIN AccountAddressMap (NOLOCK) AA ON AA.AddressID = A.AddressID
                             WHERE AA.AccountID = @accountId
                             SELECT * FROM Images(NOLOCK) WHERE ImageCategoryID=3 AND AccountID=@accountId";
            var account = new AccountsDb();
            db.GetMultiple(accountSql, (r) =>
            {
                account = r.Read<AccountsDb, SubscriptionsDb, CommunicationsDb, AccountsDb>((a, s, c) =>
                {
                    a.Subscription = s;
                    a.Communication = c;
                    return a;
                }, splitOn: "SubscriptionID,CommunicationID").FirstOrDefault();
                if (account != null)
                {
                    account.Addresses = r.Read<AddressesDb>().ToList();
                    account.Image = r.Read<ImagesDb>().FirstOrDefault();
                }
            }, new { accountId = accountId });

            if (isSTAdmin)
            {
                Logger.Current.Verbose("Getting WebAnalytics Provider for account : " + accountId);

                var webAnalyticsSql = @"SELECT TOP 1 * FROM WebAnalyticsProviders (NOLOCK) WA WHERE WA.AccountID = @accountId
                                        SELECT WNM.* FROM WebAnalyticsProviders (NOLOCK) WA
                                        INNER JOIN WebVisitUserNotificationMap (NOLOCK) WNM ON WNM.WebAnalyticsProviderID = WA.WebAnalyticsProviderID
                                        WHERE WA.AccountID = @accountId";

                db.GetMultiple(webAnalyticsSql, (r) =>
                {
                    account.WebAnalyticsProvider = r.Read<WebAnalyticsProvidersDb>().FirstOrDefault();
                    if (account.WebAnalyticsProvider != null)
                        account.WebAnalyticsProvider.NotificationGroup = r.Read<WebVisitUserNotificationMapDb>().ToList();
                }, new { accountId = accountId });
            }

            var subscribedModules = @"SELECT DISTINCT M.*, SMM.Limit AS UserLimit, SMM.ExcludedRoles FROM SubscriptionModuleMap (NOLOCK) SMM
                                      INNER JOIN Modules (NOLOCK) M ON M.ModuleID = SMM.ModuleID
                                      WHERE SMM.AccountID = @accountId AND SMM.SubscriptionID = @subscriptionId
                                      ORDER BY M.ModuleID";

            List<ModulesDb> modules = db.Get<ModulesDb>(subscribedModules, new { accountId = accountId, subscriptionId = account.SubscriptionID }).ToList();
            account.SubscribedModules = modules;
            account.UserLimit = modules.Where(w => w.ModuleID == (byte)AppModules.Users).Select(s => s.UserLimit).FirstOrDefault();
            account.ExcludedRoles = modules.Where(w => w.ModuleID == (byte)AppModules.Users).Select(s => s.ExcludedRoles).FirstOrDefault();
            return account;
        }

        /// <summary>
        /// Finds the accounts summary.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <param name="sortField">The sort field.</param>
        /// <param name="direction">The direction.</param>
        /// <returns></returns>
        IEnumerable<AccountsDb> findAccountsSummary(System.Linq.Expressions.Expression<Func<AccountsDb, bool>> predicate, string sortField, ListSortDirection direction = ListSortDirection.Descending)
        {
            IEnumerable<AccountsDb> accounts = ObjectContextFactory.Create().Accounts
                .Where(i => i.IsDeleted == false)
                .Include(i => i.Subscription)
                .AsExpandable()
                .Where(predicate).OrderByDescending(c => c.ModifiedOn).ThenBy(c => c.AccountName).Select(a =>
                    new
                    {
                        AccountID = a.AccountID,
                        AccountName = a.AccountName,
                        FirstName = a.FirstName,
                        LastName = a.LastName,
                        ContactsCount = a.ContactsCount,
                        EmailsCount = a.EmailsCount,
                        Domainurl = a.DomainURL,
                        Status = a.Status,
                        SubscriptionID = a.SubscriptionID,
                        Subscription = a.Subscription,
                    }).ToList().Select(x => new AccountsDb
                    {
                        AccountID = x.AccountID,
                        AccountName = x.AccountName,
                        FirstName = x.FirstName,
                        LastName = x.LastName,
                        ContactsCount = x.ContactsCount,
                        EmailsCount = x.EmailsCount,
                        DomainURL = x.Domainurl,
                        Status = x.Status,
                        SubscriptionID = x.SubscriptionID,
                        Subscription = x.Subscription,
                    }).AsQueryable().OrderBy(sortField, direction);

            return accounts;
        }

        /// <summary>
        /// Finds all.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="limit">The limit.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="status">The status.</param>
        /// <param name="sortField">The sort field.</param>
        /// <param name="direction">The direction.</param>
        /// <returns></returns>
        public IEnumerable<Account> FindAll(string name, int limit, int pageNumber, byte status, string sortField, ListSortDirection direction = ListSortDirection.Descending)
        {
            var predicate = PredicateBuilder.True<AccountsDb>();
            var records = (pageNumber - 1) * limit;
            if (!string.IsNullOrEmpty(name))
            {
                name = name.ToLower();
                predicate = predicate.And(a => a.AccountName.Contains(name));
            }
            if (status != 0)
            {
                predicate = predicate.And(a => a.Status == status);
            }
            IEnumerable<AccountsDb> accounts = findAccountsSummary(predicate, sortField, direction).Skip(records).Take(limit);
            foreach (AccountsDb da in accounts)
            {
                yield return ConvertToDomain(da);
            }
        }

        /// <summary>
        /// Gets the BDX accounts.
        /// </summary>
        /// <param name="accountName">Name of the account.</param>
        /// <returns></returns>
        public List<BdxAccounts> GetBdxAccounts(string accountName)
        {
            var db = ObjectContextFactory.Create();
            List<BdxAccounts> bdxAccounts = db.Accounts.Where(p => p.SubscriptionID == (byte)AccountSubscription.BDX && p.AccountName.Contains(accountName) && p.IsDeleted == false && p.Status == (byte)Status.Active)
                .Select(a => new BdxAccounts() { AccountId = a.AccountID, AccountName = a.AccountName, DomainUrl = a.DomainURL, AccountEmail = a.PrimaryEmail }).ToList();

            return bdxAccounts;
        }

        /// <summary>
        /// Finds all accounts.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="limit">The limit.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="status">The status.</param>
        /// <param name="sortField">The sort field.</param>
        /// <param name="direction">The direction.</param>
        /// <returns></returns>
        public AccountsList FindAllAccounts(string name, int limit, int pageNumber, byte status, string sortField, ListSortDirection direction = ListSortDirection.Descending)
        {

            var records = (pageNumber - 1) * limit;
            AccountsList accountList = new AccountsList();

            var sql = @"SELECT a.AccountID,a.SenderReputationCount,a.IsDeleted,a.AccountName,a.ContactsCount,a.EmailsCount,a.Status,a.DomainURL, a.CreatedOn, a.SubscriptionID, AA.ActiveUsers AS ActiveUsersCount, 
                        AA.LastCampaignSent, AA.LastLogin
                        FROM Accounts (NOLOCK) a
                        LEFT JOIN AccountActivities (NOLOCK) AA ON AA.AccountID = A.AccountID 
                        WHERE IsDeleted = 0";
            var dbData = ObjectContextFactory.Create();
            IEnumerable<AccountsGridData> accountsdata = dbData.Get<AccountsGridData>(sql);

            foreach (var account in accountsdata)
            {
                if (account.Status == 2 || account.Status == 3)
                    account.StatusMessage = "Paused";
                else if (account.Status == 1)
                    account.StatusMessage = "Active";
                else if (account.Status == 105)
                    account.StatusMessage = "Draft";
                else if (account.Status == 5)
                    account.StatusMessage = "Maintanace";
                else
                    account.StatusMessage = "Inactive";

                account.SubscriptionName = account.SubscriptionID == 1 ? "Smarttouch" : account.SubscriptionID == 2 ? "Standard" : "BDX";
            }


            if (!String.IsNullOrEmpty(name))
                accountsdata = accountsdata.Where(p => p.AccountName.ToLower().Contains(name.ToLower())).ToList();
            if (status != 0)
                accountsdata = accountsdata.Where(p => p.Status == status);

            accountList.TotalHits = accountsdata.Count();
            accountList.AccountGridData = accountsdata.AsQueryable().OrderBy(sortField, direction).Skip(records).Take(limit);
            return accountList;
        }

        /// <summary>
        /// Finds all.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="status">The status.</param>
        /// <param name="sortField">The sort field.</param>
        /// <param name="direction">The direction.</param>
        /// <returns></returns>
        public IEnumerable<Account> FindAll(string name, byte status, string sortField, ListSortDirection direction = ListSortDirection.Descending)
        {
            var predicate = PredicateBuilder.True<AccountsDb>();
            if (!string.IsNullOrEmpty(name))
            {
                name = name.ToLower();
                predicate = predicate.And(a => a.AccountName.Contains(name));
            }
            if (status != 0)
            {
                predicate = predicate.And(a => a.Status == status);
            }
            IEnumerable<AccountsDb> accounts = findAccountsSummary(predicate, sortField, direction);
            foreach (AccountsDb da in accounts)
            {
                yield return ConvertToDomain(da);
            }
        }

        /// <summary>
        /// Converts to domain.
        /// </summary>
        /// <param name="accountDbObject">The account database object.</param>
        /// <returns></returns>
        public override Account ConvertToDomain(AccountsDb accountDbObject)
        {
            Account account = new Account();
            Mapper.Map<AccountsDb, Account>(accountDbObject, account);
            return account;
        }

        /// <summary>
        /// Gets the subscription data by account identifier.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        public AccountSubscriptionData GetSubscriptionDataByAccountID(int accountId)
        {
            var db = ObjectContextFactory.Create();
            AccountSubscriptionData data = db.Accounts.Where(p => p.AccountID == accountId).Select(p => new AccountSubscriptionData() { SubscriptionID = p.SubscriptionID, AccountUrl = p.DomainURL }).FirstOrDefault();
            return data;
        }

        /// <summary>
        /// Converts the type of to database.
        /// </summary>
        /// <param name="domainType">Type of the domain.</param>
        /// <param name="db">The database.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">Invalid account id has been passed. Suspected Id forgery.</exception>
        public override AccountsDb ConvertToDatabaseType(Account domainType, CRMDb db)
        {
            AccountsDb accountsDb = new AccountsDb();

            //Existing Contact
            if (domainType.Id > 0)
            {

                var accountSql = @"SELECT * FROM Accounts(NOLOCK) A
                                 INNER JOIN Communications (NOLOCK) C ON C.CommunicationID = A.CommunicationID
                                 WHERE A.AccountID = @accountId
                                 SELECT A.* FROM Addresses (NOLOCK) A
                                 INNER JOIN AccountAddressMap (NOLOCK) AA ON AA.AddressID = A.AddressID
                                 WHERE AA.AccountID = @accountId";
                db.GetMultiple(accountSql, (r) =>
                {
                    accountsDb = r.Read<AccountsDb, CommunicationsDb, AccountsDb>((a, c) =>
                    {
                        a.Communication = c;
                        return a;
                    }, splitOn: "CommunicationID").FirstOrDefault();
                    accountsDb.Addresses = r.Read<AddressesDb>().ToList();
                }, new { accountId = domainType.Id });


                if (accountsDb == null)
                    throw new ArgumentException("Invalid account id has been passed. Suspected Id forgery.");

                accountsDb = Mapper.Map<Account, AccountsDb>(domainType, accountsDb);
            }
            else //New Contact
            {

                accountsDb = Mapper.Map<Account, AccountsDb>(domainType);
            }
            return accountsDb;
        }

        /// <summary>
        /// Persists the value objects.
        /// </summary>
        /// <param name="domainType">Type of the domain.</param>
        /// <param name="accountsDb">The accounts database.</param>
        /// <param name="db">The database.</param>
        public override void PersistValueObjects(Account domainType, AccountsDb accountsDb, CRMDb db)
        {
            persistAddresses(domainType, accountsDb, db);
            persistCommunication(domainType, accountsDb, db);
            persistVisiStat(domainType, accountsDb, db);
        }

        /// <summary>
        /// Persists the account logo.
        /// </summary>
        /// <param name="domainType">Type of the domain.</param>
        /// <param name="accountsDb">The accounts database.</param>
        /// <param name="db">The database.</param>
        void persistAccountLogo(Account domainType, AccountsDb accountsDb, CRMDb db)
        {
            ImagesDb imagesdb = Mapper.Map<Image, ImagesDb>(domainType.AccountLogo);
            if (imagesdb != null)
            {
                var varImage = db.Images.Where(Id => Id.ImageID == imagesdb.ImageID).FirstOrDefault();
                if (varImage != null)
                {
                    varImage.StorageName = domainType.AccountLogo.StorageName;
                    varImage.FriendlyName = domainType.AccountLogo.FriendlyName;
                    varImage.OriginalName = domainType.AccountLogo.OriginalName;
                }
                else
                {
                    accountsDb.Image = imagesdb;
                }
            }
        }

        /// <summary>
        /// Persists the addresses.
        /// </summary>
        /// <param name="account">The account.</param>
        /// <param name="accountsDb">The accounts database.</param>
        /// <param name="db">The database.</param>
        void persistAddresses(Account account, AccountsDb accountsDb, CRMDb db)
        {
            IEnumerable<AddressesDb> addresses = Mapper.Map<IEnumerable<Address>, IEnumerable<AddressesDb>>(account.Addresses);

            //Existing contact
            if (account.Id > 0)
            {
                foreach (AddressesDb addressDb in accountsDb.Addresses)
                {                   
                    db.Entry<AddressesDb>(addressDb).State = addressDb.AddressID > 0 ? System.Data.Entity.EntityState.Modified: System.Data.Entity.EntityState.Added;
                }
                //Existing contact, delete addresses in db if deleted in UI.
                var deletedAddresses = accountsDb.Addresses.Where(a => a.AddressID != 0 && !addresses.Select(ad => ad.AddressID).Contains(a.AddressID)).ToList();
                foreach (AddressesDb addressDb in deletedAddresses)
                {
                    accountsDb.Addresses.Remove(addressDb);
                }
                
            }
        }

        /// <summary>
        /// Persists the communication.
        /// </summary>
        /// <param name="account">The account.</param>
        /// <param name="accountsDb">The accounts database.</param>
        /// <param name="db">The database.</param>
        void persistCommunication(Account account, AccountsDb accountsDb, CRMDb db)
        {
            CommunicationsDb communicationDb;
            communicationDb = Mapper.Map<Account, CommunicationsDb>(account);

            if (accountsDb.Communication != null)
                communicationDb.CommunicationID = accountsDb.Communication.CommunicationID;
            accountsDb.Communication = communicationDb;
        }

        /// <summary>
        /// Persists the visi stat.
        /// </summary>
        /// <param name="account">The account.</param>
        /// <param name="accountsDb">The accounts database.</param>
        /// <param name="db">The database.</param>
        /// <exception cref="System.ArgumentException">Invalid VisiStat id has been passed. Suspected Id forgery.</exception>
        void persistVisiStat(Account account, AccountsDb accountsDb, CRMDb db)
        {
            Logger.Current.Verbose("In Persist VisiStat. AccountId: " + account.Id);
            if (account.WebAnalyticsProvider != null)
            {
                var isSTAdmin = db.Users.Where(u => u.AccountID == 1 && u.UserID == account.ModifiedBy).FirstOrDefault();
                if (isSTAdmin != null && account.WebAnalyticsProvider != null)
                {
                    Logger.Current.Verbose("Updating WebAnalytics Provider for account: " + account.Id);
                    var visiStatConfigurationsDb = db.WebAnalyticsProviders.Where(a => a.AccountID == account.Id).ToList();

                    var notificationGroup = db.WebVisitUserNotificationMap.Where(a => a.WebAnalyticsProviderID == account.WebAnalyticsProvider.Id).ToList();

                    if (account.WebAnalyticsProvider.Id == 0)
                    {
                        Logger.Current.Verbose("Adding WebAnalytics Provider for account: " + account.Id);
                        account.WebAnalyticsProvider.CreatedOn = DateTime.Now.ToUniversalTime();
                        WebAnalyticsProvidersDb visiStatConfigurationDb = Mapper.Map<WebAnalyticsProvider, WebAnalyticsProvidersDb>(account.WebAnalyticsProvider);
                        visiStatConfigurationDb.NotificationGroup = persistWebVisitsUsersToBeNotified(account.WebAnalyticsProvider, notificationGroup, db);
                        db.WebAnalyticsProviders.Add(visiStatConfigurationDb);
                    }
                    else
                    {
                        var visiStatConfigurationDb = visiStatConfigurationsDb.SingleOrDefault(c => c.WebAnalyticsProviderID == account.WebAnalyticsProvider.Id);
                        Logger.Current.Informational("Updating WebAnalytics Provider for account: " + account.Id + ". WebAnalytics ProviderId: " + visiStatConfigurationDb.WebAnalyticsProviderID);

                        if (visiStatConfigurationDb == null)
                            throw new ArgumentException("Invalid VisiStat id has been passed. Suspected Id forgery.");
                        db.Entry<WebAnalyticsProvidersDb>(visiStatConfigurationDb).State = System.Data.Entity.EntityState.Modified;
                        visiStatConfigurationDb = Mapper.Map<WebAnalyticsProvider, WebAnalyticsProvidersDb>(account.WebAnalyticsProvider, visiStatConfigurationDb);
                        visiStatConfigurationDb.NotificationGroup = persistWebVisitsUsersToBeNotified(account.WebAnalyticsProvider, notificationGroup, db);
                    }
                }
            }
        }

        /// <summary>
        /// Persists the web visits users to be notified.
        /// </summary>
        /// <param name="webAnalyticsProvider">The web analytics provider.</param>
        /// <param name="notificationGroupDb">The notification group database.</param>
        /// <param name="db">The database.</param>
        /// <returns></returns>
        IEnumerable<WebVisitUserNotificationMapDb> persistWebVisitsUsersToBeNotified
            (WebAnalyticsProvider webAnalyticsProvider, IEnumerable<WebVisitUserNotificationMapDb> notificationGroupDb, CRMDb db)
        {
            Logger.Current.Verbose("In persistWebVisitsUsersToBeNotified. WebAnalyticsProvider: " + webAnalyticsProvider.Id);
            IList<WebVisitUserNotificationMapDb> newNotificationGroup = new List<WebVisitUserNotificationMapDb>();
            if (webAnalyticsProvider.Id == 0)
            {
                foreach (int userId in webAnalyticsProvider.InstantNotificationGroup)
                {
                    WebVisitUserNotificationMapDb userMap = new WebVisitUserNotificationMapDb();
                    userMap.AccountID = webAnalyticsProvider.AccountID;
                    userMap.CreatedBy = webAnalyticsProvider.LastUpdatedBy;
                    userMap.CreatedOn = DateTime.UtcNow;
                    userMap.UserID = userId;
                    userMap.NotificationType = WebVisitEmailNotificationType.Instant;
                    userMap.WebAnalyticsProviderID = webAnalyticsProvider.Id;
                    db.Entry(userMap).State = System.Data.Entity.EntityState.Added;
                    newNotificationGroup.Add(userMap);
                }
                foreach (int userId in webAnalyticsProvider.DailySummaryNotificationGroup)
                {
                    WebVisitUserNotificationMapDb userMap = new WebVisitUserNotificationMapDb();
                    userMap.AccountID = webAnalyticsProvider.AccountID;
                    userMap.CreatedBy = webAnalyticsProvider.LastUpdatedBy;
                    userMap.CreatedOn = DateTime.UtcNow;
                    userMap.UserID = userId;
                    userMap.NotificationType = WebVisitEmailNotificationType.DailySummary;
                    userMap.WebAnalyticsProviderID = webAnalyticsProvider.Id;
                    db.Entry(userMap).State = System.Data.Entity.EntityState.Added;
                    newNotificationGroup.Add(userMap);
                }
            }
            else
            {
                var existingInstantUsers = notificationGroupDb
                    .Where(c => c.NotificationType == WebVisitEmailNotificationType.Instant).Select(c => c.UserID).ToList();
                foreach (int userId in webAnalyticsProvider.InstantNotificationGroup)
                {
                    if (!existingInstantUsers.Contains(userId))
                    {
                        WebVisitUserNotificationMapDb userMap = new WebVisitUserNotificationMapDb();
                        userMap.AccountID = webAnalyticsProvider.AccountID;
                        userMap.CreatedBy = webAnalyticsProvider.LastUpdatedBy;
                        userMap.CreatedOn = DateTime.UtcNow;
                        userMap.UserID = userId;
                        userMap.NotificationType = WebVisitEmailNotificationType.Instant;
                        userMap.WebAnalyticsProviderID = webAnalyticsProvider.Id;
                        db.Entry(userMap).State = System.Data.Entity.EntityState.Added;
                        newNotificationGroup.Add(userMap);
                    }
                    else
                    {
                        var currentUser = notificationGroupDb.Where(c => c.NotificationType == WebVisitEmailNotificationType.Instant && c.UserID == userId).FirstOrDefault();
                        currentUser.LastUpdatedBy = webAnalyticsProvider.LastUpdatedBy;
                        currentUser.LastUpdatedOn = DateTime.UtcNow;
                    }
                }
                var existingDailyUsers = notificationGroupDb
                    .Where(c => c.NotificationType == WebVisitEmailNotificationType.DailySummary).Select(c => c.UserID).ToList();
                foreach (int userId in webAnalyticsProvider.DailySummaryNotificationGroup)
                {
                    if (!existingDailyUsers.Contains(userId))
                    {
                        WebVisitUserNotificationMapDb userMap = new WebVisitUserNotificationMapDb();
                        userMap.AccountID = webAnalyticsProvider.AccountID;
                        userMap.CreatedBy = webAnalyticsProvider.LastUpdatedBy;
                        userMap.CreatedOn = DateTime.UtcNow;
                        userMap.UserID = userId;
                        userMap.NotificationType = WebVisitEmailNotificationType.DailySummary;
                        userMap.WebAnalyticsProviderID = webAnalyticsProvider.Id;
                        db.Entry(userMap).State = System.Data.Entity.EntityState.Added;
                        newNotificationGroup.Add(userMap);
                    }
                    else
                    {
                        var currentUser = notificationGroupDb.Where(c => c.NotificationType == WebVisitEmailNotificationType.DailySummary && c.UserID == userId).FirstOrDefault();
                        currentUser.LastUpdatedBy = webAnalyticsProvider.LastUpdatedBy;
                        currentUser.LastUpdatedOn = DateTime.UtcNow;
                    }
                }
                var unMappedInstantUsers = notificationGroupDb
                    .Where(c => !webAnalyticsProvider.InstantNotificationGroup.Contains(c.UserID) && c.NotificationType == WebVisitEmailNotificationType.Instant)
                    .ToList();
                foreach (WebVisitUserNotificationMapDb user in unMappedInstantUsers)
                {
                    Logger.Current.Verbose("Removing from Instant Notification. User: " + user.UserID);
                    db.WebVisitUserNotificationMap.Remove(user);
                }
                var unMappedDailyUsers = notificationGroupDb
                    .Where(c => !webAnalyticsProvider.DailySummaryNotificationGroup.Contains(c.UserID) && c.NotificationType == WebVisitEmailNotificationType.DailySummary)
                    .ToList();
                foreach (WebVisitUserNotificationMapDb user in unMappedDailyUsers)
                {
                    Logger.Current.Verbose("Removing from DailySummary. User: " + user.UserID);
                    db.WebVisitUserNotificationMap.Remove(user);
                }
            }
            Logger.Current.Verbose("Completed persistWebVisitsUsersToBeNotified. WebAnalyticsProvider: " + webAnalyticsProvider.Id);
            return newNotificationGroup;
        }

        /// <summary>
        /// Gets the reputation count.
        /// </summary>
        /// <param name="accountId">accountId.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns></returns>
        public int GetReputationCount(int accountId, DateTime startDate, DateTime endDate)
        {
            var sql = @"SELECT m.SenderReputationCount from dbo.GETREPUTATIONINFO(@ACCOUNTID, @FROMDATE, @TODATE) m";
            var dbData = ObjectContextFactory.Create();
            var actionContacts = dbData.Get<int>(sql, new { ACCOUNTID = accountId, FROMDATE = startDate, TODATE = endDate }).FirstOrDefault();
            return actionContacts;
        }

        /// <summary>
        /// Updates the account status.
        /// </summary>
        /// <param name="accountId">accountIds list.</param>
        /// <param name="StatusID">The status identifier.</param>
        /// <returns></returns>
        public List<string> UpdateAccountStatus(int[] accountId, byte StatusID)
        {
            List<string> emailIDs = new List<string>();
            var db = ObjectContextFactory.Create();
            foreach (int account in accountId)
            {
                var accountIDs = db.Accounts.Where(a => a.AccountID == account).FirstOrDefault();
                if (StatusID == 6)
                    accountIDs.IsDeleted = true;
                else
                    accountIDs.Status = StatusID;
                emailIDs.Add(accountIDs.PrimaryEmail + "|" + accountIDs.AccountName);
            }
            db.SaveChanges();
            return emailIDs;
        }

        /// <summary>
        /// Determines whether [is duplicate account] [the specified account name].
        /// </summary>
        /// <param name="accountName">Name of the account.</param>
        /// <param name="accountId">The account identifier.</param>
        /// <param name="domainurl">The domainurl.</param>
        /// <returns></returns>
        public bool IsDuplicateAccount(string accountName, int accountId, string domainurl)
        {
            IQueryable<AccountsDb> accounts = ObjectContextFactory.Create().Accounts.Where(c => (c.AccountName == accountName || c.DomainURL == domainurl) && c.IsDeleted == false);
            if (accountId > 0)
                accounts = accounts.Where(c => c.AccountID != accountId);
            int count = accounts.Count();
            if (count > 0)
                return true;
            return false;
        }

        /// <summary>
        /// Gets the user time zone.
        /// </summary>
        /// <param name="AccountId">accountId.</param>
        /// <returns></returns>
        public string GetUserTimeZone(int AccountId)
        {
            var db = ObjectContextFactory.Create();
            return db.Accounts.Where(i => i.AccountID == AccountId).Select(c => c.TimeZone).SingleOrDefault();
        }

        /// <summary>
        /// Finds the account by name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public Account FindByName(string name)
        {
            //AccountsDb accountDatabase = ObjectContextFactory.Create().Accounts.FirstOrDefault(c => c.AccountName.ToLower() == name.ToLower() && c.Status == 1);
            var sql = "select * from dbo.Accounts (nolock) where lower(accountname) = lower(@name) and status in (1,3,5)";
            AccountsDb accountDatabase = ObjectContextFactory.Create().Get<AccountsDb>(sql, new { name = name }).FirstOrDefault();
            if (accountDatabase != null)
                return ConvertToDomain(accountDatabase);
            return null;
        }

        /// <summary>
        /// Finds the account by accountid.
        /// </summary>
        /// <param name="id">accountId.</param>
        /// <returns></returns>
        public Account FindByAccountID(int id)
        {
            //AccountsDb accountDatabase = ObjectContextFactory.Create().Accounts.FirstOrDefault(c => c.AccountID == id && c.Status == 1);
            var sql = "select * from dbo.Accounts (nolock) where accountid = @id and status in (1,3,5)";
            AccountsDb accountDatabase = ObjectContextFactory.Create().Get<AccountsDb>(sql, new { Id = id }).FirstOrDefault();
            if (accountDatabase != null)
            {
                Account account = ConvertToDomain(accountDatabase);

                // account.AccountLogoUrl = urlService.GetUrl(personViewModel.AccountID, ImageCategory.ContactProfile, personViewModel.Image.StorageName);
                return account;
            }
            return null;
        }

        /// <summary>
        /// Gets the name of the image storage.
        /// </summary>
        /// <param name="accountId">accountId.</param>
        /// <returns></returns>
        public AccountLogoInfo GetImageStorageName(int accountId)
        {
            var db = ObjectContextFactory.Create();
            var sql = @"select a.AccountName, i.StorageName, a.PrivacyPolicy ,a.HelpURL, c.WebSiteUrl as WebsiteURL, a.SubscriptionID from accounts (nolock) a
                        join Communications (nolock) c on c.CommunicationID = a.CommunicationID
                            left outer join images (nolock) i on a.AccountID = i.AccountID and a.LogoImageID = i.ImageID
                            where a.accountid = @id and isdeleted = 0 and status = @status";
            return db.Get<AccountLogoInfo>(sql, new { Id = accountId, Status = AccountStatus.Active }).FirstOrDefault();
        }

        /// <summary>
        /// Gets the account primary email.
        /// </summary>
        /// <param name="accountId">accountId.</param>
        /// <returns></returns>
        public string GetAccountPrimaryEmail(int accountId)
        {
            var db = ObjectContextFactory.Create();
            string primaryEmail = db.Accounts.Where(a => a.AccountID == accountId && a.IsDeleted == false && a.Status == (byte)AccountStatus.Active).Select(s => s.PrimaryEmail).FirstOrDefault();
            return primaryEmail;
        }

        /// <summary>
        /// Gets the address.
        /// </summary>
        /// <param name="accountId">accountId.</param>
        /// <returns></returns>
        public Address GetAddress(int accountId)
        {
            if (accountId != 0)
            {
                var db = ObjectContextFactory.Create();
                var address = db.Accounts.Include(i => i.Addresses).
                    Where(a => a.AccountID == accountId && a.IsDeleted == false && a.Status == (byte)AccountStatus.Active)
                    .Select(s => s.Addresses).FirstOrDefault();
                if (address != null)
                {
                    //var addressId = address.Where(a => a.IsDefault.HasValue && a.IsDefault == true).Select(s => s.AddressID).FirstOrDefault();
                    var addressDb = address.Where(a => a.IsDefault.HasValue && a.IsDefault == true).FirstOrDefault();
                    if (addressDb != null)
                        return Mapper.Map<AddressesDb, Address>(addressDb);
                    else return null;
                }
                else return null;
            }
            else
                return null;
        }

        /// <summary>
        /// Gets the primary phone.
        /// </summary>
        /// <param name="accountId">accountId.</param>
        /// <returns></returns>
        public string GetPrimaryPhone(int accountId)
        {
            var db = ObjectContextFactory.Create();
            var phoneNumber = db.Accounts.Where(a => a.AccountID == accountId && a.IsDeleted == false && a.Status == (byte)AccountStatus.Active).Select(s => new
            {
                PhoneNumber = s.WorkPhone ?? s.MobilePhone ?? s.HomePhone
            }).FirstOrDefault();
            if (phoneNumber != null)
                return phoneNumber.PhoneNumber;
            else return string.Empty;
        }

        /// <summary>
        /// Finds the account by domain URL.
        /// </summary>
        /// <param name="name">account name.</param>
        /// <returns></returns>
        public Account FindByDomainUrl(string name)
        {
            var sql = "select * from accounts (nolock) where domainurl = @url and status in (1,3,5) and isdeleted = 0";
            AccountsDb accountDatabase = ObjectContextFactory.Create().Get<AccountsDb>(sql, new { url = name }).FirstOrDefault();
            if (accountDatabase != null)
            {
                Account account = new Account();
                Mapper.Map<AccountsDb, Account>(accountDatabase, account);
                return account;
            }
            return null;
        }

        /// <summary>
        /// Checks the domain availability.
        /// </summary>
        /// <param name="domainurl">The domainurl.</param>
        /// <returns></returns>
        public bool CheckDomainAvailability(string domainurl)
        {
            var db = ObjectContextFactory.Create();
            if (domainurl != null)
            {
                if (db.Accounts.Where(a => a.DomainURL == domainurl && a.IsDeleted == false).Any())
                    return false;
                else
                    return true;
            }
            else return false;
        }

        /// <summary>
        /// Gets the account permissions.
        /// </summary>
        /// <param name="accountId">accountId.</param>
        /// <returns></returns>
        public List<byte> GetAccountPermissions(int accountId)
        {
            var db = ObjectContextFactory.Create();
            var sql = @"declare @subscriptionid int;
                        select @subscriptionid = subscriptionid from accounts (nolock) where accountid = @id;
                        select moduleid from SubscriptionModuleMap (nolock) where subscriptionid = @subscriptionid and accountid = @Id";

            var modules = db.Get<byte>(sql, new { Id = accountId }).ToList();
            return modules;
        }

        /// <summary>
        /// Gets the opportunity customers.
        /// </summary>
        /// <param name="accountId">accountId.</param>
        /// <returns></returns>
        public byte? GetOpportunityCustomers(int accountId)
        {
            var db = ObjectContextFactory.Create();
            var sql = "select opportunitycustomers from accounts (nolock) where accountid = @id and isdeleted = 0";
            byte? oppCustomers = db.Get<byte?>(sql, new { Id = accountId }).FirstOrDefault();
            return oppCustomers;

        }

        /// <summary>
        /// Inserts the default roles.
        /// </summary>
        /// <param name="accountID">The account identifier.</param>
        public void InsertDefaultRoles(int accountID)
        {
            //Default roles are inserted through StoredProcedure.

            //var db = ObjectContextFactory.Create();
            //var roles = db.Roles.Where(r => r.AccountID == null);
            //if (roles != null)
            //{
            //    foreach (RolesDb role in roles)
            //    {
            //        role.AccountID = accountID;
            //        db.Roles.Add(role);
            //    }
            //    db.SaveChanges();
            //}
        }

        /// <summary>
        /// Inserts the default role configurations.
        /// </summary>
        /// <param name="accountId">accountId.</param>
        /// <param name="moduleIds">The module ids.</param>
        public void InsertDefaultRoleConfigurations(int accountId, List<byte> moduleIds)
        {
            //var db = ObjectContextFactory.Create();
            //var accountRoles = db.Roles.Where(s => s.AccountID == accountId);
            //if (accountRoles != null)
            //{
            //    foreach (RolesDb role in accountRoles)
            //    {
            //        RoleModulesMapDb roleModuleMap = new RoleModulesMapDb();
            //        roleModuleMap.RoleID = role.RoleID;
            //        roleModuleMap.ModuleID = (byte)AppModules.Contacts;
            //        db.RoleModules.Add(roleModuleMap);

            //        var excludeModules = new List<byte>();
            //        excludeModules.Add((byte)AppModules.Contacts);

            //        if (moduleIds.Contains((byte)AppModules.Dashboard))
            //        {
            //            RoleModulesMapDb roleModuleMapDb = new RoleModulesMapDb();
            //            roleModuleMapDb.RoleID = role.RoleID;
            //            roleModuleMapDb.ModuleID = (byte)AppModules.Dashboard;
            //            db.RoleModules.Add(roleModuleMapDb);
            //            excludeModules.Add((byte)AppModules.Dashboard);
            //        }

            //        if (role.RoleName.Equals("Account Administrator"))//This needs to be removed and handled in a better way.
            //        {
            //            moduleIds.Where(s => !excludeModules.Contains(s)).ForEach(s =>
            //            {
            //                RoleModulesMapDb roleModuleDB = new RoleModulesMapDb();
            //                roleModuleDB.ModuleID = s;
            //                roleModuleDB.RoleID = role.RoleID;
            //                db.RoleModules.Add(roleModuleDB);
            //            });
            //        }
            //    }
            //    db.SaveChanges();
            //}

            var parms = new List<SqlParameter>
                {
                    new SqlParameter{ParameterName ="@AccountID", Value= accountId},
                };

            using (CRMDb context = new CRMDb())
            {
                var objectContext = (context as IObjectContextAdapter).ObjectContext;
                objectContext.CommandTimeout = 1600;
                context.ExecuteStoredProcedure("[dbo].[Account_Default_Roles_v1]", parms);
            }
        }

        /// <summary>
        /// Gets the dropbox key.
        /// </summary>
        /// <param name="accountId">accountId.</param>
        /// <returns></returns>
        public string GetDropboxKey(int accountId)
        {
            var db = ObjectContextFactory.Create();
            var key = db.Accounts.Where(a => a.AccountID == accountId).Select(a => a.DropboxAppKey).FirstOrDefault();
            return key;
        }

        /// <summary>
        /// Persists the new roles.
        /// </summary>
        /// <param name="account">The account.</param>
        /// <param name="accountsDb">The accounts database.</param>
        /// <param name="db">The database.</param>
        void PersistNewRoles(Account account, AccountsDb accountsDb, CRMDb db)
        {
            var roles = db.Roles.Where(r => r.AccountID == null);
            if (roles != null)
            {
                foreach (RolesDb role in roles)
                {
                    role.AccountID = accountsDb.AccountID;
                    db.Roles.Add(role);
                }
            }
        }

        /// <summary>
        /// Persists the default role configurations.
        /// </summary>
        /// <param name="account">The account.</param>
        /// <param name="accountsDb">The accounts database.</param>
        /// <param name="db">The database.</param>
        void PersistDefaultRoleConfigurations(Account account, AccountsDb accountsDb, CRMDb db)
        {
            var accountRoles = db.Roles.Where(s => s.AccountID == accountsDb.AccountID);
            if (accountRoles != null)
            {
                foreach (RolesDb role in accountRoles)
                {
                    RoleModulesMapDb roleModuleMap = new RoleModulesMapDb();
                    roleModuleMap.RoleID = role.RoleID;
                    roleModuleMap.ModuleID = (byte)AppModules.Users;
                    db.RoleModules.Add(roleModuleMap);
                }
            }
        }

        /// <summary>
        /// Inserts the data sharing permissions.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <param name="moduleIds">The module ids.</param>
        /// <param name="subscriptionId">The subscription identifier.</param>
        public void InsertDataSharingPermissions(int accountId, List<byte> moduleIds, byte subscriptionId)
        {
            var db = ObjectContextFactory.Create();
            var dataAccessPermissionsDb = db.DataAccessPermissions.Where(r => r.AccountID == accountId).ToList();
            if (moduleIds != null)
            {
                IList<DataAccessPermissionDb> dataAccessPremisions = new List<DataAccessPermissionDb>();
                //get subscribed modules and check the module present in subscription or not before entering into DAP table.
                var subscribedModules = db.SubscriptionModules.Where(w => w.SubscriptionID == subscriptionId && w.AccountID == accountId).Select(s => s.ModuleID);
                foreach (var moduleId in moduleIds)
                {
                    if ((!(dataAccessPermissionsDb.Exists(f => f.ModuleID == moduleId))) && subscribedModules.Contains(moduleId))
                    {
                        DataAccessPermissionDb dataAccessPermissionDb = new DataAccessPermissionDb();
                        dataAccessPermissionDb.AccountID = accountId;
                        dataAccessPermissionDb.ModuleID = moduleId;
                        dataAccessPermissionDb.IsPrivate = true;
                        dataAccessPremisions.Add(dataAccessPermissionDb);
                    }
                }
                db.DataAccessPermissions.AddRange(dataAccessPremisions);
                var deletedModules = dataAccessPermissionsDb.Where(a => a.ModuleID != 0 && !moduleIds.Contains(a.ModuleID)).ToList();
                db.DataAccessPermissions.RemoveRange(deletedModules);
                db.SaveChanges();
            }
        }

        public void UpdateUserLimit(int accountId, int? limit, IEnumerable<short> excludedRoles)
        {
            using (var db = ObjectContextFactory.Create())
            {
                SubscriptionModuleMapDb dbObject = db.SubscriptionModules.Where(w => w.AccountID == accountId && w.ModuleID == (byte)AppModules.Users).FirstOrDefault();
                if (dbObject != null)
                {
                    dbObject.Limit = limit;
                    dbObject.ExcludedRoles = excludedRoles.IsAny() ? string.Join(",", excludedRoles) : null;
                    db.SaveChanges();
                }
            }
        }

        /// <summary>
        /// Gets the private modules.
        /// </summary>
        /// <param name="accountId">accountId.</param>
        /// <returns></returns>
        public IEnumerable<byte> GetPrivateModules(int accountId)
        {
            var db = ObjectContextFactory.Create();
            var subscriptionId = db.Accounts.Where(w => w.AccountID == accountId).Select(s => s.SubscriptionID).FirstOrDefault();
            var subscribedModules = db.SubscriptionModules.Where(w => w.SubscriptionID == subscriptionId && w.AccountID == accountId).Select(s => s.ModuleID);
            var privateModules = db.DataAccessPermissions.Where(s => s.AccountID == accountId && subscribedModules.Contains(s.ModuleID)).Select(s => s.ModuleID).ToList();
            return privateModules;
        }

        /// <summary>
        /// Gets the domain URL by account identifier.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        public string GetDomainUrlByAccountId(int accountId)
        {
            var db = ObjectContextFactory.Create();
            string domainUrl = db.Accounts.Where(p => p.AccountID == accountId).Select(p => p.DomainURL).FirstOrDefault();
            return domainUrl;
        }

        /// <summary>
        /// Gets the module sharing permission.
        /// </summary>
        /// <param name="moduleId">The module identifier.</param>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        public bool GetModuleSharingPermission(int moduleId, int accountId)
        {
            var db = ObjectContextFactory.Create();
            bool isPrivate = db.DataAccessPermissions
                 .Where(d => d.AccountID == accountId && d.ModuleID == moduleId).Select(d => d.IsPrivate).FirstOrDefault();
            return isPrivate; //'isPrivate == false' implies that the module id shared by default.
        }

        /// <summary>
        /// Gets the daily summary emails.
        /// </summary>
        /// <returns></returns>
        public List<DailySummaryEmail> GetDailySummaryEmails()
        {
            var db = ObjectContextFactory.Create();
            var today = DateTime.Now.ToUniversalTime();
            var yesterday = today.AddDays(-1);

            List<DailySummaryEmail> dse = new List<DailySummaryEmail>();
            var activeAccounts = db.Accounts.Where(a => a.Status == (byte)AccountStatus.Active && a.IsDeleted == false).Select(s => s.AccountID);
            var activeUsers = db.Users.Where(u => u.Status == Status.Active && u.IsDeleted == false && activeAccounts.Contains(u.AccountID)).Select(s => s.UserID); //
            var userSettings = db.UserSettings.Where(us => us.DailySummary == true && activeUsers.Contains(us.UserID));
            var allUsersActivityLogs = db.UserActivitiesLog.Where(uact => uact.UserID != null &&
                                                               (uact.ModuleID == (byte)AppModules.Contacts || uact.ModuleID == (byte)AppModules.Opportunity) &&
                                                               (uact.UserActivityID == (byte)UserActivityType.Create || uact.UserActivityID == (byte)UserActivityType.ChangeOwner) &&
                                                               uact.LogDate >= yesterday && uact.LogDate <= today);
            var distinctAccounts = userSettings.Select(s => s.AccountID).Distinct();
            foreach (var accountId in distinctAccounts)
            {
                List<UserSummary> allUsersSummary = new List<UserSummary>();
                var accountUserSettings = userSettings.Where(w => w.AccountID == accountId);
                if (accountUserSettings != null)
                {
                    foreach (var user in accountUserSettings)
                    {
                        var userSpedificLogs = allUsersActivityLogs.Where(all => all.UserID == user.UserID).ToList();
                        UserSummary userSummary = new UserSummary();
                        List<UserSettingsSummary> userSummaryList = new List<UserSettingsSummary>();
                        UserSettingsSummary userSettingsSummary = new UserSettingsSummary();
                        List<UserContactActivitySummary> usersContactSummary = new List<UserContactActivitySummary>();
                        List<UserContactActivitySummary> ownershipAssignedContacts = new List<UserContactActivitySummary>();
                        List<UserOpportunityActivitySummary> opportunitySummary = new List<UserOpportunityActivitySummary>();

                        if (userSpedificLogs.Count > 0)
                        {
                            #region ContactsCreated
                            var contactCreatedLogs = userSpedificLogs.Where(usl => usl.UserActivityID == (byte)UserActivityType.Create && usl.ModuleID == (byte)AppModules.Contacts);
                            foreach (var contact in contactCreatedLogs)
                            {
                                var createdContact = db.Contacts.Where(con => con.ContactID == contact.EntityID && con.IsDeleted == false).Select(s => new UserContactActivitySummary()
                                {
                                    contactId = s.ContactID,
                                    ContactName = s.ContactType == ContactType.Person ? (s.FirstName + " " + s.LastName == " " ? db.ContactEmails.Where(ce => ce.ContactID == contact.EntityID
                                        && ce.IsPrimary == true).Select(sce => sce.Email).FirstOrDefault() : s.FirstName + " " + s.LastName) : s.Company,
                                    ContactType = s.ContactType,
                                    LastUpdatedBy = null
                                }).FirstOrDefault();
                                if (createdContact != null)    // Skip when a contact is created and deleted on same day.
                                    usersContactSummary.Add(createdContact);
                            }
                            #endregion

                            #region OwnedContacts
                            var ownedContactLogs = userSpedificLogs.Where(usl => usl.UserActivityID == (byte)UserActivityType.ChangeOwner && usl.ModuleID == (byte)AppModules.Contacts);
                            foreach (var contact in ownedContactLogs)
                            {
                                var ownedContacts = db.Contacts.Where(con => con.ContactID == contact.EntityID && con.IsDeleted == false).Select(s => new UserContactActivitySummary()
                                {
                                    contactId = s.ContactID,
                                    ContactName = s.ContactType == ContactType.Person ? (s.FirstName + " " + s.LastName == " " ? db.ContactEmails.Where(ce => ce.ContactID == contact.EntityID
                                        && ce.IsPrimary == true).Select(sce => sce.Email).FirstOrDefault() : s.FirstName + " " + s.LastName) : s.Company,
                                    ContactType = s.ContactType,
                                    LastUpdatedBy = db.Users.Where(u => u.UserID == s.LastUpdatedBy).Select(u => s.FirstName + " " + u.LastName).FirstOrDefault()
                                }).FirstOrDefault();
                                if (ownedContacts != null)    // Skip this contact if it is deleted on the same day and made owner of that contact.
                                    ownershipAssignedContacts.Add(ownedContacts);
                            }
                            #endregion

                            #region OpportunitiesCreated
                            var opportunityCreatedLogs = userSpedificLogs.Where(usl => usl.UserActivityID == (byte)UserActivityType.Create && usl.ModuleID == (byte)AppModules.Opportunity);
                            foreach (var opportunity in opportunityCreatedLogs)
                            {
                                var createdOpportunity = db.Opportunities.Where(opp => opp.OpportunityID == opportunity.EntityID && opp.IsDeleted == false).Select(s => new UserOpportunityActivitySummary()
                                {
                                    OpportunityId = s.OpportunityID,
                                    OpportunityName = s.OpportunityName
                                }).FirstOrDefault();
                                if (createdOpportunity != null)    // Skip this opportunity if it is created and deleted on the same day.
                                    opportunitySummary.Add(createdOpportunity);
                            }
                            #endregion
                        }
                        userSettingsSummary.ContactsSummary = usersContactSummary;
                        userSettingsSummary.OwnerChangedContacts = ownershipAssignedContacts;
                        userSettingsSummary.OpportunitySummary = opportunitySummary;
                        userSettingsSummary.UserContactActionSummary = GetUserContactActionsSummary(user.UserID, today, yesterday);

                        userSummaryList.Add(userSettingsSummary);
                        userSummary.UserSettings = userSummaryList;
                        userSummary.UserEmail = db.Users.Where(u => u.UserID == user.UserID).Select(s => s.PrimaryEmail).FirstOrDefault();
                        userSummary.UserId = user.UserID;
                        var UserName = db.Users.Where(u => u.UserID == user.UserID).Select(s => new { UserName = s.FirstName + " " + s.LastName }).FirstOrDefault();
                        userSummary.UserName = UserName.UserName;

                        allUsersSummary.Add(userSummary);
                    }
                }

                var dailyEmail = db.Accounts.Where(a => a.AccountID == accountId && a.Status == (byte)AccountStatus.Active).Select(s => new
                {
                    AccountEmail = s.PrimaryEmail,
                    AccountID = s.AccountID,
                    DomainURL = s.DomainURL,
                    AccountName = s.AccountName
                    // Users = allUsersSummary,
                }).ToList().Select(z => new DailySummaryEmail()
                {
                    // Users = z.Users,
                    DomainURL = z.DomainURL,
                    AccountID = z.AccountID,
                    AccountEmail = z.AccountEmail,
                    AccountName = z.AccountName,
                    PrimaryAddress = GetAddress(z.AccountID),
                    PrimaryPhone = GetPrimaryPhone(z.AccountID)
                }).FirstOrDefault();
                dailyEmail.Users = allUsersSummary;
                dse.Add(dailyEmail);
            }
            return dse;
        }

        /// <summary>
        /// Gets the contact campaign by account.
        /// </summary>
        /// <returns></returns>
        public List<ContactAccountGroup> GetContactCampaignByAccount(int accountId)
        {
            var db = ObjectContextFactory.Create();

            List<ContactAccountGroup> accountContacts = db.Contacts.Where(c => c.IsDeleted == false && c.AccountID == accountId)
                                                .Join(db.Accounts, con => con.AccountID, acc => acc.AccountID, (con, acc) => new { con, acc })
                                                .GroupBy(a => new { a.con.AccountID, a.acc.AccountName })
                                                .Select(result => new ContactAccountGroup
                                                {
                                                    AccountID = result.Key.AccountID,
                                                    AccountName = result.Key.AccountName,
                                                    ContactsCount = result.Count()
                                                }).ToList();

            return accountContacts;
        }

        /// <summary>
        /// Gets the campaigns from database.
        /// </summary>
        /// <returns></returns>
        //public List<CampaignAccountGroup> GetCampaignsFromDb()
        //{
        //    var db = ObjectContextFactory.Create();  
        //    List<CampaignAccountGroup> accountCampaigns = db.Campaigns.Where(camp => camp.IsDeleted == false)
        //                                        .Join(db.Accounts, cam => cam.AccountID, act => act.AccountID, (cam, act) => new { cam, act })
        //                                        .GroupBy(camp => new { camp.cam.AccountID, camp.cam.CampaignStatusID, camp.act.AccountName })
        //                                        .Select(c => new CampaignAccountGroup
        //                                        {
        //                                            AccontID = c.Key.AccountID,
        //                                            Name = c.Key.AccountName,
        //                                            CampaignStatus = (CampaignStatus)c.Key.CampaignStatusID,
        //                                            CampaignsCount = c.Count(),
        //                                            Type = "Database"
        //                                        }).ToList();
        //    return accountCampaigns;
        //}

        /// <summary>
        /// Inserts the daily summary email audit.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="status">The status.</param>
        public void InsertDailySummaryEmailAudit(int userId, byte status)
        {
            var db = ObjectContextFactory.Create();
            DailySummaryEmailAuditDb DSEAudit = new DailySummaryEmailAuditDb() { AuditedOn = DateTime.UtcNow, UserID = userId, Status = status };
            db.DailySummaryEmailAudit.Add(DSEAudit);
            db.SaveChanges();
        }

        /// <summary>
        /// Get all active Web Analytics Providers of all accounts
        /// </summary>
        /// <returns></returns>
        public List<WebAnalyticsProvider> GetWebAnalyticsProviders()
        {
            var db = ObjectContextFactory.Create();
            IQueryable<WebAnalyticsProvidersDb> webAnalyticsProviders = db.WebAnalyticsProviders.Include(w => w.Account)
                    .Where(w => !string.IsNullOrEmpty(w.APIKey) && w.StatusID == WebAnalyticsStatus.Active && w.Account.Status == (byte)AccountStatus.Active);
            var domain = Mapper.Map<List<WebAnalyticsProvidersDb>, List<WebAnalyticsProvider>>(webAnalyticsProviders.ToList());
            return domain;
        }

        /// <summary>
        /// Get Web Analytics Providers by AccountId
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        public List<WebAnalyticsProvider> GetAccountWebAnalyticsProviders(int accountId)
        {
            var webAnalyticsProviders = new List<WebAnalyticsProvidersDb>();
            using (var db = ObjectContextFactory.Create())
            {
                webAnalyticsProviders = db.WebAnalyticsProviders
                    .Where(w => !string.IsNullOrEmpty(w.APIKey) && w.StatusID == WebAnalyticsStatus.Active
                        && w.Account.Status == (byte)AccountStatus.Active && w.AccountID == accountId)
                    .ToList();
            }
            var domain = Mapper.Map<List<WebAnalyticsProvidersDb>, List<WebAnalyticsProvider>>(webAnalyticsProviders);
            return domain;
        }

        /// <summary>
        /// Gets the account domain URL.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        public string GetAccountDomainUrl(int accountId)
        {
            string domainUrl = "";
            using (var db = ObjectContextFactory.Create())
            {
                if (accountId != 0)
                    domainUrl = db.Accounts.Where(a => a.AccountID == accountId).Select(a => a.DomainURL).FirstOrDefault();
            }
            return domainUrl;
        }

        /// <summary>
        /// Gets the account minimum details.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        public Account GetAccountMinDetails(int accountId)
        {
            Account account = new Account();
            using (var db = ObjectContextFactory.Create())
            {
                if (accountId != 0)
                    account = db.Accounts.Where(a => a.AccountID == accountId).Select(s => new Account()
                    {
                        AccountName = s.AccountName,
                        Id = s.AccountID,
                        DomainURL = s.DomainURL,
                        Email = new Email() { EmailId = db.AccountEmails.Where(w => w.AccountID == accountId && w.IsPrimary == true).Select(se => se.Email).FirstOrDefault() }
                    }).FirstOrDefault();
            }
            return account;
        }

        /// <summary>
        /// Gets the account identifier by contact identifier.
        /// </summary>
        /// <param name="contactId">The contact identifier.</param>
        /// <returns></returns>
        public int GetAccountIdByContactId(int contactId)
        {
            var sql = @"SELECT TOP 1 AccountID FROM Contacts (NOLOCK) WHERE ContactID = @contactId";
            int accountid = 0;
            using (var db = ObjectContextFactory.Create())
            {
               accountid =  db.Get<int>(sql, new { ContactId = contactId }).ToList().FirstOrDefault();
            }
            return accountid;
        }

        /// <summary>
        /// Gets the service provider email.
        /// </summary>
        /// <param name="serviceProviderId">The service provider identifier.</param>
        /// <returns></returns>
        public Email GetServiceProviderEmail(int serviceProviderId)
        {
            var db = ObjectContextFactory.Create();
            AccountEmailsDb emailDb = db.AccountEmails.Where(c => c.ServiceProviderID == serviceProviderId).FirstOrDefault();
            return Mapper.Map<AccountEmailsDb, Email>(emailDb);
        }

        /// <summary>
        /// Gets the account privacy policy.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        public string GetAccountPrivacyPolicy(int accountId)
        {
            string privacyPolicy = "";
            using (var db = ObjectContextFactory.Create())
            {
                privacyPolicy = db.Accounts.Where(a => a.AccountID == accountId).Select(a => a.PrivacyPolicy).FirstOrDefault();
            }
            return privacyPolicy;
        }

        /// <summary>
        /// Gets the account basic details.
        /// </summary>
        /// <param name="AccountID">The account identifier.</param>
        /// <returns></returns>
        public Account GetAccountBasicDetails(int AccountID)
        {
            using (var db = ObjectContextFactory.Create())
            {
                AccountsDb Account = db.Accounts.Include(x => x.Image).Where(i => i.AccountID == AccountID).FirstOrDefault();
                Account account = Mapper.Map<AccountsDb, Account>(Account);
                return account;
            }
        }

        /// <summary>
        /// Gets all accounts.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Account> GetAllAccounts()
        {
            var db = ObjectContextFactory.Create();
            IEnumerable<AccountsDb> accounts = db.Accounts.Where(c => c.Status == 1).Select(s => new { AccountID = s.AccountID, AccountName = s.AccountName }).ToList().Select(c => new AccountsDb { AccountID = c.AccountID, AccountName = c.AccountName });
            foreach (AccountsDb dc in accounts)
            {
                yield return Mapper.Map<AccountsDb, Account>(dc);
            }

        }

        /// <summary>
        /// Gets all standard accounts.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Account> GetAccountsBySubscription(byte Id)
        {
            var db = ObjectContextFactory.Create();
            IEnumerable<AccountsDb> accounts = Enumerable.Empty<AccountsDb>();
            if (Id != 0)
                accounts = db.Accounts.Where(c => c.Status == 1 && c.IsDeleted == false && c.SubscriptionID == Id).Select(s => new { AccountID = s.AccountID, AccountName = s.AccountName }).ToList().Select(c => new AccountsDb { AccountID = c.AccountID, AccountName = c.AccountName });
            else
                accounts = db.Accounts.Where(c => c.Status == 1 && c.IsDeleted == false && c.AccountID != 1).Select(s => new { AccountID = s.AccountID, AccountName = s.AccountName }).ToList().Select(c => new AccountsDb { AccountID = c.AccountID, AccountName = c.AccountName });
            foreach (AccountsDb dc in accounts)
            {
                yield return Mapper.Map<AccountsDb, Account>(dc);
            }

        }

        /// <summary>
        /// Gets all accounts ids.
        /// </summary>
        /// <returns></returns>
        public Dictionary<int, string> GetAllAccountsIds()
        {
            var db = ObjectContextFactory.Create();
            Dictionary<int, string> accounts = db.Accounts.Where(c => c.Status == 1 && c.AccountID != 1).Select(a => new { a.AccountID, a.AccountName }).ToDictionary(s => s.AccountID, s => s.AccountName);
            return accounts;
        }

        /// <summary>
        /// Inserts the account.
        /// </summary>
        /// <param name="Account">The account.</param>
        /// <returns></returns>
        public int InsertAccount(Account Account)
        {
            Logger.Current.Informational("Created the procedure for inserting a new Account");
            var procedureName = "";

            if (Account.SubscriptionID == (int)AccountSubscription.Standard)
                procedureName = "[dbo].[INSERT_ACCOUNT]";
            else if (Account.SubscriptionID == (int)AccountSubscription.BDX)
                procedureName = "[dbo].[INSERT_BDXACCOUNT]";

            DataTable addresses = ToAddressDataTable(Account.Addresses);
            DataTable account = ToAccountDataTable(Account);
            var parms = new List<SqlParameter>
                {
                    new SqlParameter{ParameterName="@AccountInfo", Value=account, SqlDbType= SqlDbType.Structured, TypeName="dbo.AccountInfo" },
                    new SqlParameter{ParameterName="@AddressInfo", Value=addresses, SqlDbType= SqlDbType.Structured, TypeName="dbo.AddressInfo" },
                    new SqlParameter{ParameterName="@TwilioFriendlyName", Value = Account.AccountName, SqlDbType = SqlDbType.VarChar}
                };


            var db = ObjectContextFactory.Create();
            int AccountID = db.ExecuteStoredProcedure<int>(procedureName, parms).FirstOrDefault();
            return AccountID;
        }

        /// <summary>
        /// Gets the account report.
        /// </summary>
        /// <returns></returns>
        //public List<AccountHealthReport> GetAccountReport()
        //{
        //    //var procedureName = "[dbo].[GET_Application_Report]";             
        //    var db = ObjectContextFactory.Create();
        //    List<AccountHealthReport> result = db.Database.SqlQuery<AccountHealthReport>("EXEC GET_Application_Report").ToList();
        //    return result;
        //}

        public DataSet GetAccountReport()
        {
            string conStr = System.Configuration.ConfigurationManager.ConnectionStrings["CRMDb"].ConnectionString;
            SqlConnection con = new SqlConnection(conStr);
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "GET_Application_Report";
            cmd.Connection = con;
            DataSet ds = new DataSet();
            try
            {
                con.Open();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.SelectCommand = cmd;
                da.Fill(ds);
            }
            catch (Exception ex)
            {

                throw ex;
            }
            finally
            {
                con.Close();
                con.Dispose();
            }
            return ds;
        }

        /// <summary>
        /// To the account data table.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public static DataTable ToAccountDataTable(Account item)
        {
            DataTable table = new DataTable();
            table.Columns.Add("AccountID", typeof(int));
            table.Columns.Add("AccountName", typeof(string));
            table.Columns.Add("FirstName", typeof(string));
            table.Columns.Add("LastName", typeof(string));
            table.Columns.Add("Company", typeof(string));
            table.Columns.Add("PrimaryEmail", typeof(string));
            table.Columns.Add("HomePhone", typeof(string));
            table.Columns.Add("WorkPhone", typeof(string));
            table.Columns.Add("MobilePhone", typeof(string));
            table.Columns.Add("PrivacyPolicy", typeof(string));
            table.Columns.Add("SubscriptionID", typeof(byte));
            table.Columns.Add("DateFormatID", typeof(byte));
            table.Columns.Add("CurrencyID", typeof(byte));
            table.Columns.Add("CountryID", typeof(string));
            table.Columns.Add("TimeZone", typeof(string));
            table.Columns.Add("Status", typeof(byte));
            table.Columns.Add("CreatedBy", typeof(int));
            table.Columns.Add("CreatedOn", typeof(DateTime));
            table.Columns.Add("ModifiedBy", typeof(int));
            table.Columns.Add("ModifiedOn", typeof(DateTime));
            table.Columns.Add("DomainURL", typeof(string));
            table.Columns.Add("FacebookUrl", typeof(string));
            table.Columns.Add("TwitterUrl", typeof(string));
            table.Columns.Add("GooglePlusUrl", typeof(string));
            table.Columns.Add("LinkedInUrl", typeof(string));
            table.Columns.Add("BlogUrl", typeof(string));
            table.Columns.Add("WebsiteUrl", typeof(string));
            table.Columns.Add("GoogleDriveAPIKey", typeof(string));
            table.Columns.Add("GoogleDriveClientID", typeof(string));
            table.Columns.Add("OpportunityCustomers", typeof(byte));
            table.Columns.Add("DropboxAppKey", typeof(string));
            table.Columns.Add("FacebookAPPID", typeof(string));
            table.Columns.Add("FacebookAPPSecret", typeof(string));
            table.Columns.Add("TwitterAPIKey", typeof(string));
            table.Columns.Add("TwitterAPISecret", typeof(string));
            table.Columns.Add("StatusMessage", typeof(string));
            table.Columns.Add("HelpURL", typeof(string));
            table.Columns.Add("ShowTC", typeof(bool));
            table.Columns.Add("TC", typeof(string));
            table.Columns.Add("UserLimit", typeof(int));
            table.Columns.Add("ExcludedRoles", typeof(string));

            table.Rows.Add(item.Id, item.AccountName, item.FirstName, item.LastName, item.Company, item.Email.EmailId, item.HomePhone == null ? null : item.HomePhone.Number, item.WorkPhone == null ? null : item.WorkPhone.Number,
                            item.MobilePhone == null ? null : item.MobilePhone.Number, item.PrivacyPolicy, item.SubscriptionID, item.DateFormatID, item.CurrencyID, item.CountryID, item.TimeZone, item.Status, item.CreatedBy,
                            item.CreatedOn, item.ModifiedBy, item.ModifiedOn, item.DomainURL, item.FacebookUrl == null ? null : item.FacebookUrl.URL, item.TwitterUrl == null ? null : item.TwitterUrl.URL,
                            item.GooglePlusUrl == null ? null : item.GooglePlusUrl.URL, item.LinkedInUrl == null ? null : item.LinkedInUrl.URL, item.BlogUrl == null ? null : item.BlogUrl.URL,
                            item.WebsiteUrl == null ? null : item.WebsiteUrl.URL, item.GoogleDriveAPIKey, item.GoogleDriveClientID, item.OpportunityCustomers, item.DropboxAppKey, item.FacebookAPPID,
                            item.FacebookAPPSecret, item.TwitterAPIKey, item.TwitterAPISecret, item.StatusMessage, item.HelpURL, item.ShowTC, item.TC, item.UserLimit, item.ExcludedRoles);

            return table;
        }

        /// <summary>
        /// To the address data table.
        /// </summary>
        /// <param name="addresses">The addresses.</param>
        /// <returns></returns>
        public static DataTable ToAddressDataTable(IEnumerable<Address> addresses)
        {
            DataTable dataTable = new DataTable(typeof(Address).Name);
            dataTable.Columns.Add("AddressID", typeof(int));
            dataTable.Columns.Add("AddressTypeID", typeof(short));
            dataTable.Columns.Add("AddressLine1", typeof(string));
            dataTable.Columns.Add("AddressLine2", typeof(string));
            dataTable.Columns.Add("City", typeof(string));
            dataTable.Columns.Add("StateID", typeof(string));
            dataTable.Columns.Add("CountryID", typeof(string));
            dataTable.Columns.Add("ZipCode", typeof(string));
            dataTable.Columns.Add("IsDefault", typeof(bool));

            foreach (Address item in addresses)
            {
                dataTable.Rows.Add(item.AddressID, item.AddressTypeID, item.AddressLine1, item.AddressLine2, item.City, item.State == null ? "" : item.State.Code,
                                    item.Country == null ? "" : item.Country.Code, item.ZipCode, item.IsDefault);
            }
            return dataTable;
        }

        /// <summary>
        /// Determines whether [is subscription change] [the specified account identifier].
        /// </summary>
        /// <param name="AccountID">The account identifier.</param>
        /// <param name="SubscriptionID">The subscription identifier.</param>
        /// <returns></returns>
        public bool isSubscriptionChange(int AccountID, byte SubscriptionID)
        {
            using (var db = ObjectContextFactory.Create())
            {
                bool isSubscriptionChanged = default(bool);
                isSubscriptionChanged = db.Accounts.Where(i => i.AccountID == AccountID && i.SubscriptionID == SubscriptionID).Count() == 0;
                return isSubscriptionChanged;
            }
        }

        /// <summary>
        /// Updates the default roles.
        /// </summary>
        /// <param name="AccountID">The account identifier.</param>
        /// <param name="SubscriptionID">The subscription identifier.</param>
        public void UpdateDefaultRoles(int AccountID, byte SubscriptionID)
        {
            using (var db = ObjectContextFactory.Create())
            {
                var updateRolesQuery = @"DELETE FROM dbo.RolePermissionsMap WHERE RolePermissionsMapID IN 
											(SELECT RolePermissionsMapID FROM dbo.Roles WHERE AccountID = @ACCOUNTID)
										 DELETE FROM dbo.Roles WHERE AccountID = @ACCOUNTID
											
										INSERT INTO dbo.Roles(RoleName, AccountID,SubscriptionID)
											SELECT RoleName, @AccountID,@SubscriptionID
											FROM dbo.Roles WHERE AccountID IS NULL AND SubscriptionID = @SubscriptionID

										-- MAP all default modules to newly created roles
										INSERT INTO dbo.RoleModuleMap(RoleID,ModuleID)
											SELECT R1.RoleID, M.ModuleID
											FROM Roles R 
												INNER JOIN dbo.RoleModuleMap RMM ON RMM.RoleID = R.RoleID
												INNER JOIN dbo.Modules M ON M.ModuleID = RMM.ModuleID
												INNER JOIN dbo.Roles R1 ON R.RoleName = R1.RoleName AND R1.AccountID = @AccountID
										 WHERE ISNULL(R.AccountID, 0) = 0 AND R.SubscriptionID = @SubscriptionID";
                db.Execute(updateRolesQuery, new { AccountID = AccountID, SubscriptionID = SubscriptionID });
            }
        }

        /// <summary>
        /// Gets the subscription data.
        /// </summary>
        /// <param name="AccountID">The account identifier.</param>
        /// <returns></returns>
        public AccountSubscriptionData GetSubscriptionData(int AccountID)
        {
            var sql = @"SELECT A.AccountID,M.ModuleID,M.Limit,M.SubscriptionID FROM Accounts (NOLOCK) A
                        INNER JOIN SubscriptionModuleMap (NOLOCK) M ON M.AccountID = A.AccountID AND A.SubscriptionID = M.SubscriptionID
                        WHERE A.AccountID = @AccountID  AND M.ModuleID = 2";
            var db = ObjectContextFactory.Create();
            var result = db.Get<AccountSubscriptionData>(sql, new { AccountID = AccountID });
            return result.FirstOrDefault();
        }

        /// <summary>
        ///Get Indexing Data.
        /// </summary>      
        /// <returns></returns>
        public IEnumerable<IndexingData> GetIndexingData(int chunkSize = 100)
        {
            var db = ObjectContextFactory.Create();

            var sql = @"SELECT ReferenceID, IndexType, EntityID, IsPercolationNeeded FROM IndexData (NOLOCK) ID
	                        WHERE Status = 1 ORDER BY CreatedOn
                            OFFSET 0 ROWS
                            FETCH NEXT @Chunk ROWS ONLY";
            var indexingDataDb = db.Get<IndexData>(sql, new { Chunk = chunkSize });

            IEnumerable<IndexingData> indexingData = indexingDataDb.GroupBy(p => p.IndexType).Select(p => new IndexingData()
                                                    {
                                                        IndexType = p.Select(m => m.IndexType).FirstOrDefault(),
                                                        EntityIDs = p.Select(m => m.EntityID).ToList(),
                                                        Ids = p.Select(t => new { t.EntityID, t.IsPercolationNeeded}).ToLookup(t => t.EntityID, t => t.IsPercolationNeeded),
                                                        ReferenceIDs = p.Select(s => s.ReferenceID).ToList()
                                                    });

            foreach (var item in indexingData)
            {
                Logger.Current.Informational("Application", item.IndexType.ToString() + "::Count=" + ((item.EntityIDs != null && item.EntityIDs.Any()) ? item.EntityIDs.Count.ToString() : "No Values"));
            }
            return indexingData;
        }

        /// <summary>
        ///Delete Indexed Data.
        /// </summary>      
        /// <returns></returns>
        public void DeleteIndexedData(IList<int> entityIds)
        {
            IEnumerable<int> entitys = entityIds.AsEnumerable();
            var db = ObjectContextFactory.Create();
            var sql = @"UPDATE A
                        SET Status = 2, IndexedOn = GETUTCDATE()
                        FROM IndexData A
                        JOIN @tbl B
                        ON B.ContactID = A.EntityID ";
            db.Execute(sql, new { tbl = entityIds.AsTableValuedParameter("dbo.Contact_List") });
        }

        /// <summary>
        /// Update index status to fail
        /// </summary>
        /// <param name="entityIds"></param>
        /// <param name="status"></param>
        public void UpdateIndexStatusToFail(IEnumerable<Guid> referenceIds, int status)
        {
            if (referenceIds.IsAny())
            {
                IEnumerable<SearchDefinitionContact> sdContacts = referenceIds.Select(s => new SearchDefinitionContact() 
                {
                    GroupId = s,
                    ContactId = 0,
                    SearchDefinitionId = 0
                });
                var db = ObjectContextFactory.Create();
                var sql = @"UPDATE A
                        SET Status = @status, IndexedOn = GETUTCDATE()
                        FROM IndexData A
                        JOIN @tbl B
                        ON B.GroupID = A.ReferenceID ";
                db.Execute(sql, new { tbl = sdContacts.AsTableValuedParameter("dbo.SavedSearchContacts", new string[] { "GroupId", "ContactId", "SearchDefinitionId" }), status = status });
            }
        }

        /// <summary>
        /// Inserts the indexing data.
        /// </summary>
        /// <param name="indexingData">The indexing data.</param>
        public void InsertIndexingData(IndexingData indexingData)
        {
            List<IndexData> indexdata = new List<IndexData>();
            foreach (var id in indexingData.EntityIDs)
            {
                IndexData idxdt = new IndexData();
                idxdt.ReferenceID = Guid.NewGuid();
                idxdt.EntityID = id;
                idxdt.IndexType = indexingData.IndexType;
                idxdt.Status = 1;
                idxdt.CreatedOn = DateTime.UtcNow;
                idxdt.IsPercolationNeeded = true;
                indexdata.Add(idxdt);
            }
            var db = ObjectContextFactory.Create();
            db.BulkInsert<IndexData>(indexdata);
            db.SaveChanges();
        }

        /// <summary>
        /// Gets the bulk operation data.
        /// </summary>
        /// <returns></returns>
        public BulkOperations GetBulkOperationData()
        {
            var db = ObjectContextFactory.Create();
            BulkOperationsDb bulkOperationsDb = db.BulkOperations.Where(b => b.Status == (byte)BulkOperationStatus.Created).FirstOrDefault();
            if (bulkOperationsDb != null)
                return Mapper.Map<BulkOperationsDb, BulkOperations>(bulkOperationsDb);
            else
                return null;
        }

        /// <summary>
        /// Gets the bulk contacts.
        /// </summary>
        /// <param name="operationID">The operation identifier.</param>
        /// <returns></returns>
        public int[] GetBulkContacts(int operationID)
        {
            var db = ObjectContextFactory.Create();
            int[] bulkContactids = db.BulkContactData.Where(p => p.BulkOperationID == operationID).Select(p => p.ContactID).ToArray();
            return bulkContactids;
        }

        /// <summary>
        /// Inserts the bulk data.
        /// </summary>
        /// <param name="contactIds">The contact ids.</param>
        /// <param name="bulkOperationId">The bulk operation identifier.</param>
        public void InsertBulkData(int[] contactIds, int bulkOperationId)
        {
            var db = ObjectContextFactory.Create();
            var procedureName = "[dbo].[DoBulkContactOperations]";

            IEnumerable<int> ids = contactIds.ToArray();
            Func<IEnumerable<int>, System.Data.DataTable> ConvertListToDataTable = (list) =>
            {
                // New table.
                System.Data.DataTable table = new System.Data.DataTable();
                table.Columns.Add();
                foreach (var array in list)
                {
                    table.Rows.Add(array);
                }
                return table;
            };
            System.Data.DataTable contacts = ConvertListToDataTable(ids);
            var parms = new List<SqlParameter>
                {                          
                    new SqlParameter{ ParameterName = "@Contacts", Value = contacts, SqlDbType = System.Data.SqlDbType.Structured, TypeName = "dbo.Contact_List" },
                    new SqlParameter{ ParameterName="@BulkOperationId", Value=bulkOperationId.ToString(), SqlDbType= SqlDbType.Int }
                };
            db.ExecuteStoredProcedure(procedureName, parms);
        }

        /// <summary>
        /// Deletes the bulk operation data.
        /// </summary>
        /// <param name="bulkOperationId">The bulk operation identifier.</param>
        public void DeleteBulkOperationData(int bulkOperationId)
        {
            var db = ObjectContextFactory.Create();
            var data = db.BulkOperations.Where(p => p.BulkOperationID == bulkOperationId).FirstOrDefault();
            if (data != null)
            {
                db.BulkOperations.Remove(data);

                var contactdata = db.BulkContactData.Where(p => p.BulkOperationID == bulkOperationId).ToArray();
                db.BulkContactData.RemoveRange(contactdata);

            }
            db.SaveChanges();
        }

        /// <summary>
        /// Updates the bulk operation status.
        /// </summary>
        /// <param name="bulkOperationId">The bulk operation identifier.</param>
        /// <param name="status">The status.</param>
        public void UpdateBulkOperationStatus(int bulkOperationId, BulkOperationStatus status)
        {
            var db = ObjectContextFactory.Create();
            var data = db.BulkOperations.Where(p => p.BulkOperationID == bulkOperationId).FirstOrDefault();
            if (data != null)
            {
                data.Status = (byte)status;
            }
            db.SaveChanges();
        }

        /// <summary>
        /// Gets the tc.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        public string GetTC(int accountId)
        {
            string tc = string.Empty;
            if (accountId != 0)
            {
                var db = ObjectContextFactory.Create();
                tc = db.Accounts.Where(w => w.AccountID == accountId && w.ShowTC).Select(s => s.TC).FirstOrDefault();
            }
            return tc ?? "";
        }

        /// <summary>
        /// Shows the tc.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        public bool ShowTC(int accountId)
        {
            bool showTC = default(bool);
            var db = ObjectContextFactory.Create();
            showTC = db.Accounts.Where(w => w.AccountID == accountId && !w.IsDeleted && !string.IsNullOrEmpty(w.TC)).Select(s => s.ShowTC).FirstOrDefault();
            return showTC;
        }

        /// <summary>
        /// Schedules the analytics refresh.
        /// </summary>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="entityType">Type of the entity.</param>
        public void ScheduleAnalyticsRefresh(int entityId, byte entityType)
        {
            var db = ObjectContextFactory.Create();
            //db.QueryStoredProc("dbo.ScheduleRefreshAnalytics",parameters: new { EntityType = entityType, entityId = entityId, Status = 1 });
            RefreshAnalyticsDb refreshAnalytics = new RefreshAnalyticsDb();
            refreshAnalytics.EntityID = entityId;
            refreshAnalytics.EntityType = entityType;
            refreshAnalytics.Status = 1;
            refreshAnalytics.LastModifiedOn = DateTime.Now.ToUniversalTime();
            db.RefreshAnalytics.Add(refreshAnalytics);
            db.SaveChanges();
        }

        /// <summary>
        /// Get Status Of Account
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        public AccountStatus GetAccountStatus(int accountId)
        {
            var db = ObjectContextFactory.Create();
            var sql = @"SELECT Status FROM Accounts (NOLOCK) WHERE AccountID = @id";
            return (AccountStatus)db.Get<byte>(sql, new { id = accountId }).FirstOrDefault();
        }

        public Address GetAccountAddress(int accountId)
        {
            var db = ObjectContextFactory.Create();
            ICollection<AddressesDb> address = db.Accounts.Where(w => w.AccountID == accountId).Select(s => s.Addresses).FirstOrDefault();
            if (address.Count > 0)
                return Mapper.Map<AddressesDb, Address>(address.OrderBy(o => o.AddressID).FirstOrDefault());
            else
                return null;
        }

        public bool? GetAccountDisclaimer(int accountId)
        {
            using (var db = ObjectContextFactory.Create())
            {
                var sql = @"SELECT Disclaimer FROM Accounts(NOLOCK) WHERE AccountID=@accountID AND IsDeleted=0";

                return db.Get<bool?>(sql, new { accountID = accountId }).FirstOrDefault();

            }
                
        }

        public UserActionActivitySummary GetUserContactActionsSummary(int userId, DateTime today,DateTime yesterday)
        {
            using (var db = ObjectContextFactory.Create())
            {
                UserActionActivitySummary actionSummary = new UserActionActivitySummary();
                var sql = @"SELECT ActionDetails  FROM Actions(NOLOCK) A
                            JOIN UserActionMap(NOLOCK) UA ON UA.ActionID = A.ActionID
                            WHERE UA.UserID=@userId AND A.ActionDate >= @yesterday AND A.ActionDate <= @today
                            SELECT ActionDetails  FROM Actions(NOLOCK) A
                            JOIN UserActionMap(NOLOCK) UA ON UA.ActionID = A.ActionID
                            WHERE UA.UserID=@userId AND A.RemindOn >= @yesterday AND A.RemindOn <= @today";

                db.GetMultiple(sql, (r) =>
                {
                    actionSummary.ActionDetails = r.Read<string>().ToList();
                    actionSummary.ReminderDetails = r.Read<string>().ToList();
                    
                }, new { userId = userId, yesterday = yesterday, today = today });

                return actionSummary;
            }
        }

        public string GetLitmusTestAPIKey(int accountId)
        {
            using (var db = ObjectContextFactory.Create())
            {
                var sql = @"SELECT LitmusAPIKey FROM Accounts(NOLOCK) WHERE AccountID=@accountID";
                return db.Get<string>(sql, new { accountID = accountId }).FirstOrDefault();
            }
        }

        public Account GetGoogleDriveAPIKey(int accountID)
        {
            var db = ObjectContextFactory.Create();
            return db.Accounts.Where(w => w.AccountID == accountID).Select(s => new Account() { GoogleDriveAPIKey = s.GoogleDriveAPIKey, GoogleDriveClientID = s.GoogleDriveClientID }).FirstOrDefault();
        }

        public IEnumerable<string> GetImageDomains(int accountId)
        { 
            IEnumerable<string> domains = new List<string>();
            if (accountId != 0)
            {
                var db = ObjectContextFactory.Create();
                var sql = @"SELECT DISTINCT ImageDomain FROM dbo.ServiceProviders(NOLOCK) SP
                            JOIN ImageDomains(NOLOCK) ID ON ID.ImageDomainID = SP.ImageDomainID
                            WHERE SP.AccountID = @AccountID AND SP.IsDefault = 1";
                domains = db.Get<string>(sql, new { AccountID = accountId });
            }
            return domains;
        }

        public byte GetSubscriptionIdByAccountId(int accountId)
        {
            var db = ObjectContextFactory.Create();
            var sql = @"SELECT A.SubscriptionID FROM Accounts (NOLOCK) A WHERE A.AccountID=@AccountId";
            return db.Get<byte>(sql, new { AccountId = accountId }).FirstOrDefault();
        }

        public int GetUsersCount(int accountID, IEnumerable<short> excludedRoles)
        {
            using (var db = ObjectContextFactory.Create())
            {
                int limit = 0;
                if (excludedRoles.IsAny())
                    limit = db.Users.Where(w => w.AccountID == accountID && !w.IsDeleted && w.Status == Status.Active && !excludedRoles.Contains(w.RoleID)).Count();
                else
                    limit = db.Users.Where(w => w.AccountID == accountID && !w.IsDeleted && w.Status == Status.Active).Count();
                return limit;
            }
        }

        public string GetNeverBouceValidationDoneFileName(int nerverBounceId)
        {
            using (var db = ObjectContextFactory.Create())
            {
                string name = string.Empty;
                db.QueryStoredProc("[dbo].[Getting_NeverBounce_Validation_Completed_Files]", (reader) =>
                 {
                     name = reader.Read<string>().FirstOrDefault();
                 }, new { NeverBounceRequestId = nerverBounceId });

                return name;
            }
        }

        public void BulkInsertForDeletedContactsInRefreshAnalytics(int[] contactIds)
        {
            var db = ObjectContextFactory.Create();
            List<RefreshAnalyticsDb> refreshAnalyticsDb = new List<RefreshAnalyticsDb>();
            if (contactIds.IsAny())
            {
                contactIds.Each(c =>
                {
                    RefreshAnalyticsDb refreshAnalytics = new RefreshAnalyticsDb();
                    refreshAnalytics.EntityID = c;
                    refreshAnalytics.EntityType = (byte)IndexType.Contacts_Delete;
                    refreshAnalytics.Status = 1;
                    refreshAnalytics.LastModifiedOn = DateTime.Now.ToUniversalTime();
                    refreshAnalyticsDb.Add(refreshAnalytics);
                });

                db.BulkInsert<RefreshAnalyticsDb>(refreshAnalyticsDb);
                db.SaveChanges();
            }
        }

        public string[] GetImageDoaminsById(List<byte> imageDomainIds)
        {
            using (var db = ObjectContextFactory.Create())
            {
                var sql = @"SELECT ImageDomain FROM ImageDomains (NOLOCK) WHERE ImageDomainID in @imageIds";
                return db.Get<string>(sql,new { imageIds= imageDomainIds }).ToArray();
            }
        }
    }
}

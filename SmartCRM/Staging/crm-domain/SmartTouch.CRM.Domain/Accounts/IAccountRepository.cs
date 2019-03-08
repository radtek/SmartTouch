using System;
using System.Collections.Generic;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Infrastructure.Domain;
using SmartTouch.CRM.Domain.WebAnalytics;
using SmartTouch.CRM.Domain.Images;
using System.ComponentModel;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Entities;

namespace SmartTouch.CRM.Domain.Accounts
{
    public interface IAccountRepository : IRepository<Account, int>
    {
        IEnumerable<Account> FindAll(string name, int limit, int pageNumber, byte status, string sortField, ListSortDirection listSortDirection);

        IEnumerable<Account> FindAll(string name, byte status, string sortField, ListSortDirection listSortDirection);

        List<string> UpdateAccountStatus(int[] accountId, byte StatusID);

        bool IsDuplicateAccount(string accountName, int accountId, string domainurl);

        Account FindByName(string name);

        Account FindByDomainUrl(string name);

        Address GetAddress(int accountId);

        string GetPrimaryPhone(int accountId);

        string GetAccountPrimaryEmail(int accountId);

        string GetUserTimeZone(int AccountId);

        bool CheckDomainAvailability(string domainurl);

        byte? GetOpportunityCustomers(int accountId);

        List<byte> GetAccountPermissions(int accountId);

        void InsertDefaultRoles(int accountID);

        void InsertDefaultRoleConfigurations(int accountId, List<byte> moduleIds);

        void InsertDataSharingPermissions(int accountId, List<byte> moduleIds, byte subscriptionId);

        IEnumerable<byte> GetPrivateModules(int accountId);

        List<DailySummaryEmail> GetDailySummaryEmails();

        List<ContactAccountGroup> GetContactCampaignByAccount(int accountId);

        void InsertDailySummaryEmailAudit(int userId, byte status);

        bool GetModuleSharingPermission(int moduleId, int accountId);

        Account FindByAccountID(int accountId);

        List<WebAnalyticsProvider> GetWebAnalyticsProviders();

        List<WebAnalyticsProvider> GetAccountWebAnalyticsProviders(int accountId);

        void SaveAccountLogo(Image image);

        string GetAccountDomainUrl(int accountId);

        Account GetAccountMinDetails(int accountId);

        int GetAccountIdByContactId(int contactId);

        string GetDropboxKey(int accountId);

        Email GetServiceProviderEmail(int serviceProviderId);

        string GetAccountPrivacyPolicy(int accountId);

        AccountLogoInfo GetImageStorageName(int accountId);

        Account GetAccountBasicDetails(int AccountID);

        IEnumerable<Account> GetAllAccounts();

        IEnumerable<Account> GetAccountsBySubscription(byte Id);

        Dictionary<int, string> GetAllAccountsIds();

        int InsertAccount(Account Account);

        bool isSubscriptionChange(int AccountID, byte SubscriptionID);

        void UpdateDefaultRoles(int AccountID, byte SubscriptionID);

        AccountSubscriptionData GetSubscriptionData(int AccountID);

        int GetReputationCount(int accountId, DateTime startDate, DateTime endDate);

        AccountsList FindAllAccounts(string p1, int p2, int p3, byte p4, string p5, ListSortDirection listSortDirection);

        Account GetAccount(int accountId, bool isSTAdmin);

        IEnumerable<IndexingData> GetIndexingData(int chunkSize);

        void DeleteIndexedData(IList<int> entityIds);

        void InsertIndexingData(IndexingData indexingData);

        System.Data.DataSet GetAccountReport();

        BulkOperations GetBulkOperationData();

        void InsertBulkData(int[] contactIds, int bulkOperationId);

        void DeleteBulkOperationData(int bulkOperationId);

        int[] GetBulkContacts(int operationID);

        void UpdateIndexStatusToFail(IEnumerable<Guid> referenceIds, int status);

        void UpdateBulkOperationStatus(int bulkOperationId, BulkOperationStatus status);

        List<BdxAccounts> GetBdxAccounts(string accountName);

        string GetDomainUrlByAccountId(int accountId);

        IEnumerable<SubscriptionSettings> GetSubscriptionSettings(int subscriptionId, string doaminUrl);

        AccountSubscriptionData GetSubscriptionDataByAccountID(int accountId);

        string GetTC(int accountId);

        bool ShowTC(int accountId);

        void ScheduleAnalyticsRefresh(int entityId, byte entityType);

        AccountStatus GetAccountStatus(int accountId);

        Address GetAccountAddress(int accountId);

        bool? GetAccountDisclaimer(int accountId);

        UserActionActivitySummary GetUserContactActionsSummary(int userId,DateTime today,DateTime yesterday);
        string GetLitmusTestAPIKey(int accountId);

        Account GetGoogleDriveAPIKey(int accountID);
        IEnumerable<string> GetImageDomains(int accountId);
        void UpdateUserLimit(int accountId, int? limit, IEnumerable<short> excludedRoles);
        byte GetSubscriptionIdByAccountId(int accountId);
        int GetUsersCount(int accountID, IEnumerable<short> excludedRoles);
        string GetNeverBouceValidationDoneFileName(int nerverBounceId);
        void BulkInsertForDeletedContactsInRefreshAnalytics(int[] contactIds);
        string[] GetImageDoaminsById(List<byte> imageDomainIds);
    }
}

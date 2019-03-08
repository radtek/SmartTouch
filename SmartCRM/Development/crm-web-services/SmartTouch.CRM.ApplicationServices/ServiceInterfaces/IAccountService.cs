using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.ApplicationServices.Messaging.Accounts;
using SmartTouch.CRM.ApplicationServices.Messaging.ImportData;
using System;
using SmartTouch.CRM.ApplicationServices.Messaging.Communication;
using System.Collections.Generic;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Domain.Accounts;
using SmartTouch.CRM.ApplicationServices.Messaging.Contacts;

namespace SmartTouch.CRM.ApplicationServices.ServiceInterfaces
{
    public interface IAccountService
    {
        GetAccountsResponse GetAccounts();
        GetAccountsResponse GetAllAccounts(GetAccountsRequest request);
        GetAccountResponse GetAccount(GetAccountRequest request);

        InsertAccountResponse InsertAccount(InsertAccountRequest request);
        UpdateAccountResponse UpdateAccount(UpdateAccountRequest request);
        DeleteAccountResponse DeleteAccount(DeleteAccountRequest request);

        AccountStatusUpdateResponse UpdateAccountStatus(AccountStatusUpdateRequest request);
        GetAccountResponse GetAccountByName(GetAccountNameRequest request);
        GetAddressResponse GetPrimaryAddress(GetAddressRequest request);
        GetPrimaryPhoneResponse GetPrimaryPhone(GetPrimaryPhoneRequest request);

        GetImportdataResponse GetImportData(GetImportdataRequest request);
        ImportDataResponse ImportData(ImportDataRequest request);
        GetAccountAuthorizationResponse GetAccountByDomainUrl(GetAccountAuthorizationRequest request);

        GetImportsResponse GetAllImports(GetImportsRequest request);
        GetDataAccessPermissionsResponse GetSharingPermissions(GetDataAccessPermissionsRequest request);
        GetPrivateModulesResponse GetPrivateModules(GetPrivateModulesRequest request);
        GetAccountPermissionsResponse GetAccountPermissions(GetAccountPermissionsRequest request);
        AccountViewModel CopyAccount(AccountViewModel model);

        void SendDailySummaryEmails(GetDailySummaryEmailsRequest emailsRequest);
        List<ContactGroup> GetAllContactsByAccount(int accountId);        
        GetModuleSharingPermissionResponse GetModuleSharingPermission(GetModuleSharingPermissionRequest request);

        CheckDomainURLAvailabilityResponse IsDomainURLExist(CheckDomainURLAvailabilityRequest request);
        Guid GetAccountEmailProvider(int accountId);

        GetImportForAccountResponse GetImportDataByAccountID(GetImportForAccountRequest request);

        Account GetAccountMinDetails(int accountId);
        GetAccountResponse GetAccountById(GetAccountIdRequest request);
        GetWebAnalyticsProvidersResponse GetWebAnalyticsProviders(GetWebAnalyticsProvidersRequest request);
        GetWebAnalyticsProvidersResponse GetAccountWebAnalyticsProviders(GetWebAnalyticsProvidersRequest request);

        void SaveAccountLogo(ImageViewModel image);
        GetAccountDomainUrlResponse GetAccountDomainUrl(GetAccountDomainUrlRequest request);
        GetAccountIdByContactIdResponse GetAccountIdByContactId(GetAccountIdByContactIdRequest request);
        GetAccountDropboxKeyResponse GetAccountDropboxKey(GetAccountIdRequest request);
        GetServiceProviderSenderEmailResponse GetDefaultBulkEmailProvider(GetServiceProviderSenderEmailRequest request);
        string GetAccountPrivacyPolicy(int accountId);
        GetAccountImageStorageNameResponse GetStorageName(GetAccountImageStorageNameRequest request);
        GetAccountListResponse GetAllAccounts();
        GetAllAccountsBySubscriptionResponse GetAllAccountsBySubscription(GetAllAccountsBySubscriptionRequest request);
        GetSenderReputationCountResponse GetReputationCount(GetSenderReputationCountRequest request);
        GetAccountPrimaryEmailResponse GetAccountPrimaryEmail(GetAccountPrimaryEmailRequest request);
        Dictionary<Guid, string> GetTransactionalProviderDetails(int accountId);
        AccountHealthReport GetAccountReportData();
        Dictionary<int, string> GetAllAccountsIds();
        GetImportedContactsResponse GetImportedContacts(GetImportedContactsRequest getImportedContactsRequest);
        GetIndexingDataResponce GetIndexingData(GetIndexingDataRequest getIndexingDataRequest);
        void DeleteIndexedData(IList<int> entityIds);
        InsertIndexingDataResponce InsertIndexingData(InsertIndexingDataRequest request);
        InsertBulkOperationResponse InsertBulkOperation(InsertBulkOperationRequest bulkOperationRequest);
        GetBulkOperationDataResponse GetBulkOperationData(GetBulkOperationDataRequest getBulkOperationDataRequest);
        GetBulkOperationDataResponse GetQueuedBulkOperationData(GetBulkOperationDataRequest getBulkOperationDataRequest);
        void InsertBulkData(int[] contactIds,int bulkOperationId);
        void DeleteBulkOperationData(int bulkOperationId);
        string GetRowData(int newDataId, int oldDataId);
        UpdateIndexingStatusResponse UpdateIndexingStatus(UpdateIndexingStatusRequest request);
        UpdateBulkOperationStatusResponse UpdateBulkOperationStatus(UpdateBulkOperationStatusRequest request);
        GetBdxAccountsResponse GetBdxAccounts(GetBdxAccountsRequest request);
        string GetDomainUrlByAccountId(int accountId);
        GetSubscriptionSettingsResponse GetSubscriptionSettings(GetSubscriptionSettingsRequest request);
        GetAccountSubscriptionDataResponse GetSubscriptionDataByAccountID(GetAccountSubscriptionDataRequest request);
        GetTermsAndConditionsResponse GetTermsAndConditions(GetTermsAndConditionsRequest request);
        ShowTCResponse ShowTC(ShowTCRequest request);
        GetFirstLoginUserSettingsResponse GetFirstLoginUserSettings(GetFirstLoginUserSettingsRequest request);
        UpdateTCAcceptanceResponse UpdateTCAcceptance(UpdateTCAcceptanceRequest request);
        void ScheduleAnalyticsRefresh(int entityId ,byte entityType);
        bool? AccountHasDisclaimer(int accountId);
        string GettingLitmusTestAPIKey(int accountId);
        GetGoogleDriveAPIKeyResponse GetGoogleDriveAPIKey(GetGoogleDriveAPIKeyRequest request);

        GetNeverBounceResponse GetNeverBounceRequests(GetNeverBounceRequest request);
        GetNeverBounceAcceptedResponse GetAcceptedRequests(GetNeverBounceAcceptedRequests requests);
        //GetContactEmailsResponse GetContactEmails(GetContactEmailsRequest request);
        UpdateNeverBounceResponse UpdateNeverBounceRequest(UpdateNeverBounceRequest request);
        UpdateNeverBouncePollingResponse UpdateNeverBouncePollingRequest(UpdateNeverBouncePollingRequest request);
        UpdateNeverBounceResponse UpdateScrubQueueRequests(UpdateNeverBounceRequest request);
        UpdateEmailStatusResponse InsertEmailStatuses(UpdateEmailStatusRequest request);
        NeverBounceEmailDataResponse GetEmailData(NeverBounceEmailDataRequest request);

        InsertNeverBounceResponse InsertNeverBounceRequest(InsertNeverBounceRequest request);
        string GetNeverBouceValidationDoneFileName(int neverBouceRequestId);
        void BulkInsertForDeletedContactsInRefreshAnalytics(int[] contactIds);
    }
}

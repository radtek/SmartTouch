using SmartTouch.CRM.ApplicationServices.Messaging.Campaigns;
using SmartTouch.CRM.ApplicationServices.Messaging.Contacts;
using SmartTouch.CRM.ApplicationServices.Messaging.CustomFields;
using SmartTouch.CRM.ApplicationServices.Messaging.Dashboard;
using SmartTouch.CRM.ApplicationServices.Messaging.ImplicitSync;
using SmartTouch.CRM.ApplicationServices.Messaging.Search;
using SmartTouch.CRM.ApplicationServices.Messaging.User;
using SmartTouch.CRM.ApplicationServices.Messaging.WebAnalytics;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.Users;
using SmartTouch.CRM.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ServiceInterfaces
{
    public interface IContactService
    {
        SearchContactsResponse<T> GetAllContacts<T>(SearchContactsRequest request) where T : IShallowContact;
        SearchContactsResponse<T> GetPersons<T>(SearchContactsRequest request) where T : IShallowContact;
        SearchContactsResponse<T> GetCompanies<T>(SearchContactsRequest request) where T : IShallowContact;

        DeleteContactResponse DeleteContact(int id);//DeleteContactRequest request

        GetPersonResponse GetPerson(GetPersonRequest request);
        InsertPersonResponse InsertPerson(InsertPersonRequest request);
        UpdatePersonResponse UpdatePerson(UpdatePersonRequest request);

        GetCompanyResponse GetCompany(GetCompanyRequest request);
        InsertCompanyResponse InsertCompany(InsertCompanyRequest request);
        UpdateCompanyResponse UpdateCompany(UpdateCompanyRequest request);

        DeactivateContactResponse Deactivate(DeactivateContactRequest request);
        DeactivateContactsResponse DeactivateContacts(DeactivateContactsRequest request);
        PersonViewModel CopyPerson(PersonViewModel model);
        CompanyViewModel CopyCompany(CompanyViewModel model);

        ReIndexContactsResponse ReIndexContacts(ReIndexContactsRequest request);
        //IndexCompanyNameResponse IndexCompanyNames(IndexCompanyNameRequest request);
        DeleteIndexResponse DeleteIndex(DeleteIndexRequest request);
        FindMatchedSavedSearchQueryResponse FindMatchedSearchQuery(FindMatchedSavedSearchQueryRequest request);
        FindMatchedSavedSearchesResponse FindMatchedSavedSearches(FindMatchedSavedSearchesRequest request);

        AutoCompleteSearchResponse SearchCompanyByName(AutoCompleteSearchRequest request);
        AutoCompleteSearchResponse SearchContactWithEmailId(AutoCompleteSearchRequest request);
        AutoCompleteSearchResponse SearchContactWithPhone(AutoCompleteSearchRequest request);

        Task<GetTimeLineResponse> GetTimeLinesDataAsync(GetTimeLineRequest request);
        AutoCompleteSearchResponse SearchContactTitles(AutoCompleteSearchRequest request);
        AutoCompleteSearchResponse SearchContactFullName(AutoCompleteSearchRequest request);

        GetContactTypeResponse GetContactType(GetContactTypeRequest request);
        GetUsersResponse GetUsers(GetUsersRequest request);
        ChangeOwnerResponce ChangeOwner(ChangeOwnerRequest request);

        AutoCompleteResponse SearchContactFullNameforRelation(AutoCompleteSearchRequest request);

        DeleteRelationResponse DeleteRelationship(DeleteRelationRequest request);
        //    GetRelationshipResponse GetRelationShip(GetRelationShipRequest request);
        Task<ExportPersonsResponse> GetAllContactsByIds(ExportPersonsRequest request);
        GetImportedContactResponse GetImportedContacts(GetImportedContactRequest request);
        GetTagContactsResponse GetTagRelatedContacts(GetTagContactsRequest request);
        SendEmailResponse GetEmailSignatures(SendEmailRequest request);
        GetContactLeadScoreResponse GetLeadScore(GetContactLeadScoreRequest request);

        Task<GetContactCampaignStatisticsResponse> GetContactCampaignSummary(GetContactCampaignStatisticsRequest request);
        GetOpportunitySummaryResponse GetOpportunitySummary(GetOpportunitySummaryRequest request);
        GetPersonsCountResponse GetPersonsCount(GetPersonsCountRequest request);

        IEnumerable<Contact> GetAllContactsByCompanyIds(List<int?> Campanyids, int accountId);

        GetRecentViwedContactsResponse GetContactByUserId(GetRecentViwedContactsRequest request);
        GetClientIPAddressResponse GetClientIPAddress(GetClientIPAddressRequest request);
        ContactLeadScoreListResponse GetContactLeadScoreList(ContactLeadScoreListRequest request);
        GetContactIDsByIPResponse GetContactIDsByIP(GetContactIDsByIPRequest request);
        GetContactsByReferenceIdsResponse GetContactsByReferenceIds(GetContactsByReferenceIdsRequest request);

        CompareKnownContactIdentitiesResponse CompareKnownIdentities(CompareKnownContactIdentitiesRequest request);

        ChangeLifecycleResponse ChangeLifecycle(ChangeLifecycleRequest changeLifecycleRequest);
        AssignUserResponse AssignUser(AssignUserRequest request);
        GetWorkflowContactsResponse GetWorkflowContacts(GetWorkflowContactsRequest request);
        GetCampaignContactsResponse GetCampaignContacts(GetCampaignContactsRequest request);
        GetWorkflowContactsResponse GetWorkflowRelatedContacts(GetWorkflowContactsRequest request);

        GetDashboardChartDetailsResponse GetNewLeadsChartDetails(GetDashboardChartDetailsRequest request);
        Task<GetContactWebVisitsCountResponse> GetWebVisitsCount(GetContactWebVisitsCountRequest request);
        GetContactWebVisitReportResponse GetContactWebVisits(GetContactWebVisitReportRequest request);
        Task<GetContactAuditLeadScoreResponse> GetContactLeadScore(GetContactAuditLeadScoreRequest getContactAuditLeadScoreRequest);

        UpdateContactViewResponse UpdateContactName(UpdateContactViewRequest request);

        UpdateContactViewResponse UpdateContactPhone(UpdateContactViewRequest request);

        UpdateContactViewResponse UpdateContactLifecycleStage(UpdateContactViewRequest request);

        UpdateContactViewResponse UpdateContactLeadSource(UpdateContactViewRequest request);

        UpdateContactViewResponse UpdateContactEmail(UpdateContactViewRequest request);

        GetActionRelatedContactsResponce GetActionRelatedContacts(GetActionRelatedContactsRequest request);

        UpdateContactViewResponse UpdateContactAddresses(UpdateContactViewRequest request);

        UpdateContactViewResponse UpdateContactImage(UpdateContactViewRequest request);

        UpdateContactViewResponse UpdateCompanyName(UpdateContactViewRequest request);

        UpdateContactViewResponse UpdateContactTitle(UpdateContactViewRequest request);

        GetContactsToSyncResponse GetContactsToSync(GetContactsToSyncRequest request);
        UpdateSyncedEntitiesResponse UpdateSyncedEntities(UpdateSyncedEntitiesRequest request);

        //GetTasksToSyncResponse GetTasksToSync(GetTasksToSyncRequest request);
        GetContactsToSyncResponse GetDeletedContactsToSync(GetContactsToSyncRequest request);
        InserImplicitSyncEmailInfoResponse InsertImplicitSyncEmailUpload(InserImplicitSyncEmailInfoRequest request);
        OutllokEmailSentResposne isOutlookEmailAlreadySynced(OutlookEmailSentRequest request);
        FindContactsByPrimaryEmailsResponse FindContactsByPrimaryEmails(FindContactsByPrimaryEmailsRequest request);
        FindContactsByEmailResponse FindContactsByEmail(FindContactsByEmailRequest request);
        GetSearchDefinitionContactsResponce GetSearchDefinitionContacts(GetSearchDefinitionContactsRequest request);

        Task<GetEngagementDetailsResponse> GetEngagementInformation(GetEngagementDetailsRequest request);
        ContactEmailEngagementDetails GetContactEmailEngagementDetails(int contactId, int accountId);
        GetContactEmailIdResponse GetEmailID(GetContactEmailIdRequest request);
        GetContactFormSubmissionsResponse GetContactSubmittedForms(GetContactFormSubmissionsRequest request);
        GetContactsByIDsResponse GetAllContactsByIds(GetContactsByIDsRequest request);
        void addToTopicFromImportsAndLeadadapter(int contactId, short dropdownValueId, int accountId);
        GetContactWebVisitsSummaryResponse GetContactWebVisitsSummary(GetContactWebVisitsSummaryRequest request);
        GetWebVisitByVisitIDResponse GetWebVisitByVisitID(GetWebVisitByVisitIDRequest request);
        ContactIndexingResponce ContactIndexing(ContactIndexingRequest contactIndexingRequest);
        ImportDataUpdateResponce ImportDataUpdate(ImportDataUpdateRequest contactIndexingRequest);
        GetOutlookEmailInformationResponse GetEmailInformation(GetOutlookEmailInformationRequest request);
        void InsertOutlookEmailAuditInformation(InsertOutlookEmailAuditInformationRequest request);
        GetContactCreatorsInfoResponse GetContactCreatorsInfo(GetContactCreatorsInfoRequest request);
        FindContactsOfUserByFirstAndLastNameResponse FindContactsOfUserByFirstAndLastName(FindContactsOfUserByFirstAndLastNameRequest request);
        GetBulkContactsResponse GetBulkContacts(GetBulkContactsRequest request);

        User UpdateOwnerBulkData(int ownerId, int userId, int accountId, IEnumerable<int> contacts);

        GetContactsDataResponce GetContactsData(GetContactsDataRequest getContactsDataRequest);

        Task UpdateBulkExcelExport(BulkOperations operations, IEnumerable<Contact> contactIds, IEnumerable<FieldViewModel> searchFields);
        void UpdateDeleteBulkData(int operationId, int userId, int accountId, int[] contactIds);

        CheckContactDuplicateResponse CheckIfDuplicate(CheckContactDuplicateRequest request);
        GetContactSummaryResponse GetContactSummary(GetContactSummaryRequest request);

        CustomContactViewModel PersonDuplicateCheck(InsertPersonRequest request);
        CustomContactViewModel CompanyDuplicateCheck(InsertCompanyRequest request);

        Task<System.Data.DataTable> GetDataTable(List<FieldViewModel> selectedColumns, IEnumerable<SmartTouch.CRM.Domain.Fields.Field> searchFields,
                     IEnumerable<Contact> Contacts, int accountId, IEnumerable<Owner> owners, SmartTouch.CRM.Entities.DownloadType fileType, string dateFormat, string timeZone);

        GetAllCustomFieldTabsResponse GetCustomFieldTabs(GetAllCustomFieldTabsRequest request);

        UpdateContactCustomFieldResponse UpdateContactCustomField(UpdateContactCustomFieldRequest request);
        InsertAPILeadSubmissionResponse InsertAPILeadSubmissionData(InsertAPILeadSubmissionRequest request);
        GetAPILeadSubmissionDataResponse GetAPILeadSubMissionData();
        GetContactEngagementSummaryResponse GetContactWorkflowSummaryDetails(GetContactEngagementSummaryRequest request);
        GetContactEngagementSummaryResponse GetContactEmailSummaryDetails(GetContactEngagementSummaryRequest request);
        GetContactEngagementSummaryResponse GetContactCampaignSummaryDetails(GetContactEngagementSummaryRequest request);
        RemoveFromElasticResponse RemoveFromElastic(RemoveFromElasticRequest request);
        InsertEmailOpenOrClickEntryResponse InsertEmailClickEntry(InsertEmailOpenOrClickEntryRequest request);
        GetEmailLinkURLResponse GetSendMailDetailIdByLinkId(GetEmailLinkURLRequest request);
        GetEngagementDetailsResponse GetEmailStastics(GetEngagementDetailsRequest request);

        ProcessFullContactResponse ProcessFullContactSync(ProcessFullContactRequest request);
        GetNeverBounceBadEmailContactResponse GetNeverBounceBadEmailContacts(GetNeverBounceBadEmailContactRequest request);
        List<LinkClickedDetails> GetEmailClickedLinkURLs(int sentMailDetailedId,int contactId);
        List<LinkClickedDetails> GetCampaignClickedLinkURLs(int campaignId, int recipientId);
    }
}

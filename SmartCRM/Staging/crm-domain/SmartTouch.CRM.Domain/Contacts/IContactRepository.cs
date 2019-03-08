using System.Collections.Generic;
using SmartTouch.CRM.Infrastructure.Domain;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Entities;
using System;
using SmartTouch.CRM.Domain.Campaigns;
using System.Threading.Tasks;
using SmartTouch.CRM.Domain.Dropdowns;
using SmartTouch.CRM.Domain.Images;
using SmartTouch.CRM.Domain.ImplicitSync;
using SmartTouch.CRM.Domain.Forms;
using SmartTouch.CRM.Domain.Communications;

namespace SmartTouch.CRM.Domain.Contacts
{
    public interface IContactRepository : IRepository<Contact, int>
    {
        IEnumerable<Contact> FindAll(string name);
        IEnumerable<Contact> FindAll(int pageNumber, int limit);
        // this method is for reindexing the contacts from the lead adatpers.
        IEnumerable<Contact> FindAll(IList<int> ContactID, bool isElastic = true);

        IEnumerable<Contact> FindPersons(string name);
        IEnumerable<Contact> FindCompanies(string name);
        IEnumerable<int> GetContactsByImport(int LeadAdapterJobID, string recordStatus);
        IEnumerable<int> GetContactsByTag(int TagID, int TagType, int accountID);
        IEnumerable<State> GetStates(string countryCode);
        IEnumerable<Country> GetCountries();
        IEnumerable<Owner> GetUsers(int AccountId, int UserId, bool IsSTadmin);
        IEnumerable<Owner> GetUserNames(IEnumerable<int?> OwnerIds);
        ContactType GetContactType(int contactId);
        bool IsDuplicatePerson(string firstName, string lastName, string primaryEmail, string company, int contactId, int accountId);
        bool IsDuplicateCompany(string companyName, int contactId, int accountId);

        void DeactivateContact(int contactId, int updatedBy,int accountId);
        void ChangeOwner(int? OwnerId, IEnumerable<int> Contacts,int accountId);

        int DeactivateContactsList(int[] contactids, int updatedBy,int accountId);
        Task<IEnumerable<TimeLineContact>> FindTimelinesAsync(int? contactId, int? OpportunityID, int limit, int pageNumber, string module, string period, string PageName, string[] Activities, DateTime fromDate, DateTime toDate);
        Task<IEnumerable<TimeLineContact>> FindTimelinesAsync2(int? ContactID, int? OpportunityID, int limit, int pageNumber, string module, string period, string PageName, string[] Activities, DateTime fromDate, DateTime toDate, int AccountID);
        int FindTimelinesTotalRecords(int? contactId, int? OpportunityID, string module, string period, string PageName, string[] Activities, DateTime fromDate, DateTime toDate, out IEnumerable<TimeLineGroup> timeLineGroup);
        int FindTimelinesTotalRecords2(int? contactId, int? OpportunityID, string module, string period, string PageName, string[] Activities, DateTime fromDate, DateTime toDate, out IEnumerable<TimeLineGroup> timeLineGroup, int AccountID);
        void DeleteRelation(int relationId);
        IEnumerable<Contact> FetchImages(IEnumerable<Contact> lstContact);

        Contact FindBy(int contactId, int accountId);
        Contact FindByContactId(int contactId, int accountId);
        Contact GetContactsById(int ContactID);
        IEnumerable<Contact> GetContacts(IEnumerable<int> contactIds,int accountId);
        IEnumerable<Contact> GetAllContactsByUserIds(int[] OwnerIds);

        IEnumerable<Email> GetEmailSignaturesBy(int UserID, int accountId);
        int GetLeadScore(int contactId);
        IEnumerable<State> GetAllStates();

        OpportunitySummary GetOpportunitySummary(int contactId, string Period);
        int GetPersonsCount(int contactId, int accountId);
        List<int> GetPersonsOfCompany(int contactId, int accountId);
        Company FindCompanyName(string name, int companyId, int accountId);
        IList<int> GetContactByUserId(int p,int[] contactIds,int accountId);
        Email UpdateContactEmail(int contactId, string email, EmailStatus status,int accountId, short? ComplainedStatus);

        IEnumerable<Contact> GetAllContactByIds(List<int?> Campanyids,int accountId);
        int GetContactIdByEmailAndCampaign(string email, int campaignId, int accountId);

        Task<CampaignStatistics> GetContactCampaignSummary(int contactID, DateTime period, int accountId);
        Task<EmailStatistics> GetContactEmailStatistics(int accountId, int contactId, DateTime period);
        void TrackContactIPAddress(int contactId, string ipAddress, string stiTrackingId);
        IEnumerable<Contact> GetContactLeadScoreList(int accountID);

        Person ChangeLifecycle(short dropdownValueId, int contactId,int accountId);
        void ChangeOwner(int userId, int contactId);
        IEnumerable<int> FindContactIdsByIP(string ip);
        IEnumerable<int> FindContactIdsByReferenceIds(IEnumerable<string> referenceIds);
        IEnumerable<string> FilterKnownIps(IEnumerable<string> ips, int accountId);
        IEnumerable<string> FilterKnownIdentities(IEnumerable<string> identities, int accountId);

        IEnumerable<dynamic> GetTopFiveLeadSources(DateTime fromDate, DateTime toDate, int accountID, int[] OwnerIds);
        IEnumerable<dynamic> GetTopLeadsByCustomDate(DateTime fromDate, DateTime toDate, int accountID, int[] OwnerIds,
            bool IsCompared, DateTime previousDate, out int previousLeadCount);

        IEnumerable<int> GetContactsForWorkflow(short WorkflowID, WorkflowContactsState WorkflowContactState);
        IEnumerable<int> GetContactsForCampaign(int CampaignID, CampaignDrillDownActivity CampaignDrillDownActivity, int accountId, int? CampaignLinkID);
        IEnumerable<int> GetWorkflowContactsForCampaign(short WorkflowID, int campaignID, CampaignDrillDownActivity CampaignDrillDownActivity,DateTime? fromDate,DateTime? toDate);

        IList<int> GetContactsByUserIds(int[] ownerIds, int accountId);
        IEnumerable<DashboardPieChartDetails> NewLeadsPieChartDetails(int accountID, int userID, bool isAccountAdmin, DateTime fromDate, DateTime toDate);
        IEnumerable<DashboardAreaChart> NewLeadsAreaChartDetails(int accountID, int userID, bool isAccountAdmin, out int previousCount, DateTime fromDate, DateTime toDate);
        Task<int> GetContactWebVisitsCount(int contactId, DateTime period);
        IEnumerable<WebVisit> GetContactWebVisits(int contactId, string period);
        Task<int> GetContactLeadScore(int contactId, DateTime period, int accountId);

        int? FindCreatedUserByContactId(int contactId);
        void UpdateLastTouched(int contactId, DateTime lastContactedOn, byte moduleId,int accountId);
        void UpdateLastTouchedInformation(List<LastTouchedDetails> LastTouchedInformation, AppModules ModuleID,short? ActionType);
        IEnumerable<int> GetContactByEmailID(IEnumerable<int> ContactEmailIDs);
        IEnumerable<int> GetContactByPhoneNumberID(IEnumerable<int> ContactPhoneNumberIDs);

        IEnumerable<ContactCreatorInfo> GetContactCreatorsInfo(IEnumerable<int> contactIds);
        void UpdateLifecycleStage(int ContactID, short LifecycleStageID);
        bool CheckIsDeletedContact(int contactId, int accountId);

        int UpdateContactName(int contactId, string firstName, string lastName, DateTime lastUpdatedOn, int? lastUpdatedBy,int accountId);

        int UpdateContactPhone(int contactId, Phone phone, int accountId, DateTime lastUpdatedOn, int? lastUpdatedBy);

        int UpdateContactLifecycleStage(int contactId, short lifecycleStage, DateTime lastUpdatedOn, int? lastUpdatedBy,int accountId);

        int UpdateContactLeadSource(int contactId, short leadsourceId, DateTime lastUpdatedOn, int? lastUpdatedBy,int accountId);

        int UpdateContactEmail(int contactId, Email email, int accountId, DateTime lastUpdatedOn, int? lastUpdatedBy);

        IList<int> GetActionRelatedContacts(int actionId);

        int UpdateContactAddresses(int contactId, Address address, DateTime lastUpdatedOn, int? lastUpdatedBy,int accountId);

        int UpdateContactImage(int contactId, Image image, int accountId, int? userId, DateTime lastUpdatedOn, int? lastUpdatedBy);

        int UpdateCompanyName(int contactId, string companyName, DateTime lastUpdatedOn, int? lastUpdatedBy,int accountId);

        int UpdateContactTitle(int contactId, string title, DateTime lastUpdatedOn, int? lastUpdatedBy,int accountId);
        IEnumerable<Company> GetCompanyDetails(IEnumerable<int> ContactID,int accountId);
        IEnumerable<OutlookEmailInformation> GetEmailInformation(IEnumerable<string> Emails, int AccountID);
        void InsertOutlookEmailAuditInformation(IEnumerable<OutlookEmailInformation> Emails, int AccountID, int UserID, Guid guid, DateTime sentUTCDate);
        IEnumerable<Person> GetContactsToSync(int accountId, int userId, int? maxNumRecords, DateTime? timeStamp, bool firstSync, CRUDOperationType operationType);
        IEnumerable<CRMOutlookSync> GetEntityOutlookSyncMap(int accountId, int userId, int? maxNumRecords, DateTime? timeStamp, IEnumerable<int> entityIds);
        void UpdateSyncedEntities(Dictionary<int, string> outlookKeys);
        IEnumerable<int> GetDeletedContactsToSync(int accountId, int userId, int? maxNumRecords, DateTime? timeStamp);
        IEnumerable<Contact> FindAll(int pageNumber, int limit, int accountId, int lastIndexedContact);
        IEnumerable<int> FindContactsByPrimaryEmails(IEnumerable<string> emails, int accountId);

        /// <summary>
        /// Find contacts by primary or secondary email
        /// </summary>
        /// <param name="email"></param>
        /// <param name="accountId"></param>
        /// <returns></returns>
        IEnumerable<int> FindContactsByEmail(string email, int accountId);
        
        TaxRate GetTaxRateBasedOnZipCode(string ZipCode);
        
        /// <summary>
        /// Updates the CRMOutlookSync Table when ever a contact is updated outside Outlook. 
        /// </summary>
        /// <param name="contact"></param>
        void PersistContactOutlookSync(Contact contact);

        IEnumerable<int> GetSearchDefinitionActiveContacts(List<int> contactIds, int accountId);
        int GetEmailID(string emailID, int contactID);
        IEnumerable<FormSubmission> GetContactFormSubmissions(int contactId);
        IEnumerable<FormSubmission> GetContactsFormSubmissionsList(int[] contactsIds);
        void ExecuteStoredProc(int contactID, byte flag);
        IEnumerable<WebVisit> GetWebVisitDetailsByVisitReference(string visitReference, int userId);
        IEnumerable<Person> GetContactsByContactIDs(IEnumerable<int> contactIds);

        bool IsExistedPersonsforCompanies(int[] contactids,int accountId);
        IEnumerable<ContactWebVisitSummary> GetContactWebVisitsSummary(int contactId, short pageNumber, short pageSize, out int TotalHits);

        IEnumerable<WebVisit> GetWebVisitByVisitID(int visitID);
        Guid? GetContactReferenceId(int contactId);

        IEnumerable<int> FindContactsOfUserByFirstAndLastName(IEnumerable<string> contactNames, int userId, int accountId);


        void InsertBulkOperation(BulkOperations bulkOperations,int[] contactIds);

        void UpdateExportBulkOperation(int operationId, string fileKey,string fileName);
        string GetCompanyNameById(int companyId);
        IEnumerable<Person> GetEmailById(IEnumerable<int> contactIds);
        int GetAccountIdById(int contactId);
        void UpdateContactLastTouchedThrough(IEnumerable<int> contactIds,int aacountId);
        string GetContactSummary(int contactId);
        ContactNoteSummary GetContactNoteSummary(int contactId);
        IEnumerable<ContactOwner> GetAllContactOwners(IEnumerable<int> contactIds, IEnumerable<int> userIds, int ownerId);
        IEnumerable<Contact> CheckDuplicate(string firstName, string lastName, string email, string company, int contactID, int accountID,byte contactType);
        bool IsNewContact(int contactId);
        ContactTableType InsertAndUpdateContact(ContactTableType contact);
        int UpdateContactCustomField(int contactId, int fieldId, string newValue);
        /*Comented by kiran on 30/05/2018 NEXG-3014 
        //void InsertAPILeadSubmissionData(APILeadSubmission apileadsubmission);
        */
        //added by kiran on 30/05/2018 NEXG-3014 
        int InsertAPILeadSubmissionData(APILeadSubmission apileadsubmission);
        void UpdateAPILeadSubmissionData(int? contactId, byte isProcessed, string remarks, int apiLeadSubmissionId);
        APILeadSubmission GetAPILeadSubmittedData();
        //added by kiran on 30/05/2018 NEXG-3014
        APILeadSubmission GetAPILeadSubmittedData(int apiLeadSubmissionID);
        void InsertBulkSavedSearchesContacts(List<SmartSearchContact> savedSearchContacts);
        void DeleteSavedSearchContactsBySearchDefinitionId(int searchDefinitionId,int accountId);
        NightlyScheduledDeliverabilityReport GetSenderRecipientInfoNightlyReport();
        IEnumerable<CampaignSenderRecipientNightlyReport> GetCampaignSenderRecipientInfoNightlyReport();
        ContactEmailEngagementDetails GetContactEmailEngagementDtails(int contactId, int accountId);
        IEnumerable<ContactWorkflowSummary> GetContactWorkflowSummaryDetails(int contactId, int accountId);
        IEnumerable<ContactEmailSummary> GetContactEmailSummaryDetails(int contactId, int accountId);
        IEnumerable<ContactCampaigSummary> GetContactCampaignSummaryDetails(int contactId, int accountId);

        Contact GetDeletedContact(int contactId);

        IEnumerable<Contact> GetContacts(int accountId, int limit, int pageNumber);
        int InsertAndUpdateCommunication(Communications.Communication communication, int contactId, int accountId);
        int UpdateImage(Image image, int userId);
        void UpdateContact(Contact contact, int communicationId);
        List<ContactOwnerPhone> GetContactOwerPhoneNubers(List<int> contactIds);
        IEnumerable<int> GetNeverBounceBadEmailContactIds(int neverBounceRequestId, byte emailStatus);
        List<LinkClickedDetails> GetEmailClickedLinkURLs(int sentMailDetailId,int contactId);
        List<LinkClickedDetails> GetCampaignClickedLinkURLs(int campaignid, int campaignRecipientId);
    }
}

using SmartTouch.CRM.Domain.Images;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Domain.Tags;
using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Domain.Search;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Domain.Users;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.Reports;
using System.Data;
using System.ComponentModel;

namespace SmartTouch.CRM.Domain.Campaigns
{
    public interface ICampaignRepository : IRepository<Campaign, int>
    {
        IEnumerable<Campaign> SearchCampaign(string name);

        IEnumerable<Campaign> FindByCampaignDate(DateTime campaignDate);

        IEnumerable<Campaign> FindByEmail(string email);

        Campaign GetCampaignById(int campaignId);

        int GetContactCampaignMapId(int campaignId, int contactId);

        IEnumerable<int> GetUniqueCampaignIDsByRecipients(IEnumerable<int> campaignRecipients, int accountId);

        int CampaignContactsCount(int campaignId, int accountId);

        Boolean IsDuplicateCampaignLayout(string name, int accountId);

        IEnumerable<CampaignTemplate> GetTemplates(int accountId);

        CampaignTemplate GetTemplate(int campaignTemplateID);

        IEnumerable<CampaignTemplate> GetTemplateNames(int accountId);

        IEnumerable<Campaign> FindAll(string name, int limit, int pageNumber, byte status, int AccountID);

        IEnumerable<Campaign> FindAll(string name, byte status, int AccountID);

        IEnumerable<Image> FindAllImages(string name, int limit, int pageNumber, int? AccountID);

        IEnumerable<Image> FindAllImages(string name, int? AccountID);

        IEnumerable<Image> GetCampaignPublicImages();

        int InsertCampaignImage(Image image);

        int InsertCampaignTemplates(CampaignTemplate campaignTemplate);

        void DeleteCampaignImage(int imageId, int accountId);

        List<Image> GetCampaignImages(int? accountID);

        bool IsCampaignNameUnique(Campaign campaign);

        /// <summary>
        /// Creates a list of campaign recipients and queues the campaign. This method is out of UOW because of performance implications.
        /// </summary>
        /// <param name="campaignId"></param>
        /// <param name="accountId"></param>
        /// <returns></returns>
        int AddCampaignRecipients(int campaignId, int accountId, IEnumerable<int> contactsFromSearch, int userId, int? tagRecipients, int? sSRecipients);

        int CampaignRecipientsCount(IEnumerable<Tag> tags, int accountId);

        void DeactivateCampaign(int[] campaignIds, int updatedBy);

        Campaign GetNextCampaignToTrigger();

        IEnumerable<CampaignRecipient> GetCampaignRecipients(int campaignId, int accountId);

        Campaign UpdateCampaignTriggerStatus(int campaingId, CampaignStatus status, DateTime sentDateTime, string serviceProviderCampaignId, List<string> finalRecipients, string remarks, int? serviceProviderId, bool isDelayedCampaign, out List<LastTouchedDetails> lasttouched, IEnumerable<int> recipients = null);

        Campaign UpdateCampaignFailedStatus(int campaingId, CampaignStatus status, string remarks);

        void UpdateCampaignStatus(int campaignId, CampaignStatus status);

        /// <summary>
        /// Record a campaign open or click entry
        /// </summary>
        /// <param name="campaignId"></param>
        /// <param name="linkId"></param>
        /// <param name="campaignRecipientId"></param>
        CampaignRecipient InsertCampaignOpenEntry(int campaignId, byte? linkId, int campaignRecipientId);

        Campaign CancelCampaign(int campaignId);

        IEnumerable<SearchDefinition> GetCampaignSearchDefinitionsMap(int campaignId);

        CampaignRecipientTypes CampaignUniqueRecipientsCount(IEnumerable<Tag> tags, int accountId, IEnumerable<int> contactsFromSearch, long toTagStatus);

        int CampaignUniqueRecipientsCount(IEnumerable<Tag> tags, int accountId, IEnumerable<int> contactsFromSearch, int userId);

        CampaignStatistics GetCampaignStatistics(int accountId, int campaignId);

        //CampaignStatistics GetCampaignStatistics(int campaignId);

        CampaignLink GetLinkUrl(int campaignId, byte? linkId);

        IEnumerable<CampaignLink> GetCampaignLinkUrls(IEnumerable<int> campaignId);

        CampaignLink GetCampaigLinknByCampaignlinkID(int CampaignLinkId);

        bool IsImageFriendlyNameUnique(Image image);

        CampaignStatus? GetCampaignStatus(int campaignId);

        void UpdateCampaignRecipientStatus(int campaignRecipientId, CampaignDeliveryStatus deliveryStatus, DateTime deliveredOn, string remarks, int? serviceProviderId, DateTime timeLogged, int accountId, short? OptOutStatus);

        IEnumerable<CampaignRecipient> GetCampaignRecipientsByLastModifiedDate(DateTime lastModified, int accountId);

        IEnumerable<CampaignDeliveryStatus> GetDeliveryStatusesByMailId(string mailId, int accountId, int take = 5);

        Campaign UpdateCampaignDeliveryStatus(string mailChimpCampaingId, CampaignDeliveryStatus status, DateTime sentDateTime, string reason);

        CampaignRecipient UpdateCampaignRecipientDeliveryStatus(int campaignRecipientId, CampaignDeliveryStatus status, int accountId);

        CampaignRecipient GetCampaignRecipient(string mailChimpCampaingId, string email);

        IEnumerable<CampaignLink> GetCampaignLinks(int CampaignID);

        void UpdateCampaignRecipientoptOutStatus(int campaignId, int contactId, int workflowId);

        //CampaignRecipient GetCampaignRecipient(int campaignRecipientID);

        CampaignRecipient GetCampaignRecipient(int campaignRecipientID, int accountId);

        IEnumerable<CampaignReportData> GetCampaignListByClicks(DateTime customStartDate, DateTime customEndDate, int accountID, int pageNumber, long limit, bool isAccountAdmin, int userID, out int TotalHits, bool isReputationReport);

        IEnumerable<string> GetWorkflwosForCampaignReport(int campaignId);

        IEnumerable<CampaignReportData> GetCampaignListByClicks(DateTime customStartDate, DateTime customEnddate, int accountID, bool isAccountAdmin, int userID);

        void UpdateAutomationCampaignRecipients(int contactId, int campaignId, int workflowId, int accountId);

        IEnumerable<CampaignRecipient> GetNextAutomationCampaignRecipients();

        IEnumerable<Campaign> GetAutomationCampaigns(IEnumerable<int> campaignIds);

        IEnumerable<UserSocialMediaPosts> GetPosts(int campaignID, int userID, string communicationType);

        IEnumerable<Campaign> GetCampaignIdsSeekingAnalysis();

        string DeleteCampaigns(int[] campaignIds, int updatedBy, int accountID);

        IEnumerable<Campaign> ArchiveCampaigns(int[] campaignIds, int updatedBy);

        IEnumerable<CampaignTheme> GetCampaignThemes(int userId, int accountId);

        void SaveResendCampaign(int parentCampaignId, int campaignId, CampaignResentTo CampaignResentTo);

        int GetResentCampaignCount(int parentCampaignId, CampaignResentTo CampaignResentTo);

        bool GetResentCampaignsData(int childCampaignId);

        Campaign UpdateCampaignTriggerStatusForWorkflow(int campaignId, CampaignStatus status, DateTime sentDateTime, string serviceProviderCampaignId, IEnumerable<int> finalRecipients, string remarks, int? serviceProviderId, out List<LastTouchedDetails> lasttouched);

        bool IsworkFlowAttachedCampaign(int campaignID);

        IEnumerable<CampaignTemplate> GetTemplatesForContactEmails(int accountId);

        string GetTemplateHTML(int ID, CampaignTemplateType Type);

        IEnumerable<Campaign> FindAll(int accountId = 0);

        CampaignRecipient GetCampaignRecipientbyID(int campaignID, string email, int accountId);

        IEnumerable<IDictionary<string, object>> GetCampaignRecipientsInfo(int campaignId, bool isLinkedToWorkflow);

        void UpdateCampaignRecipientsStatus(List<int> campaignRecipientIDs, string remarks, DateTime deliveredOn, DateTime sentOn, CampaignDeliveryStatus deliveryStatus);

        DataTable GetCampaignReportExport(DateTime customStartDate, DateTime customEndDate, bool isAccountAdmin, int accountId, int? userId);

        List<int> GetSearchDefinitionIds(int campaignId);

        IEnumerable<CampaignUniqueRecipient> GetCampaignUniqueRecipients(IEnumerable<int> tags, IEnumerable<SearchDefinitionContact> searchDefinitions, int accountId, int roleId, int owner, bool isDataSharingOn);

        void InsertCampaignLogDetails(IEnumerable<CampaignLogDetails> campaignLogDetails);

        void InsertBulkRecipients(IEnumerable<TemporaryRecipient> temporaryRecipient);

        int GetCampaignRecipientCount(int campaignId);

        Campaign GetCampaignBasicInfoById(int campaignId);

        IEnumerable<Campaign> FindAll(int accountId, int campaignStatusId, int pageNumber, int pageSize, int[] userIds, DateTime? startDate, DateTime? endDate, string name, string sortField, ListSortDirection direction);

        string GetCampaignNameById(int campaignId);

        string GetCampaignLinkURLByLinkId(int linkId);

        IEnumerable<CampaignLinkInfo> GetWorkflowCampaignActionLinks(int accountId);

        IEnumerable<CampaignReEngagementInfo> GetReEngagementSummary(DateTime startDate, DateTime endDate, bool isDefaultDateRange, int accountId, IEnumerable<int> linkIds);

        IEnumerable<int> GetReEngagedContacts(int accountId, int campaignId, DateTime startDate, DateTime endDate, bool isDefaultDateRange, bool hasSelectedLinks, IEnumerable<int> linkIds, byte drillDownPeriod);

        Campaign GetCampaignUTMInformation(int campaignId);

        IEnumerable<PrimitiveContactValue> GetContactMergeFields(IEnumerable<string> mergeFields, int contactId);

        IEnumerable<CampaignLitmusMap> GetPendingLitmusRequests();

        void UpdateLitmusId(CampaignLitmusMap map);

        void RequestLitmusCheck(int campaignId);

        IEnumerable<CampaignLitmusMap> GetLitmusIdByCampaignId(int campaignId, int accountId, int ownerId);

        void NotifyLitmusCheck();

        bool CampaignHasLitmusResults(int campaignId);

        void InsertMailTesterRequest(int campaignId, Guid guid, int userId);

        IEnumerable<CampaignMailTester> GetMailTesterRequests();

        void UpdateCampaignMailTester(IEnumerable<CampaignMailTester> mailTester);

        Dictionary<byte, string> GetCampaignLinkURLsByCampaignId(int campaignId);

        Guid GetMailTesterGuid(int campaignId);

        int GetUniqueContactscount(IEnumerable<int> tags, IEnumerable<SearchDefinitionContact> searchDefinitions, int accountId);

        void UpdateCampaignSentFlagStatus(Int32 _campaignId, bool _mailsentflag);  // Added by Ram on 9th May 2018  for Ticket NEXG-3004
        void InsertCampaignRecipients(Campaign _campaign, CampaignRecipient _campRecp);  // Added by Ram on 9th May 2018  for Ticket NEXG-3005
        void InsertAllSScontacts(CampaignRecipient _campRecp); 
    }
}
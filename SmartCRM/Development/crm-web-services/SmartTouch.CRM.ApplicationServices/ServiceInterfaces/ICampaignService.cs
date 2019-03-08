using SmartTouch.CRM.ApplicationServices.Messaging.Campaigns;
using SmartTouch.CRM.ApplicationServices.Messaging;
using SmartTouch.CRM.Domain.Campaigns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.ApplicationServices.Messaging.Communication;

namespace SmartTouch.CRM.ApplicationServices.ServiceInterfaces
{
    public interface ICampaignService
    {
        GetCampaignTemplatesResponse GetCampaignTemplates(GetCampaignTemplatesRequest request);
        SearchCampaignsResponse GetAllCampaigns(SearchCampaignsRequest request);
        GetCampaignTemplateResponse GetCampaignTemplate(GetCampaignTemplateRequest request);
        GetCampaignResponse GetCampaign(GetCampaignRequest request);
        GetCampaignResponse GetCampaignBasicInfo(GetCampaignRequest request);
        GetCampaignTemplateNamesResponse GetTemplateNamesRequest(GetCampaignTemplateNamesRequest request);

        InsertCampaignResponse InsertCampaign(InsertCampaignRequest request);
        //DeleteCampaignResponse DeleteCampaign(DeleteCampaignRequest request);
        UpdateCampaignResponse UpdateCampaign(UpdateCampaignRequest request);
        QueueCampaignResponse QueueCampaign(QueueCampaignRequest request);

        GetCampaignImagesResponse GetPublicCampaignImages(GetCampaignImagesRequest request);
        InsertCampaignImageResponse InsertCampaignImage(InsertCampaignImageRequest request);
        DeleteCampaignImageResponse DeleteCampaignImage(DeleteCampaignImageRequest request);
        GetAccountCampaignImagesResponse GetCampaignImages(GetCampaignImagesRequest request);
        DeleteCampaignResponse Deactivate(DeleteCampaignRequest request);
        DeleteCampaignResponse Archive(DeleteCampaignRequest request);
        CancelCampaignResponse CancelCampaign(CancelCampaignRequest request);
        Task<GetNextCampaignToTriggerResponse> GetNextCampaignToTriggerAsync();
        void UpdateCampaignTriggerStatus(UpdateCampaignTriggerStatusRequest request);

        GetAccountCampaignImagesResponse FindAllImages(GetCampaignImagesRequest request);
        ReIndexDocumentResponse ReIndexCampaigns(ReIndexDocumentRequest request);
        GetCampaignRecipientsResponse GetCampaignRecipients(GetCampaignRecipientsRequest request);
        GetCampaignUniqueRecipientsCountResponse GetCampaignTotalUniqueRecipientsCount(GetCampaignUniqueRecipientsCountRequest request);

        InsertCampaignOpenOrClickEntryResponse InsertCampaignOpenEntry(InsertCampaignOpenOrClickEntryRequest request);
        GetCampaignStatisticsResponse GetCampaignStatistics(GetCampaignStatisticsRequest request);
        GetLinkUrlResponse GetLinkURl(GetLinkUrlRequest request);
        GetCampaignStatusResponse GetCampaignStatus(GetCampaignStatusRequest request);
        GetLinkUrlsResponse GetCampaignLinks(GetLinkUrlRequest request);
        MailChimpWebhookResponse MailChimpWebhookUpdate(MailChimpWebhookRequest request);
        UpdateCampaignDeliveryStatusResponse UpdateCampaignDeliveryStatus(UpdateCampaignDeliveryStatusRequest request);
        GetClientIPAddressResponse GetClientIPAddress(GetClientIPAddressRequest request);
        void UpdateCampaignRecipientOptOutStatus(int contactId, int campaignId, int workflowId);
        GetCampaignRecipientResponse GetCampaignRecipient(GetCampaignRecipientRequest request);

        InsertCampaignRecipientResponse InsertCampaignRecipients(InsertCampaignRecipientRequest request);
        GetNextAutomationCampaignRecipientsResponse GetNextAutomationCampaignRecipients(GetNextAutomationCampaignRecipientsRequest request);
        GetAutomationCampaignResponse GetAutomationCampaigns(GetAutomationCampaignRequest request);
        GetCampaignsSeekingAnalysisResponse GetCampaignsSeekingAnalysis(GetCampaignsSeekingAnalysisRequest request);
        GetServiceProviderSenderEmailResponse GetDefaultBulkEmailProvider(GetServiceProviderSenderEmailRequest request);
        GetServiceProviderSenderEmailResponse GetEmailProviderById(GetServiceProviderSenderEmailRequest request);
        GetCampaignThemesResponse GetCampaignThemes(GetCampaignThemesRequest request);
        SendTestEmailResponse SendTestEmail(SendTestEmailRequest request);
        InsertCampaignTemplateResponse InsertCampaignTemplate(InsertCampaignTemplateRequest request);
        void UpdateCampaignStatistics(GetCampaignRequest request);

        ResentCampaignResponse SaveResendCampaign(ResentCampaignRequest resendCampaignRequest);

        GetResentCampaignCountResponse GetResentCampaignCount(GetResentCampaignCountRequest getResentCampaignCountRequest);
        GetCampaignTemplatesResponse GetCampaignTemplatesForEmails(GetCampaignTemplatesRequest request);
        GetCampaignTemplateResponse GetCampaignTemplateHTML(GetCampaignTemplateRequest request);
        GetCampaignRecipientsResponse GetCampaignRecipientsInfo(GetCampaignRecipientsRequest request);
        void UpdateCampaignRecipientsStatus(UpdateCampaignRecipientsStatusRequest request);
        GetCampaignSocialMediaPostResponse GetCampaignPosts(GetCampaignSocialMediaPostRequest request);
        Task<GetCampaignRecipientIdsResponse> GetCampaignRecipientsByID(GetCampaignRecipientIdsRequest request);
        Task<InsertBulkRecipientsResponse> InsertCampaignRecipients(InsertBulkRecipientsRequest request);
        CampaignIndexingResponce CampaignIndexing(CampaignIndexingRequest request);
        GetUniqueRecipientsCountResponse GetUniqueRecipients(GetUniqueRecipientsCountRequest request);
        GetUniqueRecipientsCountResponse GetEmailValidatorContactsCount(GetUniqueRecipientsCountRequest request);

        InsertCampaignLogDetailsResponce InsertCampaignLogDetails(InsertCampaignLogDetailsRequest insertCampaignLogDetailsRequest);
        GetWorkflowLinkActionsResponse GetWorkflowCampaignActionLinks(GetWorkflowLinkActionsRequest request);
        GetReEngagementInfoResponse GetReEngagementInfo(GetReEngagementInfoRequest request);
        Campaign GetCampaignUTMInformation(int campignId);
        GetCampaignLitmusResponse GetPendingLitmusRequests();
        void UpdateLitmusId(UpdateCampaignLitmusMap request);
        void RequestLitmusCheck(RequestLitmusCheck request);
        GetCampaignLitmusResponse GetCampaignLitmusMap(GetCampaignLitmusMapRequest request);
        void NotifyLitmusCheck();
        UpdateCampaignStatusResponse UpdateCampaignStatus(UpdateCampaignStatusRequest request);

        InsertCampaignMailTesterResponse InsertMailTesterRequest(InsertCampaignMailTesterRequest request);
        GetCampaignMailTesterResponse GetMailTestData(GetCampaignMailTesterRequest request);
        UpdateMailTesterResponse UpdateMailTester(UpdateMailTesterRequest request);
        Dictionary<byte, string> GetCampaignURLSById(int campaignId);
        GetCampaignMailTesterGuidResponse GetMailTesterGuid(GetCampaignMailTesterGuid request);
        void UpdateCampaignSentFlagStatus(Int32 _CampaignId, bool _mailsentFalg);// Added by Ram on 9th May 2018  for Ticket NEXG-3004
    }
}

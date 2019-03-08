using System;
using System.Collections.Generic;
using System.Linq;
using LandmarkIT.Enterprise.CommunicationManager.MailTriggers;
using LandmarkIT.Enterprise.CommunicationManager.Operations;
using LandmarkIT.Enterprise.CommunicationManager.Requests;
using LandmarkIT.Enterprise.Utilities.ExceptionHandling;
using Quartz;
using SmartTouch.CRM.ApplicationServices.Messaging.Campaigns;
using SmartTouch.CRM.ApplicationServices.Messaging.Communication;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.CustomFields;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.JobProcessor.QuartzScheduler.Scheduler;

namespace SmartTouch.CRM.JobProcessor.Jobs.CampaignJobs
{
    public class AutomationCampaignJob : BaseJob
    {
        private readonly ICampaignService _campaignService;
        private readonly ICommunicationService _communicationService;
        private readonly ICustomFieldRepository _customFieldRepository;
        private readonly IContactRepository _contactRepository;
        private readonly IAccountService _accountService;
        private readonly MailService _mailService;
        private readonly JobServiceConfiguration _jobConfig;

        public AutomationCampaignJob(
            ICampaignService campaignService,
            ICommunicationService communicationService,
            ICustomFieldRepository customFieldRepository,
            IContactRepository contactRepository,
            IAccountService accountService,
            MailService mailService,
            JobServiceConfiguration jobConfig)
        {
            _campaignService = campaignService;
            _communicationService = communicationService;
            _customFieldRepository = customFieldRepository;
            _contactRepository = contactRepository;
            _accountService = accountService;
            _mailService = mailService;
            _jobConfig = jobConfig;
        }

        protected override void ExecuteInternal(IJobExecutionContext context)
        {
            var imageHostingUrl = _jobConfig.ImageHostingServiceUrl;
            var timeToRescheduleInMinutes = _jobConfig.RescheduleMinutes;

            try
            {
                var automationCampaignRecipients = _campaignService
                    .GetNextAutomationCampaignRecipients(new GetNextAutomationCampaignRecipientsRequest())
                    .AutomationCampaignRecipients
                    .ToArray();

                if (automationCampaignRecipients.Any())
                {
                    Log.Informational("Automation campaign recipients found");
                    var campaignIds = automationCampaignRecipients
                        .Select(s => s.CampaignID)
                        .Distinct()
                        .ToArray();

                    var campaigns = _campaignService
                        .GetAutomationCampaigns(new GetAutomationCampaignRequest
                        {
                            CampaignIds = campaignIds
                        }).Campaigns;

                    foreach (var campaign in campaigns)
                    {
                        var firstRecipient = automationCampaignRecipients.First(x => x.CampaignID == campaign.Id);
                        var createdOn = firstRecipient.CreatedDate;
                        var scheduledTo = firstRecipient.ScheduleTime;

                        var campaignRecipients = new List<EmailRecipient>();
                        try
                        {
                            Log.Informational("Request received to get email providers, automation campaign id " + campaign.Id);
                            var communicationResponse = (campaign.ServiceProviderID.HasValue && campaign.ServiceProviderID.Value > 0)
                                ? _communicationService.GetEmailProviderById(new GetServiceProviderByIdRequest { AccountId = campaign.AccountID, ServiceProviderId = campaign.ServiceProviderID.Value }).CampaignEmailProvider
                                : _communicationService.GetDefaultCampaignEmailProvider(new GetDefaultCampaignEmailProviderRequest { AccountId = campaign.AccountID }).CampaignEmailProvider;

                            Log.Informational("Request received to get default email providers, automation campaign id " + campaign.Id);
                            var accountDomain = communicationResponse.Account.DomainURL;
                            var accountAddress = "";
                            if (communicationResponse.Account.Addresses != null && communicationResponse.Account.Addresses.Any())
                            {
                                var primaryAddress = communicationResponse.Account.Addresses.FirstOrDefault(a => a.IsDefault);
                                if (primaryAddress != null)
                                    accountAddress = primaryAddress.ToString();
                            }
                            var providerSenderEmailResponse = _campaignService.GetDefaultBulkEmailProvider(
                                new GetServiceProviderSenderEmailRequest
                                {
                                    AccountId = campaign.AccountID,
                                    RequestedBy = campaign.CreatedBy
                                });

                            var recipients = _campaignService.GetCampaignRecipientsInfo(new GetCampaignRecipientsRequest { CampaignId = campaign.Id, IsLinkedToWorkflow = true });
                            if (recipients != null)
                            {
                                campaignRecipients = recipients.RecipientsInfo.Select(c => new EmailRecipient()
                                {
                                    ContactId = Convert.ToInt32(c.Value["CONTACTID"]),
                                    CampaignRecipientID = Convert.ToInt32(c.Value["CRID"]),
                                    EmailId = c.Value["EMAILID"].ToString().TrimEnd(),
                                    ContactFields = c.Value
                                }).ToList();
                                _contactRepository.GetCompanyDetails(campaignRecipients.Select(c => c.ContactId), campaign.AccountID);
                            }

                            var mailRegistration = _mailService.GetVMTADetails(new List<Guid>() { communicationResponse.LoginToken });
                            var imageDomainResponse = _communicationService.GetServiceProviderImageDomain(new GetServiceProviderImageDomainRequest() { Guid = mailRegistration.Guid }).ServiceProvider;
                            if (imageDomainResponse != null && imageDomainResponse.ImageDomain != null && imageDomainResponse.ImageDomain.Domain != null)
                                mailRegistration.ImageDomain = imageDomainResponse.ImageDomain.Domain;

                            if (mailRegistration != null && mailRegistration.MailProviderID == MailProvider.SmartTouch)
                            {
                                Log.Informational("VMTA service provider found for this account");
                                string content;
                                if (mailRegistration.ImageDomain != null)
                                {
                                    content = campaign.HTMLContent.Replace(imageHostingUrl, mailRegistration.ImageDomain);
                                    var index = mailRegistration.ImageDomain.IndexOf("//");
                                    var dotCount = mailRegistration.ImageDomain.Count(d => d == '.');
                                    var linkDomain = mailRegistration.ImageDomain;
                                    if (index >= 0 && dotCount == 1)
                                    {

                                        linkDomain = mailRegistration.ImageDomain.Insert(index + 2, communicationResponse.AccountCode + ".").Replace("https://", "").Replace("http://", "").Replace("www.", "");
                                        content = content.Replace("http://" + accountDomain, "http://" + linkDomain).Replace("https://" + accountDomain, "http://" + linkDomain);
                                    }
                                }
                                else
                                    content = campaign.HTMLContent;

                                content = content.Replace("*|CAMPID|*", campaign.Id.ToString()).Replace("<o:p>", "").Replace("</o:p>", "");
                                campaign.HTMLContent = content;

                                var senderEmail = providerSenderEmailResponse.SenderEmail != null ? providerSenderEmailResponse.SenderEmail.EmailId : "";
                                var customFieldsValueOptions = _customFieldRepository.GetCustomFieldsValueOptions(campaign.AccountID);

                                var mailProvider = new SmartTouchProvider();
                                mailProvider.SendCampaign(
                                        campaign, campaignRecipients, customFieldsValueOptions,
                                        communicationResponse.AccountCode, accountAddress,
                                        accountDomain , senderEmail, mailRegistration);
                                var successfulRecipients = campaignRecipients;

                                Log.Informational("Automation campaign sent to vmta, automation campaign id " + campaign.Id);
                                if (successfulRecipients.Any())
                                {
                                    _campaignService.UpdateCampaignTriggerStatus(new UpdateCampaignTriggerStatusRequest(campaign.Id)
                                    {
                                        Status = CampaignStatus.Sent,
                                        SentDateTime = DateTime.UtcNow,
                                        ServiceProviderID = communicationResponse.CommunicationLogID,
                                        Remarks = "Campaign sent successfully",
                                        SentCount = campaignRecipients.Count(),
                                        RecipientIds = campaignRecipients.Select(c => c.CampaignRecipientID),
                                        ServiceProviderCampaignId = communicationResponse.AccountCode + "/" + campaign.Id,
                                        IsRelatedToWorkFlow = true,
                                        AccountId = campaign.AccountID
                                    });
                                    _accountService.ScheduleAnalyticsRefresh(campaign.Id, (byte)IndexType.Campaigns);
                                }
                                else
                                    _campaignService.UpdateCampaignTriggerStatus(new UpdateCampaignTriggerStatusRequest(campaign.Id)
                                    {
                                        Status = CampaignStatus.Failure,
                                        SentDateTime = DateTime.UtcNow,
                                        Remarks = "Could not send the campaign through VMTA service.",
                                        SentCount = campaignRecipients.Count(),
                                        RecipientIds = campaignRecipients.Select(c => c.CampaignRecipientID),
                                        IsRelatedToWorkFlow = true,
                                        AccountId = campaign.AccountID
                                    });
                            }
                        }
                        catch (Exception ex)
                        {
                            //Workaround for connection issue we have on SmartTouch mail server
                            if (scheduledTo - createdOn < TimeSpan.FromMinutes(timeToRescheduleInMinutes))
                            {
                                _campaignService.ReScheduleAutomationCampaign(campaign.Id,  DateTime.UtcNow.AddMinutes(5));
                            }
                            else
                            {
                                Log.Error("Exception occurred while sending a campaign:" + campaign.Id, ex);
                                _campaignService.UpdateCampaignTriggerStatus(
                                    new UpdateCampaignTriggerStatusRequest(campaign.Id)
                                    {
                                        Status = CampaignStatus.Failure,
                                        SentDateTime = DateTime.UtcNow,
                                        Remarks = ex.InnerException?.Message ?? ex.Message,
                                        RecipientIds = campaignRecipients.Select(c => c.CampaignRecipientID),
                                        IsRelatedToWorkFlow = true,
                                        AccountId = campaign.AccountID
                                    });
                                ExceptionHandler.Current.HandleException(ex, DefaultExceptionPolicies.LOG_ONLY_POLICY);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionHandler.Current.HandleException(ex, DefaultExceptionPolicies.LOG_ONLY_POLICY);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using LandmarkIT.Enterprise.CommunicationManager.Database;
using LandmarkIT.Enterprise.CommunicationManager.MailTriggers;
using LandmarkIT.Enterprise.CommunicationManager.Operations;
using LandmarkIT.Enterprise.Extensions;
using LandmarkIT.Enterprise.Utilities.ExceptionHandling;
using Quartz;
using SmartTouch.CRM.ApplicationServices.Messaging.Campaigns;
using SmartTouch.CRM.ApplicationServices.Messaging.Communication;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.Campaigns;
using SmartTouch.CRM.Domain.CustomFields;
using SmartTouch.CRM.Domain.Users;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.JobProcessor.QuartzScheduler;
using SmartTouch.CRM.JobProcessor.QuartzScheduler.Scheduler;
using SmartTouch.CRM.JobProcessor.Utilities;
using LM = LandmarkIT.Enterprise.CommunicationManager.Requests;

namespace SmartTouch.CRM.JobProcessor.Jobs.CampaignJobs
{
    public class CampaignSendJob : BaseJob<CampaignSendEmailRequest>
    {
        private readonly ICampaignService _campaignService;
        private readonly ICommunicationService _communicationService;
        private readonly IAccountService _accountService;
        private readonly ICustomFieldRepository _customFieldRepository;
        private readonly ISocialIntegrationService _socialIntegrationService;
        private readonly EmailValidator _emailValidator;
        private readonly MailService _mailService;
        private readonly JobServiceConfiguration _jobConfig;
        private readonly JobScheduler _jobScheduler;

        public CampaignSendJob(
            ICampaignService campaignService,
            ICommunicationService communicationService,
            ICustomFieldRepository customFieldRepository,
            ISocialIntegrationService socialIntegrationService,
            IAccountService accountService,
            EmailValidator emailValidator,
            MailService mailService,
            JobServiceConfiguration jobConfig,
            JobScheduler jobScheduler)
        {
            _campaignService = campaignService;
            _communicationService = communicationService;
            _accountService = accountService;
            _emailValidator = emailValidator;
            _customFieldRepository = customFieldRepository;
            _socialIntegrationService = socialIntegrationService;
            _mailService = mailService;
            _jobConfig = jobConfig;
            _jobScheduler = jobScheduler;
        }

        protected override void Execute(IJobExecutionContext context, CampaignSendEmailRequest request)
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

                foreach (var automationCampaign in campaigns)
                {
                    request = new CampaignSendEmailRequest(automationCampaign.Id);
                    _jobScheduler.Queue(request);
                }
            }
        }

        private IMailProvider GetEmailProvider(MailRegistrationDb mailRegistration)
        {
            switch (mailRegistration.MailProviderID)
            {
                case LM.MailProvider.MailChimp:
                    return new MailChimpCampaign(mailRegistration.APIKey);
                case LM.MailProvider.SmartTouch:
                    return new SmartTouchProvider();
                case LM.MailProvider.Undefined:
                case LM.MailProvider.Smtp:
                case LM.MailProvider.SendGrid:
                case LM.MailProvider.CustomSmartTouch:
                case LM.MailProvider.CustomMailChimp:
                default:
                    break;
            }
            return null;
        }

        private string ProcessEmailContent(Campaign campaign, MailRegistrationDb mailRegistration, ServiceProviderViewModel serviceProvider)
        {
            string content;
            var imageHostingUrl = _jobConfig.ImageHostingServiceUrl;
            var accountDomain = serviceProvider.Account.DomainURL.ToLower();

            if (!string.IsNullOrEmpty(serviceProvider.ImageDomain?.Domain))
            {
                mailRegistration.ImageDomain = serviceProvider.ImageDomain.Domain;
                content = campaign.HTMLContent.Replace(imageHostingUrl, serviceProvider.ImageDomain.Domain);
                var imageDomainProtocol = serviceProvider.ImageDomain.Domain.Substring(0, serviceProvider.ImageDomain.Domain.IndexOf("://") + 3);
                var index = serviceProvider.ImageDomain.Domain.IndexOf("//");
                var dotCount = serviceProvider.ImageDomain.Domain.Count(d => d == '.');
                var linkDomain = serviceProvider.ImageDomain.Domain;
                if (index >= 0 && dotCount == 1)
                {
                    linkDomain = serviceProvider.ImageDomain.Domain.Replace(imageDomainProtocol, imageDomainProtocol + serviceProvider.AccountCode + ".");
                    content = content.Replace("http://" + accountDomain, linkDomain).Replace("https://" + accountDomain, linkDomain);
                }
            }
            else
                content = campaign.HTMLContent;

            return content.Replace("*|CAMPID|*", campaign.Id.ToString()).Replace("<o:p>", "").Replace("</o:p>", "");
        }

        private void ShareSocialPost(ISocialIntegrationService socialIntegrationService, Campaign campaign)
        {
            var posts = _campaignService.GetCampaignPosts(new GetCampaignSocialMediaPostRequest() { CampaignId = campaign.Id, UserId = 0, CommunicationType = string.Empty }).Posts ?? new List<UserSocialMediaPosts>();

            posts.ToList().ForEach(post =>
            {
                Log.Informational(string.Format("In sending social posts {0}, communication type : {1}", campaign.ToString(), post.CommunicationType));
                if (post.CommunicationType == "Facebook")
                {
                    socialIntegrationService.PostToFacebook(post.UserID, post.Post, post.AttachmentPath);
                }
                else
                {
                    socialIntegrationService.Tweet(post.UserID, post.Post);
                }
            });
        }

        private void UpdateCampaignTriggerStatus(Campaign campaign, CampaignStatus status, string remarks, ServiceProviderViewModel serviceProvider, IEnumerable<int> successfulRecipientIDs, int accountId, bool isDelayedCampaign = false, string mailChimpCampaignId = null)
        {
            _campaignService.UpdateCampaignTriggerStatus(new UpdateCampaignTriggerStatusRequest(campaign.Id)
            {
                Status = status,
                SentDateTime = DateTime.UtcNow,
                ServiceProviderID = serviceProvider.CommunicationLogID,
                Remarks = remarks,
                ServiceProviderCampaignId = string.IsNullOrEmpty(mailChimpCampaignId) ? serviceProvider.AccountCode + "/" + campaign.Id : mailChimpCampaignId,
                IsDelayedCampaign = isDelayedCampaign,
                RecipientIds = successfulRecipientIDs,
                AccountId = accountId
            });
        }
    }
}

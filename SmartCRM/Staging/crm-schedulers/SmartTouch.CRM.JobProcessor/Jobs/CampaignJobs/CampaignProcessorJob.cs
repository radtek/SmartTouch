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
    public class CampaignProcessorJob : BaseJob<CampaignEmailRequest>
    {
        private readonly ICampaignService _campaignService;
        private readonly ICommunicationService _communicationService;
        private readonly IAccountService _accountService;
        private readonly ICustomFieldRepository _customFieldRepository;
        private readonly ISocialIntegrationService _socialIntegrationService;
        private readonly EmailValidator _emailValidator;
        private readonly MailService _mailService;
        private readonly JobServiceConfiguration _jobConfig;
        private readonly IJobScheduler _jobScheduler;

        public CampaignProcessorJob(
            ICampaignService campaignService,
            ICommunicationService communicationService,
            ICustomFieldRepository customFieldRepository,
            ISocialIntegrationService socialIntegrationService,
            IAccountService accountService,
            EmailValidator emailValidator,
            MailService mailService,
            JobServiceConfiguration jobConfig,
            IJobScheduler jobScheduler)
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

        protected override void Execute(IJobExecutionContext context, CampaignEmailRequest request)
        {
            Campaign campaign = null;
            try
            {
                campaign = _campaignService.GetNextCampaignToTriggerAsync().Result.Campaign;
                request = new CampaignEmailRequest(campaign.Id);

                while (campaign != null)
                {
                    var isDelayed = campaign.CampaignStatus == CampaignStatus.Delayed;

                    _jobScheduler.Queue(request);
                    
                    //Campaign primary information
                    var serviceProvider = campaign.ServiceProviderID.HasValue
                        ? _communicationService.GetEmailProviderById(new GetServiceProviderByIdRequest { AccountId = campaign.AccountID, ServiceProviderId = campaign.ServiceProviderID.Value }).CampaignEmailProvider
                        : _communicationService.GetDefaultCampaignEmailProvider(new GetDefaultCampaignEmailProviderRequest { AccountId = campaign.AccountID }).CampaignEmailProvider;

                    var successfulRecipients = new List<int>();

                    //Update campaign to sending status
                    UpdateCampaignTriggerStatus(campaign, isDelayed ? CampaignStatus.Retrying : CampaignStatus.Sending, "The campaign has been picked up for sending", serviceProvider, successfulRecipients, campaign.AccountID, isDelayed);

                    campaign = _campaignService.GetNextCampaignToTriggerAsync().Result.Campaign;
                }
            }
            catch (Exception ex)
            {
                ExceptionHandler.Current.HandleException(ex, DefaultExceptionPolicies.LOG_ONLY_POLICY);
                if (campaign != null)
                {
                    var remarks = "Error while sending campaign, please contact administrator. Additional Info: " + ex;
                    _campaignService.UpdateCampaignTriggerStatus(new UpdateCampaignTriggerStatusRequest(campaign.Id)
                    {
                        Status = CampaignStatus.Failure,
                        SentDateTime = DateTime.UtcNow,
                        Remarks = remarks
                    });
                }
            }
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
        /// <summary>
        /// Updates campaign to given status and updates remarks, serviceprovider.
        /// </summary>
        /// <param name="campaign"></param>
        /// <param name="status"></param>
        /// <param name="remarks"></param>
        /// <param name="serviceProvider"></param>
        /// <param name="mailChimpCampaignId"></param>
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

        /// <summary>
        /// Analyze campaign statistics for mailchimp
        /// </summary>
        private void AnalizeMailChimpCampaigns()
        {
            var campaigns = _campaignService.GetCampaignsSeekingAnalysis(new GetCampaignsSeekingAnalysisRequest()).Campaigns;
            var uniqueAccounts = campaigns.Select(c => c.AccountID).Distinct();
            var mailChimpAuthKey = new Dictionary<int, IEnumerable<ProviderRegistrationViewModel>>();
            foreach (int accountId in uniqueAccounts)
            {
                var provider = _communicationService.GetCommunicationProviders(new GetCommunicatioProvidersRequest() { AccountId = accountId });
                mailChimpAuthKey.Add(accountId, provider.RegistrationListViewModel.Where(p => p.MailProviderID == LM.MailProvider.MailChimp));
            }
            foreach (var campaign in campaigns)
            {
                try
                {
                    Log.Informational(string.Format("In seeking analysis for mailchimp campaigns, campaign {0}", campaign.ToString()));
                    var providers = mailChimpAuthKey.ContainsKey(campaign.AccountID) ? mailChimpAuthKey[campaign.AccountID] : default(IEnumerable<ProviderRegistrationViewModel>);
                    if (providers != null && providers.Any(provider => provider.ServiceProviderID == campaign.ServiceProviderID))
                    {
                        var authKey = providers.First(provider => provider.ServiceProviderID == campaign.ServiceProviderID).ApiKey;
                        var total = 0;
                        var recipients = new MailChimpCampaign(authKey).AnalyzeCampaign(campaign.ServiceProviderCampaignID, out total);
                        _campaignService.UpdateCampaignTriggerStatus(new UpdateCampaignTriggerStatusRequest(campaign.Id)
                        {
                            Status = CampaignStatus.Sent,
                            SentDateTime = DateTime.Now.ToUniversalTime(),
                            ServiceProviderCampaignId = campaign.ServiceProviderCampaignID,
                            SentCount = total,
                            Recipients = recipients.ToList(),
                            Remarks = "Campaign sent successfully",
                            AccountId = campaign.AccountID
                        });
                        _accountService.ScheduleAnalyticsRefresh(campaign.Id, (byte)IndexType.Campaigns);
                    }

                }
                catch (Exception ex)
                {
                    Log.Error("Could not get campaign analytics at this point of time. CampaignId: " + campaign, ex);
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
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
using SmartTouch.CRM.JobProcessor.QuartzScheduler.Scheduler;
using SmartTouch.CRM.JobProcessor.Utilities;
using LM = LandmarkIT.Enterprise.CommunicationManager.Requests;

namespace SmartTouch.CRM.JobProcessor.Jobs.CampaignJobs
{
    public class CampaignJob : BaseJob
    {
        private readonly ICampaignService _campaignService;
        private readonly ICommunicationService _communicationService;
        private readonly IAccountService _accountService;
        private readonly ICustomFieldRepository _customFieldRepository;
        private readonly ISocialIntegrationService _socialIntegrationService;
        private readonly EmailValidator _emailValidator;
        private readonly MailService _mailService;
        private readonly JobServiceConfiguration _jobConfig;

        public CampaignJob(
            ICampaignService campaignService,
            ICommunicationService communicationService,
            ICustomFieldRepository customFieldRepository,
            ISocialIntegrationService socialIntegrationService,
            IAccountService accountService,
            EmailValidator emailValidator,
            MailService mailService,
            JobServiceConfiguration jobConfig)
        {
            _campaignService = campaignService;
            _communicationService = communicationService;
            _accountService = accountService;
            _emailValidator = emailValidator;
            _customFieldRepository = customFieldRepository;
            _socialIntegrationService = socialIntegrationService;
            _mailService = mailService;
            _jobConfig = jobConfig;
        }

        protected override void ExecuteInternal(IJobExecutionContext context)
        {
            var mailChimpCampaignId = string.Empty;
            Campaign campaign = null;

            var imageHostingUrl = _jobConfig.ImageHostingServiceUrl; 
            var maxHours = _jobConfig.RetryMaxHours; 
            var timeToRescheduleInMinutes = _jobConfig.RescheduleMinutes;

            try
            {
                //Get the campaign to start processing
                campaign = _campaignService.GetNextCampaignToTriggerAsync().Result.Campaign;

                while (campaign != null)
                {
                    var isDelayed = campaign.CampaignStatus == CampaignStatus.Delayed;
                    Log.Informational(string.Format("Campaign {0} has been picked up for sending.", campaign));

                    //Campaign primary information
                    var serviceProvider = campaign.ServiceProviderID.HasValue
                        ? _communicationService.GetEmailProviderById(new GetServiceProviderByIdRequest { AccountId = campaign.AccountID, ServiceProviderId = campaign.ServiceProviderID.Value }).CampaignEmailProvider
                        : _communicationService.GetDefaultCampaignEmailProvider(new GetDefaultCampaignEmailProviderRequest { AccountId = campaign.AccountID }).CampaignEmailProvider;

                    var emailProvider = campaign.ServiceProviderID.HasValue
                        ? _campaignService.GetEmailProviderById(new GetServiceProviderSenderEmailRequest { AccountId = campaign.AccountID, RequestedBy = campaign.CreatedBy, ServiceProviderID = campaign.ServiceProviderID.Value })
                        : _campaignService.GetDefaultBulkEmailProvider(new GetServiceProviderSenderEmailRequest { AccountId = campaign.AccountID, RequestedBy = campaign.CreatedBy });

                    var accountDomain = serviceProvider.Account.DomainURL.ToLower();
                    var accountAddress = serviceProvider.Account.Addresses?.Any(a => a.IsDefault) ?? false ?
                                serviceProvider.Account.Addresses.First(a => a.IsDefault).ToString() :
                                string.Empty;

                    Log.Informational(string.Format("Update campaign status to sending {0}", campaign));
                    var successfulRecipients = new List<int>();

                    //Update campaign to sending status
                    UpdateCampaignTriggerStatus(campaign, isDelayed ? CampaignStatus.Retrying : CampaignStatus.Sending, "The campaign has been picked up for sending", serviceProvider, successfulRecipients, campaign.AccountID, isDelayed);
                    //Campaign Recipients
                    Log.Informational(string.Format("Get recipients info {0}", campaign));
                    try
                    {
                        var campaignRecipientsInfo = _campaignService.GetCampaignRecipientsInfo(new GetCampaignRecipientsRequest { CampaignId = campaign.Id, IsLinkedToWorkflow = false });
                        if (campaignRecipientsInfo.RecipientsInfo.IsAny())
                        {
                            var customFieldsValueOptions = _customFieldRepository.GetCustomFieldsValueOptions(campaign.AccountID).ToArray();

                            var campaignRecipients = campaignRecipientsInfo.RecipientsInfo.Select(c => new EmailRecipient
                            {
                                ContactId = Convert.ToInt32(c.Value["CONTACTID"]),
                                CampaignRecipientID = Convert.ToInt32(c.Value["CRID"]),
                                EmailId = c.Value["EMAILID"].ToString().TrimEnd(),
                                ContactFields = c.Value
                            }).ToList();

                            var mailRegistration = _mailService.GetMailRegistrationDetails(serviceProvider.LoginToken);
                            string content;
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

                            content = content.Replace("*|CAMPID|*", campaign.Id.ToString()).Replace("<o:p>", "").Replace("</o:p>", "");
                            campaign.HTMLContent = content;
                            var mailProvider = default(IMailProvider);
                            switch (mailRegistration.MailProviderID)
                            {
                                case LM.MailProvider.MailChimp:
                                    mailProvider = new MailChimpCampaign(mailRegistration.APIKey);
                                    break;
                                case LM.MailProvider.SmartTouch:
                                    mailProvider = new SmartTouchProvider();
                                    break;
                                case LM.MailProvider.Undefined:
                                case LM.MailProvider.Smtp:
                                case LM.MailProvider.SendGrid:
                                case LM.MailProvider.CustomSmartTouch:
                                case LM.MailProvider.CustomMailChimp:
                                default:
                                    break;
                            }

                            var failedRecipientIDs = new List<Tuple<Exception, int[]>>();
                            try
                            {
                                var numberOfIternations = mailProvider.BatchCount == 0 ? 1 : Math.Ceiling(campaignRecipients.Count / (decimal)mailProvider.BatchCount);
                                #region Batch Process
                                for (int i = 0; i < numberOfIternations; i++)
                                {
                                    var batchRecipientIDs = campaignRecipients
                                        .Skip(mailProvider.BatchCount * i)
                                        .Take(mailProvider.BatchCount)
                                        .Select(r => r.CampaignRecipientID)
                                        .ToArray();

                                    var batchCount = mailRegistration.MailProviderID == LM.MailProvider.MailChimp ? campaignRecipients.Count() : mailProvider.BatchCount;
                                    Log.Informational("Batch count: " + batchCount);

                                    var recipients = campaignRecipients
                                        .Skip(mailProvider.BatchCount * i)
                                        .Take(batchCount)
                                        .ToList();

                                    var validRecipients = recipients
                                        .Where(x => _emailValidator.IsValidEmail(x.EmailId))
                                        .ToList();

                                    try
                                    {
                                        Log.Informational(string.Format("Sending campaign to vmta {0}, iteration {1}", campaign, i));
                                        mailChimpCampaignId = mailProvider.SendCampaign(campaign, validRecipients
                                            , customFieldsValueOptions, serviceProvider.AccountCode, accountAddress, accountDomain
                                            , (emailProvider.SenderEmail != null ? emailProvider.SenderEmail.EmailId : string.Empty), mailRegistration);

                                        _campaignService.UpdateCampaignRecipientsStatus(new UpdateCampaignRecipientsStatusRequest
                                        {
                                            CampaignRecipientIDs = validRecipients.Select(r => r.CampaignRecipientID).ToList(),
                                            Remarks = "Campaign Sent Successfully",
                                            SentOn = DateTime.UtcNow,
                                            DeliveredOn = DateTime.UtcNow,
                                            DeliveryStatus = CampaignDeliveryStatus.Delivered
                                        });

                                        successfulRecipients.AddRange(batchRecipientIDs);

                                        var invalidRecipients = recipients
                                            .Except(validRecipients)
                                            .ToList();

                                        if (invalidRecipients.Any())
                                        {
                                            Log.Informational(string.Format("Updating invalid email recipients {0}, count {1}", campaign, invalidRecipients.Count()));
                                            _campaignService.UpdateCampaignRecipientsStatus(new UpdateCampaignRecipientsStatusRequest
                                            {
                                                CampaignRecipientIDs = invalidRecipients.Select(r => r.CampaignRecipientID).ToList(),
                                                Remarks = "Failed due to invalid email",
                                                SentOn = DateTime.UtcNow,
                                                DeliveredOn = DateTime.UtcNow,
                                                DeliveryStatus = CampaignDeliveryStatus.Failed
                                            });
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        failedRecipientIDs.Add(Tuple.Create(ex, batchRecipientIDs));
                                        var message = string.Format("Campaign batch failed - {0} - Iteration {1} - Batch Count {2}", campaign, i, mailProvider.BatchCount);
                                        Log.Critical(message, ex);
                                    }
                                }

                                #endregion

                                var isCampaignPartialFailed = failedRecipientIDs.Any();
                                if (isCampaignPartialFailed &&
                                    campaign.ScheduleTime - campaign.CreatedDate <
                                    TimeSpan.FromMinutes(timeToRescheduleInMinutes))
                                {
                                    _campaignService.ReScheduleCampaign(campaign.Id, DateTime.UtcNow.AddMinutes(5));
                                    UpdateCampaignTriggerStatus(campaign, CampaignStatus.Scheduled, "The campaign has been rescheduled", serviceProvider, successfulRecipients, campaign.AccountID, isDelayed);
                                }
                                else
                                {
                                    foreach (var failedBatch in failedRecipientIDs)
                                    {
                                        _campaignService.UpdateCampaignRecipientsStatus(
                                            new UpdateCampaignRecipientsStatusRequest
                                            {
                                                CampaignRecipientIDs = failedBatch.Item2.ToList(),
                                                Remarks = string.Format("Exception:{0}", failedBatch.Item1.Message),
                                                SentOn = DateTime.UtcNow,
                                                DeliveredOn = DateTime.UtcNow,
                                                DeliveryStatus = CampaignDeliveryStatus.Failed
                                            });
                                    }
                                }

                                var remarks = isCampaignPartialFailed ? "Campaign partially failed due to bad emails" : "The campaign has been picked up for sending";
                                var status = isCampaignPartialFailed ? CampaignStatus.Delayed :
                                             mailRegistration.MailProviderID == LM.MailProvider.MailChimp ? 
                                                CampaignStatus.Analyzing : CampaignStatus.Sent;

                                //if (status == CampaignStatus.Delayed && (campaign.CampaignStatus == CampaignStatus.Scheduled || campaign.CampaignStatus == CampaignStatus.Queued))
                                //{
                                //    SendSupportNotificationEmail(campaign.AccountID, campaign.CreatedBy, campaign.Id, false, campaign.Name);
                                //}
                                //else
                                if (status == CampaignStatus.Delayed && 
                                    campaign.CampaignStatus == CampaignStatus.Delayed &&
                                    (DateTime.UtcNow - campaign.ScheduleTime)?.TotalHours > maxHours)
                                {
                                    status = CampaignStatus.Draft;
                                    //SendSupportNotificationEmail(campaign.AccountID, campaign.CreatedBy, campaign.Id, true, campaign.Name);
                                }

                                _accountService.ScheduleAnalyticsRefresh(campaign.Id, (byte)IndexType.Campaigns);
                                UpdateCampaignTriggerStatus(campaign, status, remarks, serviceProvider, successfulRecipients, campaign.AccountID, isDelayed, mailChimpCampaignId);
                            }
                            catch (Exception ex)
                            {
                                Log.Critical("Campaign batch failed " + campaign, ex);
                                var remarks = "Error while sending campaign, please contact administrator. Additional Info: " + ex;
                                UpdateCampaignTriggerStatus(campaign, CampaignStatus.Failure, remarks, serviceProvider, successfulRecipients, campaign.AccountID, isDelayed);
                            }
                        }
                        else
                        {
                            var remarks = "No campaign recipients found";
                            UpdateCampaignTriggerStatus(campaign, CampaignStatus.Failure, remarks, serviceProvider, successfulRecipients, campaign.AccountID, isDelayed);
                        }

                        //Social posts
                        ShareSocialPost(_socialIntegrationService, campaign);
                    }
                    catch (Exception ex)
                    {
                        var remarks = "Error while sending campaign, Additional Info: " + ex;
                        var campaignKey = "Campaign";
                        if(!ex.Data.Contains(campaignKey))
                            ex.Data.Add(campaignKey, campaign.Id.ToString());

                        _campaignService.UpdateCampaignTriggerStatus(
                            new UpdateCampaignTriggerStatusRequest(campaign.Id)
                            {
                                Status = CampaignStatus.Delayed,
                                SentDateTime = DateTime.UtcNow,
                                Remarks = remarks
                            });

                        Log.Error(remarks, ex);
                    }
                    finally
                    {
                        campaign = _campaignService.GetNextCampaignToTriggerAsync().Result.Campaign;
                    }
                }
                AnalizeMailChimpCampaigns();
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
                            SentDateTime = DateTime.UtcNow,
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

using LandmarkIT.Enterprise.CommunicationManager.Database;
using LandmarkIT.Enterprise.CommunicationManager.MailTriggers;
using LandmarkIT.Enterprise.CommunicationManager.Operations;
using LM = LandmarkIT.Enterprise.CommunicationManager.Requests;
using LandmarkIT.Enterprise.Extensions;
using LandmarkIT.Enterprise.Utilities.ExceptionHandling;
using LandmarkIT.Enterprise.Utilities.Logging;
using SmartTouch.CRM.ApplicationServices.Messaging.Campaigns;
using SmartTouch.CRM.ApplicationServices.Messaging.Communication;
using SmartTouch.CRM.ApplicationServices.ServiceAgents;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.Accounts;
using SmartTouch.CRM.Domain.Campaigns;
using SmartTouch.CRM.Domain.Communication;
using SmartTouch.CRM.Domain.CustomFields;
using SmartTouch.CRM.Domain.Users;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;
using System.Net;
using System.Net.Mail;
using System.Net.Configuration;

namespace SmartTouch.CRM.JobProcessor
{
    public class CampaignProcessor : CronJobProcessor
    {
        bool _MailsendFalg = false; // Added by Ram on 9th May 2018  for Ticket NEXG-3004
        readonly ICampaignService campaignService;
        readonly ICommunicationService communicationService;
        readonly IAccountService accountService;
        readonly IAccountRepository accountRepository;
        readonly IServiceProviderRepository serviceProviderRepository;
        readonly IUserRepository userRepository;
        public CampaignProcessor(CronJobDb cronJob, JobService jobService, string campaignProcessorCacheName)
            : base(cronJob, jobService, campaignProcessorCacheName)
        {
            this.campaignService = IoC.Container.GetInstance<ICampaignService>();
            this.communicationService = IoC.Container.GetInstance<ICommunicationService>();
            this.accountService = IoC.Container.GetInstance<IAccountService>();
            this.accountRepository = IoC.Container.GetInstance<IAccountRepository>();
            this.serviceProviderRepository = IoC.Container.GetInstance<IServiceProviderRepository>();
            this.userRepository = IoC.Container.GetInstance<IUserRepository>();
        }
        protected override void Execute()
        {
            #region local variables

            var customFieldRepository = IoC.Container.GetInstance<ICustomFieldRepository>();
            var socialIntegrationService = IoC.Container.GetInstance<ISocialIntegrationService>();
            var mailService = new MailService();
            var mailChimpCampaignId = string.Empty;
            Campaign campaign = null;
            bool isDelayedCampaign = false;
            #endregion

            try
            {
                //Get the campaign to start processing
                campaign = campaignService.GetNextCampaignToTriggerAsync().Result.Campaign;

                while (campaign != null)
                {
                    isDelayedCampaign = campaign.CampaignStatus == CampaignStatus.Delayed ? true : false;
                    var imageHostingUrl = ConfigurationManager.AppSettings["IMAGE_HOSTING_SERVICE_URL"].ToString();

                    Logger.Current.Informational(string.Format("Campaign {0} has been picked up for sending.", campaign.ToString()));

                    //Campaign primary information
                    var serviceProvider = campaign.ServiceProviderID.HasValue
                        ? communicationService.GetEmailProviderById(new GetServiceProviderByIdRequest { AccountId = campaign.AccountID, ServiceProviderId = campaign.ServiceProviderID.Value }).CampaignEmailProvider
                        : communicationService.GetDefaultCampaignEmailProvider(new GetDefaultCampaignEmailProviderRequest { AccountId = campaign.AccountID }).CampaignEmailProvider;

                    var emailProvider = campaign.ServiceProviderID.HasValue
                        ? campaignService.GetEmailProviderById(new GetServiceProviderSenderEmailRequest { AccountId = campaign.AccountID, RequestedBy = campaign.CreatedBy, ServiceProviderID = campaign.ServiceProviderID.Value })
                        : campaignService.GetDefaultBulkEmailProvider(new GetServiceProviderSenderEmailRequest { AccountId = campaign.AccountID, RequestedBy = campaign.CreatedBy });

                    var accountDomain = serviceProvider.Account.DomainURL.ToLower();
                    var accountAddress = (serviceProvider.Account.Addresses != null && serviceProvider.Account.Addresses.Where(a => a.IsDefault == true).Any()) ? serviceProvider.Account.Addresses.Where(a => a.IsDefault == true).First().ToString() : string.Empty;
                    Logger.Current.Informational(string.Format("Update campaign status to sending {0}", campaign.ToString()));
                    List<int> successfulRecipients = new List<int>();

                    //Update campaign to sending status
                    UpdateCampaignTriggerStatus(campaign, isDelayedCampaign ? CampaignStatus.Retrying : CampaignStatus.Sending, "The campaign has been picked up for sending", serviceProvider, successfulRecipients, campaign.AccountID, isDelayedCampaign);
                    //Campaign Recipients
                    Logger.Current.Informational(string.Format("Get recipients info {0}", campaign.ToString()));
                    try
                    {
                        var campaignRecipientsInfo = campaignService.GetCampaignRecipientsInfo(new GetCampaignRecipientsRequest { CampaignId = campaign.Id, IsLinkedToWorkflow = false });
                        if (campaignRecipientsInfo.RecipientsInfo.IsAny())
                        {
                            var customFieldsValueOptions = customFieldRepository.GetCustomFieldsValueOptions(campaign.AccountID);

                            var campaignRecipients = campaignRecipientsInfo.RecipientsInfo.Select(c => new EmailRecipient
                            {
                                ContactId = Convert.ToInt32(c.Value["CONTACTID"]),
                                CampaignRecipientID = Convert.ToInt32(c.Value["CRID"]),
                                EmailId = c.Value["EMAILID"].ToString().TrimEnd(),
                                ContactFields = c.Value
                            }).ToList();

                            var mailRegistration = mailService.GetMailRegistrationDetails(serviceProvider.LoginToken);
                            var content = "";
                            if (imageHostingUrl != null && serviceProvider.ImageDomain != null && !string.IsNullOrEmpty(serviceProvider.ImageDomain.Domain))
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

                            try
                            {
                                var numberOfIternations = mailProvider.BatchCount == 0 ? 1 : Math.Ceiling(campaignRecipients.Count / (decimal)mailProvider.BatchCount);

                                var isCampaignPartialFailed = default(bool);

                                #region Batch Process
                                for (int i = 0; i < numberOfIternations; i++)
                                {
                                    IEnumerable<int> batchRecipientIDs = campaignRecipients.Skip(mailProvider.BatchCount * i).Take(mailProvider.BatchCount).Select(r => r.CampaignRecipientID).ToList();

                                    var batchCount = mailRegistration.MailProviderID == LM.MailProvider.MailChimp ? campaignRecipients.Count() : mailProvider.BatchCount;
                                    Logger.Current.Informational("Batch count: " + batchCount);
                                    var validRecipients = GetValidEmailRecipients(campaignRecipients.Skip(mailProvider.BatchCount * i).Take(batchCount).ToList());
                                    try
                                    {
                                        Logger.Current.Informational(string.Format("Sending campaign to vmta {0}, iteration {1}", campaign.ToString(), i));
                                        mailChimpCampaignId = mailProvider.SendCampaign(campaign, validRecipients
                                            , customFieldsValueOptions, serviceProvider.AccountCode, accountAddress, accountDomain
                                            , (emailProvider.SenderEmail != null ? emailProvider.SenderEmail.EmailId : string.Empty), mailRegistration);

                                        _MailsendFalg = true; // Added by Ram on 9th May 2018  for Ticket NEXG-3004
                                        UpdateCampaignSentFlagStatus(campaign.Id, _MailsendFalg); // Added by Ram on 9th May 2018  for Ticket NEXG-3004

                                        campaignService.UpdateCampaignRecipientsStatus(new UpdateCampaignRecipientsStatusRequest
                                        {
                                            CampaignRecipientIDs = validRecipients.Select(r => r.CampaignRecipientID).ToList(),
                                            Remarks = "Campaign Sent Successfully",
                                            SentOn = DateTime.UtcNow,
                                            DeliveredOn = DateTime.UtcNow,
                                            DeliveryStatus = CampaignDeliveryStatus.Delivered
                                        });

                                        successfulRecipients.AddRange(batchRecipientIDs);

                                        var invalidRecipients = campaignRecipients.Skip(mailProvider.BatchCount * i).Take(mailProvider.BatchCount).ToList().Except(validRecipients);
                                        if (invalidRecipients != null && invalidRecipients.Any())
                                        {
                                            Logger.Current.Informational(string.Format("Updating invalid email recipients {0}, count {1}", campaign.ToString(), invalidRecipients.Count()));
                                            campaignService.UpdateCampaignRecipientsStatus(new UpdateCampaignRecipientsStatusRequest
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
                                        var deliveryStatus = CampaignDeliveryStatus.Failed;
                                        if (ex is TimeoutException)
                                        {
                                            isCampaignPartialFailed = false;
                                            deliveryStatus = CampaignDeliveryStatus.Delivered;
                                        }
                                        else
                                        {
                                            isCampaignPartialFailed = true;
                                            deliveryStatus = CampaignDeliveryStatus.Failed;
                                        }
                                        if (deliveryStatus == CampaignDeliveryStatus.Delivered)
                                        {
                                            successfulRecipients.AddRange(batchRecipientIDs);
                                        }
                                        campaignService.UpdateCampaignRecipientsStatus(new UpdateCampaignRecipientsStatusRequest
                                        {
                                            CampaignRecipientIDs = batchRecipientIDs.ToList(),
                                            Remarks = string.Format("Exception:{0}", ex.Message),
                                            SentOn = DateTime.UtcNow,
                                            DeliveredOn = DateTime.UtcNow,
                                            DeliveryStatus = deliveryStatus
                                        });

                                        var message = string.Format("Campaign batch failed - {0} - Iteration {1} - Batch Count {2}", campaign.ToString(), i, mailProvider.BatchCount);
                                        Logger.Current.Critical(message, ex);
                                    }
                                }
                                accountService.ScheduleAnalyticsRefresh(campaign.Id, (byte)IndexType.Campaigns);

                                #endregion

                                var remarks = isCampaignPartialFailed ? "Campaign partially failed due to bad emails" : "The campaign has been picked up for sending";
                                var status = isCampaignPartialFailed ? CampaignStatus.Delayed :
                                             (mailRegistration.MailProviderID == LM.MailProvider.MailChimp) ? CampaignStatus.Analyzing :
                                             CampaignStatus.Sent;
                                int maxHours = int.Parse(ConfigurationManager.AppSettings["RetryMaxHours"].ToString());
                                if (status == CampaignStatus.Delayed && (campaign.CampaignStatus == CampaignStatus.Scheduled || campaign.CampaignStatus == CampaignStatus.Queued))
                                {
                                    SendEmail(campaign.AccountID, campaign.CreatedBy, campaign.Id, false, campaign.Name);
                                }
                                else if (status == CampaignStatus.Delayed && campaign.CampaignStatus == CampaignStatus.Delayed && campaign.ScheduleTime.HasValue && DateTime.UtcNow.Subtract(campaign.ScheduleTime.Value).TotalHours > maxHours)
                                {
                                    status = CampaignStatus.Draft;
                                    SendEmail(campaign.AccountID, campaign.CreatedBy, campaign.Id, true, campaign.Name);
                                }
                                UpdateCampaignTriggerStatus(campaign, status, remarks, serviceProvider, successfulRecipients, campaign.AccountID, isDelayedCampaign, mailChimpCampaignId);
                            }
                            catch (Exception ex)
                            {
                                Logger.Current.Critical("Campaign batch failed " + campaign.ToString(), ex);
                                var remarks = "Error while sending campaign, please contact administrator. Additional Info: " + ex.ToString();
                                UpdateCampaignTriggerStatus(campaign, CampaignStatus.Failure, remarks, serviceProvider, successfulRecipients, campaign.AccountID, isDelayedCampaign);
                            }

                        }
                        else
                        {
                            var remarks = "No campaign recipients found";
                            UpdateCampaignTriggerStatus(campaign, CampaignStatus.Failure, remarks, serviceProvider, successfulRecipients, campaign.AccountID, isDelayedCampaign);
                        }

                        //Social posts
                        ShareSocialPost(socialIntegrationService, campaign);
                    }
                    catch (Exception ex)
                    {
                        var remarks = "Error while sending campaign, Additional Info: " + ex.ToString();
                        var campaignKey = "Campaign";
                        if(!ex.Data.Contains(campaignKey))
                            ex.Data.Add(campaignKey, campaign.Id.ToString());

                        campaignService.UpdateCampaignTriggerStatus(new UpdateCampaignTriggerStatusRequest(campaign.Id)
                        {
                            Status = CampaignStatus.Delayed,
                            SentDateTime = DateTime.Now.ToUniversalTime(),
                            Remarks = remarks
                        });

                        Logger.Current.Error(remarks, ex);

                    }
                    finally
                    {
                        campaign = campaignService.GetNextCampaignToTriggerAsync().Result.Campaign;

                    }
                }
                AnalizeMailChimpCampaigns();
           }
            catch (Exception ex)
            {
                ExceptionHandler.Current.HandleException(ex, DefaultExceptionPolicies.LOG_ONLY_POLICY);
                if (campaign != null)
                {
                    var remarks = "Error while sending campaign, please contact administrator. Additional Info: " + ex.ToString();
                    campaignService.UpdateCampaignTriggerStatus(new UpdateCampaignTriggerStatusRequest(campaign.Id)
                    {
                        Status = CampaignStatus.Failure,
                        SentDateTime = DateTime.Now.ToUniversalTime(),
                        Remarks = remarks
                    });
                }

            }
        }

        private void ShareSocialPost(ISocialIntegrationService socialIntegrationService, Campaign campaign)
        {

            var posts = campaignService.GetCampaignPosts(new GetCampaignSocialMediaPostRequest() { CampaignId = campaign.Id, UserId = 0, CommunicationType = string.Empty }).Posts ?? new List<UserSocialMediaPosts>();

            posts.ToList().ForEach(post =>
            {
                Logger.Current.Informational(string.Format("In sending social posts {0}, communication type : {1}", campaign.ToString(), post.CommunicationType));
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
            campaignService.UpdateCampaignTriggerStatus(new UpdateCampaignTriggerStatusRequest(campaign.Id)
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
        /// Find if set of emails are valid or not
        /// </summary>
        /// <param name="recipients"></param>
        /// <returns></returns>
        private static List<EmailRecipient> GetValidEmailRecipients(List<EmailRecipient> recipients)
        {
            var validRecipients = new List<EmailRecipient>();
            foreach (var recipient in recipients)
            {
                if (IsValidEmail(recipient.EmailId))
                    validRecipients.Add(recipient);
            }
            return recipients;
        }
        /// <summary>
        /// Checks if given email is valid or not
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        private static bool IsValidEmail(string email)
        {
            return Regex.IsMatch(email, @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z", RegexOptions.IgnoreCase);
        }
        /// <summary>
        /// Analyze campaign statistics for mailchimp
        /// </summary>
        private void AnalizeMailChimpCampaigns()
        {
            var campaigns = campaignService.GetCampaignsSeekingAnalysis(new GetCampaignsSeekingAnalysisRequest()).Campaigns;
            var uniqueAccounts = campaigns.Select(c => c.AccountID).Distinct();
            var mailChimpAuthKey = new Dictionary<int, IEnumerable<ProviderRegistrationViewModel>>();
            foreach (int accountId in uniqueAccounts)
            {
                var provider = communicationService.GetCommunicationProviders(new GetCommunicatioProvidersRequest() { AccountId = accountId });
                mailChimpAuthKey.Add(accountId, provider.RegistrationListViewModel.Where(p => p.MailProviderID == LM.MailProvider.MailChimp));
            }
            foreach (var campaign in campaigns)
            {
                try
                {
                    Logger.Current.Informational(string.Format("In seeking analysis for mailchimp campaigns, campaign {0}", campaign.ToString()));
                    var total = 0;
                    var providers = mailChimpAuthKey.ContainsKey(campaign.AccountID) ? mailChimpAuthKey[campaign.AccountID] : default(IEnumerable<ProviderRegistrationViewModel>);
                    if (providers != null && providers.Where(provider => provider.ServiceProviderID == campaign.ServiceProviderID).Any())
                    {
                        var authKey = providers.Where(provider => provider.ServiceProviderID == campaign.ServiceProviderID).First().ApiKey;
                        var recipients = new MailChimpCampaign(authKey).AnalyzeCampaign(campaign.ServiceProviderCampaignID, out total);
                        campaignService.UpdateCampaignTriggerStatus(new UpdateCampaignTriggerStatusRequest(campaign.Id)
                        {
                            Status = CampaignStatus.Sent,
                            SentDateTime = DateTime.Now.ToUniversalTime(),
                            ServiceProviderCampaignId = campaign.ServiceProviderCampaignID,
                            SentCount = total,
                            Recipients = recipients.ToList(),
                            Remarks = "Campaign sent successfully",
                            AccountId = campaign.AccountID
                        });
                        accountService.ScheduleAnalyticsRefresh(campaign.Id, (byte)IndexType.Campaigns);
                    }

                }
                catch (Exception ex)
                {
                    Logger.Current.Error("Could not get campaign analytics at this point of time. CampaignId: " + campaign, ex);
                }
            }
        }

        private void SendEmail(int accountId, int userId, int campaignId, bool finalRetry, string campaignName)
        {
            string supportEmail = ConfigurationManager.AppSettings["SupportEmailId"].ToString();
            string email = userRepository.GetUserPrimaryEmail(userId);

            var smtpSection = (SmtpSection)ConfigurationManager.GetSection("system.net/mailSettings/smtp");
            string username = smtpSection.Network.UserName;
            string host = smtpSection.Network.Host;
            int port = smtpSection.Network.Port;
            string password = smtpSection.Network.Password;
            bool ssl = smtpSection.Network.EnableSsl;
            string from = smtpSection.From.Split(';')[0];

            NetworkCredential credentials = new NetworkCredential(username, password);
            MailAddress recipients = new MailAddress(email);
            MailAddress fromMail = new MailAddress(from);

            using (var client = new SmtpClient(host, port)
            {
                Credentials = credentials,
                EnableSsl = ssl
            })
            using (var message = new MailMessage(fromMail, recipients)
            {
                Subject = "'" + campaignName + "' campaign was delayed due to VMTA failure.",
                Body = (finalRetry)?"Scheduled campaign was delayed and changed status to Draft.": "Scheduled campaign was delayed"
            })
            {
                message.CC.Add(supportEmail);
                try
                {
                    Logger.Current.Informational("Sending mail...");
                    client.Send(message);
                }
                catch (SmtpException e)
                {
                    Logger.Current.Error("SMTP error sending email: ", e);
                }
                catch (InvalidOperationException e)
                {
                    Logger.Current.Error("Invalid operation exception while sending SMTP error email: ", e);
                }
            }
        }

        // Added by Ram on 9th May 2018  for Ticket NEXG-3004
        private void UpdateCampaignSentFlagStatus(Int32 _CampaignId, bool _mailsentFalg)
        {
            campaignService.UpdateCampaignSentFlagStatus(_CampaignId, _mailsentFalg);
        }
    }
}

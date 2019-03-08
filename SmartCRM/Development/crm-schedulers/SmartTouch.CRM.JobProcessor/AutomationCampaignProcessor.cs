using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LandmarkIT.Enterprise.CommunicationManager.Database;
using LandmarkIT.Enterprise.CommunicationManager.Operations;
using System.Configuration;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.Domain.CustomFields;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.ApplicationServices.Messaging.Campaigns;
using SmartTouch.CRM.ApplicationServices.Messaging.Communication;
using LandmarkIT.Enterprise.Utilities.Logging;
using LandmarkIT.Enterprise.CommunicationManager.Requests;
using SmartTouch.CRM.Domain.ValueObjects;
using LandmarkIT.Enterprise.CommunicationManager.MailTriggers;
using SmartTouch.CRM.Entities;
using LandmarkIT.Enterprise.Utilities.ExceptionHandling;

namespace SmartTouch.CRM.JobProcessor
{
    public class AutomationCampaignProcessor : CronJobProcessor
    {
        readonly ICampaignService campaignService;
        readonly ICommunicationService communicationService;
        readonly ICustomFieldRepository customFieldRepository;
        readonly IContactRepository contactRepository;
        readonly IAccountService accountService;

        public AutomationCampaignProcessor(CronJobDb cronJob, JobService jobService, string cacheName) : base(cronJob, jobService, cacheName)
        {
            campaignService = IoC.Container.GetInstance<ICampaignService>();
            communicationService = IoC.Container.GetInstance<ICommunicationService>();
            customFieldRepository = IoC.Container.GetInstance<ICustomFieldRepository>();
            contactRepository = IoC.Container.GetInstance<IContactRepository>();
            accountService = IoC.Container.GetInstance<IAccountService>();
        }

        protected override void Execute()
        {
            try
            {
                var imageHostingUrl = ConfigurationManager.AppSettings["IMAGE_HOSTING_SERVICE_URL"].ToString();
                var campaignService = IoC.Container.GetInstance<ICampaignService>();
                var communicationService = IoC.Container.GetInstance<ICommunicationService>();
                var customFieldRepository = IoC.Container.GetInstance<ICustomFieldRepository>();
                var contactRepository = IoC.Container.GetInstance<IContactRepository>();
                var accountService = IoC.Container.GetInstance<IAccountService>();
                var automationCampaignRecipients = campaignService.GetNextAutomationCampaignRecipients(new GetNextAutomationCampaignRecipientsRequest() { }).AutomationCampaignRecipients;

                if (automationCampaignRecipients != null && automationCampaignRecipients.Any())
                {
                    Logger.Current.Informational("Automation campaign recipients found");
                    var campaignIds = automationCampaignRecipients.Select(s => s.CampaignID);
                    var campaigns = campaignService.GetAutomationCampaigns(new GetAutomationCampaignRequest() { CampaignIds = campaignIds }).Campaigns;
                    foreach (var campaign in campaigns)
                    {
                        IEnumerable<EmailRecipient> campaignRecipients = new List<EmailRecipient>();
                        try
                        {
                            Logger.Current.Informational("Request received to get email providers, automation campaign id " + campaign.Id);
                          //  var response = communicationService.GetEmailProviders(new GetEmailProvidersRequest() { AccountId = campaign.AccountID });
                           // if (response.Exception != null)
                                //throw response.Exception;
                            var communicationResponse = (campaign.ServiceProviderID.HasValue && campaign.ServiceProviderID.Value>0)
                        ? communicationService.GetEmailProviderById(new GetServiceProviderByIdRequest { AccountId = campaign.AccountID, ServiceProviderId = campaign.ServiceProviderID.Value }).CampaignEmailProvider
                        : communicationService.GetDefaultCampaignEmailProvider(new GetDefaultCampaignEmailProviderRequest { AccountId = campaign.AccountID }).CampaignEmailProvider;

                            Logger.Current.Informational("Request received to get default email providers, automation campaign id " + campaign.Id);
                            var accountDomain = communicationResponse.Account.DomainURL;
                            var accountAddress = "";
                            if (communicationResponse.Account.Addresses != null && communicationResponse.Account.Addresses.Any())
                            {
                                var primaryAddress = communicationResponse.Account.Addresses.Where(a => a.IsDefault == true).FirstOrDefault();
                                if (primaryAddress != null)
                                    accountAddress = primaryAddress.ToString();
                            }
                            //var serviceProviderGuids = response.ServiceProviderGuids;

                            IEnumerable<Company> companies = default(IEnumerable<Company>);
                            GetServiceProviderSenderEmailResponse providerSenderEmailResponse = campaignService.GetDefaultBulkEmailProvider(new GetServiceProviderSenderEmailRequest()
                            {
                                AccountId = campaign.AccountID,
                                RequestedBy = campaign.CreatedBy
                            });

                            //var recipients = automationCampaignRecipients.Where(cr => cr.CampaignID == campaign.Id);
                            var recipients = campaignService.GetCampaignRecipientsInfo(new GetCampaignRecipientsRequest { CampaignId = campaign.Id, IsLinkedToWorkflow = true });
                            if (recipients != null)
                            {
                                campaignRecipients = recipients.RecipientsInfo.Select(c => new EmailRecipient()
                                {
                                    ContactId = Convert.ToInt32(c.Value["CONTACTID"]),
                                    CampaignRecipientID = Convert.ToInt32(c.Value["CRID"]),
                                    EmailId = c.Value["EMAILID"].ToString().TrimEnd(),
                                    ContactFields = c.Value
                                });
                                companies = contactRepository.GetCompanyDetails(campaignRecipients.Select(c => c.ContactId), campaign.AccountID);
                            }

                            var senderName = campaign.SenderName;

                            MailService mailService = new MailService();
                            MailRegistrationDb mailRegistration = mailService.GetVMTADetails( new List<Guid>() { communicationResponse.LoginToken });
                            var imageDomainResponse = communicationService.GetServiceProviderImageDomain(new GetServiceProviderImageDomainRequest() { Guid = mailRegistration.Guid }).ServiceProvider;
                            if (imageDomainResponse != null && imageDomainResponse.ImageDomain != null && imageDomainResponse.ImageDomain.Domain != null)
                                mailRegistration.ImageDomain = imageDomainResponse.ImageDomain.Domain;

                            var contactTagIds = campaign.ContactTags.Select(c => c.Id).ToList();
                            var searchDefinidtionIds = campaign.SearchDefinitions.Select(c => c.Id).ToList();

                            if (mailRegistration != null && mailRegistration.MailProviderID == MailProvider.SmartTouch)
                            {
                                Logger.Current.Informational("VMTA service provider found for this account");
                                var content = string.Empty;
                                var vmta = new VMTACampaign(mailRegistration.VMTA, mailRegistration.UserName, mailRegistration.Password, mailRegistration.Host, mailRegistration.Port);


                                if (imageHostingUrl != null && mailRegistration.ImageDomain != null)
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

                                if (vmta == null)
                                    continue;
                                var senderEmail = providerSenderEmailResponse.SenderEmail != null ? providerSenderEmailResponse.SenderEmail.EmailId : "";
                                IEnumerable<FieldValueOption> customFieldsValueOptions = customFieldRepository.GetCustomFieldsValueOptions(campaign.AccountID);

                                //var successfulRecipients = vmta.SendCampaign(campaign.Id, campaign.Name, contactTagIds, searchDefinidtionIds
                                //    , campaignRecipients, companies, customFieldsValueOptions, campaign.Subject, campaign.Subject, content
                                //    , campaign.From, senderName, communicationResponse.CampaignEmailProvider.AccountCode
                                //    , mailRegistration.SenderDomain, accountDomain, senderEmail, accountAddress,campaign.AccountID,null);

                                var mailProvider = new SmartTouchProvider();

                                mailProvider.SendCampaign(campaign, campaignRecipients.ToList()
                                        , customFieldsValueOptions, communicationResponse.AccountCode, accountAddress, accountDomain
                                        , senderEmail, mailRegistration);
                                var successfulRecipients = campaignRecipients;

                                Logger.Current.Informational("Automation campaign sent to vmta, automation campaign id " + campaign.Id);
                                if (successfulRecipients != null && successfulRecipients.Any())
                                {
                                    campaignService.UpdateCampaignTriggerStatus(new UpdateCampaignTriggerStatusRequest(campaign.Id)
                                    {
                                        Status = CampaignStatus.Sent,
                                        SentDateTime = DateTime.Now.ToUniversalTime(),
                                        ServiceProviderID = communicationResponse.CommunicationLogID,
                                        Remarks = "Campaign sent successfully",
                                        SentCount = campaignRecipients.Count(),
                                        RecipientIds = campaignRecipients.Select(c => c.CampaignRecipientID),
                                        ServiceProviderCampaignId = communicationResponse.AccountCode + "/" + campaign.Id,
                                        IsRelatedToWorkFlow = true,
                                        AccountId = campaign.AccountID
                                    });
                                    accountService.ScheduleAnalyticsRefresh(campaign.Id, (byte)IndexType.Campaigns);
                                }
                                else
                                    campaignService.UpdateCampaignTriggerStatus(new UpdateCampaignTriggerStatusRequest(campaign.Id)
                                    {
                                        Status = CampaignStatus.Failure,
                                        SentDateTime = DateTime.Now.ToUniversalTime(),
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
                            Logger.Current.Error("Exception occurred while sending a campaign:" + campaign.Id, ex);
                            campaignService.UpdateCampaignTriggerStatus(new UpdateCampaignTriggerStatusRequest(campaign.Id)
                            {
                                Status = CampaignStatus.Failure,
                                SentDateTime = DateTime.Now.ToUniversalTime(),
                                Remarks = ex.InnerException != null && ex.InnerException.Message != null ?
                                        ex.InnerException.Message.ToString() : ex.Message.ToString(),
                                RecipientIds = campaignRecipients.Select(c => c.CampaignRecipientID),
                                IsRelatedToWorkFlow = true,
                                AccountId = campaign.AccountID
                            });
                            ExceptionHandler.Current.HandleException(ex, DefaultExceptionPolicies.LOG_ONLY_POLICY);
                            continue;
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

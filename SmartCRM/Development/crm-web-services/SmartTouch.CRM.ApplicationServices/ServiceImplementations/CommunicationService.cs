using AutoMapper;
using LandmarkIT.Enterprise.Extensions;
using LandmarkIT.Enterprise.Utilities.Logging;
using SmartTouch.CRM.ApplicationServices.Messaging.Communication;
using SmartTouch.CRM.ApplicationServices.ServiceAgents;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.Communication;
using SmartTouch.CRM.Domain.ContactAudit;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using SmartTouch.CRM.SearchEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CMO = LandmarkIT.Enterprise.CommunicationManager.Operations;
using CMR = LandmarkIT.Enterprise.CommunicationManager.Requests;
using CMRS = LandmarkIT.Enterprise.CommunicationManager.Responses;
using LandmarkIT.Enterprise.CommunicationManager.Extensions;
using SmartTouch.CRM.ApplicationServices.Messaging.Campaigns;
using SmartTouch.CRM.Domain.Users;
using SmartTouch.CRM.MessageQueues;
using SmartTouch.CRM.SearchEngine.Indexing;
using SmartTouch.CRM.Domain.ValueObjects;
using LandmarkIT.Enterprise.Utilities.ExceptionHandling;
using System.Diagnostics;
using LandmarkIT.Enterprise.CommunicationManager.Database;
using LandmarkIT.Enterprise.CommunicationManager.Operations;
using SmartTouch.CRM.Domain.Accounts;
using System.Configuration;
using LandmarkIT.Enterprise.CommunicationManager.Processors;
using SmartTouch.CRM.Domain.Workflows;
using Microsoft.Web.Administration;


namespace SmartTouch.CRM.ApplicationServices.ServiceImplementations
{
    public class CommunicationService : ICommunicationService
    {
        readonly ICommunicationRepository communicationRepository;
        readonly IServiceProviderRepository serviceProviderRepository;
        readonly IContactEmailAuditRepository contactEmailAuditRepository;
        readonly IContactTextMessageAuditRepository contactTextMessageAuditRepository;
        readonly IUserRepository userRepository;
        readonly IAccountRepository accountRepository;
        readonly IMessageService messageService;
        readonly IUnitOfWork unitOfWork;
        readonly IContactRepository contactRepository;
        readonly IIndexingService indexingService;

        public CommunicationService(ICommunicationRepository communicationRepository, IServiceProviderRepository serviceProviderRepository,
            IContactEmailAuditRepository contactEmailAuditRepository, IContactTextMessageAuditRepository contactTextMessageAuditRepository,
            IUserRepository userRepository, IUnitOfWork unitOfWork, IMessageService messageService, IContactRepository contactRepository,
            IIndexingService indexingService, IAccountRepository accountRepository)
        {
            if (communicationRepository == null) throw new ArgumentNullException("communicationRepository");
            if (unitOfWork == null) throw new ArgumentNullException("unitOfWork");
            this.communicationRepository = communicationRepository;
            this.serviceProviderRepository = serviceProviderRepository;
            this.contactEmailAuditRepository = contactEmailAuditRepository;
            this.contactTextMessageAuditRepository = contactTextMessageAuditRepository;
            this.userRepository = userRepository;
            this.accountRepository = accountRepository;
            this.messageService = messageService;
            this.contactRepository = contactRepository;
            this.unitOfWork = unitOfWork;
            this.indexingService = indexingService;
        }

        private CommunicationTrackerResponse SaveCommunicationTrackerRequest(CommunicationTrackerRequest request)
        {
            Logger.Current.Verbose("Request to saving communication tracker information.");
            CommunicationTracker communicationTracker = Mapper.Map<CommunicationTrackerViewModel, CommunicationTracker>(request.CommunicationTrackerViewModel);
            communicationRepository.Insert(communicationTracker);
            unitOfWork.Commit();
            return new CommunicationTrackerResponse();
        }

        public CommunicationTrackerResponse GetFindByContactId(CommunicationTrackerRequest request)
        {
            Logger.Current.Verbose("Request to fetch communication tracker information by using contactId.");
            CommunicationTrackerResponse response = new CommunicationTrackerResponse();
            CommunicationTracker communicationTracker = communicationRepository.FindByContactId(Convert.ToInt16(request.CommunicationTrackerViewModel.ContactID),
               request.CommunicationTrackerViewModel.CommunicationTypeID);
            if (communicationTracker != null)
            {
                response.CommunicationTrackerViewModel = Mapper.Map<CommunicationTracker, CommunicationTrackerViewModel>(communicationTracker as CommunicationTracker);
            }
            return response;
        }

        public CMRS.RegistrationResponse CommunicationProviderRegistration(CommunicationProviderRegistrationRequest request)
        {
            Logger.Current.Verbose("Request to Register the communication providers for particular account.");
            CMRS.RegistrationResponse response = new CMRS.RegistrationResponse();

            CMO.ServiceRegistration serviceRegistration = new CMO.ServiceRegistration();
            CMR.RegisterMailRequest registerMailRequest = new CMR.RegisterMailRequest();
            CMR.RegisterTextRequest registerTextRequest = new CMR.RegisterTextRequest();
            foreach (ProviderRegistrationViewModel registerviewmodel in request.ProviderRegistrationViewModels)
            {
                if (registerviewmodel.RegistrationId == 0 && (registerviewmodel.UserName != "" || registerviewmodel.ApiKey != ""))
                {

                    if (registerviewmodel.CommunicationType == CommunicationType.Mail)
                    {
                        registerMailRequest = Mapper.Map<ProviderRegistrationViewModel, CMR.RegisterMailRequest>(registerviewmodel);
                        response = MailRequestConverterExtensions.EmailRegistrationRequest(registerMailRequest);
                        ServiceProvider serviceprovider = InsertServiceProviderDetails(registerviewmodel, request.AccountId, request.RequestedBy.Value, response);
                        if (serviceprovider != null)
                            serviceProviderRepository.InsertServiceproviderEmail(serviceprovider, registerviewmodel.Email);

                    }
                    else if (registerviewmodel.CommunicationType == CommunicationType.Text)
                    {
                        registerTextRequest = Mapper.Map<ProviderRegistrationViewModel, CMR.RegisterTextRequest>(registerviewmodel);
                        response = MailRequestConverterExtensions.TextRegistrationRequest(registerTextRequest);
                        ServiceProvider serviceprovider = InsertServiceProviderDetails(registerviewmodel, request.AccountId, request.RequestedBy.Value, response);
                        if (serviceprovider != null)
                            serviceProviderRepository.InsertServiceproviderEmail(serviceprovider, registerviewmodel.Email);
                    }
                }
                else if (registerviewmodel.RegistrationId > 0)
                {
                    if (registerviewmodel.CommunicationType == CommunicationType.Mail)
                    {
                        registerMailRequest = Mapper.Map<ProviderRegistrationViewModel, CMR.RegisterMailRequest>(registerviewmodel);
                        response = serviceRegistration.UpdateMailRegistration(registerMailRequest);
                        ServiceProvider serviceprovider = updateServiceProviderDetails(registerviewmodel, request, response);
                        serviceProviderRepository.UpdateServiceproviderEmail(serviceprovider, registerviewmodel.Email);
                    }
                    else if (registerviewmodel.CommunicationType == CommunicationType.Text)
                    {
                        registerTextRequest = Mapper.Map<ProviderRegistrationViewModel, CMR.RegisterTextRequest>(registerviewmodel);
                        response = serviceRegistration.UpdateTextRegistration(registerTextRequest);
                        updateServiceProviderDetails(registerviewmodel, request, response);
                    }
                }
            }
            return response;
        }

        private ServiceProvider InsertServiceProviderDetails(ProviderRegistrationViewModel registerviewmodel, int accountID, int userID,
           LandmarkIT.Enterprise.CommunicationManager.Responses.RegistrationResponse response)
        {
            Logger.Current.Verbose("Inserting  the serviceprovider details for particular Provider" + registerviewmodel.MailProviderType);
            ServiceProvider provider = new ServiceProvider
            {
                CommunicationTypeID = registerviewmodel.CommunicationType,
                LoginToken = response.Token,
                CreatedBy = userID,
                CreatedDate = DateTime.Now.ToUniversalTime(),
                AccountId = accountID,
                MailType = registerviewmodel.MailProviderType,
                IsDefault = registerviewmodel.IsDefault,
                SenderPhoneNumber = registerviewmodel.SenderPhoneNumber,
                ProviderName = registerviewmodel.ProviderName
            };
            serviceProviderRepository.Insert(provider);
            return unitOfWork.Commit() as ServiceProvider;
        }

        private ServiceProvider updateServiceProviderDetails(ProviderRegistrationViewModel registerviewmodel, CommunicationProviderRegistrationRequest request,
          LandmarkIT.Enterprise.CommunicationManager.Responses.RegistrationResponse response)
        {
            Logger.Current.Verbose("updating  the serviceprovider details for particular Provider" + registerviewmodel.MailProviderType);
            ServiceProvider provider = new ServiceProvider
            {
                CommunicationTypeID = registerviewmodel.CommunicationType,
                LoginToken = response.Token,
                CreatedBy = (int)request.RequestedBy,
                CreatedDate = DateTime.Now.ToUniversalTime(),
                AccountId = request.AccountId,
                MailType = registerviewmodel.MailProviderType,
                IsDefault = registerviewmodel.IsDefault,
                Id = registerviewmodel.ServiceProviderID,
                SenderPhoneNumber = registerviewmodel.SenderPhoneNumber,
                ProviderName = registerviewmodel.ProviderName,
                ImageDomainId = registerviewmodel.ImageDomainId
            };
            serviceProviderRepository.Update(provider);
            return unitOfWork.Commit() as ServiceProvider;
        }

        public GetDefaultCampaignEmailProviderResponse GetDefaultCampaignEmailProvider(GetDefaultCampaignEmailProviderRequest request)
        {
            GetDefaultCampaignEmailProviderResponse response = new GetDefaultCampaignEmailProviderResponse();
            Logger.Current.Verbose("Request received to get default campaign service provider for account id: " + request.AccountId);
            var serviceProvider = serviceProviderRepository.GetDefaultCampaignProvider(request.AccountId);
            if (serviceProvider == null)
                throw new UnsupportedOperationException("Default campaign provider details are not configured.");
            response.CampaignEmailProvider = Mapper.Map<ServiceProvider, ServiceProviderViewModel>(serviceProvider);
            return response;
        }

        public GetServiceProviderByIdResponse GetEmailProviderById(GetServiceProviderByIdRequest request)
        {
            Logger.Current.Verbose("Request received to get default campaign service provider for account id: " + request.AccountId);
            var serviceProvider = serviceProviderRepository.GetServiceProviderById(request.AccountId, request.ServiceProviderId);
            if (serviceProvider == null)
                throw new UnsupportedOperationException("Default campaign provider details are not configured.");
            return new GetServiceProviderByIdResponse() { CampaignEmailProvider = Mapper.Map<ServiceProvider, ServiceProviderViewModel>(serviceProvider) };
        }

        public GetEmailProvidersResponse GetEmailProviders(GetEmailProvidersRequest request)
        {
            Logger.Current.Informational("Request received for fetching email providers for accountId " + request.AccountId);
            GetEmailProvidersResponse response = new GetEmailProvidersResponse();
            response.ServiceProviderGuids = serviceProviderRepository.GetEmailProviderTokens(request.AccountId);
            return response;
        }

        public SendMailResponse SendMail(SendMailRequest request)
        {
            Logger.Current.Verbose("Request to send mail to respective contacts.");
            SendMailResponse response = new SendMailResponse();
            LandmarkIT.Enterprise.CommunicationManager.Requests.SendMailRequest sendMailRequest =
                        Mapper.Map<SendMailViewModel, CMR.SendMailRequest>(request.SendMailViewModel as SendMailViewModel);

            sendMailRequest.RequestGuid = Guid.NewGuid();
            var emailRecipients = new List<EmailRecipient>();
            try
            {
                if (sendMailRequest != null)
                {
                    if (sendMailRequest.To != null && sendMailRequest.To.Count > 0)
                        emailRecipients.AddRange(sendMailRequest.To.Select(s => new EmailRecipient() { EmailId = s }));

                    if (sendMailRequest.CC != null && sendMailRequest.CC.Count > 0)
                        emailRecipients.AddRange(sendMailRequest.CC.Select(s => new EmailRecipient() { EmailId = s }));

                    if (sendMailRequest.BCC != null && sendMailRequest.BCC.Count > 0)
                        emailRecipients.AddRange(sendMailRequest.BCC.Select(s => new EmailRecipient() { EmailId = s }));
                }
                Guid emailLoginToken = new Guid();
                IEnumerable<ServiceProvider> serviceProviders = serviceProviderRepository.GetAccountCommunicationProviders(request.AccountId, CommunicationType.Mail, MailType.TransactionalEmail);

                if (serviceProviders.IsAny() && serviceProviders.FirstOrDefault() != null)
                    emailLoginToken = serviceProviders.FirstOrDefault().LoginToken;
                else
                    throw new UnsupportedOperationException("[|Email providers are not configured for this account|]");

                var emailSentStatus = false;
                LandmarkIT.Enterprise.CommunicationManager.Requests.SendMailRequest mailRequest = new LandmarkIT.Enterprise.CommunicationManager.Requests.SendMailRequest();
                MailService mailService = new MailService();
                var domainurl = accountRepository.GetAccountDomainUrl(request.AccountId);
                if (emailLoginToken != new Guid())
                {
                    try
                    {
                        mailRequest.Body = request.SendMailViewModel.Body;
                        mailRequest.From = request.SendMailViewModel.From;
                        mailRequest.IsBodyHtml = true;
                        mailRequest.Subject = request.SendMailViewModel.Subject;
                        mailRequest.ScheduledTime = DateTime.Now.ToUniversalTime().AddSeconds(5);
                        mailRequest.DisplayName = request.UserName;
                        mailRequest.To = emailRecipients.Select(s => s.EmailId).ToList();
                        mailRequest.TokenGuid = emailLoginToken;
                        mailRequest.RequestGuid = sendMailRequest.RequestGuid;
                        mailRequest.AccountDomain = domainurl;
                        mailRequest.CategoryID = (byte)EmailNotificationsCategory.ContactSendEmail;
                        mailRequest.AccountID = request.AccountId;
                        mailService.Send(mailRequest);
                        emailSentStatus = true;
                    }
                    catch (Exception ex)
                    {
                        Logger.Current.Error("Error Logging while  sending an email", ex);
                        throw new UnsupportedOperationException(ex.Message);
                    }
                }

                if (emailSentStatus == true)
                {
                    response.ResponseStatus = CommunicationStatus.Success;
                    InsertEmailAuditData(request.SendMailViewModel, new CMRS.SendMailResponse() { StatusID = CMRS.CommunicationStatus.Success }, sendMailRequest);
                }

                //MailService mailService = new MailService();
                //ServiceProvider serviceProvider = serviceProviderRepository.GetAccountCommunicationProviders(request.AccountId, CommunicationType.Mail, MailType.TransactionalEmail).FirstOrDefault();
                //if (serviceProvider == null || serviceProvider.LoginToken == Guid.Empty)
                //{
                //    response.Exception = new Exception("Campaign service provider is not configured for this account");
                //    return response;
                //}
                //MailRegistrationDb mailRegistration = mailService.GetMailRegistrationDetails(serviceProvider.LoginToken);
                //if (mailRegistration == null)
                //{
                //    response.Exception = new Exception("Not able to find default provider details for this account");
                //    return response;
                //}
                //var senderEmail = accountRepository.GetServiceProviderEmail(serviceProvider.Id);
                //var emailRecipients = new List<EmailRecipient>();
                //if (sendMailRequest != null)
                //{
                //    if (sendMailRequest.To != null && sendMailRequest.To.Count > 0)
                //        emailRecipients.AddRange(sendMailRequest.To.Select(s => new EmailRecipient() { EmailId = s }));

                //    if (sendMailRequest.CC != null && sendMailRequest.CC.Count > 0)
                //        emailRecipients.AddRange(sendMailRequest.CC.Select(s => new EmailRecipient() { EmailId = s }));

                //    if (sendMailRequest.BCC != null && sendMailRequest.BCC.Count > 0)
                //        emailRecipients.AddRange(sendMailRequest.BCC.Select(s => new EmailRecipient() { EmailId = s }));
                //}
                //Logger.Current.Informational("Total no of recipients : " + emailRecipients.Count);


                //var emailSentStatus = false;

                //CMRS.SendMailResponse sendMailResponse = new CMRS.SendMailResponse()
                //{
                //    Token = serviceProvider.LoginToken,
                //    RequestGuid = sendMailRequest.RequestGuid,
                //    StatusID = CMRS.CommunicationStatus.Queued,
                //    ServiceResponse = string.Empty
                //};

                //if (mailRegistration != null)
                //{
                //    if (mailRegistration.MailProviderID == CMR.MailProvider.MailChimp)
                //    {
                //        Logger.Current.Verbose("Email is being sent by MailChimp. RegistrationID: " + mailRegistration.MailRegistrationID);
                //        var mc = new MailChimpCampaign(mailRegistration.APIKey);
                //        IEnumerable<Company> companies = new List<Company>() { };
                //        string mailChimpCampaignId = mc.SendCampaign(0, "Send Email", emailRecipients, companies, sendMailRequest.Subject, sendMailRequest.Subject,
                //            sendMailRequest.Body, request.SendMailViewModel.From, request.SendMailViewModel.SenderName, null, senderEmail.AccountID, null, null);

                //        sendMailRequest.RequestGuid = Guid.NewGuid();


                //        if (!string.IsNullOrEmpty(mailChimpCampaignId))
                //            emailSentStatus = true;
                //        else
                //        {
                //            response.Exception = new Exception("Unable to send email from MailChimp.");
                //            response.ResponseMessage = CommunicationStatus.Failure;

                //        }
                //    }
                //    else if (mailRegistration.MailProviderID == CMR.MailProvider.SmartTouch)
                //    {
                //        Logger.Current.Verbose("Send email is being sent by VMTA. RegistrationID: " + mailRegistration.MailRegistrationID);
                //        var content = string.Empty;
                //        var imageHostingUrl = ConfigurationManager.AppSettings["IMAGE_HOSTING_SERVICE_URL"].ToString();
                //        var imageDomain = serviceProvider.ImageDomain;
                //        var vmta = new VMTACampaign(mailRegistration.VMTA, mailRegistration.UserName, mailRegistration.Password, mailRegistration.Host, mailRegistration.Port);

                //        if (imageHostingUrl != null && imageDomain != null && imageDomain.Status != false && !string.IsNullOrEmpty(imageDomain.Domain))
                //            content = sendMailRequest.Body.Replace(imageHostingUrl, serviceProvider.ImageDomain.Domain);
                //        else
                //            content = sendMailRequest.Body;

                //        var vmtaEmail = senderEmail != null ? senderEmail.EmailId : "";
                //        Logger.Current.Verbose("Vmta email:  " + vmtaEmail);
                //        sendMailRequest.RequestGuid = Guid.NewGuid();

                //        var successful = vmta.SendEmail(content, emailRecipients.Select(s => s.EmailId), request.SendMailViewModel.From, request.SendMailViewModel.SenderName, sendMailRequest.Subject, vmtaEmail, mailRegistration.SenderDomain, request.AccountDomain);
                //        if (successful)
                //            emailSentStatus = true;
                //        else
                //        {
                //            response.Exception = new Exception("Unable to send email from VMTA.");
                //            response.ResponseMessage = CommunicationStatus.Failure;
                //        }
                //    }
                //    else if (mailRegistration.MailProviderID == CMR.MailProvider.Smtp || mailRegistration.MailProviderID == CMR.MailProvider.SendGrid)
                //    {
                //        Logger.Current.Verbose("Send email is being sent by SMTP. RegistrationID: " + mailRegistration.MailRegistrationID);
                //        var smtp = new SmtpMailService(mailRegistration);
                //        var smtpResponse = smtp.Send(new LandmarkIT.Enterprise.CommunicationManager.Requests.SendMailRequest()
                //        {
                //            CampaignReceipients = emailRecipients,
                //            From = request.SendMailViewModel.From,
                //            DisplayName = request.SendMailViewModel.SenderName,
                //            Subject = sendMailRequest.Subject,
                //            IsBodyHtml = true,
                //            Body = sendMailRequest.Body
                //        });

                //        sendMailRequest.RequestGuid = Guid.NewGuid();


                //        if (smtpResponse.StatusID == CMRS.CommunicationStatus.Success)
                //            emailSentStatus = true;
                //        else
                //        {
                //            response.Exception = new Exception("Unable to send email through Sendgrid.");
                //            response.ResponseMessage = CommunicationStatus.Failure;
                //        }
                //    }

                //    if (emailSentStatus == true)
                //    {
                //        response.ResponseMessage = CommunicationStatus.Success;
                //        mailService.LogResponse(sendMailRequest, sendMailResponse);
                //        InsertEmailAuditData(request.SendMailViewModel, new CMRS.SendMailResponse() { StatusID = CMRS.CommunicationStatus.Success }, sendMailRequest);
                //    }
            }
            catch (Exception ex)
            {
                Logger.Current.Error("An error occured while sending an email using send-email functionality. ", ex);
            }

            return response;
        }

        public ScheduleMailResponse ScheduleEmail(ScheduleMailRequest request)
        {
            ScheduleMailResponse response = new ScheduleMailResponse();
            Guid loginToken = new Guid();
            string toEmail = string.Empty;
            string fromEmail = string.Empty;
            Email ContactEmail = communicationRepository.GetContactEmail(request.ContactId, request.AccountId);
            Email UserEmail = userRepository.GetUserEmail(request.RequestedBy.Value);
            if (request.SendMailViewModel != null && request.ContactId != 0 && request.RequestedBy.HasValue && ContactEmail != null && UserEmail != null && UserEmail.UserID.HasValue && !string.IsNullOrEmpty(ContactEmail.EmailId))
            {
                SendMailViewModel SendMailViewModel = request.SendMailViewModel;
                toEmail = ContactEmail.EmailId;
                fromEmail = UserEmail.EmailId;
                IEnumerable<string> fromEmails = new List<string> { fromEmail };

                UserBasicInfo userBasifInfo = userRepository.FindUsersByEmails(fromEmails, SendMailViewModel.AccountID).FirstOrDefault();
                var domainurl = accountRepository.GetAccountDomainUrl(SendMailViewModel.AccountID);
                IEnumerable<ServiceProvider> serviceProviders = serviceProviderRepository.GetAccountCommunicationProviders(SendMailViewModel.AccountID, CommunicationType.Mail, MailType.TransactionalEmail);
                if (serviceProviders.FirstOrDefault() != null)
                    loginToken = serviceProviders.FirstOrDefault().LoginToken;
                LandmarkIT.Enterprise.CommunicationManager.Requests.SendMailRequest mailRequest = new LandmarkIT.Enterprise.CommunicationManager.Requests.SendMailRequest();
                if (loginToken != new Guid() && toEmail != null && fromEmail != null)
                {
                    EmailAgent agent = new EmailAgent();
                    mailRequest.Body = SendMailViewModel.Body;
                    mailRequest.From = fromEmail;
                    mailRequest.IsBodyHtml = true;
                    mailRequest.ScheduledTime = DateTime.UtcNow.AddMinutes(2);
                    mailRequest.Subject = SendMailViewModel.Subject;
                    mailRequest.DisplayName = userBasifInfo.FirstName + " " + userBasifInfo.LastName;
                    mailRequest.To = new List<string>() { toEmail };
                    mailRequest.TokenGuid = loginToken;
                    mailRequest.RequestGuid = Guid.NewGuid();
                    mailRequest.AccountDomain = domainurl;
                    mailRequest.CategoryID = (byte)EmailNotificationsCategory.WorkflowSendEmail;
                    mailRequest.AccountID = SendMailViewModel.AccountID;
                    agent.SendEmail(mailRequest);
                }
                UpdateLastTouched(request.ContactId);
                AuditEmailData(ContactEmail.EmailID, toEmail, UserEmail.UserID.Value, CMRS.CommunicationStatus.Success, mailRequest.RequestGuid);
                accountRepository.InsertIndexingData(new IndexingData() { IndexType = (int)IndexType.Contacts, EntityIDs = new List<int>() { request.ContactId } });
            }

            return response;
        }

        private void UpdateLastTouched(int contactId)
        {
            LastTouchedDetails details = new LastTouchedDetails()
            {
                ContactID = contactId,
                LastTouchedDate = DateTime.UtcNow
            };
            contactRepository.UpdateLastTouchedInformation(new List<LastTouchedDetails>() { details }, AppModules.SendMail, null);
        }

        private void AuditEmailData(int contactEmailIdKey, string contactEmail, int userId, CMRS.CommunicationStatus status, Guid requestGuid)
        {
            ContactEmailAudit emailAudit = new ContactEmailAudit();
            emailAudit.ContactEmailID = contactEmailIdKey;
            emailAudit.RequestGuid = requestGuid;
            emailAudit.Status = (byte)status;
            emailAudit.SentBy = userId;
            emailAudit.SentOn = DateTime.Now.ToUniversalTime();

            contactEmailAuditRepository.InsertContactEmailAuditList(new List<ContactEmailAudit>() { emailAudit });
            unitOfWork.Commit();
        }



        private void InsertEmailAuditData(SendMailViewModel sendMailViewModel, CMRS.SendMailResponse response, CMR.SendMailRequest request)
        {
            Logger.Current.Verbose("Inserting the ContactEmails data into contactEmailAudit table.");
            List<LastTouchedDetails> lasttouched = new List<LastTouchedDetails>();
            var contactemailids = sendMailViewModel.Contacts.Select(x => (int)x.DocumentId);
            var contacts = sendMailViewModel.Contacts.ToList();
            var contactids = contactRepository.GetContactByEmailID(contactemailids).ToList();
            List<ContactEmailAudit> emailAudit = new List<ContactEmailAudit>();
            for (var ent = 0; ent < contactids.Count(); ent++)
            {
                var entry = contacts[ent];
                DateTime sentOn = DateTime.Now.ToUniversalTime();
                LastTouchedDetails details = new LastTouchedDetails()
                {
                    ContactID = contactids[ent],
                    LastTouchedDate = sentOn
                };
                var contactEmailAudit = new ContactEmailAudit
                {
                    ContactEmailID = entry.DocumentId,
                    SentBy = entry.DocumentOwnedBy,
                    SentOn = sentOn,
                    Status = (byte)response.StatusID,
                    RequestGuid = request.RequestGuid
                };
                lasttouched.Add(details);
                emailAudit.Add(contactEmailAudit);

                addToTopicEmailSent(contactEmailAudit.ContactEmailID, sendMailViewModel.AccountID, contactids[ent]);
            }
            contactEmailAuditRepository.InsertContactEmailAuditList(emailAudit);
            unitOfWork.Commit();
            contactRepository.UpdateLastTouchedInformation(lasttouched, AppModules.SendMail, null);

            IEnumerable<Contact> Contacts = contactRepository.FindAll(contactids);
            indexingService.IndexContacts(Contacts);
        }

        void addToTopicEmailSent(int contactEmailID, int accountId, int contactId)
        {
            var message = new TrackMessage()
            {
                EntityId = contactEmailID,
                AccountId = accountId,
                ContactId = contactId,
                LeadScoreConditionType = (int)LeadScoreConditionType.AnEmailSent
            };
            messageService.SendMessages(new Messaging.Messages.SendMessagesRequest()
            {
                Message = message
            });
        }

        private void IndexContacts(List<int> contactids)
        {
            IEnumerable<Contact> Contacts = contactRepository.FindAll(contactids);
            indexingService.IndexContacts(Contacts);
        }

        public CampaignUnsubscribeResponse UpdateContactEmailStatus(CampaignUnsubscribeRequest request)
        {
            CampaignUnsubscribeResponse response = new CampaignUnsubscribeResponse();

            var contactId = contactEmailAuditRepository.UpdateContactEmailStatus(request.AccountId, request.ContactId, request.CampaignId, request.Email, request.SnoozeUntil);
            accountRepository.ScheduleAnalyticsRefresh(request.ContactId, (byte)IndexType.Contacts);
            if (contactId != null)
            {
                response.contactId = contactId;
            }

            addToTopic(request.ContactId, request.AccountId, request.SnoozeUntil);
            return response;
        }

        public SendTextResponse SendText(SendTextRequest request)
        {
            Logger.Current.Verbose("Request for sendText");
            SendTextResponse response = new SendTextResponse();

            SendTextViewModel sendTextViewModel = request.SendTextViewModel;
            ServiceProvider serviceProviders = serviceProviderRepository.GetSendTextServiceProviders(sendTextViewModel.AccountID, Entities.CommunicationType.Text);
            if (serviceProviders.LoginToken == new Guid())
                throw new UnsupportedOperationException("Default Sender Number not configured.");
            if (sendTextViewModel.ServiceProvider != null)
            {
                CMR.SendTextRequest sendTextRequest = Mapper.Map<SendTextViewModel, CMR.SendTextRequest>(sendTextViewModel as SendTextViewModel);
                var contactphonenumberids = sendTextViewModel.Contacts.Select(x => (int)x.DocumentId);
                var contactids = contactRepository.GetContactByPhoneNumberID(contactphonenumberids).ToList();

                //var contactids = sendTextViewModel.Contacts.Select(x => (int)x.ContactID).ToList();
                List<ContactOwnerPhone> ownerNumbers = contactRepository.GetContactOwerPhoneNubers(contactids.Select(v => v.Value).ToList());
                if (ownerNumbers.IsAny())
                {
                    ownerNumbers.Each(n =>
                    {
                        contactids.Each(c =>
                        {
                            if (n.ContactID == c.Value)
                                n.ContactNumber = sendTextViewModel.Contacts.Where(p => (int)p.DocumentId == c.Key).Select(s => (string)s.Phone).FirstOrDefault();

                        });
                    });
                }

                sendTextRequest.OwnerNumbers = ownerNumbers;

                sendTextRequest.TokenGuid = sendTextViewModel.ServiceProvider.LoginToken;
                TextAgent agent = new TextAgent();
                sendTextRequest.RequestGuid = Guid.NewGuid();
                var varsentTextresponse = agent.SendText(sendTextRequest);

                if (varsentTextresponse == null)
                {
                    response = null;
                }
                else
                {
                    response.SMSStatus = Convert.ToString(varsentTextresponse.StatusID);
                    if (response.SMSStatus == "Success")
                    {
                        InsertTextMessageAuditData(sendTextRequest, varsentTextresponse, sendTextViewModel, contactids.Select(i => i.Value).ToList());
                    }
                    response.Message = varsentTextresponse.ServiceResponse;
                }
            }
            return response;
        }

        private void InsertTextMessageAuditData(CMR.SendTextRequest request, CMRS.SendTextResponse response, SendTextViewModel sendtextviewModel, List<int> contactIds)
        {
            Logger.Current.Verbose("Inserting the ContactTextMessages data into contactTextAudit table.");
            List<LastTouchedDetails> lasttouched = new List<LastTouchedDetails>();
            var contacts = sendtextviewModel.Contacts.ToList();
            for (var ent = 0; ent < contacts.Count(); ent++)
            {
                var entry = contacts[ent];
                DateTime sentOn = DateTime.Now.ToUniversalTime();

                if (ent < contactIds.Count())
                {
                    LastTouchedDetails details = new LastTouchedDetails()
                    {
                        ContactID = contactIds[ent],
                        LastTouchedDate = sentOn
                    };

                    var contactTextAudit = new ContactTextMessageAudit
                    {
                        ContactPhoneNumberID = entry.DocumentId,
                        SentBy = entry.DocumentOwnedBy,
                        SentOn = DateTime.Now.ToUniversalTime(),
                        Status = (byte)response.StatusID,
                        RequestGuid = request.RequestGuid
                    };
                    contactTextMessageAuditRepository.Insert(contactTextAudit);
                    addToTopicTextSent(contactTextAudit.ContactPhoneNumberID, sendtextviewModel.AccountID, contactIds[ent]);
                    lasttouched.Add(details);
                }
            }
            unitOfWork.Commit();
            contactRepository.UpdateLastTouchedInformation(lasttouched, AppModules.SendText, null);
            IEnumerable<Contact> Contacts = contactRepository.FindAll(contactIds);
            indexingService.IndexContacts(Contacts);
        }

        void addToTopicTextSent(int contactPhoneNumberId,int accountId,int contactId)
        {
            var message = new TrackMessage()
            {
                EntityId = contactPhoneNumberId,
                AccountId = accountId,
                ContactId = contactId,
                LeadScoreConditionType = (int)LeadScoreConditionType.ContactSendText
            };
            messageService.SendMessages(new Messaging.Messages.SendMessagesRequest()
            {
                Message = message
            });
        }

        public SendTextResponse GetSendTextviewModel(SendTextRequest request)
        {
            SendTextResponse response = new SendTextResponse();
            SendTextViewModel viewmodel = new SendTextViewModel();
            ServiceProvider provider = serviceProviderRepository.GetSendTextServiceProviders(request.AccountId, CommunicationType.Text);
            viewmodel.ServiceProvider = Mapper.Map<ServiceProvider, ServiceProviderViewModel>(provider);
            List<string> phoneNumbers = new List<string>();
            var userPhoneNumbers = userRepository.GetUserPhoneNumbers(request.UserId);
            if (userPhoneNumbers.Any())
            {
                phoneNumbers = userPhoneNumbers;
                viewmodel.From = userPhoneNumbers[0];
            }
            if (viewmodel.ServiceProvider != null && viewmodel.ServiceProvider.LoginToken != new Guid())
            {
                phoneNumbers.Add(viewmodel.ServiceProvider.SenderPhoneNumber);
                viewmodel.From = viewmodel.ServiceProvider.SenderPhoneNumber;
            }
            else
                viewmodel.From = "";

            viewmodel.FromPhones = phoneNumbers;
            response.SendTextViewModel = viewmodel;
            return response;
        }

        public GetCommunicatioProvidersResponse GetCommunicationProviders(GetCommunicatioProvidersRequest request)
        {
            Logger.Current.Verbose("Fetching the communication providers for Account: " + request.AccountId);
            GetCommunicatioProvidersResponse response = new GetCommunicatioProvidersResponse();
            IList<ProviderRegistrationViewModel> RegistrationList = new List<ProviderRegistrationViewModel>();
            IList<ProviderViewModel> campaignProviderList = new List<ProviderViewModel>();
            IEnumerable<ServiceProvider> serviceProviders = request.FromCache
                ? serviceProviderRepository.AccountServiceProviders(request.AccountId, request.FromCache)
                : serviceProviderRepository.AccountServiceProviders(request.AccountId);
            if (serviceProviders != null)
            {
                CMO.MailService mailService = new CMO.MailService();
                CMO.TextService textService = new CMO.TextService();


                foreach (ServiceProvider provider in serviceProviders)
                {
                    ProviderRegistrationViewModel registrationViewModel = new ProviderRegistrationViewModel();
                    if (provider.CommunicationTypeID == CommunicationType.Mail)
                    {
                        var registration = mailService.GetMailRegistrationDetails(provider.LoginToken);
                        var AccountEmail = serviceProviderRepository.GetServiceProviderEmail(provider.Id);
                        if (registration != null)
                        {
                            registrationViewModel.RegistrationId = registration.MailRegistrationID;
                            registrationViewModel.UserName = registration.UserName;
                            registrationViewModel.Password = registration.Password;
                            registrationViewModel.ApiKey = registration.APIKey;
                            registrationViewModel.MailProviderID = registration.MailProviderID;
                            registrationViewModel.MailProviderType = provider.MailType;
                            registrationViewModel.RequestGuid = registration.Guid;
                            registrationViewModel.SenderFriendlyName = registration.Name;
                            registrationViewModel.CommunicationType = CommunicationType.Mail;
                            registrationViewModel.IsDefault = provider.IsDefault;
                            registrationViewModel.Host = registration.Host;
                            registrationViewModel.ServiceProviderID = provider.Id;
                            registrationViewModel.SenderDomain = registration.SenderDomain;
                            registrationViewModel.ImageDomain = registration.ImageDomain;
                            registrationViewModel.VMTA = registration.VMTA;
                            registrationViewModel.Port = registration.Port;
                            registrationViewModel.MailChimpListID = registration.MailChimpListID;
                            registrationViewModel.ImageDomainId = provider.ImageDomainId;
                            registrationViewModel.ProviderName = provider.ProviderName;
                            if (provider.MailType == MailType.BulkEmail)
                            {
                                ProviderViewModel providerView = new ProviderViewModel();
                                providerView.ServiceProviderId = provider.Id; providerView.Name = provider.ProviderName; providerView.Id = registration.MailProviderID;
                                campaignProviderList.Add(providerView);
                            }
                        }
                        if (AccountEmail != null)
                            registrationViewModel.Email = AccountEmail.EmailId;

                    }
                    else if (provider.CommunicationTypeID == CommunicationType.Text)
                    {
                        var registration = textService.GetTextRegistrationDetails(provider.LoginToken);
                        var AccountEmail = serviceProviderRepository.GetServiceProviderEmail(provider.Id);
                        registrationViewModel.RegistrationId = registration.TextRegistrationID;
                        registrationViewModel.UserName = registration.UserName;
                        registrationViewModel.Password = registration.Password;
                        registrationViewModel.ApiKey = registration.APIKey;
                        registrationViewModel.TextProviderID = registration.TextProviderID;
                        registrationViewModel.MailProviderType = provider.MailType;
                        registrationViewModel.RequestGuid = registration.Guid;
                        registrationViewModel.SenderFriendlyName = registration.Name;
                        registrationViewModel.CommunicationType = CommunicationType.Text;
                        registrationViewModel.ServiceProviderID = provider.Id;
                        registrationViewModel.LoginToken = registration.Token;
                        registrationViewModel.IsDefault = provider.IsDefault;
                        registrationViewModel.SenderPhoneNumber = provider.SenderPhoneNumber;
                        registrationViewModel.ProviderName = provider.ProviderName;
                        // registrationViewModel.SenderId = registration.SenderId;
                        if (AccountEmail != null)
                            registrationViewModel.Email = AccountEmail.EmailId;
                    }
                    RegistrationList.Add(registrationViewModel);
                }
            }
            response.RegistrationListViewModel = RegistrationList;
            response.campaignProviderViewModel = campaignProviderList;
            return response;
        }

        private Exception GetContactLogInDetailsNotFoundException()
        {
            Logger.Current.Error("Exception occurred while getting communication tracker information");
            throw new NotImplementedException();
        }
        public InsertServiceProviderResponse AddServiceprovider(InsertServiceProviderRequest request)
        {
            int total = serviceProviderRepository.FindByName(request.ProviderViewModel.ProviderName, request.AccountId);
            if (total > 0)
                throw new UnsupportedOperationException("[|Provider Name already exists.|]");
            CMR.RegisterMailRequest registerMailRequest = Mapper.Map<ProviderRegistrationViewModel, CMR.RegisterMailRequest>(request.ProviderViewModel);
            CMRS.RegistrationResponse response = MailRequestConverterExtensions.EmailRegistrationRequest(registerMailRequest);
            ServiceProvider serviceprovider = InsertServiceProviderDetails(request.ProviderViewModel, request.AccountId, request.RequestedBy.Value, response);
            if (serviceprovider != null)
            {
                serviceProviderRepository.InsertServiceproviderEmail(serviceprovider, request.ProviderViewModel.Email);
                AddImageDomainBindingss(request.Url, request.ProviderViewModel.ImageDomain);
            }
            return new InsertServiceProviderResponse();
        }

        void AddImageDomainBindingss(string domainName, string accountCode)
        {
            string smartTouchSiteName = System.Configuration.ConfigurationManager.AppSettings["SmarttouchSiteName"];

            Logger.Current.Informational("Adding Image sub-domain with domain name : " + domainName + " account : " + accountCode);
            int count = 0;
            var port = 80;
            var domainNameWithOutProtocol = domainName.Replace("http://", "").Replace("https://", "").Replace("www.", "");
            //check if given domain name is sub-domain. if it is already sub domain, we shouldn't add binding.
            foreach (char c in domainNameWithOutProtocol)
                if (c == '.') count++;
            if (count == 1)
            {
                try
                {
                    var serverManager = new ServerManager();
                    var smartTouchSite = serverManager.Sites[smartTouchSiteName];

                    accountCode = accountCode.Replace("http://", "").Replace("https://", "").Replace("www.", "").Split('.').FirstOrDefault();
                    Logger.Current.Informational("Creating sub-domain with name as : " + domainNameWithOutProtocol);
                    smartTouchSite.Bindings.Add(string.Format("*:{0}:{1}.{2}", port, accountCode, domainNameWithOutProtocol), "http");
                    smartTouchSite.ServerAutoStart = false;
                    serverManager.CommitChanges();
                    Logger.Current.Informational("Added " + string.Format("*:{0}:{1}.{2}", port, accountCode, domainNameWithOutProtocol) + " binding successfully.");
                }
                catch (Exception ex)
                {
                    Logger.Current.Error("Error while adding Image binding " + domainNameWithOutProtocol, ex);
                }
            }
        }

        void addToTopic(int contactId, int accountId, DateTime scheduledDate)
        {
            if (contactId != 0 && accountId != 0)
            {
                var message = new TrackMessage()
                {
                    LeadScoreConditionType = (int)LeadScoreConditionType.UnsubscribeEmails,
                    ContactId = contactId,
                    AccountId = accountId,
                    CreatedOn = scheduledDate
                };
                messageService.SendMessages(new Messaging.Messages.SendMessagesRequest()
                {
                    Message = message
                });
            }
        }

        public byte? GetServiceProviders(int accountID)
        {
            ServiceProvider serviceProvider = serviceProviderRepository
                .GetServiceProviders(accountID, CommunicationType.Mail, MailType.TransactionalEmail);
            MailService mailService = new MailService();
            MailRegistrationDb mailRegistration = mailService.GetMailRegistrationDetails(serviceProvider.LoginToken);
            if (serviceProvider.MailType == MailType.TransactionalEmail)
                return (byte)mailRegistration.MailProviderID;
            else
                return null;
        }


        public GetEmailBodyResponse GetEmailBody(GetEmailBodyRequest request)
        {
            MailService mailService = new MailService();
            string EmailBody = string.Empty;
            if (request.ReceivedMailInfoID > 0)
                EmailBody = mailService.GetReceivedEmailBody(request.ReceivedMailInfoID);
            else
                EmailBody = mailService.GetEmailBody(request.SendMailID);
            GetEmailBodyResponse response = new GetEmailBodyResponse();
            response.EmailBody = EmailBody;
            return response;
        }

        public GetServiceProviderImageDomainResponse GetServiceProviderImageDomain(GetServiceProviderImageDomainRequest request)
        {
            var response = new GetServiceProviderImageDomainResponse();
            response.ServiceProvider = serviceProviderRepository.GetAutomationCampaignProvider(request.Guid);

            return response;
        }

        //public string SendTextMessage(SendTextViewModel sendTextViewModel)
        //{
        //    RestClient client = new RestClient();
        //    client.BaseUrl = new Uri("http://www.smsidea.co.in/sendsms.aspx");
        //    RestRequest Request = new RestRequest();
        //    Request.AddParameter("mobile", "7794930839");
        //    Request.AddParameter("pass", "PJVKQ");
        //    Request.AddParameter("senderid", "SMSWEB");
        //    Request.AddParameter("to", "9866050699,8801272203,9032859725");
        //    Request.AddParameter("msg", "Hi Avinash Anna");
        //    var response = client.Execute(Request);

        //    Logger.Current.Informational("Api response" + response.Content);
        //    return response.Content;
        //}
    }
}

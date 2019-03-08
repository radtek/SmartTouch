using LandmarkIT.Enterprise.CommunicationManager.Contracts;
using LandmarkIT.Enterprise.CommunicationManager.Database;
using LandmarkIT.Enterprise.CommunicationManager.Extensions;
using LandmarkIT.Enterprise.CommunicationManager.Processors;
using LandmarkIT.Enterprise.CommunicationManager.Requests;
using LandmarkIT.Enterprise.CommunicationManager.Responses;
using LandmarkIT.Enterprise.Utilities.ExceptionHandling;
using LandmarkIT.Enterprise.Utilities.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SmartTouch.CRM.Domain.ImplicitSync;
using SmartTouch.CRM.Entities;
using LandmarkIT.Enterprise.CommunicationManager.Repositories;
using System.Text.RegularExpressions;

namespace LandmarkIT.Enterprise.CommunicationManager.Operations
{
    public class MailService : BaseService
    {
        private IMailService GetService(Guid token)
        {
            Logger.Current.Verbose("Request received for getting mail-service with token :" + token);
            var service = default(IMailService);
            var provider = MailProvider.Undefined;

            var registration = this.unitOfWork.MailRegistrationsRepository.FirstOrDefault(ft => ft.Guid == token);
            if (registration != null) provider = registration.MailProviderID;

            switch (provider)
            {
                case MailProvider.Smtp:
                    service = new SmtpMailService(unitOfWork, token);
                    Logger.Current.Informational("smtp mail service is configured for the above token");
                    break;
                case MailProvider.SendGrid:
                    service = new SendGridMailService(unitOfWork, token);
                    Logger.Current.Informational("SendGrid mail service is configured for the above token");
                    break;
                case MailProvider.SmartTouch:
                    service = new VmtaMailService(unitOfWork,token);
                    Logger.Current.Informational("VMTA mail service is configured for the above token");
                    break;
                default:
                    break;
            }
            return service;
        }

        /// <summary>
        /// Sends email instantly
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="token"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public SendMailResponse Send(SendMailRequest request)
        {
            var response = default(SendMailResponse);

            try
            {
                Logger.Current.Verbose("Request received for sending an email");
                if (request.ScheduledTime > DateTime.Now.ToUniversalTime())
                {
                    Logger.Current.Verbose("Request received for sending an email with scheduled-time");
                    this.QueueMessages(new List<SendMailRequest> { request });
                    Logger.Current.Verbose("Mail request was queued successfully");
                    response = new SendMailResponse
                    {
                        Token = request.TokenGuid,
                        RequestGuid = request.RequestGuid,
                        StatusID = Responses.CommunicationStatus.Queued,
                        ServiceResponse = string.Empty
                    };
                }
                else
                {
                    Logger.Current.Verbose("Request received for sending an email with out scheduled-time");
                    response = RawSendMail(request);
                    this.LogResponse(request, response);
                }
            }
            catch (Exception ex)
            {
                var rethrowException = ExceptionHandler.Current.HandleException(ex, DefaultExceptionPolicies.LOG_AND_RETHROW_POLICY);
                if (rethrowException) throw;
            }

            return response;
        }

        /// <summary>
        /// Sends email instantly (async call)
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="token"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public Task<SendMailResponse> SendAsync(SendMailRequest request)
        {
            Logger.Current.Verbose("Request received for sending an email async");
            return Task<SendMailResponse>.Run(() => Send(request));
        }

        public List<SendMailResponse> Send(List<SendMailRequest> request)
        {
            Logger.Current.Verbose("Request recieved for sending list of mail requests");
            this.QueueMessages(request);
            var response = new List<SendMailResponse>();
            request.ForEach(item => { response.Add(item.ToSendMailResponse()); });
            return response;
        }

        public Task<List<SendMailResponse>> SendAsync(List<SendMailRequest> requests)
        {
            Logger.Current.Verbose("Request received for sending list of mail requests asynchronously");
            return Task<List<SendMailResponse>>.Run(() => Send(requests));
        }

        public SendMailResponse RawSendMail(SendMailRequest request)
        {
            Logger.Current.Verbose("Request received for sending an email in RawSendMail function");
            return GetService(request.TokenGuid).Send(request);
        }

        public void LogResponse(SendMailRequest request, SendMailResponse response)
        {
            var queueTime = DateTime.UtcNow;
            //SentMail & SentMailDetail Table
            Logger.Current.Verbose("Request received for logging the mail response");
            var sentMail = new SentMailDb
            {
                TokenGuid = response.Token,
                RequestGuid = request.RequestGuid,
                From = request.From,
                PriorityID = request.PriorityID,
                ScheduledTime = request.ScheduledTime,
                QueueTime = queueTime,
                StatusID = response.StatusID,
                ServiceResponse = response.ServiceResponse
            };
            var sentMailDetail = new SentMailDetailDb
            {
                RequestGuid = request.RequestGuid,
                DisplayName = request.DisplayName,
                ReplyTo = request.ReplyTo,
                To = request.To.ToPlainString(),
                //To = request.To,
                CC = request.CC.ToPlainString(),
                BCC = request.BCC.ToPlainString(),
                Subject = request.Subject,
                Body = request.Body,
                IsBodyHtml = request.IsBodyHtml,
                AttachmentGUID = !string.IsNullOrEmpty(request.NotificationAttachementGuid) ? new Guid(request.NotificationAttachementGuid) : request.AttachmentGUID,
                CategoryID = request.CategoryID,
                AccountID = request.AccountID
            };
            unitOfWork.SentMailsRepository.Add(sentMail);
            unitOfWork.SentMailDetailsRepository.Add(sentMailDetail);
            unitOfWork.Commit();
            Logger.Current.Verbose("Logging mail response was successful");
        }



        public Guid InsertImplicitSyncEmailUpload(EmailInfo emailinfo)
        {
            Guid requestguid = Guid.NewGuid();
            Logger.Current.Verbose("Request received for logging the mail response");
            var sentMail = new SentMailDb
            {
                TokenGuid = Guid.NewGuid(),
                RequestGuid = requestguid,
                From = emailinfo.FromEmail,
                PriorityID = 0,
                ScheduledTime = emailinfo.SentDate,
                QueueTime = DateTime.Now.ToUniversalTime(),
                StatusID = Responses.CommunicationStatus.Success,
                ServiceResponse = ""
            };
            var sentMailDetail = new SentMailDetailDb
            {
                RequestGuid = requestguid,
                To = emailinfo.To != null ? string.Join(",", emailinfo.To) : "",
                CC = emailinfo.CC != null ? string.Join(",", emailinfo.CC) : "",
                BCC = emailinfo.BCC != null ? string.Join(",", emailinfo.BCC) : "",
                Subject = emailinfo.Subject,
                Body = emailinfo.Body,
                IsBodyHtml = true
            };
            unitOfWork.SentMailsRepository.Add(sentMail);
            unitOfWork.SentMailDetailsRepository.Add(sentMailDetail);
            unitOfWork.Commit();
            Logger.Current.Verbose("Logging mail response was successful");
            return requestguid;
        }

        public void InsertEmailReceived(EmailInfo emailinfo, string referenceId)
        {
            Logger.Current.Verbose("Request received for logging the mail response");
            if (emailinfo.To != null)
            {
                foreach (var email in emailinfo.To)
                {
                    var receivedMail = new ReceivedMailInfoDb
                    {
                        FromEmail = emailinfo.FromEmail,
                        Recipient = email,
                        RecipientType = EmailRecipientType.To,
                        Subject = emailinfo.Subject,
                        Body = emailinfo.Body,
                        ReferenceID = referenceId,
                        ReceivedOn = emailinfo.SentDate,
                        TrackedOn = emailinfo.SentDate,
                        EmailReferenceID = referenceId,
                    };
                    unitOfWork.ReceivedMailInfoRepository.Add(receivedMail);

                }
            }
            if (emailinfo.CC != null)
            {
                foreach (var email in emailinfo.CC)
                {
                    var receivedMail = new ReceivedMailInfoDb
                    {
                        FromEmail = emailinfo.FromEmail,
                        Recipient = email,
                        RecipientType = EmailRecipientType.CC,
                        Subject = emailinfo.Subject,
                        Body = emailinfo.Body,
                        ReferenceID = referenceId,
                        ReceivedOn = emailinfo.SentDate,
                        TrackedOn = emailinfo.SentDate,
                        EmailReferenceID = referenceId,
                    };
                    unitOfWork.ReceivedMailInfoRepository.Add(receivedMail);
                }
            }

            if (emailinfo.BCC != null)
            {
                foreach (var email in emailinfo.BCC)
                {
                    var receivedMail = new ReceivedMailInfoDb
                    {
                        FromEmail = emailinfo.FromEmail,
                        Recipient = email,
                        RecipientType = EmailRecipientType.BCC,
                        Subject = emailinfo.Subject,
                        Body = emailinfo.Body,
                        ReferenceID = referenceId,
                        ReceivedOn = emailinfo.SentDate,
                        TrackedOn = emailinfo.SentDate,
                        EmailReferenceID = referenceId,
                    };
                    unitOfWork.ReceivedMailInfoRepository.Add(receivedMail);
                }
            }
            unitOfWork.Commit();
            Logger.Current.Verbose("Logging mail response was successful");
        }
        public string GetEmailBody(int SendMailID)
        {
            string emailBody = unitOfWork.SentMailDetailsRepository.AsQueryable()
                                         .Where(i => i.SentMailDetailID == SendMailID)
                                         .Select(x=>x.Body).FirstOrDefault();
            string result = Regex.Replace(emailBody, "<style>(.|\n)*?</style>", string.Empty);
            string finanEmailBody = Regex.Replace(result, "<!--.*?-->", string.Empty,RegexOptions.Singleline | RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Compiled);
            return finanEmailBody;
        }

        public string GetReceivedEmailBody(int ReceivedMailID)
        {
            string emailBody = unitOfWork.ReceivedMailInfoRepository.AsQueryable()
                                         .Where(i => i.ReceivedMailID == ReceivedMailID)
                                         .Select(x => x.Body).FirstOrDefault();
            string result = Regex.Replace(emailBody, "<style>(.|\n)*?</style>", string.Empty);
            string finanEmailBody = Regex.Replace(result, "<!--.*?-->", string.Empty, RegexOptions.Singleline | RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Compiled);
            return finanEmailBody;
        }

        public bool isOutlookEmailAlreadySent(DateTime sentDate)
        {           
            bool isOutlookEmailSent = unitOfWork.SentMailsRepository.AsQueryable().Where(i=>i.ScheduledTime == sentDate).Count() > 0;
            return isOutlookEmailSent;
        }

        public void LogScheduledMessageResponse(SendMailRequest request, SendMailResponse response)
        {
            var queueTime = DateTime.UtcNow;
            Logger.Current.Verbose("Request received for logging scheduled mails response");
            var sentMail = new SentMailDb
            {
                TokenGuid = request.TokenGuid,
                RequestGuid = request.RequestGuid,
                From = request.From,
                PriorityID = request.PriorityID,
                ScheduledTime = request.ScheduledTime,
                QueueTime = queueTime,
                StatusID = response.StatusID,
                ServiceResponse = response.ServiceResponse
            };
            unitOfWork.SentMailsRepository.Add(sentMail);
            unitOfWork.Commit();
            Logger.Current.Verbose("Logging mail response was successful");
        }

        private void QueueMessages(List<SendMailRequest> request)
        {
            var batchLoad = 300;
            var queueTime = DateTime.UtcNow;

            var iterations = (request.Count / batchLoad);
            if ((request.Count % batchLoad) > 0) iterations++;

            Logger.Current.Verbose("Request received for queueing list of mail requests");

            for (int i = 0; i < iterations; i++)
            {
                var currentIterations = request.Skip(i * batchLoad).Take(batchLoad).ToList();

                currentIterations.ForEach(item =>
                {

                    this.unitOfWork.SentMailQueuesRepository.Add(new SentMailQueueDb
                    {
                        TokenGuid = item.TokenGuid,
                        RequestGuid = item.RequestGuid,
                        From = item.From,
                        PriorityID = item.PriorityID,
                        ScheduledTime = item.ScheduledTime,
                        QueueTime = queueTime,
                        StatusID = LandmarkIT.Enterprise.CommunicationManager.Responses.CommunicationStatus.Queued,
                        ServiceResponse = string.Empty,
                        GetProcessedByClassic = item.GetProcessedByClassic
                    });
                    this.unitOfWork.SentMailDetailsRepository.Add(new SentMailDetailDb
                    {
                        RequestGuid = item.RequestGuid,
                        DisplayName = item.DisplayName,
                        ReplyTo = item.ReplyTo,
                        To = item.To.ToPlainString(),
                        //To = item.To,
                        CC = item.CC.ToPlainString(),
                        BCC = item.BCC.ToPlainString(),
                        Subject = item.Subject,
                        Body = item.Body,
                        IsBodyHtml = item.IsBodyHtml,
                        AttachmentGUID = !string.IsNullOrEmpty(item.NotificationAttachementGuid) ? new Guid(item.NotificationAttachementGuid) : item.AttachmentGUID,
                        CategoryID = item.CategoryID,
                        AccountID = item.AccountID
                    });
                });
                unitOfWork.Commit();
            }
            Logger.Current.Verbose("Queueing mail requests was successful");
        }

        public MailRegistrationDb GetMailRegistrationDetails(Guid token)
        {
            Logger.Current.Verbose("Request received for fetching mail registration details with token :" + token);
            MailRegistrationDb registration = this.unitOfWork.MailRegistrationsRepository.FirstOrDefault(ft => ft.Guid == token);
            return registration;
        }

        public MailRegistrationDb GetVMTADetails(IEnumerable<Guid> tokens)
        {
            Logger.Current.Verbose("Request received for fetching VMTA details : " );
            MailRegistrationDb registration = this.unitOfWork.MailRegistrationsRepository.FirstOrDefault(mr => tokens.Contains(mr.Guid) && mr.MailProviderID == MailProvider.SmartTouch);
            return registration;
        }

        public MailRegistrationDb GetMailChimpDetails(IEnumerable<Guid> tokens)
        {
            Logger.Current.Verbose("Request received for fetching VMTA details : ");
            MailRegistrationDb registration = this.unitOfWork.MailRegistrationsRepository.FirstOrDefault(mr => tokens.Contains(mr.Guid) && mr.MailProviderID == MailProvider.MailChimp);
            return registration;
        }

        public void RemoveScheduledReminder(Guid requestGuid)
        {
            Logger.Current.Verbose("Request received for removing scheduled reminder from SentMailQueue");
            if (requestGuid != null)
            {
                SentMailQueueDb sentMailQueue = this.unitOfWork.SentMailQueuesRepository.FirstOrDefault(sm => sm.RequestGuid == requestGuid);
                if (sentMailQueue != null)
                {
                    this.unitOfWork.SentMailQueuesRepository.Delete(sentMailQueue);
                    unitOfWork.Commit();
                }
            }
        }

        public void RemoveAllScheduledReminder(Guid requestGuid)
        {
            Logger.Current.Verbose("Request received for removing scheduled reminder from SentMailQueue");
            if (requestGuid != null)
            {
                IEnumerable<SentMailQueueDb> sentMailQueue = this.unitOfWork.SentMailQueuesRepository.Where(sm => sm.RequestGuid == requestGuid).ToList();
                if (sentMailQueue != null)
                {
                    this.unitOfWork.SentMailQueuesRepository.RemoveRange(sentMailQueue);
                    unitOfWork.Commit();
                }
            }
        }

        public void UpdateScheduledReminder(Guid requestGuid, SendMailRequest updateRequest)
        {
            Logger.Current.Verbose("Request recieved for updating scheduled reminder in SentMailQueue and SentMailDetail");
            if (requestGuid != null && updateRequest != null)
            {
                SentMailDetailDb sentMailDetail = this.unitOfWork.SentMailDetailsRepository.FirstOrDefault(smd => smd.RequestGuid == requestGuid);
                if (sentMailDetail != null)
                {
                    sentMailDetail.To = updateRequest.To.FirstOrDefault();
                    sentMailDetail.Body = updateRequest.Body;
                }
                SentMailQueueDb sentMailQueue = this.unitOfWork.SentMailQueuesRepository.FirstOrDefault(sm => sm.RequestGuid == requestGuid);
                if (sentMailQueue != null)
                {
                    sentMailQueue.From = updateRequest.From;
                    sentMailQueue.ScheduledTime = updateRequest.ScheduledTime;
                }
                else if (sentMailQueue == null && sentMailDetail != null)
                {
                    Logger.Current.Informational("Data not found in Queue but found in mail-details.So inserting new reminder in to queue");
                    var queueTime = DateTime.UtcNow;
                    this.unitOfWork.SentMailQueuesRepository.Add(new SentMailQueueDb
                    {
                        TokenGuid = updateRequest.TokenGuid,
                        RequestGuid = sentMailDetail.RequestGuid,
                        From = updateRequest.From,
                        PriorityID = updateRequest.PriorityID,
                        ScheduledTime = updateRequest.ScheduledTime,
                        QueueTime = queueTime,
                        StatusID = LandmarkIT.Enterprise.CommunicationManager.Responses.CommunicationStatus.Queued,
                        ServiceResponse = string.Empty
                    });
                }
                unitOfWork.Commit();
            }
        }

        public void InsertEmailClickEntry(int sentMailDetailID, int contactID, int? emailLinkID, byte activityType, DateTime activityDate, string ipAddress)
        {
            ((EfUnitOfWork)unitOfWork).EmailStatisticsRepository.Add(new EmailStatisticsDb
            {
                SentMailDetailID = sentMailDetailID,
                ContactID = contactID,
                EmailLinkID = emailLinkID,
                ActivityType = activityType,
                ActivityDate = activityDate,
                IPAddress = ipAddress
            });
            ((EfUnitOfWork)unitOfWork).Commit();
        }

        public EmailLinksDb GetSendMailDetailIdByLinkId(int linkId)
        {
            EmailLinksDb emailLink = ((EfUnitOfWork)unitOfWork).EmailLinksRepository.Where(l => l.EmailLinkID == linkId).Select(s => s).FirstOrDefault();
            return emailLink;
        }
    }
}

using LandmarkIT.Enterprise.CommunicationManager.Database;
using LandmarkIT.Enterprise.CommunicationManager.Extensions;
using LandmarkIT.Enterprise.CommunicationManager.Repositories;
using LandmarkIT.Enterprise.CommunicationManager.Responses;
using LandmarkIT.Enterprise.CommunicationManager.Requests;
using LandmarkIT.Enterprise.Utilities.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using LandmarkIT.Enterprise.Extensions;
using SmartTouch.CRM.Entities;

namespace LandmarkIT.Enterprise.CommunicationManager.Operations
{
    public class JobService : BaseService
    {
        public List<JobDb> GetAllJobs()
        {
            return this.unitOfWork.JobsRepository.GetAll().ToList();
        }

        public List<ScheduledJobDb> GetAllScheduledJobs()
        {
            var scheduledJobs = this.unitOfWork.ScheduledJobsRepository.GetAll();
            return scheduledJobs.ToList();
        }

        public void ProcessMailQueue(int instance = 1)
        {
            //This method will be called 3 times(one with ScheduleJob as 'SendQueueMail' & other as 'ParseMailChimpResponse' & other as 'SendTextQueue') in every 6 min
            //update the job schedule
            Logger.Current.Verbose("Request received for processing mail queue");
            Logger.Current.Informational("Which instance is currently running: " + instance);
            
            var Requests = ((EfUnitOfWork)unitOfWork).ExecuteProcessQueue(30,instance:instance);
            Logger.Current.Informational("No of requests received for sending mails is : " + Requests.Count);

            var mailRequests = ConvertToSendMailRequest(Requests);

            var mailResponses = new ConcurrentBag<SendMailResponse>();

            var mailService = new MailService();

            while (mailRequests != null && mailRequests.Count > 0)
            {
                Logger.Current.Verbose("Processing mail requests paralley");
                
                foreach (var mailRequest in mailRequests)
                {
                    if (mailRequest.CategoryID == (byte)EmailNotificationsCategory.ContactSendEmail || mailRequest.CategoryID == (byte)EmailNotificationsCategory.WorkflowSendEmail)
                    {
                        HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                        doc.LoadHtml(mailRequest.Body);
                        List<string> notlinks = new List<string>() { "#", "not found", "javascript: void(0)", "" };

                        var hrefList = doc.DocumentNode.SelectNodes("//a") != null ? doc.DocumentNode.SelectNodes("//a").Select(p => p.GetAttributeValue("href", "not found")).ToList() : new List<string>();

                        var finalLinks = hrefList.Where(s => !notlinks.Contains(s)).ToList();
                        if (finalLinks.IsAny())
                        {
                            //inserting links to DB
                            GetInsertedLinkIndexes(finalLinks, mailRequest.SentMailDetailID);
                            foreach (var link in finalLinks.Select((value, index) => new { Value = value, Index = index }))
                            {
                                doc.DocumentNode.SelectNodes("//a").Each(attr =>
                                {
                                    string dummyLink = "*|LINK" + link.Index.ToString() + "|*";
                                    if (attr.GetAttributeValue("href", "not found") == link.Value)
                                        attr.SetAttributeValue("href", dummyLink);
                                });
                            }
                        }

                        //For 1*1 Pixel Tracking;
                        string pixelTrackingContent = "<div><img src='*|PTX|*' width='1' height='1' alt='ptx'/></div>";
                        HtmlAgilityPack.HtmlNode newNode = HtmlAgilityPack.HtmlNode.CreateNode(pixelTrackingContent);
                        doc.DocumentNode.PrependChild(newNode);
                        string mailBody = doc.DocumentNode.InnerHtml;
                        mailRequest.Body = mailBody;
                        //get merge field contents
                        mailRequest.MergeValues = ((EfUnitOfWork)unitOfWork).GetMergeFieldValues(mailRequest.SentMailDetailID);
                    }

                    var to = mailRequest.To.IsAny() ? mailRequest.To.FirstOrDefault().Split(',').ToList<string>() : new List<string>();
                    mailRequest.To = to;
                    var cc = mailRequest.CC.IsAny() ? mailRequest.CC.FirstOrDefault().Split(',').ToList<string>() : new List<string>();
                    mailRequest.CC = cc;
                    var bcc = mailRequest.BCC.IsAny() ? mailRequest.BCC.FirstOrDefault().Split(',').ToList<string>() : new List<string>();
                    mailRequest.BCC = bcc;

                    SendMailResponse response = mailService.RawSendMail(mailRequest);
                    mailResponses.Add(response);
                    mailService.LogScheduledMessageResponse(mailRequest, response);
                }

                Logger.Current.Verbose("Mail requests processed parallely. Requests and responses are fed to Mail StoredProcedure");
                Requests = ((EfUnitOfWork)unitOfWork).ExecuteProcessQueue(30, Requests, mailResponses.ToList(),instance);
                mailRequests = ConvertToSendMailRequest(Requests);
            }
        }

        public void GetInsertedLinkIndexes(List<string> links,int sentMailDetailId)
        {
            for(int i = 0;i < links.Count; i++)
            {
                ((EfUnitOfWork)unitOfWork).EmailLinksRepository.Add(new EmailLinksDb
                {
                    SentMailDetailID = sentMailDetailId,
                    LinkURL = links[i],
                    LinkIndex = (byte)i,
                    CreatedOn = DateTime.Now.ToUniversalTime()
                });
            }
            ((EfUnitOfWork)unitOfWork).Commit();
        }

        public List<SendMailRequest> ConvertToSendMailRequest(List<SendSingleMailRequest> requests)
        {
            Logger.Current.Verbose("Converting SendSingleMailRequest to SendMailRequest");
            List<SendMailRequest> sendMailRequests = new List<SendMailRequest>();
            requests.ForEach(item =>
            {
                SendMailRequest mailRequest = new SendMailRequest();
                mailRequest.To = (item.To != null && item.To != string.Empty) ? new List<string>() { item.To } : null;
                mailRequest.CC = (item.CC != null && item.CC != string.Empty) ? new List<string>() { item.CC } : null;
                mailRequest.BCC = (item.BCC != null && item.BCC != string.Empty) ? new List<string>() { item.BCC } : null;
                mailRequest.Body = item.Body;
                mailRequest.DisplayName = item.DisplayName;
                mailRequest.From = item.From;
                mailRequest.IsBodyHtml = item.IsBodyHtml;
                mailRequest.PriorityID = item.PriorityID;
                mailRequest.ReplyTo = item.ReplyTo;
                mailRequest.RequestGuid = item.RequestGuid;
                mailRequest.ScheduledTime = item.ScheduledTime;
                mailRequest.Subject = item.Subject;
                mailRequest.TokenGuid = item.TokenGuid;
                mailRequest.ServiceProviderEmail = item.ServiceProviderEmail;
                mailRequest.AccountDomain = item.AccountDomain;
                mailRequest.AttachmentGUID = item.AttachmentGUID;
                mailRequest.CategoryID = item.CategoryID;
                mailRequest.SentMailDetailID = item.SentMailDetailID;
                sendMailRequests.Add(mailRequest);
            });
            return sendMailRequests;
        }
        public void ProcessTextQueue()
        {
            Logger.Current.Verbose("Request received for processing text queue");
            
            var Requests = ((EfUnitOfWork)unitOfWork).ExecuteTextQueue(30, null, null);
            Logger.Current.Informational("No of requests received for sending text messages is : " + Requests.Count);

            var textRequests = ConvertToSendTextRequest(Requests);

            var textResponses = new ConcurrentBag<SendTextResponse>();

            var textService = new TextService();

            while (textRequests != null && textRequests.Count > 0)
            {
                Logger.Current.Verbose("Processing text requests paralley");
                foreach (var request in textRequests)
                {
                    SendTextResponse response = textService.Send(request);
                    textResponses.Add(response);
                }
                Logger.Current.Verbose("Text requests processed parallely. Requests and responses are fed to Text StoredProcedure");
                Requests = ((EfUnitOfWork)unitOfWork).ExecuteTextQueue(30, Requests, textResponses.ToList());
                textRequests = ConvertToSendTextRequest(Requests);
            }
        }
        public List<SendTextRequest> ConvertToSendTextRequest(List<SendSingleTextRequest> requests)
        {
            Logger.Current.Verbose("Converting SendSingleTextRequest to SendTextRequest");
            List<SendTextRequest> sendTextRequests = new List<SendTextRequest>();
            requests.ForEach(item =>
            {
                SendTextRequest textRequest = new SendTextRequest();
                textRequest.From = item.From;
                textRequest.Message = item.Message;
                textRequest.RequestGuid = item.RequestGuid;
                textRequest.ScheduledTime = item.ScheduledTime;
                textRequest.SenderId = item.SenderId;
                textRequest.TokenGuid = item.TokenGuid;
                textRequest.To = (item.To != null && item.To != string.Empty) ? new List<string>() { item.To } : null;
                sendTextRequests.Add(textRequest);
            });
            return sendTextRequests;
        }

        public List<CronJobDb> GetAllCronJobs()
        {            
            return this.unitOfWork.CronJobsRepository.GetAll().ToList();
        }
        public void StartJob(CronJobType cronJobID, DateTime lastRunTime)
        {
            using(var _unitOfWork = new EfUnitOfWork())
            {
                var cronJob = _unitOfWork.CronJobsRepository.Single(cj => cj.CronJobID == cronJobID);
                cronJob.LastRunOn = lastRunTime;
                cronJob.IsRunning = true;
                cronJob.LastNotifyDateTime = lastRunTime;
                _unitOfWork.CronJobsRepository.Edit(cronJob);

                var cronJobHistory = new CronJobHistoryDb
                {
                    CronJobID = cronJobID,
                    StartTime = DateTime.UtcNow,
                };
                _unitOfWork.CronJobHistoryRepository.Add(cronJobHistory);
                _unitOfWork.Commit();
            }
            
        }
        public void UpdateLastNotifyDateTime(CronJobType cronJobID, DateTime lastNotifyDateTime)
        {
            using(var _unitOfWork = new EfUnitOfWork())
            {
                var cronJob = _unitOfWork.CronJobsRepository.Single(cj => cj.CronJobID == cronJobID);
                cronJob.LastNotifyDateTime = lastNotifyDateTime;
                _unitOfWork.CronJobsRepository.Edit(cronJob);
                _unitOfWork.Commit();
            }
            
        }
        public void StopAllCronJobs()
        {
            var cronJobs = this.unitOfWork.CronJobsRepository.Find(cj => cj.IsRunning == true).ToList();
            foreach (var item in cronJobs)
            {
                item.IsRunning = false;
                item.LastNotifyDateTime = DateTime.UtcNow;
                using(var _unitOfWork = new EfUnitOfWork())
                {
                    _unitOfWork.CronJobsRepository.Edit(item);
                    if (cronJobs.Count > 0) _unitOfWork.Commit();
                }
                
            }
        }

        public void StopCronJob(CronJobType cronJobID, DateTime stopTime)
        {
            using(var _unitOfWork = new EfUnitOfWork())
            {
                Logger.Current.Informational("Request for stopping cronjob");
                var cronJobHistory = _unitOfWork.CronJobHistoryRepository.Find(ch => ch.CronJobID == cronJobID).OrderByDescending(ch => ch.StartTime).FirstOrDefault();
                //var cronJobHistory = _unitOfWork.CronJobHistoryRepository.Find(ch => ch.CronJobID == cronJobID).OrderByDescending(ch => ch.StartTime).Take(1).FirstOrDefault();
                var cronJob = _unitOfWork.CronJobsRepository.Single(cj => cj.CronJobID == cronJobID);
                cronJob.IsRunning = false;
                cronJob.LastNotifyDateTime = stopTime;

                if (cronJobHistory != null)
                {
                    cronJobHistory.EndTime = DateTime.UtcNow;
                }
                else
                {
                    Logger.Current.Error("Start entry doesn't exists for jobid " + cronJobID);
                }

                _unitOfWork.CronJobsRepository.Edit(cronJob);
                _unitOfWork.CronJobHistoryRepository.Edit(cronJobHistory);
                _unitOfWork.Commit();
                Logger.Current.Informational("Cronjob stopped successfully");
            }
        }
    }
}

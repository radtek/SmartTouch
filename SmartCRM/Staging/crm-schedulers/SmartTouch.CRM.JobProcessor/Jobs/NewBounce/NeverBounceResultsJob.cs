﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using LandmarkIT.Enterprise.CommunicationManager.Requests;
using Quartz;
using RestSharp;
using RestSharp.Extensions;
using SmartTouch.CRM.ApplicationServices.Messaging.Accounts;
using SmartTouch.CRM.ApplicationServices.Messaging.ImportData;
using SmartTouch.CRM.ApplicationServices.ServiceAgents;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.Domain.Accounts;
using SmartTouch.CRM.Domain.Communication;
using SmartTouch.CRM.Domain.ImportData;
using SmartTouch.CRM.Domain.Users;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.JobProcessor.QuartzScheduler.Scheduler;
using SmartTouch.CRM.JobProcessor.Utilities;

namespace SmartTouch.CRM.JobProcessor.Jobs.NewBounce
{
    public class NeverBounceResultsJob : BaseJob
    {
        private readonly IAccountService _accountService;
        private readonly IServiceProviderRepository _serviceProviderRepository;
        private readonly IUrlService _urlService;
        private readonly IUserRepository _userRepository;
        private readonly ApiManager _apiManager;
        private readonly JobServiceConfiguration _jobConfig;

        public NeverBounceResultsJob(
            IAccountService accountService,
            IServiceProviderRepository serviceProviderRepository,
            IUrlService urlService,
            IUserRepository userRepository,
            ApiManager apiManager,
            JobServiceConfiguration jobConfig)
        {
            _accountService = accountService;
            _serviceProviderRepository = serviceProviderRepository;
            _urlService = urlService;
            _userRepository = userRepository;
            _apiManager = apiManager;
            _jobConfig = jobConfig;
        }

        protected override void ExecuteInternal(IJobExecutionContext context)
        {
            try
            {
                Log.Informational("Request received for updating staus of neverbounce requests by continuse polling");
                var requests = _accountService.GetAcceptedRequests(new GetNeverBounceAcceptedRequests() { Status = NeverBounceStatus.PollingCompleted });
                if (requests != null && requests.Requests.Any())
                {
                    string token = _apiManager.GetOAuthToken();
                    string serviceUrl = _jobConfig.NeverBounceApiUrl;
                    foreach (var request in requests.Requests)
                    {
                        if (!string.IsNullOrEmpty(token) && request.NeverBounceJobID.HasValue)
                        {
                            try
                            {
                                Log.Informational("Request received for fetching results for NeverBounceRequestID : " + request.NeverBounceRequestID + " NeverBounceJobId : " + request.NeverBounceJobID);
                                string downloadPath = _jobConfig.NeverBounceResults;
                                downloadPath = Path.Combine(downloadPath, request.NeverBounceRequestID + ".csv");
                                FetchResults(token, request.NeverBounceJobID.Value, downloadPath, serviceUrl);
                                IEnumerable<NeverBounceResult> results = GenerateObject(downloadPath, request.NeverBounceRequestID);
                                if (results != null && results.Any())
                                    _accountService.InsertEmailStatuses(new UpdateEmailStatusRequest() { Results = results });
                                request.ServiceStatus = NeverBounceStatus.Processed;
                                _accountService.UpdateNeverBounceRequest(new UpdateNeverBounceRequest() { Request = request });
                                SendUpdateEmail(request.AccountID, request.NeverBounceRequestID,request.CreatedBy);
                                SendNotification(request.NeverBounceRequestID, request.CreatedBy);
                            }
                            catch (Exception ex)
                            {
                                Log.Error("An error occured while updating neverbounce polling response", ex);
                                request.ServiceStatus = NeverBounceStatus.Failed;
                                request.Remarks = ex.Message;
                                _accountService.UpdateNeverBounceRequest(new UpdateNeverBounceRequest() { Request = request });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("An error occured while processing NeverBounce results processor", ex);
            }
        }

        private void FetchResults(string accessToken, int neverBounceJobId, string downloadPath, string url)
        {
            Log.Informational("Request received to fetch results from NeverBounce for NeverBounceJoobId : " + neverBounceJobId);

            string baseURL = url + "download";
            RestClient client = new RestClient(baseURL);

            var request = new RestRequest(Method.POST);
            //request.AddHeader("content-type", "application/x-www-form-urlencoded");
            //request.AddParameter("access_token", accessToken, ParameterType.RequestBody);
            //request.AddParameter("job_id", neverBounceJobId, ParameterType.RequestBody);

            string body = "access_token=" + accessToken + "&job_id=" + neverBounceJobId;
            request.AddParameter("application/x-www-form-urlencoded", body, ParameterType.RequestBody);

            client.DownloadData(request).SaveAs(downloadPath);
            Log.Informational("downloaded successfully");
        }

        private IEnumerable<NeverBounceResult> GenerateObject(string path, int neverBounceRequestID)
        {
            Log.Informational("Generating object by reading data from CSV file for NeverBounceRequestID : " + neverBounceRequestID);
            IEnumerable<NeverBounceResult> results = new List<NeverBounceResult>();
            if (!string.IsNullOrEmpty(path))
            {
                results = File.ReadAllLines(path)
                   .Skip(1)
                   .Select(x => x.Split(','))
                   .Select(x => new NeverBounceResult()
                   {
                       ContactID = int.Parse(x[0]),
                       ContactEmailID = int.Parse(x[1]),
                       IsValid = x[3] == "valid" ? true : false,
                       NeverBounceRequestID = neverBounceRequestID
                   });
                Log.Informational("Gerenated object successfully");
            }
            return results;
        }

        private void SendUpdateEmail(int accountId,  int neverBounceRequestId,int userId)
        {
            if (neverBounceRequestId != 0 & accountId != 0)
            {
                Guid loginToken = new Guid();
                string accountPrimaryEmail = string.Empty;
                Email senderEmail = new Email();
                Account account = _accountService.GetAccountMinDetails(accountId);
                string toEmail = _jobConfig.NeverBounceUpdateEmail;
                if (account != null && !string.IsNullOrEmpty(toEmail))
                {
                    if (account.Email != null)
                        accountPrimaryEmail = account.Email.EmailId;

                    IEnumerable<ServiceProvider> serviceProviders = _serviceProviderRepository.GetAccountCommunicationProviders(accountId, CommunicationType.Mail, MailType.TransactionalEmail);
                    if (serviceProviders != null && serviceProviders.FirstOrDefault() != null)
                    {
                        loginToken = serviceProviders.FirstOrDefault().LoginToken;
                        senderEmail = _serviceProviderRepository.GetServiceProviderEmail(serviceProviders.FirstOrDefault().Id);
                    }

                    var tomails = toEmail.Split(',');
                    foreach (string ToMail in tomails)
                    {
                        if (loginToken != new Guid() && accountPrimaryEmail != null)
                        {
                            string fromEmail = (senderEmail != null && !string.IsNullOrEmpty(senderEmail.EmailId)) ? senderEmail.EmailId : accountPrimaryEmail;

                            EmailAgent agent = new EmailAgent();
                            SendMailRequest mailRequest = new SendMailRequest();
                            mailRequest.Body = GetBody(neverBounceRequestId, accountId, userId);
                            mailRequest.From = fromEmail;
                            mailRequest.IsBodyHtml = true;
                            mailRequest.ScheduledTime = DateTime.Now.ToUniversalTime().AddSeconds(5);
                            mailRequest.Subject = string.Format("{0} - Email List Finished Validating",account.AccountName);
                            mailRequest.To = new List<string>() { ToMail };
                            mailRequest.TokenGuid = loginToken;
                            mailRequest.RequestGuid = Guid.NewGuid();
                            mailRequest.AccountDomain = account.DomainURL;
                            mailRequest.CategoryID = (byte)EmailNotificationsCategory.MailTesterEmail;
                            mailRequest.AccountID = accountId;
                            agent.SendEmail(mailRequest);
                        }
                    }
                }
            }
        }

        private string GetBody(int neverBounceRequestID, int accountId,int userId)
        { 
            string body = string.Empty;
            string accountLogo = string.Empty;
            string accountName = string.Empty;
            string accountImage = string.Empty;

            if (neverBounceRequestID != 0)
            {
                var primaryEmail = _userRepository.GetUserPrimaryEmail(userId);
                string userName = _userRepository.GetUserName(userId);
                NeverBounceEmailDataResponse response = _accountService.GetEmailData(new NeverBounceEmailDataRequest() { NeverBounceRequestID = neverBounceRequestID });
                string accountAddress = _accountService.GetPrimaryAddress(new GetAddressRequest() { AccountId = accountId }).Address;
                string accountPhoneNumber = _accountService.GetPrimaryPhone(new GetPrimaryPhoneRequest() { AccountId = accountId }).PrimaryPhone;
                GetAccountImageStorageNameResponse imgStorage = _accountService.GetStorageName(new GetAccountImageStorageNameRequest()
                {
                    AccountId = accountId
                });

                if (imgStorage.AccountLogoInfo != null)
                {
                    if (!String.IsNullOrEmpty(imgStorage.AccountLogoInfo.StorageName))
                        accountLogo = _urlService.GetUrl(accountId, ImageCategory.AccountLogo, imgStorage.AccountLogoInfo.StorageName);
                    else
                        accountLogo = "";
                    accountName = imgStorage.AccountLogoInfo.AccountName;
                }
                if (!string.IsNullOrEmpty(accountLogo))
                    accountImage = accountImage + "<td align='right' valign='center' style='margin:0px;padding:0px 0px 25px 0px;'><img src='" + accountLogo + "' alt='" + accountName + "' style='width:100px;' width='100'></td>";
                if (response.Data != null)
                {
                    string entityName = response.Data.EntityID == 1 ? "File Name" : response.Data.EntityID == 2 ? "Tag Name(s)" : response.Data.EntityID == 3 ? "Search Definition Name(s)" : "";
                    string filename = EmailTemplate.NeverBounceBadEmailsTemplate.ToString() + ".txt";
                    string savedFileName = Path.Combine(_jobConfig.EmailTemplatesPhysicalPath.ToString(), filename);
                    using (StreamReader reader = new StreamReader(savedFileName))
                    {
                        do
                        {
                            body = reader.ReadToEnd().Replace("[AccountName]", accountName).Replace("[AccountImage]", accountImage).Replace("[TotalImported]", response.Data.ImportTotal.ToString())
                                                     .Replace("[BadEmailsCount]", response.Data.BadEmails.ToString()).Replace("[GoodEmailsCount]", response.Data.GoodEmails.ToString())
                                                     .Replace("[ADDRESS]", accountAddress).Replace("[PHONE]", accountPhoneNumber).Replace("[UserName]", userName).Replace("[UserEmail]", primaryEmail)
                                                     .Replace("[FileData]", response.Data.EntityNames).Replace("[FileName]", entityName);
                        } while (!reader.EndOfStream);
                    }

                }
            }
            return body;
        }

        private void SendNotification(int nerverbouceRequestId,int userId)
        {
            string fileName = _accountService.GetNeverBouceValidationDoneFileName(nerverbouceRequestId);
            Notification notification = new Notification();
            notification.EntityId = nerverbouceRequestId;
            notification.Subject = string.Format("{0} - Email List Finished Validating.", fileName);
            notification.Details = string.Format("{0} - Email List Finished Validating.", fileName);
            notification.Time = DateTime.Now.ToUniversalTime();
            notification.Status = NotificationStatus.New;
            notification.UserID = userId;
            notification.ModuleID = (byte)AppModules.ImportData;
            notification.DownloadFile = null;
            _userRepository.AddNotification(notification);
        }
    }
}

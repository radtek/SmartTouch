using Quartz;
using SmartTouch.CRM.JobProcessor.QuartzScheduler.Scheduler;
using System;
using LandmarkIT.Enterprise.CommunicationManager.Operations;
using LandmarkIT.Enterprise.CommunicationManager.Requests;
using LandmarkIT.Enterprise.Extensions;
using SmartTouch.CRM.ApplicationServices.Messaging.Accounts;
using SmartTouch.CRM.ApplicationServices.Messaging.DropdownValues;
using SmartTouch.CRM.ApplicationServices.Messaging.User;
using SmartTouch.CRM.ApplicationServices.Messaging.WebAnalytics;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.Domain.Accounts;
using SmartTouch.CRM.Domain.Reports;
using SmartTouch.CRM.Domain.Users;
using SmartTouch.CRM.Entities;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SmartTouch.CRM.JobProcessor.Extensions;

namespace SmartTouch.CRM.JobProcessor.Jobs.WebAnalytics
{
    public class WebVisitEmailNotifierJob : BaseJob
    {
        private readonly string _visitDailySummaryTemplate = EmailTemplate.WebVisitDailySummaryEmail + ".txt";
        private const string DefaultEmail = "defaultuser@smarttouch.com";

        private readonly IAccountService _accountService;
        private readonly IUrlService _urlService;
        private readonly IWebAnalyticsProviderService _webAnalyticsService;
        private readonly IUserService _userService;
        private readonly IDropdownValuesService _dropdownService;
        private readonly IAccountRepository _accountRepository;
        private readonly MailService _mailService;
        private readonly JobServiceConfiguration _jobConfig;

        public WebVisitEmailNotifierJob(
            IAccountService accountService,
            IUrlService urlService,
            IWebAnalyticsProviderService webAnalyticsService,
            IUserService userService,
            IDropdownValuesService dropdownService,
            IAccountRepository accountRepository,
            MailService mailService,
            JobServiceConfiguration jobConfig)
        {
            _accountService = accountService;
            _urlService = urlService;
            _webAnalyticsService = webAnalyticsService;
            _userService = userService;
            _dropdownService = dropdownService;
            _accountRepository = accountRepository;
            _mailService = mailService;
            _jobConfig = jobConfig;
        }

        protected override void ExecuteInternal(IJobExecutionContext context)
        {
            var webVisitsToBeNotified = _webAnalyticsService.GetCurrentWebVistNotifications(new GetCurrentWebVisitNotificationsRequest());
            var accountIds = webVisitsToBeNotified
                .CurrentVisits
                .Select(c => c.AccountId)
                .Distinct()
                .ToArray();

            var emailResults = new List<KeyValuePair<IEnumerable<string>, string>>();
            foreach (var accountId in accountIds)
            {
                var accountInfo = _accountRepository.GetAccountBasicDetails(accountId);
                if (accountInfo.Status == (byte)AccountStatus.Suspend)
                    continue;

                Log.Informational($"Process Real-Time Alerts for AccountID: {accountId}");

                var userIds = webVisitsToBeNotified.CurrentVisits
                    .Where(c => c.AccountId == accountId)
                    .Select(c => c.OwnerID)
                    .Distinct()
                    .ToArray();

                if (!userIds.IsAny())
                    continue;

                var emailProvider = _accountService.GetTransactionalProviderDetails(accountId);
                var lifeCycleStages = _dropdownService.GetDropdownValue(new GetDropdownValueRequest { AccountId = accountId, DropdownID = (byte)DropdownFieldTypes.LifeCycle });

                var usersToNotify = _userService.GetUsersOptedInstantWebVisitEmail(new GetUsersOptedInstantWebVisitEmailRequest { AccountId = accountId })
                    .Users
                    .ToArray();
                var ownersToNotify = _userService.GetUsersByUserIDs(new GetUsersByUserIDsRequest { UserIDs = userIds })
                    .Users
                    .ToList();

                //add empty owner
                var emptyOwner = new UserBasicInfo
                {
                    UserID = 0,
                    Email = DefaultEmail
                };
                if (ownersToNotify.All(f => f.Email != DefaultEmail))
                    ownersToNotify.Add(emptyOwner);

                //is account subscribed
                var provider = _accountService.GetAccountWebAnalyticsProviders(
                    new GetWebAnalyticsProvidersRequest
                    {
                        AccountId = accountId
                    }).WebAnalyticsProviders.First();

                foreach (var owner in ownersToNotify)
                {
                    List<WebVisitReport> relatedVisits;
                    if (owner.Email == DefaultEmail)
                    {
                        relatedVisits = webVisitsToBeNotified.CurrentVisits
                            .Where(c => c.AccountId == accountId && c.OwnerID == null)
                            .OrderByDescending(c => c.VisitedOn)
                            .ToList();
                        owner.FirstName = "Not";
                        owner.LastName = "Assigned";
                        owner.TimeZone = accountInfo.TimeZone;
                    }
                    else
                        relatedVisits = webVisitsToBeNotified.CurrentVisits
                            .Where(c => c.AccountId == accountId && c.OwnerID == owner.UserID)
                            .OrderByDescending(c => c.VisitedOn)
                            .ToList();

                    if (!relatedVisits.IsAny())
                        continue;

                    try
                    {
                        if (provider.NotificationStatus)
                        {
                            //when owner is default user or unassigned, email will not be sent to owner.
                            NotifyByEmail(accountInfo, relatedVisits, owner, "SmartTouch Current Web Visit Alert",
                                lifeCycleStages, emailProvider, owner.Email);
                        }
                        foreach (var user in usersToNotify)
                        {
                            try
                            {
                                NotifyByEmail(accountInfo, relatedVisits, owner,
                                    "SmartTouch Current Web Visit Alert (Admin)", lifeCycleStages,
                                    emailProvider, user.Email);
                            }
                            catch (Exception ex)
                            {
                                Log.Error("Unable to send web visit email to user: " + owner.UserID + ", email: " + user.Email, ex);
                            }
                        }
                        emailResults.Add(
                            new KeyValuePair<IEnumerable<string>, string>(
                                relatedVisits.Select(c => c.VisitReference).ToList(), "Success"));
                    }
                    catch (Exception ex)
                    {
                        Log.Error("Unable to send web visit email to user: " + owner.UserID, ex);
                        emailResults.Add(
                            new KeyValuePair<IEnumerable<string>, string>(
                                relatedVisits.Select(c => c.VisitReference).ToList(), "Failed"));
                    }
                }
            }

            if (emailResults.Any())
                _webAnalyticsService.UpdateWebVisitNotifications(new UpdateWebVisitNotificationsRequest { VisitReferences = emailResults });

            Log.Informational("Completed processing Real-Time Alerts");
        }

        private string GenerateEmailBody(string emailContent, int accountId)
        {
            var accountAddress = _accountService.GetPrimaryAddress(new GetAddressRequest() { AccountId = accountId }).Address;
            var accountPhoneNumber = _accountService.GetPrimaryPhone(new GetPrimaryPhoneRequest() { AccountId = accountId }).PrimaryPhone;
            var storageNameResponse = _accountService.GetStorageName(new GetAccountImageStorageNameRequest()
            {
                AccountId = accountId
            });

            var accountLogo = !string.IsNullOrEmpty(storageNameResponse.AccountLogoInfo.StorageName) ? _urlService.GetUrl(accountId, ImageCategory.AccountLogo, storageNameResponse.AccountLogoInfo.StorageName) : "";
            var accountName = storageNameResponse.AccountLogoInfo.AccountName;
            var accountImage = !string.IsNullOrEmpty(accountLogo) ?
                "<td align='right' valign='center' style='margin:0px;padding:0px" +
                " 0px 25px 0px;'><img src='" + accountLogo + "' alt='" + accountName +
                "' style ='width:100px;' width ='100'></td>" : "";

            return File.ReadAllText(Path.Combine(_jobConfig.EmailTemplatesPhysicalPath, _visitDailySummaryTemplate))
                .Replace("[AccountName]", accountName)
                .Replace("[AccountImage]", accountImage)
                .Replace("[WebVisitData]", emailContent)
                .Replace("[ADDRESS]", accountAddress)
                .Replace("[PHONE]", accountPhoneNumber);
        }

        private void NotifyByEmail(Account accountInfo, List<WebVisitReport> relatedVisits, UserBasicInfo owner, string subject,
            GetDropdownValueResponse lifeCycleStages, Dictionary<Guid, string> emailProvider, string to)
        {
            Log.Verbose("Distinct UserId: " + owner.UserID + " distinct User name: " + owner.FirstName + " " + owner.LastName);

            var emailContent = GetEmailContent(owner, relatedVisits, lifeCycleStages);
            var accountName = relatedVisits.First().AccountName;

            var visitReference = relatedVisits.Select(c => c.VisitReference).FirstOrDefault();

            if (string.IsNullOrEmpty(to) || to == DefaultEmail)
                return;

            var subjectLine = subject + " - " + accountName;
            emailContent = "<div style='font-family:arial,sans-serif;font-size:12px'><label>Account: <strong style='font-family:arial,sans-serif'>  " + accountName
                           + "<br></strong> Account Executive: <strong>" + owner.FullName + "</strong><br><br></label>" + emailContent
                           + (relatedVisits.Any() ? "<div style='font-family:arial,sans-serif;font-size:12px'><label>Above timestamps are according to " + owner.TimeZone + "<br></div>" : "")
                           + "</div>";

            if (!(emailProvider?.Any() ?? false))
                return;

            var fromEmailId = !string.IsNullOrEmpty(emailProvider.FirstOrDefault().Value) ? emailProvider.FirstOrDefault().Value : "support@smarttouchinteractive.com";
            var emailRequest = new SendMailRequest
            {
                TokenGuid = emailProvider.FirstOrDefault().Key,
                RequestGuid = Guid.NewGuid(),
                ScheduledTime = DateTime.UtcNow.NextMinute(),
                Subject = subjectLine,
                Body = GenerateEmailBody(emailContent, accountInfo.AccountID),
                To = new List<string> { to },
                IsBodyHtml = true,
                From = fromEmailId,
                CategoryID = (short)EmailNotificationsCategory.WebVisitInstantNotification,
                AccountID = accountInfo.AccountID
            };
            _mailService.Send(emailRequest);
            Log.Informational("Successfully sent web visit notification: " + visitReference + ". To user: " + to);
        }

        private string GetEmailContent(UserBasicInfo ownerInfo, List<WebVisitReport> relatedVisits, GetDropdownValueResponse lifeCycleStages)
        {
            var emailContent = string.Empty;

            if (!relatedVisits.IsAny())
                return emailContent;

            relatedVisits.ForEach(c =>
            {
                c.VisitedOn = c.VisitedOn.ToTimezone(ownerInfo.TimeZone);
                c.LifecycleStage = lifeCycleStages
                    .DropdownValues
                    .DropdownValuesList
                    .Where(d => d.DropdownValueID == c.LifecycleStageId)
                    .Select(d => d.DropdownValue)
                    .FirstOrDefault() ?? "";
            });

            emailContent += relatedVisits.GetTable(
                p => p.FirstName,
                p => p.LastName,
                p => p.Email,
                p => p.Phone,
                p => p.Zip,
                p => p.LifecycleStage,
                p => p.VisitedOn,
                p => p.PageViews,
                p => p.Duration,
                p => p.Page1,
                p => p.Page2,
                p => p.Page3,
                p => p.Source,
                p => p.LeadScore,
                p => p.Location);

            return emailContent;
        }
    }
}

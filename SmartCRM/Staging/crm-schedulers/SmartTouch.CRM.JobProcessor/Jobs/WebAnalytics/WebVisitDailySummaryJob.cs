using Quartz;
using SmartTouch.CRM.JobProcessor.QuartzScheduler.Scheduler;
using System;
using LandmarkIT.Enterprise.CommunicationManager.Operations;
using SmartTouch.CRM.ApplicationServices.Messaging.WebAnalytics;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.Domain.Reports;
using System.Collections.Generic;
using System.Linq;
using LandmarkIT.Enterprise.Extensions;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.ApplicationServices.Messaging.DropdownValues;
using System.IO;
using SmartTouch.CRM.ApplicationServices.Messaging.Accounts;
using SmartTouch.CRM.Domain.Accounts;
using SmartTouch.CRM.JobProcessor.Extensions;

namespace SmartTouch.CRM.JobProcessor.Jobs.WebAnalytics
{
    public class WebVisitDailySummaryJob : BaseJob
    {
        private readonly string _visitDailySummaryTemplate = EmailTemplate.WebVisitDailySummaryEmail + ".txt";
        private const string SupportEmail = "support@smarttouchinteractive.com";

        private readonly IAccountService _accountService;
        private readonly IUrlService _urlService;
        private readonly IWebAnalyticsProviderService _webAnalyticsService;
        private readonly IUserService _userService;
        private readonly IDropdownValuesService _dropdownService;
        private readonly IAccountRepository _accountRepository;
        private readonly MailService _mailService;
        private readonly JobServiceConfiguration _jobConfig;

        public WebVisitDailySummaryJob(
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
            var webAnalyticsProviders = _accountService
                .GetWebAnalyticsProviders(new GetWebAnalyticsProvidersRequest())
                .WebAnalyticsProviders;

            var accountIds = webAnalyticsProviders.Select(a => a.AccountID);
            var currentTimeUtc = DateTime.UtcNow;

            foreach (var accountId in accountIds)
            {
                var accountInfo = _accountRepository.GetAccountBasicDetails(accountId);
                if (accountInfo.Status == (byte)AccountStatus.Suspend)
                    continue;

                Log.Informational($"Process Daily Summary Alerts for AccountID: {accountId}");

                var webAnalyticProvider = webAnalyticsProviders.First(p => p.AccountID == accountId);
                var lifeCycleStages = _dropdownService.GetDropdownValue(new GetDropdownValueRequest { AccountId = accountInfo.AccountID, DropdownID = (byte)DropdownFieldTypes.LifeCycle });
                var providerDetails = _accountService.GetTransactionalProviderDetails(accountInfo.AccountID);

                try
                {
                    var startDate = currentTimeUtc.Yesterday().ToTimezone(accountInfo.TimeZone);
                    var endDate = (_jobConfig.IncludeCurrentDayInDailySummary ? DateTime.UtcNow : DateTime.Today).ToTimezone(accountInfo.TimeZone);

                    Log.Informational($"Get WebVisits from {startDate} to {endDate}");

                    var webVisitsSummary = _webAnalyticsService.GetWebVisitDailySummary(new GetWebVisitDailySummaryRequest { AccountId = accountInfo.AccountID, StartDate = startDate, EndDate = endDate });
                    var usersToNotify = _userService.GetUsersOptedWebVisitSummaryEmail(new GetUsersOptedWebVisitSummaryEmailRequest { AccountId = accountInfo.AccountID });

                    var emailCopyContent = string.Empty;
                    #region Send Mail To Owner
                    foreach (var userInfo in usersToNotify.AllUsers)
                    {
                        Log.Informational("Sending daily summary email to user: " + userInfo.UserID + ". To owner: " + userInfo.Email);
                        try
                        {
                            var relatedVisits = webVisitsSummary
                                .WebVisits
                                .Where(c => c.OwnerID == userInfo.UserID)
                                .OrderBy(c => c.VisitedOn)
                                .ToList();

                            var emailContent = GetEmailContent(accountInfo, relatedVisits, lifeCycleStages, userInfo.FirstName + " " + userInfo.LastName);
                            if (string.IsNullOrEmpty(emailContent))
                                continue;

                            emailCopyContent = emailCopyContent + emailContent + "<br><hr>";
                            if (webAnalyticProvider.DailyStatusEmailOpted)
                                NotifyByEmail(accountInfo, "SmartTouch Web Visit Daily Summary", providerDetails, userInfo.Email, emailContent);
                        }
                        catch (Exception ex)
                        {
                            Log.Error("Error occured while sending web visit daily summary emails for account user: " + userInfo.UserID + " " + userInfo.Email, ex);
                        }
                    }
                    #endregion

                    #region Send Mail To Opted User
                    //add no owner visits to html
                    var unassignedVisits = webVisitsSummary
                        .WebVisits
                        .Where(v => v.OwnerID == 1 || v.OwnerID == null)
                        .OrderBy(c => c.VisitedOn)
                        .ToList();

                    emailCopyContent += GetEmailContent(accountInfo, unassignedVisits, lifeCycleStages, "Not Assigned");
                    if (string.IsNullOrEmpty(emailCopyContent))
                        continue;

                    foreach (var userInfo in usersToNotify.UsersOpted)
                    {
                        try
                        {
                            Log.Informational("Sending daily summary email to userID: " + userInfo.UserID + ". To owner: " + userInfo.Email);
                            NotifyByEmail(accountInfo, "SmartTouch Web Visit Daily Summary", providerDetails, userInfo.Email, emailCopyContent + "<br>");
                        }
                        catch (Exception ex)
                        {
                            Log.Error("Error occured while sending Opted User Daily Web Visit Daily Summary email for user: " + userInfo.UserID + " " + userInfo.Email, ex);
                        }
                    }
                    #endregion
                }
                catch (Exception ex)
                {
                    Log.Error("Error occured while sending web visit daily summary emails for account: " + accountInfo, ex);
                }
            }
            Log.Informational("Completed processing Daily Summary Alerts");
        }

        private string EmailBodyUserLevel(string accountLabel, string emailContent, int accountId)
        {
            var accountAddress = _accountService.GetPrimaryAddress(new GetAddressRequest { AccountId = accountId }).Address;
            var accountPhoneNumber = _accountService.GetPrimaryPhone(new GetPrimaryPhoneRequest { AccountId = accountId }).PrimaryPhone;
            var storage = _accountService.GetStorageName(new GetAccountImageStorageNameRequest { AccountId = accountId });

            var accountName = storage.AccountLogoInfo.AccountName;
            var accountLogo = !string.IsNullOrEmpty(storage.AccountLogoInfo.StorageName) ? _urlService.GetUrl(accountId, ImageCategory.AccountLogo, storage.AccountLogoInfo.StorageName) : "";

            var accountImage = !string.IsNullOrEmpty(accountLogo) ?
                "<td align='right' valign='center' style='margin:0px;padding:0px" +
                " 0px 25px 0px;'><img src='" + accountLogo + "' alt='" + accountName +
                "' style ='width:100px;' width ='100'></td>" : "";

            var emailTemplatePath = Path.Combine(_jobConfig.EmailTemplatesPhysicalPath, _visitDailySummaryTemplate);

            return File.ReadAllText(emailTemplatePath)
                .Replace("[AccountName]", accountName)
                .Replace("[AccountImage]", accountImage)
                .Replace("[WebVisitData]", accountLabel + emailContent)
                .Replace("[ADDRESS]", accountAddress)
                .Replace("[PHONE]", accountPhoneNumber);
        }

        private void NotifyByEmail(Account accountInfo, string subject,
            Dictionary<Guid, string> emailProvider, string to, string emailContent)
        {
            var accountLabel = "<label style='font-family:verdana'>Account: <strong> "
                               + accountInfo.AccountName + " </strong> <br></label>";
            if (string.IsNullOrEmpty(to))
                return;

            var subjectLine = $"{subject} - {accountInfo.AccountName}";
            var fromEmailId = !string.IsNullOrEmpty(emailProvider.FirstOrDefault().Value) ? emailProvider.FirstOrDefault().Value : SupportEmail;

            if (!emailProvider.Any())
                return;

            var request = new LandmarkIT.Enterprise.CommunicationManager.Requests.SendMailRequest
            {
                TokenGuid = emailProvider.FirstOrDefault().Key,
                RequestGuid = Guid.NewGuid(),
                ScheduledTime = DateTime.UtcNow.NextMinute(),
                Subject = subjectLine,
                Body = EmailBodyUserLevel(accountLabel, emailContent, accountInfo.AccountID),
                To = new List<string> { to },
                IsBodyHtml = true,
                From = fromEmailId,
                CategoryID = (byte) EmailNotificationsCategory.WebVisitDailySummaryNotification,
                AccountID = accountInfo.AccountID
            };
            _mailService.Send(request);

            Log.Informational("Successfully sent web visit daily summary to user " + to);
        }

        private string GetEmailContent(Account accountInfo, List<WebVisitReport> relatedVisits, GetDropdownValueResponse lifeCycleStages, string ownerName)
        {
            var emailContent = string.Empty;

            if (!relatedVisits.IsAny())
                return emailContent;

            emailContent = "<div style='font-family:verdana;font-size:12px'> " +
                           "Account Executive: <strong>" + ownerName + "</strong>" +
                           "<br>" + emailContent + "</div><br>";

            relatedVisits.ForEach(x =>
            {
                x.VisitedOn = x.VisitedOn.ToTimezone(accountInfo.TimeZone);
                x.LifecycleStage = lifeCycleStages
                    .DropdownValues
                    .DropdownValuesList
                    .Where(d => d.DropdownValueID == x.LifecycleStageId)
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
using LandmarkIT.Enterprise.CommunicationManager.Database;
using LandmarkIT.Enterprise.CommunicationManager.Operations;
using LandmarkIT.Enterprise.Utilities.Logging;
using Quartz;
using SmartTouch.CRM.ApplicationServices.Messaging.Communication;
using SmartTouch.CRM.ApplicationServices.Messaging.WebAnalytics;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.Domain.Reports;
using SmartTouch.CRM.Domain.Users;
using SmartTouch.CRM.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LandmarkIT.Enterprise.Extensions;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.ApplicationServices.Messaging.DropdownValues;
using System.IO;
using SmartTouch.CRM.ApplicationServices.Messaging.Accounts;
using SmartTouch.CRM.JobProcessor;
using SmartTouch.CRM.Domain.Accounts;
using SmartTouch.CRM.Domain.WebAnalytics;

namespace SmartTouch.CRM.WebAnalyticsEngine
{
    [System.Runtime.InteropServices.Guid("1003571F-BB2C-48AA-A684-0ADE2E52DFEA")]
    public class WebVisitDailySummaryProcessor : CronJobProcessor
    {

        private static IAccountService accountService;
        private static IUrlService urlService;
        private static IContactService contactService;
        private static IWebAnalyticsProviderService webAnalyticsService;
        private static ICommunicationService communicationService;
        private static IUserService userService;
        private static IDropdownValuesService dropdownService;
        private static MailService mailService;
        private static IAccountRepository accountRepository;

        private static bool dailySummaryInprogress = default(bool);

        public WebVisitDailySummaryProcessor(CronJobDb cronJob, JobService jobService, string cacheName)
            : base(cronJob, jobService, cacheName)
        {
            accountService = IoC.Container.GetInstance<IAccountService>();
            urlService = IoC.Container.GetInstance<IUrlService>();
            contactService = IoC.Container.GetInstance<IContactService>();
            webAnalyticsService = IoC.Container.GetInstance<IWebAnalyticsProviderService>();
            communicationService = IoC.Container.GetInstance<ICommunicationService>();
            userService = IoC.Container.GetInstance<IUserService>();
            dropdownService = IoC.Container.GetInstance<IDropdownValuesService>();
            accountRepository = IoC.Container.GetInstance<IAccountRepository>();
            mailService = new MailService();
        }

        protected override void Execute()
        {
            Trigger(null);
        }
        public static void Trigger(object stateInfo)
        {
            try
            {
                if (dailySummaryInprogress)
                    return;
                dailySummaryInprogress = true;
                Logger.Current.Verbose("WebVisit DailySummary Processor triggered");

                string includeToday = System.Configuration.ConfigurationManager.AppSettings["INCLUDE_CURRENT_DAY_IN_DAILY_SUMMARY"].ToLower();

                var webAnalyticProviders = accountRepository.GetWebAnalyticsProviders();

                var accounts = webAnalyticProviders.Select(a => a.AccountID);
                
                var startDate = DateTime.Now.ToUniversalTime().Date.AddDays(-1);
                var endDate = includeToday == "true" ? DateTime.Now.ToUniversalTime() : DateTime.Now.ToUniversalTime().Date;
                

                foreach (var accountId in accounts)
                {
                    Logger.Current.Informational("Current Account:", accountId.ToString());

                    var account = accountRepository.GetAccountBasicDetails(accountId);

                    var webAnalyticProvider = webAnalyticProviders.Where(p => p.AccountID == accountId).FirstOrDefault()?? new WebAnalyticsProvider();

                    var accountInfo = new AccountBasicInfo()
                    {
                        AccountID = accountId,
                        AccountName = account.AccountName,
                        TimeZone = account.TimeZone,
                        WebAnalyticsID = webAnalyticProvider.Id
                    };
                    

                    GetDropdownValueResponse lifeCycleStages = dropdownService.GetDropdownValue(new GetDropdownValueRequest() { AccountId = accountInfo.AccountID, DropdownID = (byte)DropdownFieldTypes.LifeCycle });

                    var emailProviderResponse = communicationService.GetEmailProviders(new GetEmailProvidersRequest() { AccountId = accountInfo.AccountID });
                    if (emailProviderResponse.Exception != null)
                        throw emailProviderResponse.Exception;

                    var communicationResponse = communicationService.GetDefaultCampaignEmailProvider(new GetDefaultCampaignEmailProviderRequest() { AccountId = accountInfo.AccountID });
                    var serviceProviderGuids = emailProviderResponse.ServiceProviderGuids;
                    MailRegistrationDb mailRegistration = mailService.GetVMTADetails(serviceProviderGuids);
                    var providerDetails = accountService.GetTransactionalProviderDetails(accountInfo.AccountID);
                    string emailContentToAdmins = string.Empty;
                    try
                    {
                        TimeZoneInfo accountTzInfo = TimeZoneInfo.FindSystemTimeZoneById(accountInfo.TimeZone);
                        startDate = TimeZoneInfo.ConvertTimeFromUtc(startDate, accountTzInfo);
                        endDate = includeToday == "true" ? endDate : TimeZoneInfo.ConvertTimeFromUtc(endDate, accountTzInfo);

                        Logger.Current.Informational("StartDate After: " + startDate);
                        Logger.Current.Informational("StartDate After: " + endDate);

                        GetWebVisitDailySummaryResponse response = webAnalyticsService.GetWebVisitDailySummary(new GetWebVisitDailySummaryRequest() { AccountId = accountInfo.AccountID, StartDate = startDate, EndDate = endDate });

                        GetUsersOptedWebVisitSummaryEmailResponse users = userService.GetUsersOptedWebVisitSummaryEmail(new GetUsersOptedWebVisitSummaryEmailRequest() { AccountId = accountInfo.AccountID });

                        byte status = accountRepository.GetAccountStatus(accountInfo.AccountID);

                        if(status !=(byte)AccountStatus.Suspend)
                        {
                            string emailCopyContent = string.Empty;
                            #region Send Mail To Owner
                            foreach (UserBasicInfo userInfo in users.AllUsers)
                            {
                                Logger.Current.Informational("Sending daily summary email to user: " + userInfo.UserID + ". To owner: " + userInfo.Email);
                                try
                                {
                                    List<WebVisitReport> relatedVisits = response.WebVisits.Where(c => c.OwnerID == userInfo.UserID).OrderBy(c => c.VisitedOn).ToList(); 

                                    string emailContent = GetEmailContent(accountInfo, relatedVisits, lifeCycleStages, userInfo.FirstName + " " + userInfo.LastName);
                                    if(emailContent.Length > 0)
                                        emailCopyContent = emailCopyContent + emailContent + "<br><hr>";

                                    if (webAnalyticProvider.DailyStatusEmailOpted && emailContent.Length>0)
                                        NotifyByEmail(accountInfo, "SmartTouch Web Visit Daily Summary", providerDetails, userInfo.Email, emailContent);
                                }
                                catch (Exception ex)
                                {
                                    Logger.Current.Error("Error occured while sending web visit daily summary emails for account user: " + userInfo.UserID + " " + userInfo.Email, ex);
                                }
                            }
                            #endregion

                            #region Send Mail To Opted User
                            //add no owner visits to html
                            List<WebVisitReport> unassignedVisits = response.WebVisits.Where(v => ((v.OwnerID.HasValue) ? v.OwnerID.Value == 1 : v.OwnerID == null)).OrderBy(c => c.VisitedOn).ToList();
                            emailCopyContent += GetEmailContent(accountInfo, unassignedVisits, lifeCycleStages, "Not Assigned");
                            foreach (UserBasicInfo userInfo in users.UsersOpted)
                            {
                                Logger.Current.Informational("ADMIN - Sending daily summary email to user: " + userInfo.UserID + ". To owner: " + userInfo.Email);
                                try
                                {
                                    if (emailCopyContent.Length > 0)
                                    {
                                        NotifyByEmail(accountInfo, "SmartTouch Web Visit Daily Summary", providerDetails, userInfo.Email, emailCopyContent + "<br>");
                                    }
                                        
                                }
                                catch(Exception ex)
                                {
                                    Logger.Current.Error("Error occured while sending Opted User Daily Web Visit Daily Summary email for user: " + userInfo.UserID + " " + userInfo.Email, ex);
                                }
                            } 
                            #endregion
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Current.Error("Error occured while sending web visit daily summary emails for account: " + accountInfo, ex);
                    }
                }

                dailySummaryInprogress = false;
                Logger.Current.Informational("Completed processing daily summary emails");

            }
            catch (Exception ex)
            {
                dailySummaryInprogress = false;
                Logger.Current.Error("Error occured while sending web visit daily summary emails", ex);
            }

        }

        private static string EmailBodyUserLevel(string accountLabel, string emailContent, int accountId)
        {
            string body = "";
            string accountLogo = string.Empty;
            string accountAddress = accountService.GetPrimaryAddress(new GetAddressRequest() { AccountId = accountId }).Address;
            string accountPhoneNumber = accountService.GetPrimaryPhone(new GetPrimaryPhoneRequest() { AccountId = accountId }).PrimaryPhone;

            ApplicationServices.Messaging.Accounts.GetAccountImageStorageNameResponse response = accountService.GetStorageName(new ApplicationServices.Messaging.Accounts.GetAccountImageStorageNameRequest()
            {
                AccountId = accountId
            });

            if (!String.IsNullOrEmpty(response.AccountLogoInfo.StorageName))
                accountLogo = urlService.GetUrl(accountId, ImageCategory.AccountLogo, response.AccountLogoInfo.StorageName);
            else
                accountLogo = "";
            string accountName = response.AccountLogoInfo.AccountName;
            string accountImage = string.Empty;
            if (!string.IsNullOrEmpty(accountLogo))
            {
                accountImage = accountImage + "<td align='right' valign='center' style='margin:0px;padding:0px 0px 25px 0px;'><img src='" + accountLogo + "' alt='" + accountName + "' style ='width:100px;' width ='100'></td>";
            }
            else
            {
                accountImage = "";
            }
            string filename = EmailTemplate.WebVisitDailySummaryEmail.ToString() + ".txt";
            string savedFileName = Path.Combine(System.Configuration.ConfigurationManager.AppSettings["EMAILTEMPLATES_PHYSICAL_PATH"].ToString(), filename);


            using (StreamReader reader = new StreamReader(savedFileName))
            {
                do
                {

                    body = reader.ReadToEnd().Replace("[AccountName]", accountName).Replace("[AccountImage]", accountImage).Replace("[WebVisitData]", accountLabel + emailContent).Replace("[ADDRESS]", accountAddress).Replace("[PHONE]", accountPhoneNumber);

                } while (!reader.EndOfStream);
            }

            return body;
        }
        private static string FormatPhoneNumber(string phone)
        {
            var formattedPhone = phone;
            double convertedPhone = 0;
            double.TryParse(phone, out convertedPhone);
            if (convertedPhone > 0)
                formattedPhone = String.Format("{0:### ###-#### - ###}", convertedPhone);
            return formattedPhone;
        }

        private static void NotifyByEmail(AccountBasicInfo accountInfo, string subject,
            Dictionary<Guid, string> emailProvider, string to,string emailContent)
        {
            var accountLabel = "<label style='font-family:verdana'>Account: <strong>  " + accountInfo.AccountName + " </strong> <br></label>";
            if (!string.IsNullOrEmpty(to))
            {
                var subjectLine = subject + " - " + accountInfo.AccountName;
                
                LandmarkIT.Enterprise.CommunicationManager.Requests.SendMailRequest request = new LandmarkIT.Enterprise.CommunicationManager.Requests.SendMailRequest();
                if (emailProvider != null && emailProvider.Count > 0)
                {
                    var fromEmailId = !string.IsNullOrEmpty(emailProvider.FirstOrDefault().Value) ? emailProvider.FirstOrDefault().Value : "support@smarttouchinteractive.com";
                    request.TokenGuid = emailProvider.FirstOrDefault().Key;
                    request.RequestGuid = Guid.NewGuid();
                    request.ScheduledTime = DateTime.Now.AddMinutes(1).ToUniversalTime();
                    request.Subject = subjectLine;
                    request.Body = EmailBodyUserLevel(accountLabel, emailContent, accountInfo.AccountID);
                    request.To = new List<string>() { to };
                    request.IsBodyHtml = true;
                    request.From = fromEmailId;
                    request.CategoryID = (byte)EmailNotificationsCategory.WebVisitDailySummaryNotification;
                    request.AccountID = accountInfo.AccountID;
                    var sendMailResponse = mailService.Send(request);
                }

                Logger.Current.Informational("Successfully sent web visit daily summary to user " + to );
            }
        }

        private static string GetEmailContent(AccountBasicInfo accountInfo, List<WebVisitReport> relatedVisits, GetDropdownValueResponse lifeCycleStages, string ownerName)
        {
            string emailContent = string.Empty;
            
            if (relatedVisits.IsAny())
            {
                emailContent = "<div style='font-family:verdana;font-size:12px'> Account Executive: <strong>" + ownerName + "</strong><br>" + emailContent + "</div><br>";

                relatedVisits.ForEach(c =>
                {
                    TimeZoneInfo tzInfo = TimeZoneInfo.FindSystemTimeZoneById(accountInfo.TimeZone);
                    c.VisitedOn = TimeZoneInfo.ConvertTimeFromUtc(c.VisitedOn, tzInfo);

                    c.LifecycleStage = lifeCycleStages.DropdownValues.DropdownValuesList.Where(d => d.DropdownValueID == c.LifecycleStageId).Select(d => d.DropdownValue).FirstOrDefault() ?? "";
                    c.Phone = FormatPhoneNumber(c.Phone);
                });
                emailContent += relatedVisits.GetTable(p => p.FirstName
                    , p => p.LastName
                    , p => p.Email
                    , p => p.Phone
                    , p => p.Zip
                    , p => p.LifecycleStage
                    , p => p.VisitedOn
                    , p => p.PageViews
                    , p => p.Duration
                    , p => p.Page1
                    , p => p.Page2
                    , p => p.Page3
                    , p => p.Source
                    , p => p.LeadScore
                    , p => p.Location);
                
            }
            return emailContent;
        }
    }
}

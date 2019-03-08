using AutoMapper;
using LandmarkIT.Enterprise.CommunicationManager.Database;
using LandmarkIT.Enterprise.CommunicationManager.Operations;
using LandmarkIT.Enterprise.CommunicationManager.Requests;
using LandmarkIT.Enterprise.Extensions;
using LandmarkIT.Enterprise.Utilities.Logging;
using SmartTouch.CRM.ApplicationServices.Messaging.Accounts;
using SmartTouch.CRM.ApplicationServices.Messaging.DropdownValues;
using SmartTouch.CRM.ApplicationServices.Messaging.User;
using SmartTouch.CRM.ApplicationServices.Messaging.WebAnalytics;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.Domain.Accounts;
using SmartTouch.CRM.Domain.Reports;
using SmartTouch.CRM.Domain.Users;
using SmartTouch.CRM.Domain.WebAnalytics;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.JobProcessor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SmartTouch.CRM.WebAnalyticsEngine
{
    public class WebVisitEmailNotifier : CronJobProcessor
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
        private static string defaultEmail;
        private static bool emailNotifierIsProcessing = default(bool);

        public WebVisitEmailNotifier(CronJobDb cronJob, JobService jobService, string cacheName)
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
            defaultEmail = "defaultuser@smarttouch.com";
        }

        protected override void Execute()
        {
            Trigger(null);
        }
        public static void Trigger(Object stateInfo)
        {
            try
            {
                if (emailNotifierIsProcessing) return;
                emailNotifierIsProcessing = true;
                Logger.Current.Verbose("WebVisitEmailNotifier triggered");


                Logger.Current.Verbose("Getting visits");

                GetCurrentWebVisitNotificationsResponse webVisitsToBeNotified = webAnalyticsService.GetCurrentWebVistNotifications(new GetCurrentWebVisitNotificationsRequest());
                Logger.Current.Informational("WebVisitsToBeNotified Count" + webVisitsToBeNotified.CurrentVisits.Count());

                var accounts = webVisitsToBeNotified.CurrentVisits.Select(c => c.AccountId).ToList().Distinct();

                var emailResults = new List<KeyValuePair<IEnumerable<string>, string>>();
                var failedEmails = new List<KeyValuePair<IEnumerable<string>, string>>();

                foreach (int accountId in accounts)
                {
                    var userIds = webVisitsToBeNotified.CurrentVisits
                        .Where(c => c.AccountId == accountId)
                        .Select(c => c.OwnerID).Distinct().ToList();
                    if (userIds.IsAny())
                    {
                        Dictionary<Guid, string> emailProvider = accountService.GetTransactionalProviderDetails(accountId);
                        GetDropdownValueResponse lifeCycleStages = dropdownService.GetDropdownValue(new GetDropdownValueRequest() { AccountId = accountId, DropdownID = (byte)DropdownFieldTypes.LifeCycle });

                        IEnumerable<UserBasicInfo> optedUsers
                            = userService.GetUsersOptedInstantWebVisitEmail(new GetUsersOptedInstantWebVisitEmailRequest() { AccountId = accountId }).Users;


                        List<UserBasicInfo> owners = userService.GetUsersByUserIDs(new GetUsersByUserIDsRequest() { UserIDs = userIds }).Users.ToList();
                        //add empty owner
                        var eo = new UserBasicInfo()
                        {
                            UserID = 0,
                            Email = defaultEmail
                        };
                        if (!owners.Any(f => f.Email == defaultEmail))
                            owners.Add(eo);


                        
                        var account = accountRepository.GetAccountBasicDetails(accountId);
                        if (account.Status != (byte)AccountStatus.Suspend)
                        {
                            #region == Send email to each user ==
                            //is account subscribed
                            var providerResponse = accountService.GetAccountWebAnalyticsProviders(new GetWebAnalyticsProvidersRequest()
                            {
                                AccountId = accountId
                            });
                            
                            WebAnalyticsProvider provider = providerResponse.WebAnalyticsProviders.FirstOrDefault();

                            foreach (var owner in owners)
                            {
                                IEnumerable<WebVisitReport> relatedVisits = null;
                                if (owner.Email == defaultEmail)
                                {
                                    relatedVisits = webVisitsToBeNotified.CurrentVisits.Where(c => c.AccountId == accountId && c.OwnerID == null).OrderByDescending(c => c.VisitedOn).ToList();
                                    owner.FirstName = "Not";
                                    owner.LastName = "Assigned";
                                    owner.TimeZone = account.TimeZone;
                                }
                                else
                                    relatedVisits = webVisitsToBeNotified.CurrentVisits.Where(c => c.AccountId == accountId && c.OwnerID == owner.UserID).OrderByDescending(c => c.VisitedOn).ToList();

                                if (relatedVisits.IsAny())
                                {
                                    try
                                    {
                                        if(provider.NotificationStatus)
                                        {
                                            //when owner is default user or un assigned, email will not be sent to owner.
                                            NotifyByEmail(accountId, relatedVisits, owner, "SmartTouch Current Web Visit Alert", lifeCycleStages, emailProvider, owner.Email);
                                        }
                                        foreach (var optedUser in optedUsers)
                                        {
                                            try
                                            {
                                                NotifyByEmail(accountId, relatedVisits, owner, "SmartTouch Current Web Visit Alert (Admin)", lifeCycleStages, emailProvider, optedUser.Email);
                                            }
                                            catch(Exception ex)
                                            {
                                                ex.Data.Clear();
                                                ex.Data.Add("Copy User", optedUser.UserID);
                                                Logger.Current.Error("Unable to send web visit email to user: " + owner.UserID + ", email: "+ optedUser.Email, ex);
                                            }
                                        }
                                        emailResults.Add(new KeyValuePair<IEnumerable<string>, string>(relatedVisits.Select(c => c.VisitReference).ToList(), "Success"));
                                    }
                                    catch (Exception ex)
                                    {
                                        ex.Data.Clear();
                                        ex.Data.Add("Copy User", owner.UserID);
                                        Logger.Current.Error("Unable to send web visit email to user: " + owner.UserID, ex);
                                        failedEmails.Add(new KeyValuePair<IEnumerable<string>, string>(relatedVisits.Select(c => c.VisitReference).ToList(), "Failed"));
                                    }
                                }

                            }
                            #endregion

                        }
                        emailResults.AddRange(failedEmails);
                    }
                }

                #region == Update Audits ==
                if (emailResults != null && emailResults.Any())
                    webAnalyticsService.UpdateWebVisitNotifications(new UpdateWebVisitNotificationsRequest() { VisitReferences = emailResults });
                #endregion

                emailNotifierIsProcessing = false;
            }

            catch (Exception ex)
            {
                emailNotifierIsProcessing = false;
                Logger.Current.Error("Exception occured in Email Notifier.", ex);
            }

            finally
            {
                //emailNotifierIsProcessing = false;
            }
        }

        private static string GenerateEmailBody(string emailContent, int accountId)
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
                accountImage = accountImage + "<td align='right' valign='center' style='margin:0px;padding:0px 0px 25px 0px;'><img src='" + accountLogo + "' alt='" + accountName + "' style='width:100px;' width='100'></td>";
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

                    body = reader.ReadToEnd().Replace("[AccountName]", accountName).Replace("[AccountImage]", accountImage).Replace("[WebVisitData]", emailContent).Replace("[ADDRESS]", accountAddress).Replace("[PHONE]", accountPhoneNumber);

                } while (!reader.EndOfStream);
            }

            return body;
        }

        private static void NotifyByEmail(int accountId, IEnumerable<WebVisitReport> relatedVisits, UserBasicInfo owner, string subject,
            GetDropdownValueResponse lifeCycleStages, Dictionary<Guid, string> emailProvider, string to)
        {
            Logger.Current.Verbose("Distinct UserId: " + owner.UserID);
            Logger.Current.Verbose("Distinct User name: " + owner.FirstName + " " + owner.LastName);
            relatedVisits.Each(c =>
            {
                TimeZoneInfo tzInfo = TimeZoneInfo.FindSystemTimeZoneById(owner.TimeZone);
                c.VisitedOnUTZ = TimeZoneInfo.ConvertTimeFromUtc(c.VisitedOn, tzInfo);
                c.LifecycleStage = lifeCycleStages.DropdownValues.DropdownValuesList.Where(d => d.DropdownValueID == c.LifecycleStageId).Select(d => d.DropdownValue).FirstOrDefault() ?? "";
            });
            Logger.Current.Verbose("Sending notification email to userId:" + owner.UserID);
            var emailContent = relatedVisits.GetTable(p => p.FirstName
                        , p => p.LastName
                        , p => p.Email
                        , p => p.Phone
                        , p => p.Zip
                        , p => p.LifecycleStage
                        , p => p.VisitedOnUTZ
                        , p => p.PageViews
                        , p => p.Duration
                        , p => p.Page1
                        , p => p.Page2
                        , p => p.Page3
                        , p => p.Source
                        , p => p.LeadScore
                        , p => p.Location);
            var ownerName = owner.FirstName + " " + owner.LastName;
            var accountName = relatedVisits.First().AccountName;
            //var accountLabel = "<label style='font-family:arial,sans-serif'>Account: <strong>  " + accountName + " </strong> <br></label>";

            var visitReference = relatedVisits.Select(c => c.VisitReference).FirstOrDefault();
            
            if (!string.IsNullOrEmpty(to) && to != defaultEmail)
            {
                Logger.Current.Verbose("Sending 1 mail to " + to);
                var subjectLine = subject + " - " + accountName;
                emailContent = "<div style='font-family:arial,sans-serif;font-size:12px'><label>Account: <strong style='font-family:arial,sans-serif'>  " + accountName
               + "<br></strong> Account Executive: <strong>" + ownerName + "</strong><br><br></label>" + emailContent
               + ((relatedVisits != null && relatedVisits.Any()) ? "<div style='font-family:arial,sans-serif;font-size:12px'><label>Above timestamps are according to " + owner.TimeZone + "<br></div>" : "")
                + "</div>";

                SendMailRequest emailRequest = new SendMailRequest();
                if (emailProvider != null && emailProvider.Count > 0)
                {
                    var fromEmailId = !string.IsNullOrEmpty(emailProvider.FirstOrDefault().Value) ? emailProvider.FirstOrDefault().Value : "support@smarttouchinteractive.com";
                    emailRequest.TokenGuid = emailProvider.FirstOrDefault().Key;
                    emailRequest.RequestGuid = Guid.NewGuid();
                    emailRequest.ScheduledTime = DateTime.Now.AddMinutes(1).ToUniversalTime();
                    emailRequest.Subject = subjectLine;
                    emailRequest.Body = GenerateEmailBody(emailContent, accountId);
                    emailRequest.To = new List<string>() { to };
                    emailRequest.IsBodyHtml = true;
                    emailRequest.From = fromEmailId;
                    emailRequest.CategoryID = (short)EmailNotificationsCategory.WebVisitInstantNotification;
                    emailRequest.AccountID = accountId;
                    var sendMailResponse = mailService.Send(emailRequest);
                }
                Logger.Current.Informational("Successfully sent web visit notification: " + visitReference + ". To user: " + to);
            }
        }
    }
}

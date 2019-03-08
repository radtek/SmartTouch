using System;
using System.Diagnostics;
using System.Linq;
using Quartz;
using SmartTouch.CRM.JobProcessor.QuartzScheduler.Scheduler;
using SmartTouch.CRM.ApplicationServices.Messaging.Accounts;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.Messaging.WebAnalytics;
using SmartTouch.CRM.Domain.Accounts;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.JobProcessor.Extensions;
using SmartTouch.CRM.JobProcessor.Services;
using SmartTouch.CRM.JobProcessor.Utilities;

namespace SmartTouch.CRM.JobProcessor.Jobs.WebAnalytics
{
    public class WebAnalyticsKickfireJob : BaseJob
    {
        private readonly IContactService _contactService;
        private readonly IAccountService _accountService;
        private readonly IWebAnalyticsProviderService _webAnalyticsService;
        private readonly IKickfireService _kickfireService;
        private readonly IAccountRepository _accountRepository;
        private readonly JobServiceConfiguration _jobConfig;

        public WebAnalyticsKickfireJob(
            IContactService contactService,
            IAccountService accountService,
            IWebAnalyticsProviderService webAnalyticsService,
            IKickfireService kickfireService,
            IAccountRepository accountRepository,
            JobServiceConfiguration jobConfig)
        {
            _contactService = contactService;
            _accountService = accountService;
            _webAnalyticsService = webAnalyticsService;
            _kickfireService = kickfireService;
            _accountRepository = accountRepository;
            _jobConfig = jobConfig;
        }

        protected override void ExecuteInternal(IJobExecutionContext context)
        {
            var webAnalyticsProviders = _accountService
                .GetWebAnalyticsProviders(new GetWebAnalyticsProvidersRequest())
                .WebAnalyticsProviders;

            var utcNow = DateTime.UtcNow;
            var yesterday = utcNow.Yesterday();

            Log.Informational("Query partners info");
            var partnerInfos = _kickfireService.GetPartnerInfos().ToArray();

            foreach (var webAnalyticsProvider in webAnalyticsProviders)
            {
                try
                {
                    if (webAnalyticsProvider.AccountID == 340)
                        Debugger.Break();

                    var accountInfo = _accountRepository.GetAccountBasicDetails(webAnalyticsProvider.AccountID);
                    //Do not query suspended accounts
                    if (accountInfo.Status == (byte)AccountStatus.Suspend)
                        continue;

                    Log.Informational("Analyzing web visits for web analytics provider id: " + webAnalyticsProvider.Id + ". AccountID: " + webAnalyticsProvider.AccountID);
                    var lastUpdatedAt = webAnalyticsProvider.LastAPICallTimeStamp ?? yesterday;

                    var accountTimeZone = int.Parse(partnerInfos
                        .Where(x => x.ApiKey == webAnalyticsProvider.APIKey)
                        .Select(x => x.TimeZoneId)
                        .First());

                    var kickfireTimeZone = VisiStatTimeZones
                        .TimeZones()
                        .First(x => x.TimeZoneId == accountTimeZone)
                        .GlobalTimeZoneId;

                    Log.Informational($"Account {webAnalyticsProvider.AccountID} CurrentTime: {utcNow.ToTimezone(kickfireTimeZone)}");

                    var startDate = lastUpdatedAt
                        .SetKind(DateTimeKind.Utc)
                        .ToTimezone(kickfireTimeZone);
                    var endDate = utcNow
                        .ToTimezone(kickfireTimeZone);

                    Log.Informational($"Query unique visitors for AccountID: {webAnalyticsProvider.AccountID}");
                    var identities = _kickfireService.GetUniqueVisitors(webAnalyticsProvider.APIKey, startDate, endDate)
                        .Select(x => x.Identity)
                        .Take(2000) //Max value that dapper can handle
                        .ToArray();

                    var knownIdentities = _contactService
                        .CompareKnownIdentities(new CompareKnownContactIdentitiesRequest
                        {
                            ReceivedIdentities = identities,
                            AccountId = webAnalyticsProvider.AccountID,
                        }).KnownIdentities
                        .ToArray();

                    Log.Informational($"Query page visits for AccountID: {webAnalyticsProvider.AccountID}");
                    var webVisits = _kickfireService
                        .GetPageVisitors(webAnalyticsProvider.APIKey, knownIdentities, startDate, endDate)
                        .Select(visit => new WebVisit
                        {
                            VisitedOn = visit.VisitedOn
                                .ConvertToTimeZone(kickfireTimeZone, accountInfo.TimeZone)
                                .SetKind(DateTimeKind.Utc),
                            PageVisited = visit.PageName,
                            City = visit.City,
                            State = visit.Region,
                            Country = visit.Country,
                            ISPName = visit.IspName,
                            IPAddress = visit.IpAddress,
                            ContactReference = visit.ContactReference,
                            Duration = visit.DurationInSeconds
                        }).ToList();

                    _webAnalyticsService.AddContactWebVisits(new AddContactWebVisitRequest
                    {
                        ContactWebVisits = webVisits,
                        LastAPICallTimeStamp = utcNow,
                        WebAnalyticsProvider = webAnalyticsProvider,
                        SplitVisitInterval = _jobConfig.SplitVisitInterval
                    });
                }
                catch (Exception ex)
                {
                    Log.Error("Exception occurred while making an API call. AccountID: " + webAnalyticsProvider.AccountID, ex);
                }
                Log.Informational("Analyzing web visits for web analytics provider id: " + webAnalyticsProvider.Id + " is completed");
            }
        }
    }
}
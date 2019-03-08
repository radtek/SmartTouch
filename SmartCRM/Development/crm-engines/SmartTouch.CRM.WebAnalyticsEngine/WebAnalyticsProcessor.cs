#region Assembly Newtonsoft.Json.dll, v6.0.0.0
// D:\SmartTouch\Src\Development\packages\Newtonsoft.Json.6.0.1\lib\net45\Newtonsoft.Json.dll
#endregion

using SmartTouch.CRM.ApplicationServices.Messaging.Accounts;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Script.Serialization;
using SmartTouch.CRM.ApplicationServices.Messaging.WebAnalytics;
using LandmarkIT.Enterprise.Utilities.Logging;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.JobProcessor;
using LandmarkIT.Enterprise.CommunicationManager.Database;
using LandmarkIT.Enterprise.CommunicationManager.Operations;
using System.Configuration;

namespace SmartTouch.CRM.WebAnalyticsEngine
{
    public class WebAnalyticsProcessor : CronJobProcessor
    {
        private static bool webTrackingInProgress = default(bool);
        readonly IContactService contactService;
        readonly IAccountService accountService;
        readonly IWebAnalyticsProviderService webAnalyticsService;


        public WebAnalyticsProcessor(CronJobDb cronJob, JobService jobService, string cacheName)
            : base(cronJob, jobService, cacheName)
        {
            contactService = IoC.Container.GetInstance<IContactService>();
            accountService = IoC.Container.GetInstance<IAccountService>();
            webAnalyticsService = IoC.Container.GetInstance<IWebAnalyticsProviderService>();
        }

        protected override void Execute()
        {
            Trigger(null);
        }
        public void Trigger(object stateInfo)
        {
            try
            {
                if (webTrackingInProgress) return;
                webTrackingInProgress = true;

                var accountsWithWebAnalytics = accountService.GetWebAnalyticsProviders(new GetWebAnalyticsProvidersRequest()).WebAnalyticsProviders;

                DateTime currentTime = DateTime.Now.ToUniversalTime();

                string currentTimeString = string.Format("{0:u}", currentTime).Replace(":", "-");

                dynamic UniqueVisitors;
                APIManager apiManager = new APIManager();

                string apiLookupStringPath = ConfigurationManager.AppSettings["WEBANALYTICS_API_RESPONSE_PATH"] + "/APILookUp"
                        + "/APILookUp_UTC_" + currentTimeString;

                string smartTouchPartnerKey = ConfigurationManager.AppSettings["SMARTTOUCH_PARTNER_KEY"];

                JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();

                ValidatePhysicalPaths();
                apiManager.APICall("http://api.visistat.com/api-lookup.php?hid=" + smartTouchPartnerKey + "&act=2", apiLookupStringPath);
                var initialSetupProcessingStartHour = ConfigurationManager.AppSettings["INITIAL_SETUP_PROCESSING_START_HOUR"];
                var initialSetupProcessingStopHour = ConfigurationManager.AppSettings["INITIAL_SETUP_PROCESSING_STOP_HOUR"];
                var splitVisitInterval = ConfigurationManager.AppSettings["SPLIT_VISIT_INTERVAL"];
                IList<dynamic> accountTimeZones = new List<dynamic>();

                using (StreamReader streamReader = new StreamReader(apiLookupStringPath + ".txt"))
                {
                    while (streamReader.Peek() >= 0)
                    {
                        var readLine = streamReader.ReadLine();
                        if (string.IsNullOrEmpty(readLine))
                            break;
                        var accountInfo = readLine.Split('|').ToArray();
                        accountTimeZones.Add(new { APIKey = accountInfo[11], TimeZoneID = accountInfo[12], TrackingDomain = accountInfo[2] });
                    }
                }

                #region -- Processing --
                foreach (var webAnalyticsProvider in accountsWithWebAnalytics)
                {
                    var accountCurrentTime = currentTime;
                    Logger.Current.Verbose("accountCurrentTime : " + accountCurrentTime.ToString());
                    IList<WebVisit> WebVisits = new List<WebVisit>();

                    var isInitialSetup = webAnalyticsProvider.LastAPICallTimeStamp == null;
                    if (!isInitialSetup || (isInitialSetup && accountCurrentTime.Hour > int.Parse(initialSetupProcessingStartHour)
                        && accountCurrentTime.Hour < int.Parse(initialSetupProcessingStopHour)))
                    {
                        IList<string> identities = new List<string>();
                        Logger.Current.Informational("Analyzing web visits for web analytics provider id: " + webAnalyticsProvider.Id + ". AccountID: " + webAnalyticsProvider.AccountID);
                        var content = "";
                        try
                        {
                            webAnalyticsProvider.LastAPICallTimeStamp = webAnalyticsProvider.LastAPICallTimeStamp != null ? webAnalyticsProvider.LastAPICallTimeStamp.Value : accountCurrentTime.AddDays(-180);

                            var accountTimeZone = accountTimeZones.Where(tz => tz.APIKey == webAnalyticsProvider.APIKey).Select(tz => tz.TimeZoneID).FirstOrDefault();
                            var accountUTCTimeSpan = VisiStatTimeZones.TimeZones().Where(tz => tz.TimeZoneId == byte.Parse(accountTimeZone)).Select(tz => tz.TimeSpan).FirstOrDefault();
                            var accountTimeZoneId = VisiStatTimeZones.TimeZones().Where(tz => tz.TimeZoneId == byte.Parse(accountTimeZone)).Select(tz => tz.GlobalTimeZoneId).FirstOrDefault();

                            TimeZoneInfo tzInfo = TimeZoneInfo.FindSystemTimeZoneById(accountTimeZoneId);
                            var isDaylightSaving = tzInfo.IsDaylightSavingTime(TimeZoneInfo.ConvertTime(webAnalyticsProvider.LastAPICallTimeStamp.Value, tzInfo));   //  http://www.timeanddate.com/time/change/usa/new-york?year=2015

                            if (isDaylightSaving)
                            {
                                accountUTCTimeSpan = accountUTCTimeSpan.Add(new TimeSpan(1, 0, 0));
                            }

                            var accountStartDate = webAnalyticsProvider.LastAPICallTimeStamp.Value.AddHours(accountUTCTimeSpan.Hours).AddMinutes(accountUTCTimeSpan.Minutes);
                            var accountEndDate = accountCurrentTime.AddHours(accountUTCTimeSpan.Hours).AddMinutes(accountUTCTimeSpan.Minutes);

                            TimeSpan startEndTimeSpan = (TimeSpan)(accountEndDate - accountStartDate);
                            if (startEndTimeSpan.TotalHours > 3)
                            {
                                accountEndDate = accountStartDate.AddHours(3);
                                accountCurrentTime = webAnalyticsProvider.LastAPICallTimeStamp.Value.AddHours(3);
                                Logger.Current.Verbose("accountEndDate: " + accountEndDate.ToString());
                            }

                            string accountStartDateString = String.Format("{0:u}", accountStartDate);
                            string accountEndDateString = String.Format("{0:u}", accountEndDate);

                            int year = accountStartDate.Year;
                            int month = accountStartDate.Month;

                            var identityLocation = Path.Combine(ConfigurationManager.AppSettings["WEBANALYTICS_API_RESPONSE_PATH"] , 
                                                                "Received", 
                                                                webAnalyticsProvider.AccountID.ToString(),
                                                                year.ToString(), 
                                                                month.ToString());

                            if(!Directory.Exists(identityLocation))
                            {
                                Directory.CreateDirectory(identityLocation);
                            }

                            string uvIdentitiesLocation = Path.Combine(identityLocation,
                                "UVByIdentity_" + accountEndDateString.Replace(":", "-") + "_AccID_" + webAnalyticsProvider.AccountID + "_WAID_" + webAnalyticsProvider.Id);


                            string uvByIddUrl = "http://api.visistat.com/stats-api-v25.php?key=" + webAnalyticsProvider.APIKey
                                + "&qt=idd&d=json&sdate=" + accountStartDateString + "&edate=" + accountEndDateString;
                            Logger.Current.Informational("AccountId: " + webAnalyticsProvider.AccountID + ", uvByIddURL: " + uvByIddUrl);

                            content = apiManager.APICall(uvByIddUrl, uvIdentitiesLocation);
                            UniqueVisitors = (dynamic)jsonSerializer.DeserializeObject(content);

                            foreach (dynamic item in UniqueVisitors)
                            {
                                string identity = item[1];
                                identities.Add(identity);
                            }

                            IList<string> knownIdentities = contactService
                                .CompareKnownIdentities(new CompareKnownContactIdentitiesRequest()
                                {
                                    ReceivedIdentities = identities,
                                    AccountId = webAnalyticsProvider.AccountID,

                                }
                                ).KnownIdentities.ToList();

                            IEnumerable<string> cpByIdentityLocation = new List<string>();

                            cpByIdentityLocation = apiManager.GetCPByIdentityFiles(webAnalyticsProvider, knownIdentities, accountStartDateString, accountEndDateString);
                            int ipIterator = 0;
                            foreach (string location in cpByIdentityLocation)
                            {

                                string[] readFile = File.ReadAllLines(location + ".txt");
                                var visits = (dynamic)jsonSerializer.DeserializeObject(readFile[0]);
                                int visitIterator = 0;
                                foreach (var visit in visits)
                                {
                                    if (visit[2] != "city")
                                    {
                                        DateTime currentPageVisitedOn = Convert.ToDateTime(visit[0]);
                                        DateTime nextPageVisitedOn = visitIterator > 1 ? Convert.ToDateTime(visits[visitIterator - 1][0]) : currentPageVisitedOn;

                                        //if (isDaylightSaving)
                                        //{
                                        //    currentPageVisitedOn = currentPageVisitedOn.AddHours(-1);
                                        //    nextPageVisitedOn = nextPageVisitedOn.AddHours(-1);
                                        //}
                                        WebVisit newWebVisit = new WebVisit()
                                        {
                                            VisitedOn = currentPageVisitedOn.AddHours(-accountUTCTimeSpan.Hours).AddMinutes(-accountUTCTimeSpan.Minutes),
                                            PageVisited = visit[1],
                                            City = visit[2],
                                            State = visit[3],
                                            Country = visit[4],
                                            ISPName = visit[7],
                                            ContactReference = knownIdentities[ipIterator],

                                            IPAddress = visit[6],
                                        };
                                        if (visitIterator > 1)
                                            newWebVisit.Duration = (short)nextPageVisitedOn.Subtract(currentPageVisitedOn).TotalSeconds;

                                        WebVisits.Add(newWebVisit);
                                    }
                                    visitIterator = visitIterator + 1;
                                }
                                ipIterator = ipIterator + 1;
                            }
                            try
                            {
                                Logger.Current.Verbose("After collecting the info the accountCurrentTime : " + accountCurrentTime.ToString());
                                webAnalyticsService.AddContactWebVisits(new AddContactWebVisitRequest()
                                {
                                    ContactWebVisits = WebVisits,
                                    LastAPICallTimeStamp = accountCurrentTime,
                                    WebAnalyticsProvider = webAnalyticsProvider,
                                    SplitVisitInterval = short.Parse(splitVisitInterval)
                                });
                            }
                            catch (Exception ex)
                            {
                                Logger.Current.Error("Exception occurred while saving the web visits to the database. AccountID: " + webAnalyticsProvider.AccountID, ex);
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Current.Error("Exception occurred while making an API call. AccountID: " + webAnalyticsProvider.AccountID, ex);
                        }
                        Logger.Current.Informational("Analyzing web visits for web analytics provider id: " + webAnalyticsProvider.Id + " is completed");
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                Logger.Current.Error("An exception occured with getting web analytics.", ex);
            }

            webTrackingInProgress = false;
        }

        static void ValidatePhysicalPaths()
        {
            if (!Directory.Exists(ConfigurationManager.AppSettings["WEBANALYTICS_API_RESPONSE_PATH"] + "/APILookUp"))
            {
                Directory.CreateDirectory(ConfigurationManager.AppSettings["WEBANALYTICS_API_RESPONSE_PATH"] + "/APILookUp");
            }
            if (!Directory.Exists(ConfigurationManager.AppSettings["WEBANALYTICS_API_RESPONSE_PATH"] + "/Received"))
            {
                Directory.CreateDirectory(ConfigurationManager.AppSettings["WEBANALYTICS_API_RESPONSE_PATH"] + "/Received");
            }
            if (!Directory.Exists(ConfigurationManager.AppSettings["WEBANALYTICS_API_RESPONSE_PATH"] + "/Failed"))
            {
                Directory.CreateDirectory(ConfigurationManager.AppSettings["WEBANALYTICS_API_RESPONSE_PATH"] + "/Failed");
            }
            if (!Directory.Exists(ConfigurationManager.AppSettings["WEBANALYTICS_API_RESPONSE_PATH"] + "/Processed"))
            {
                Directory.CreateDirectory(ConfigurationManager.AppSettings["WEBANALYTICS_API_RESPONSE_PATH"] + "/Processed");
            }
        }
    }
}

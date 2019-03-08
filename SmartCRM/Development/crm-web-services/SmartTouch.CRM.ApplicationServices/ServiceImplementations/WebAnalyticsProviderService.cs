using LandmarkIT.Enterprise.Utilities.Logging;
using SmartTouch.CRM.ApplicationServices.Messaging.User;
using SmartTouch.CRM.ApplicationServices.Messaging.WebAnalytics;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.Accounts;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Domain.WebAnalytics;
using SmartTouch.CRM.Domain.Workflows;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.MessageQueues;
using SmartTouch.CRM.SearchEngine.Indexing;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace SmartTouch.CRM.ApplicationServices.ServiceImplementations
{
    public class WebAnalyticsProviderService : IWebAnalyticsProviderService
    {
        readonly IAccountService accountService;
        readonly IContactService contactService;
        readonly IWebAnalyticsProviderRepository webAnalyticsProviderRepository;
        readonly IMessageQueuingService queuingService;
        readonly IMessageService messageService;
        readonly IIndexingService indexingService;
        public WebAnalyticsProviderService(IAccountRepository accountRepository, IContactService contactService
            , IAccountService accountService, IWebAnalyticsProviderRepository webAnalyticsProviderRepository
            , IMessageQueuingService queuingService, IMessageService messageService, IIndexingService indexingService)
        {
            this.contactService = contactService;
            this.accountService = accountService;
            this.webAnalyticsProviderRepository = webAnalyticsProviderRepository;
            this.queuingService = queuingService;
            this.messageService = messageService;
            this.indexingService = indexingService;
        }

        public CompareKnownContactIdentitiesResponse CompareKnownIps(CompareKnownContactIdentitiesRequest request)
        {
            CompareKnownContactIdentitiesResponse response = contactService.CompareKnownIdentities(new CompareKnownContactIdentitiesRequest() { ReceivedIps = request.ReceivedIps });
            return response;
        }

        public AddContactWebVisitResponse AddContactWebVisits(AddContactWebVisitRequest request)
        {
            AddContactWebVisitResponse response = new AddContactWebVisitResponse();
            IList<WebVisit> webVisits = webAnalyticsProviderRepository
                .AddWebVisists(request.ContactWebVisits, request.WebAnalyticsProvider, request.LastAPICallTimeStamp, request.SplitVisitInterval);
            foreach (var visit in webVisits)
            {
                if (!request.ContactWebVisits.Where(c => c.VisitedOn == visit.VisitedOn).Any()) // This will exclude the contact's previous visit which was already added to topic
                    this.addToTopic(visit.Duration, visit.Id, visit.ContactID, request.WebAnalyticsProvider.AccountID,
                        LeadScoreConditionType.ContactVisitsWebPage, visit.PageVisited);

                if (visit.Duration > 0)                            // This will remove the page that the contact is currently viewing. Will wait till he exits.
                    this.addToTopic(visit.Duration, visit.Id, visit.ContactID, request.WebAnalyticsProvider.AccountID,
                       LeadScoreConditionType.PageDuration, visit.PageVisited);
            }
          
            if (webVisits != null && webVisits.Any())
            {
                var uniqueContactIds = webVisits.Select(c => c.ContactID).ToList().Distinct();
                contactService.ContactIndexing(new Messaging.Contacts.ContactIndexingRequest() { ContactIds = uniqueContactIds.ToList(), Ids = uniqueContactIds.ToLookup(o => o, o => { return true; }) });
            }
            return response;
        }

        public ReIndexWebVisitsResponse ReIndexWebVisits(ReIndexWebVisitsRequest request)
        {
            int indexedWebVisits = 0;
            if (request.AccountId != 0)
            {
                IEnumerable<WebVisit> documents = webAnalyticsProviderRepository.FindWebvisitsByAccount(request.AccountId);
                if (documents == null || !documents.Any())

                    indexedWebVisits = indexedWebVisits + indexingService.ReIndexAll(documents);
            }
            return new ReIndexWebVisitsResponse() { IndexedWebVisits = indexedWebVisits };
        }


        /// <summary>
        /// Add to topic(web page view)
        /// </summary>
        /// <param name="trackingDomain"></param>
        /// <param name="webVisitId"></param>
        /// <param name="contactId"></param>
        /// <param name="accountId"></param>
        /// <param name="visitType"></param>
        void addToTopic(int duration, int webVisitId, int contactId, int accountId, LeadScoreConditionType visitType, string webPage)
        {
            var message = new TrackMessage()
            {
                EntityId = webVisitId,
                AccountId = accountId,
                ContactId = contactId,
                LeadScoreConditionType = (byte)visitType,
                LinkedEntityId = duration,
                ConditionValue = webPage,
            };
            messageService.SendMessages(new Messaging.Messages.SendMessagesRequest()
                {
                    Message = message
                });
        }

        public ValidateVisiStatKeyResponse ValidateVisiStatKey(ValidateVisiStatKeyRequest request)
        {
            ValidateVisiStatKeyResponse response = new ValidateVisiStatKeyResponse();
            var cookieJar = new CookieContainer();
            var baseUrl = "http://api.visistat.com/api-lookup.php?hid=" + request.VisiStatKey + "&Domain=" + request.TrackingDomain + "&act=1";
            HttpWebRequest ApiResponder = (HttpWebRequest)WebRequest.Create(baseUrl);
            ApiResponder.CookieContainer = cookieJar;
            ApiResponder.ContentType = "application/x-www-form-urlencoded";
            ApiResponder.Method = "POST";
            byte[] postBody = Encoding.UTF8.GetBytes("");
            ApiResponder.ContentLength = postBody.Length;
            Stream postStream = ApiResponder.GetRequestStream();
            postStream.Write(postBody, 0, postBody.Length);
            postStream.Close();

            HttpWebResponse ApiResponse = (HttpWebResponse)ApiResponder.GetResponse();
            Stream receiveStream = ApiResponse.GetResponseStream();
            StreamReader reader2 = new StreamReader(receiveStream, Encoding.UTF8);
            string content = reader2.ReadToEnd();
            //TextWriter tw = new StreamWriter(location + ".txt", true);
            //tw.WriteLine(content);
            //tw.Close();
            var isValidKey = content.Split('|');
            if (isValidKey.Count() > 1)
                response.IsValidKey = isValidKey[11] == "Active" ? true : false;
            else
            {
                response.IsValidKey = false;
                response.ResponseDescription = isValidKey[0];
            }
            return response;
        }

        public GetCurrentWebVisitNotificationsResponse GetCurrentWebVistNotifications(GetCurrentWebVisitNotificationsRequest request)
        {
            Logger.Current.Verbose("Request Recieved to get current web visit notifications");
            GetCurrentWebVisitNotificationsResponse response = new GetCurrentWebVisitNotificationsResponse();
            response.CurrentVisits = webAnalyticsProviderRepository.GetCurrentWebVisits().GroupBy(c=>c.VisitReference).Select(c=>c.First());
            return response;
        }

        public UpdateWebVisitNotificationsResponse UpdateWebVisitNotifications(UpdateWebVisitNotificationsRequest request)
        {
            UpdateWebVisitNotificationsResponse response = new UpdateWebVisitNotificationsResponse();
            webAnalyticsProviderRepository.UpdateWebVisitNotifications(request.VisitReferences);
            return response;
        }

        public GetAccountIdsToSendWebVisitDailySummaryResponse GetAccountIdsToSendWebVisitDailySummary(GetAccountIdsToSendWebVisitDailySummaryRequest request)
        {
            GetAccountIdsToSendWebVisitDailySummaryResponse response = new GetAccountIdsToSendWebVisitDailySummaryResponse();
            response.AccountsBasicInfo = webAnalyticsProviderRepository.GetAccountIdsToSendWebVisitDailySummary();
            return response;

        }

        public GetWebVisitDailySummaryResponse GetWebVisitDailySummary(GetWebVisitDailySummaryRequest request)
        {
            GetWebVisitDailySummaryResponse response = new GetWebVisitDailySummaryResponse();
            response.WebVisits = webAnalyticsProviderRepository.GetWebVisitDailySummary(request.AccountId, request.StartDate, request.EndDate);
            return response;
        }
    }
}
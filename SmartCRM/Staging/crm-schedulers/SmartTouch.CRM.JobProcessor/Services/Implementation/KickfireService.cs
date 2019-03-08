using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using LandmarkIT.Enterprise.Utilities.Logging;
using RestSharp;
using SmartTouch.CRM.JobProcessor.Extensions;

namespace SmartTouch.CRM.JobProcessor.Services.Implementation
{
    public class KickfireService : IKickfireService
    {
        private const string PartnerApiUrl = "http://api.visistat.com/api-lookup.php";
        private const string StatsApiUrl = "http://api.visistat.com/stats-api-v25.php";

        private readonly string _kickFireApiKey;
        protected readonly Logger Log = Logger.Current;

        public KickfireService(string kickFireApiKey)
        {
            _kickFireApiKey = kickFireApiKey;
        }

        public IEnumerable<PartnerInfo> GetPartnerInfos()
        {
            var response = ApiCall($"{PartnerApiUrl}?hid={_kickFireApiKey}&act=2");
            using (var reader = new StringReader(response))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrEmpty(line))
                        break;

                    var accountInfo = line.Split('|').ToArray();
                    yield return new PartnerInfo
                    {
                        ApiKey = accountInfo[11],
                        TimeZoneId = accountInfo[12],
                        TrackingDomain = accountInfo[2]
                    };
                }
            }
        }

        public IEnumerable<UniqueVisitor> GetUniqueVisitors(string kickFireClientApiKey, DateTime startDate, DateTime endDate)
        {
            var jsonSerializer = new JavaScriptSerializer();

            var response = ApiCall(
                    $"{StatsApiUrl}?key={kickFireClientApiKey}&qt=idd&d=json" +
                    $"&sdate={startDate.ToUniversalString()}" +
                    $"&edate={endDate.ToUniversalString()}");

            dynamic uniqueVisitors = jsonSerializer.DeserializeObject(response);
            foreach (var row in uniqueVisitors)
            {
                string date = row[0];
                string identity = row[1];
                string pageViews = row[2];

                //Skip first row
                if (date == "date")
                    continue;

                yield return new UniqueVisitor
                {
                    Date = DateTime.Parse(date),
                    Identity = identity,
                    PageViews = int.Parse(pageViews)
                }; 
            }
        }

        public IEnumerable<PageVisitInfo> GetPageVisitors(string kickFireClientApiKey, string identity, DateTime startDate, DateTime endDate)
        {
            var jsonSerializer = new JavaScriptSerializer();

            var response = ApiCall(
                $"{StatsApiUrl}?key={kickFireClientApiKey}&qt=cpbyid&d=json" +
                $"&sdate={startDate.ToUniversalString()}" +
                $"&edate={endDate.ToUniversalString()}&myid={identity}");

            dynamic cpVisitors = jsonSerializer.DeserializeObject(response);

            DateTime? prevVisitedOn = null;
            foreach (var row in cpVisitors)
            {
                string visitedOn = row[0];
                string pageName = row[1];
                string city = row[2];
                string region = row[3];
                string country = row[4];
                string ispName = row[5];
                string ipAddress = row[6];
                string referral = row[7];

                //Skip first row
                if (visitedOn == "date/time")
                    continue;

                var visitedOnDate = DateTime.Parse(visitedOn);
                var duration = (prevVisitedOn ?? visitedOnDate) - visitedOnDate;
                prevVisitedOn = visitedOnDate;

                yield return new PageVisitInfo
                {
                    VisitedOn = visitedOnDate,
                    PageName = pageName,
                    City = city,
                    Region = region,
                    Country = country,
                    IspName = ispName,
                    IpAddress = ipAddress,
                    SearchReferral = referral,
                    ContactReference = identity,
                    DurationInSeconds = (short)duration.TotalSeconds
                };
            }
        }

        public IEnumerable<PageVisitInfo> GetPageVisitors(string kickFireClientApiKey, string[] identities, DateTime startDate, DateTime endDate)
        {
            return identities.SelectMany(x => GetPageVisitors(kickFireClientApiKey, x, startDate, endDate));
        }

        private string ApiCall(string url)
        {
            Log.Informational($"Fetching: {url}");
            var client = new RestClient {BaseUrl = new Uri(url)};
            var request = new RestRequest {RequestFormat = DataFormat.Json};
            IRestResponse response;
            var task = Task.Run(() => client.Execute(request));
            if (task.Wait(TimeSpan.FromSeconds(20)))
            {
                response = client.Execute(request);
                Log.Informational($"Response received successfully. Url: {url}");
            }
            else
            {
                Log.Informational("Request timed out");
                throw new TimeoutException($"Request to VisiStat has timed out. Url: {url}");
            }
            return response.Content;
        }
    }

    public class PartnerInfo
    {
        public string ApiKey { get; set; }
        public string TimeZoneId { get; set; }
        public string TrackingDomain { get; set; }
    }

    public class UniqueVisitor
    {
        public DateTime Date { get; set; }
        public string Identity { get; set; }
        public int PageViews { get; set; }
    }

    public class PageVisitInfo
    {
        public DateTime VisitedOn { get; set; }
        public string PageName { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
        public string Country { get; set; }
        public string IspName { get; set; }
        public string IpAddress { get; set; }
        public string SearchReferral { get; set; }
        public string ContactReference { get; set; }
        public short DurationInSeconds { get; set; }
    }
}

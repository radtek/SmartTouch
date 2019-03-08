using LandmarkIT.Enterprise.Utilities.ExceptionHandling;
using LandmarkIT.Enterprise.Utilities.Logging;
using RestSharp;
using SmartTouch.CRM.Domain.WebAnalytics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.WebAnalyticsEngine
{
    public class APIManager
    {
        public string APICall(string baseUrl, string location)
        {
            Logger.Current.Verbose(string.Format("Fetching: {0}", baseUrl));
            Logger.Current.Verbose(string.Format("Stored at: {0}", location));
            RestClient client = new RestClient();
            client.BaseUrl = new Uri(baseUrl);
            RestRequest request = new RestRequest();
            request.RequestFormat = DataFormat.Json;
            IRestResponse response = new RestResponse();
            string content = string.Empty;
            var task = Task.Run(()=>client.Execute(request));
            if (task.Wait(TimeSpan.FromSeconds(20)))
            {
                Logger.Current.Verbose("Calling");
                response = client.Execute(request);
                content = response.Content;
                Logger.Current.Verbose(string.Format("Response received successfully.  Url: {0}", baseUrl));
            }
            else
            {
                Logger.Current.Informational("Request timed out");
                throw new UnsupportedOperationException(string.Format("Request to VisiStat has timed out. Url: {0}",baseUrl));
            }                

            using (TextWriter tw = new StreamWriter(location + ".txt", true))
            {
                if (response.StatusCode == HttpStatusCode.OK)
                    tw.WriteLine(content);
                else
                {
                    Logger.Current.Informational(string.Format("VisiStat Error Message: {0}", response.ErrorMessage));
                    tw.WriteLine(response.ErrorMessage);
                    throw new UnsupportedOperationException(string.Format("{0}. Url: {1}", response.ErrorMessage, baseUrl));
                }
                tw.Close();
            }
            return content;
        }

        public IEnumerable<string> GetCPByIdentityFiles(WebAnalyticsProvider webAnalyticsProvider, IList<string> identities, string startDate, string endDate)
        {
            identities.Remove("ip address");
            APIManager apiCalls = new APIManager();

            foreach (string identity in identities)
            {
                string cpByIdentityUrl = "http://api.visistat.com/stats-api-v25.php?key=" + webAnalyticsProvider.APIKey
                    + "&qt=cpbyid&d=json&sdate=" + startDate + "&edate=" + endDate + "&myid=" + identity;

                string cpByIdentityLocation = System.Configuration.ConfigurationManager.AppSettings["WEBANALYTICS_API_RESPONSE_PATH"] + "/Received"
                    + "\\CPByIdentity " + endDate.Replace(":", "-") + "_AccID_" + webAnalyticsProvider.AccountID + "_Identity_" + identity.Replace(".", "~");
                Logger.Current.Informational("AccountId: " + webAnalyticsProvider.AccountID + ", cpByIdentityURL: " + cpByIdentityUrl);

                apiCalls.APICall(cpByIdentityUrl, cpByIdentityLocation);
                yield return cpByIdentityLocation;
            }
        }

        public string GetAccountTimeZone(string baseUrl, string location)
        {
            var cookieJar = new CookieContainer();

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
            TextWriter tw = new StreamWriter(location + ".txt", true);
            tw.WriteLine(content);
            tw.Close();
            return content;
        }
    }
}

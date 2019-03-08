using LandmarkIT.Enterprise.Utilities.Logging;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;

namespace SmartTouch.CRM.JobProcessor
{
    public static class Utilities
    {
        public static string GetOAuthToken()
        {
            Logger.Current.Informational("Posting data to NeverBounce for fetching access_token");
            NeverBounceAccessTokenResponse response = new NeverBounceAccessTokenResponse();
            try
            {
                string serviceURL = ConfigurationManager.AppSettings["NeverBounce_API_URL"];
                string baseURL = "access_token";
                var client = new RestClient(serviceURL + baseURL);

                Logger.Current.Informational("RestClient object");
                string userName = ConfigurationManager.AppSettings["NeverBounce_UserName"];
                string secretKey = ConfigurationManager.AppSettings["NeverBounce_SecretKey"];
                string auth = userName + ":" + secretKey;
                string authEnc = Convert.ToBase64String(Encoding.ASCII.GetBytes(auth));


                var request = new RestRequest(Method.POST);
                request.AddHeader("cache-control", "no-cache");
                request.AddHeader("authorization", "Basic " + authEnc);
                request.AddHeader("content-type", "application/x-www-form-urlencoded");
                request.AddParameter("application/x-www-form-urlencoded", "grant_type=client_credentials&scope=basic+user", ParameterType.RequestBody);

                Logger.Current.Informational("Requesting neverbounce for Access_Token");
                response = client.Execute<NeverBounceAccessTokenResponse>(request).Data;
            }
            catch (Exception ex)
            {
                Logger.Current.Error("An exception occured while requesting OAUth token", ex);
            }
            return response.access_token;
        }
    }
}

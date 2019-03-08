using RestSharp;
using RestSharp.Authenticators;
using SmartTouch.CRM.ApplicationServices.Messaging.MailGun;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using System;

namespace SmartTouch.CRM.ApplicationServices.ServiceImplementations
{
    /// <summary>
    /// Mail Validation Service (MailGun)
    /// </summary>
    public class MailGunService : IMailGunService
    {
        /// <summary>
        /// Emails the validate.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        public GetRestResponse EmailValidate(GetRestRequest request)
       {
           return MailGunEmailValidation("/address/validate", "address", request);
       }

        public GetRestResponse BulkEmailValidate(GetRestRequest request)
        {
            return MailGunEmailValidation("/address/parse","addresses", request);
        }

        private static GetRestResponse MailGunEmailValidation(string resource, string parameter, GetRestRequest request)
        {
            var mailGunApiKey = System.Configuration.ConfigurationManager.AppSettings["mailgun_apikey"];
            var mailGunUrl = System.Configuration.ConfigurationManager.AppSettings["mailgun_url"];
            GetRestResponse response = new GetRestResponse();
            RestClient client = new RestClient();
            client.BaseUrl = new Uri(mailGunUrl);
            client.Authenticator = new HttpBasicAuthenticator("api", mailGunApiKey);
            RestRequest Request = new RestRequest();
            Request.Resource = resource;
            Request.AddParameter(parameter, request.Email);
            response.RestResponse = client.Execute(Request);
            return response;
        }
    }
}

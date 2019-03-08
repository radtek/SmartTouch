using LandmarkIT.Enterprise.CommunicationManager.Database;
using LandmarkIT.Enterprise.CommunicationManager.Operations;
using LandmarkIT.Enterprise.Utilities.Logging;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using SmartTouch.CRM.ApplicationServices.Messaging.Campaigns;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.Campaigns;
using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.JobProcessor
{
    public class LitmusTestProcessor : CronJobProcessor
    {
        readonly ICampaignService campaignService;
        readonly IAccountService accountService;
        public LitmusTestProcessor(CronJobDb cronJob, JobService jobService, string litmusTestCacheName)
            : base(cronJob, jobService, litmusTestCacheName)
        {
            campaignService = IoC.Container.GetInstance<ICampaignService>();
            accountService = IoC.Container.GetInstance<IAccountService>();
        }

        protected override void Execute()
        {
            /*
             * TODO get litmus results
             * Step 1: use HTTP Basic Authentication
             * Step 2: submit html to litmus and insert email guid to database 
             */
            //Update any litmus notifiers
            try
            {
                //For completed requests, notify user after 2 minutes.
                campaignService.NotifyLitmusCheck();
                var requests = GetLitmusRequests();
                foreach (var litmusMap in requests)
                {
                    try
                    {
                        var campaign = campaignService.GetCampaign(new ApplicationServices.Messaging.Campaigns.GetCampaignRequest(litmusMap.CampaignId)).CampaignViewModel;
                        var json = SubmitHtmlToLitmus(campaign);
                        Logger.Current.Informational("JSON : " + json);
                        string emailGuid = ((dynamic)JsonConvert.DeserializeObject(json))["email_guid"];
                        Logger.Current.Informational("EmailGuid : " + emailGuid);
                        litmusMap.ProcessingStatus = (int)LitmusCheckStatus.LitmusCheckCompleted;
                        litmusMap.LitmusId = emailGuid;
                        litmusMap.Remarks = "Litmus test completed";
                        campaignService.UpdateLitmusId(new UpdateCampaignLitmusMap()
                        {
                            CampaignLitmusMap = litmusMap
                        });
                    }
                    catch(Exception ex)
                    {
                        var remarks = ex.Message;
                        litmusMap.ProcessingStatus = (int)LitmusCheckStatus.LitmusCheckFailed;
                        litmusMap.Remarks = remarks.Substring(0, Math.Min(remarks.Length,250));
                        litmusMap.LitmusId = null;
                        campaignService.UpdateLitmusId(new UpdateCampaignLitmusMap()
                        {
                            CampaignLitmusMap = litmusMap
                        });
                        Logger.Current.Critical("Error while checking for litmus check", ex);
                    }
                }
            }
            catch(Exception ex)
            {
                Logger.Current.Critical("Error while running litmus processor", ex);
            }
        }

        private string SubmitHtmlToLitmus(CampaignViewModel campaign)
        {
            string apiKey = accountService.GettingLitmusTestAPIKey(campaign.AccountID);
            var baseUrl = ConfigurationManager.AppSettings["LITMUS_BASE_URL"].ToString();
            var restClient = new RestClient(baseUrl)
            {
                Authenticator = new HttpBasicAuthenticator(apiKey, "")
            };
            var request = new RestRequest("emails", Method.POST);
            request.JsonSerializer.ContentType = "application/json; charset=utf-8";
            var jsonStr = Serialize(campaign.From, "",campaign.HTMLContent, campaign.Subject);
            request.AddParameter("application/json", jsonStr, ParameterType.RequestBody);
            var d = restClient.Execute(request);
            return d.Content;
        }

        private string Serialize(string from, string fromDisplayName, string html, string subject)
        {
            var email = new LitmusEmailTest()
            {
                HtmlText = html,
                FromAddress = from,
                Subject = subject
            };
            var serializedString = JsonConvert.SerializeObject(email, Formatting.None, new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore
            });
            return serializedString;
        }

        private IEnumerable<CampaignLitmusMap> GetLitmusRequests()
        {
            return campaignService.GetPendingLitmusRequests().CampaignLitmusMaps;
        }
    }
    public class LitmusEmailTest
    {
        [JsonProperty(PropertyName = "html_text")]
        public string HtmlText { get; set; }
        [JsonProperty(PropertyName = "plain_text")]
        public string PlainText { get; set; }
        [JsonProperty(PropertyName = "subject")]
        public string Subject { get; set; }
        [JsonProperty(PropertyName = "from_address")]
        public string FromAddress { get; set; }
        [JsonProperty(PropertyName = "from_display_name")]
        public string FromDisplayName { get; set; }
        [JsonProperty(PropertyName = "configurations")]
        public IList<string> Configurations { get; set; }
    }
}

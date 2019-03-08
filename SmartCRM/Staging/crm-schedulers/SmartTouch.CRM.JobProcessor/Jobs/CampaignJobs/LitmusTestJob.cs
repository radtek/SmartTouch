using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Quartz;
using RestSharp;
using RestSharp.Authenticators;
using SmartTouch.CRM.ApplicationServices.Messaging.Campaigns;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.Campaigns;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.JobProcessor.QuartzScheduler.Scheduler;

namespace SmartTouch.CRM.JobProcessor.Jobs.CampaignJobs
{
    public class LitmusTestJob : BaseJob
    {
        private readonly ICampaignService _campaignService;
        private readonly IAccountService _accountService;
        private readonly JobServiceConfiguration _jobConfig;

        public LitmusTestJob(
            ICampaignService campaignService,
            IAccountService accountService,
            JobServiceConfiguration jobConfig)
        {
            _campaignService = campaignService;
            _accountService = accountService;
            _jobConfig = jobConfig;
        }

        protected override void ExecuteInternal(IJobExecutionContext context)
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
                _campaignService.NotifyLitmusCheck();
                var requests = GetLitmusRequests();
                foreach (var litmusMap in requests)
                {
                    try
                    {
                        var campaign = _campaignService.GetCampaign(new GetCampaignRequest(litmusMap.CampaignId)).CampaignViewModel;
                        var json = SubmitHtmlToLitmus(campaign);
                        Log.Informational("JSON : " + json);
                        string emailGuid = ((dynamic)JsonConvert.DeserializeObject(json))["email_guid"];
                        Log.Informational("EmailGuid : " + emailGuid);
                        litmusMap.ProcessingStatus = (int)LitmusCheckStatus.LitmusCheckCompleted;
                        litmusMap.LitmusId = emailGuid;
                        litmusMap.Remarks = "Litmus test completed";
                        _campaignService.UpdateLitmusId(new UpdateCampaignLitmusMap()
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
                        _campaignService.UpdateLitmusId(new UpdateCampaignLitmusMap()
                        {
                            CampaignLitmusMap = litmusMap
                        });
                        Log.Critical("Error while checking for litmus check", ex);
                    }
                }
            }
            catch(Exception ex)
            {
                Log.Critical("Error while running litmus processor", ex);
            }
        }

        private string SubmitHtmlToLitmus(CampaignViewModel campaign)
        {
            string apiKey = _accountService.GettingLitmusTestAPIKey(campaign.AccountID);
            var baseUrl = _jobConfig.LimitusBaseUrl;
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
            var email = new LitmusEmailTest
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
            return _campaignService.GetPendingLitmusRequests().CampaignLitmusMaps;
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

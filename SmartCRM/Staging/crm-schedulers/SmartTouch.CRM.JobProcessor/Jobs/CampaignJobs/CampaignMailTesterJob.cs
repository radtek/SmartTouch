using System;
using System.Collections.Generic;
using System.Configuration;
using LandmarkIT.Enterprise.CommunicationManager.Database;
using LandmarkIT.Enterprise.Extensions;
using Quartz;
using RestSharp;
using SmartTouch.CRM.ApplicationServices.Messaging.Campaigns;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.Domain.Campaigns;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.JobProcessor.QuartzScheduler.Scheduler;
using SmartTouch.CRM.JobProcessor.Utilities;

namespace SmartTouch.CRM.JobProcessor.Jobs.CampaignJobs
{
    public class CampaignMailTesterJob : BaseJob
    {
        private readonly ICampaignService _campaignService;
        private readonly JobServiceConfiguration _jobConfig;

        public CampaignMailTesterJob(ICampaignService campaignService, JobServiceConfiguration jobConfig)
        {
            _campaignService = campaignService;
            _jobConfig = jobConfig;
        }

        protected override void ExecuteInternal(IJobExecutionContext context)
        {
            try
            {
                List<CampaignMailTester> mailTests = new List<CampaignMailTester>();
                var data = _campaignService.GetMailTestData(new GetCampaignMailTesterRequest());
                if (data.CampaignMailTesterData.IsAny())
                { 
                    foreach(var test in data.CampaignMailTesterData)
                    {
                        string testResult = FetchData(test.UniqueID);
                        test.RawData = testResult;
                        test.Status = !string.IsNullOrEmpty(testResult) ? (byte)LitmusCheckStatus.LitmusCheckCompleted : (byte)LitmusCheckStatus.LitmusCheckFailed;
                        mailTests.Add(test);
                    }
                    _campaignService.UpdateMailTester(new UpdateMailTesterRequest() { MailTester = mailTests });
                }
            }
            catch (Exception ex)
            {
                Log.Error("An error occured while processing campaign mail test requests", ex);
            }
        }

        public string FetchData(Guid guid)
        {
            string url = _jobConfig.MailTesterUrl; 
            string rawData = string.Empty;
            if (!string.IsNullOrEmpty(url))
            {
                url = url.Replace("A", guid.ToString());
                var client = new RestClient(url);
                var request = new RestRequest(Method.GET);
                request.RequestFormat = DataFormat.Json;

                try
                {
                    rawData = client.Execute(request).Content;
                }
                catch (Exception ex)
                {
                    Log.Error("An error occured while fetching data from Mail Tester:", ex);
                }
            }
            return rawData;
        }
    }
}

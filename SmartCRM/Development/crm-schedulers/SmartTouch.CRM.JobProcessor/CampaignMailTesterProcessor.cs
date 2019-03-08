using LandmarkIT.Enterprise.CommunicationManager.Database;
using LandmarkIT.Enterprise.CommunicationManager.Operations;
using LandmarkIT.Enterprise.Utilities.Logging;
using LandmarkIT.Enterprise.Extensions;
using RestSharp;
using SmartTouch.CRM.ApplicationServices.Messaging.Campaigns;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
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
    public class CampaignMailTesterProcessor : CronJobProcessor
    {
        readonly ICampaignService campaignService;
        public CampaignMailTesterProcessor(CronJobDb cronJob, JobService jobService, string mailTesterCacheName)
            : base(cronJob, jobService, mailTesterCacheName)
        {
            campaignService = IoC.Container.GetInstance<ICampaignService>();
        }

        protected override void Execute()
        {
            try
            {
                List<CampaignMailTester> mailTests = new List<CampaignMailTester>();
                var data = campaignService.GetMailTestData(new GetCampaignMailTesterRequest());
                if (data.CampaignMailTesterData.IsAny())
                { 
                    foreach(var test in data.CampaignMailTesterData)
                    {
                        string testResult = FetchData(test.UniqueID);
                        test.RawData = testResult;
                        test.Status = !string.IsNullOrEmpty(testResult) ? (byte)LitmusCheckStatus.LitmusCheckCompleted : (byte)LitmusCheckStatus.LitmusCheckFailed;
                        mailTests.Add(test);
                    }
                    campaignService.UpdateMailTester(new UpdateMailTesterRequest() { MailTester = mailTests });
                }
            }
            catch (Exception ex)
            {
                Logger.Current.Error("An error occured while processing campaign mail test requests", ex);
            }
        }

        public string FetchData(Guid guid)
        {
            string url = ConfigurationManager.AppSettings["MailTesterURL"];
            string RawData = string.Empty;
            if (!string.IsNullOrEmpty(url))
            {
                url = url.Replace("A", guid.ToString());
                var client = new RestClient(url);
                var request = new RestRequest(Method.GET);
                request.RequestFormat = DataFormat.Json;

                try
                {
                    RawData = client.Execute(request).Content;
                }
                catch (Exception ex)
                {
                    Logger.Current.Error("An error occured while fetching data from Mail Tester:", ex);
                }
            }
            return RawData;
        }
    }
}

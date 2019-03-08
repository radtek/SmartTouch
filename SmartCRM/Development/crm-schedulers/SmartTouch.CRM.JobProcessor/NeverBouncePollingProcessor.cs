using LandmarkIT.Enterprise.CommunicationManager.Database;
using LandmarkIT.Enterprise.CommunicationManager.Operations;
using LandmarkIT.Enterprise.Utilities.Logging;
using Newtonsoft.Json;
using RestSharp;
using SmartTouch.CRM.ApplicationServices.Messaging.ImportData;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.JobProcessor
{
    public class NeverBouncePollingProcessor : CronJobProcessor
    {
        readonly IAccountService accountService;

        public NeverBouncePollingProcessor(CronJobDb cronJob, JobService jobService, string neverBouncePollingCacheName)
            : base(cronJob, jobService, neverBouncePollingCacheName)
        {
            accountService = IoC.Container.GetInstance<IAccountService>();
        }

        protected override void Execute()
        {
            try
            {
                Logger.Current.Informational("Request received for updating staus of neverbounce requests by continuse polling");
                var requests = accountService.GetAcceptedRequests(new GetNeverBounceAcceptedRequests() { Status = NeverBounceStatus.CSVGenerated });
                if (requests != null && requests.Requests.Any())
                {
                    string token = Utilities.GetOAuthToken();
                    string serviceURL = ConfigurationManager.AppSettings["NeverBounce_API_URL"];
                    Logger.Current.Informational("Requests count : " + requests.Requests.Count());
                    foreach (var request in requests.Requests)
                    {
                        if (!string.IsNullOrEmpty(token) && request.NeverBounceJobID.HasValue)
                        {
                            try
                            {
                                Logger.Current.Informational("Checking status for NeverBounceRequestID : " + request.NeverBounceRequestID);
                                NeverBounceStatusResponse response = CheckStatus(request.NeverBounceJobID.Value, token, serviceURL);
                                if (response != null && response.status != NeverBouncePollingStatus.Completed && response.status != NeverBouncePollingStatus.Failed)
                                    request.ScheduledPollingTime = request.EmailsCount < 50000 ? DateTime.UtcNow.AddSeconds(10) : DateTime.UtcNow.AddSeconds(25);
                                else if (response != null && response.status == NeverBouncePollingStatus.Completed)
                                    request.ServiceStatus = NeverBounceStatus.PollingCompleted;
                                else
                                    request.ServiceStatus = NeverBounceStatus.Failed;
                                request.PollingRemarks = response.content;
                                request.PollingStatus = (byte)response.status;

                                accountService.UpdateNeverBouncePollingRequest(new UpdateNeverBouncePollingRequest() { Request = request });
                            }
                            catch (Exception ex)
                            {
                                Logger.Current.Error("An error occured while updating neverbounce polling response for NeverBounceRequesID : " + request.NeverBounceRequestID, ex);
                                request.ServiceStatus = NeverBounceStatus.Failed;
                                request.PollingStatus = (byte)NeverBouncePollingStatus.Failed;
                                request.Remarks = ex.Message;
                                accountService.UpdateNeverBouncePollingRequest(new UpdateNeverBouncePollingRequest() { Request = request });
                                continue;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Current.Error("An error occured while polling never bounce", ex);
            }
        }

        private NeverBounceStatusResponse CheckStatus(int jobId, string accessToken, string url)
        {
            Logger.Current.Informational("Request received for checking status for jobId : " + jobId);

            string baseURL = url + "status";
            var client = new RestClient(baseURL);
            var request = new RestRequest(Method.POST);
            //request.AddHeader("content-type", "application/x-www-form-urlencoded");
            //request.AddParameter("access_token", accessToken, ParameterType.RequestBody);
            //request.AddParameter("version", 3.1, ParameterType.RequestBody);
            //request.AddParameter("job_id", jobId, ParameterType.RequestBody);

            string body = "access_token=" + accessToken + "&version=3.1&job_id=" + jobId;
            request.AddParameter("application/x-www-form-urlencoded", body, ParameterType.RequestBody);
            NeverBounceStatusResponse response = new NeverBounceStatusResponse();
            try
            {
                string content = client.Execute<NeverBounceStatusResponse>(request).Content;
                Logger.Current.Informational("Response of polling : " + content);
                if (!string.IsNullOrEmpty(content))
                    response = JsonConvert.DeserializeObject<NeverBounceStatusResponse>(content);
                response.content = content;
            }
            catch (Exception ex)
            {
                Logger.Current.Error("An error occured while checking status for jobId : " + jobId, ex);
            }
            return response;
        }
    }

    public class NeverBounceStatusResponse
    {
        public bool success { get; set; }
        public int id { get; set; }
        public NeverBouncePollingStatus status { get; set; }
        public string orig_name { get; set; }
        public string content { get; set; }
    }
}

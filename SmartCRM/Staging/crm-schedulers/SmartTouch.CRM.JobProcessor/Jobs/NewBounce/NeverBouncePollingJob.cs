using System;
using System.Configuration;
using System.Linq;
using Newtonsoft.Json;
using RestSharp;
using Quartz;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.ApplicationServices.Messaging.ImportData;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.JobProcessor.QuartzScheduler.Scheduler;
using SmartTouch.CRM.JobProcessor.Utilities;

namespace SmartTouch.CRM.JobProcessor.Jobs.NewBounce
{
    public class NeverBouncePollingJob : BaseJob
    {
        private readonly IAccountService _accountService;
        private readonly ApiManager _apiManager;
        private readonly JobServiceConfiguration _jobConfig;

        public NeverBouncePollingJob(
            IAccountService accountService,
            ApiManager apiManager,
            JobServiceConfiguration jobConfig)
        {
            _accountService = accountService;
            _apiManager = apiManager;
            _jobConfig = jobConfig;
        }

        protected override void ExecuteInternal(IJobExecutionContext context)
        {
            try
            {
                Log.Informational("Request received for updating staus of neverbounce requests by continuse polling");
                var requests = _accountService.GetAcceptedRequests(new GetNeverBounceAcceptedRequests() { Status = NeverBounceStatus.CSVGenerated });
                if (requests != null && requests.Requests.Any())
                {
                    string token = _apiManager.GetOAuthToken();
                    string serviceUrl = _jobConfig.NeverBounceApiUrl;
                    Log.Informational("Requests count : " + requests.Requests.Count());
                    foreach (var request in requests.Requests)
                    {
                        if (!string.IsNullOrEmpty(token) && request.NeverBounceJobID.HasValue)
                        {
                            try
                            {
                                Log.Informational("Checking status for NeverBounceRequestID : " + request.NeverBounceRequestID);
                                NeverBounceStatusResponse response = CheckStatus(request.NeverBounceJobID.Value, token, serviceUrl);
                                if (response != null && response.status != NeverBouncePollingStatus.Completed && response.status != NeverBouncePollingStatus.Failed)
                                    request.ScheduledPollingTime = request.EmailsCount < 50000 ? DateTime.UtcNow.AddSeconds(10) : DateTime.UtcNow.AddSeconds(25);
                                else if (response != null && response.status == NeverBouncePollingStatus.Completed)
                                    request.ServiceStatus = NeverBounceStatus.PollingCompleted;
                                else
                                    request.ServiceStatus = NeverBounceStatus.Failed;
                                request.PollingRemarks = response.content;
                                request.PollingStatus = (byte)response.status;

                                _accountService.UpdateNeverBouncePollingRequest(new UpdateNeverBouncePollingRequest() { Request = request });
                            }
                            catch (Exception ex)
                            {
                                Log.Error("An error occured while updating neverbounce polling response for NeverBounceRequesID : " + request.NeverBounceRequestID, ex);
                                request.ServiceStatus = NeverBounceStatus.Failed;
                                request.PollingStatus = (byte)NeverBouncePollingStatus.Failed;
                                request.Remarks = ex.Message;
                                _accountService.UpdateNeverBouncePollingRequest(new UpdateNeverBouncePollingRequest() { Request = request });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("An error occured while polling never bounce", ex);
            }
        }

        private NeverBounceStatusResponse CheckStatus(int jobId, string accessToken, string url)
        {
            Log.Informational("Request received for checking status for jobId : " + jobId);

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
                Log.Informational("Response of polling : " + content);
                if (!string.IsNullOrEmpty(content))
                    response = JsonConvert.DeserializeObject<NeverBounceStatusResponse>(content);
                response.content = content;
            }
            catch (Exception ex)
            {
                Log.Error("An error occured while checking status for jobId : " + jobId, ex);
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

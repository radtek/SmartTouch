using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using LandmarkIT.Enterprise.Extensions;
using LandmarkIT.Enterprise.Utilities.Excel;
using LandmarkIT.Enterprise.Utilities.ExceptionHandling;
using Newtonsoft.Json;
using Quartz;
using RestSharp;
using SmartTouch.CRM.ApplicationServices.Messaging.ImportData;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.JobProcessor.QuartzScheduler.Scheduler;
using SmartTouch.CRM.JobProcessor.Utilities;

namespace SmartTouch.CRM.JobProcessor.Jobs.NewBounce
{
    public class NeverBounceFileJob : BaseJob
    {
        private readonly IAccountService _accountService;
        private readonly IImportDataService _importService;
        private readonly ApiManager _apiManager;
        private readonly JobServiceConfiguration _jobConfig;

        public NeverBounceFileJob(
            IAccountService accountService,
            IImportDataService importService,
            ApiManager apiManager,
            JobServiceConfiguration jobConfig)
        {
            _accountService = accountService;
            _importService = importService;
            _apiManager = apiManager;
            _jobConfig = jobConfig;
        }

        protected override void ExecuteInternal(IJobExecutionContext context)
        {
            try
            {
                Log.Informational("Inside NeverBounce CSV file preparation");
                var requests = _accountService.GetAcceptedRequests(new GetNeverBounceAcceptedRequests() { Status = NeverBounceStatus.Accepted });
                if (requests != null && requests.Requests.IsAny())
                {
                    string serviceUrl = _jobConfig.NeverBounceApiUrl;
                    foreach (var request in requests.Requests)
                    {
                        try
                        {
                            Log.Informational("Generating CSV for NeverBounceRequestID : " + request.NeverBounceRequestID);
                            var contacts = _importService.GetContactEmails(new GetContactEmailsRequest() { EntityIds = request.EntityIds, EntityType = request.EntityType, AccountId = request.AccountID }).Contacts;
                            request.EmailsCount = contacts.Count();
                            Log.Informational("Emails count for the above jobid : " + request.EmailsCount);
                            request.ScheduledPollingTime = contacts.Count() < 50000 ? DateTime.UtcNow.AddSeconds(10) : DateTime.UtcNow.AddSeconds(25);

                            request.FileName = request.NeverBounceRequestID + ".csv";
                            GenerateCSV(contacts, request.FileName);
                            string access_token = _apiManager.GetOAuthToken();
                            if (!string.IsNullOrEmpty(access_token))
                            {
                                request.NeverBounceJobID = PostNeverBounce(access_token, request.FileName, serviceUrl);
                                request.ServiceStatus = NeverBounceStatus.CSVGenerated;
                                request.Remarks = "Successfully generated CSV";
                                _accountService.UpdateNeverBounceRequest(new UpdateNeverBounceRequest() { Request = request });
                            }
                            else
                                Log.Informational("Unable to fetch accesstoken");
                        }
                        catch (Exception e)
                        {
                            Log.Error("An error occured while generating CSV file for this NeverBounceRequestID : " + request.NeverBounceRequestID, e);
                            request.ServiceStatus = Entities.NeverBounceStatus.Failed;
                            request.Remarks = e.Message;
                            _accountService.UpdateNeverBounceRequest(new UpdateNeverBounceRequest { Request = request });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("An error occured while generating CSV file", ex);
            }
        }

        private void GenerateCSV(IEnumerable<ReportContact> reportContacts, string fileName)
        {
            if (reportContacts.IsAny())
            {
                Log.Informational("Request received for generating CSV with filename as : " + fileName);
                string location = _jobConfig.NeverBounceCsvFiles;
                location += fileName.Replace(".xls", ".csv").Replace(".xlsx", ".csv").Replace(".xml", ".csv");

                DataTable table = ConvertToDataTable(reportContacts);
                ReadExcel ex = new ReadExcel();
                byte[] bytes = ex.ConvertDataSetToCSV(table, string.Empty);
                File.WriteAllBytes(location, bytes);
                Log.Informational("CSV generated successfully");
            }
            else
                throw new UnsupportedOperationException("Couldn't find contacts with email for filename : " + fileName);
        }

        private DataTable ConvertToDataTable(IEnumerable<ReportContact> contacts)
        {
            DataTable table = new DataTable();
            Log.Informational("Request received for converting contacts to DataTable");
            if (contacts.IsAny())
            {
                table.Columns.Add("contactid");
                table.Columns.Add("contactemailid");
                table.Columns.Add("email");

                foreach (var contact in contacts)
                {
                    var row = new object[3];
                    row[0] = contact.contactID;
                    row[1] = contact.ContactEmailID;
                    row[2] = contact.email;
                    table.Rows.Add(row);
                }
            }
            return table;
        }

        private int PostNeverBounce(string access_token, string fileName, string url)
        {
            Log.Informational("Posting CSV location to NeverBounce");

            string baseURL = "bulk";
            var client = new RestClient(Path.Combine(url , baseURL));
            string localURL = _jobConfig.NeverBounceCsvurl;
            fileName = fileName.Replace(".xls", ".csv").Replace(".xlsx", ".csv").Replace(".xml", ".csv");

            var request = new RestRequest(Method.POST);
            //request.AddHeader("Content-Type", "x-www-form-urlencoded");

            //request.AddParameter("access_token", access_token, ParameterType.RequestBody);
            //request.AddParameter("input_location", 0, ParameterType.RequestBody);
            //request.AddParameter("input", Path.Combine(localURL , fileName), ParameterType.RequestBody);
            //request.AddParameter("filename", fileName, ParameterType.RequestBody);

            string body = "access_token=" + access_token + "&input_location=0&input=" + localURL + fileName + "&filename=" + fileName;
            request.AddParameter("application/x-www-form-urlencoded", body, ParameterType.RequestBody);

            NeverBounceResponse response = new NeverBounceResponse();
            try
            {
                string content = client.Execute(request).Content;
                Log.Informational("Response content : " + content);
                if(!string.IsNullOrEmpty(content))
                    response = JsonConvert.DeserializeObject<NeverBounceResponse>(content);
            }
            catch (Exception ex)
            {
                Log.Error("An exception occured while posting request to NeverBounce for filename : " + fileName, ex);
            }
            return response.job_id;
        }
    }

    public class TokenResponse
    {
        public string access_token { get; set; }
        public string expires_in { get; set; }
        public string token_type { get; set; }
        public string scope { get; set; }
    }

    public class NeverBounceResponse
    {
        //public bool success { get; set; }
        public int job_status { get; set; }
        public int job_id { get; set; }
    }

    public class NeverBounceAccessTokenResponse
    {
        public string error { get; set; }
        public string error_description { get; set; }
        public string access_token { get; set; }
        public int expires_in { get; set; }
        public string token_type { get; set; }
        public string scope { get; set; }
    }
}

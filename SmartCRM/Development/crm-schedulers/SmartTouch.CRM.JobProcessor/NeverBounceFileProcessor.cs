using LandmarkIT.Enterprise.CommunicationManager.Database;
using LandmarkIT.Enterprise.CommunicationManager.Operations;
using LandmarkIT.Enterprise.Utilities.Logging;
using LandmarkIT.Enterprise.Extensions;
using SmartTouch.CRM.ApplicationServices.Messaging.ImportData;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Domain.Contacts;
using System.Data;
using LandmarkIT.Enterprise.Utilities.Excel;
using System.IO;
using System.Configuration;
using LandmarkIT.Enterprise.Utilities.ExceptionHandling;
using RestSharp;
using System.Net;
using Newtonsoft.Json;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.UnitOfWork;

namespace SmartTouch.CRM.JobProcessor
{
    public class NeverBounceFileProcessor : CronJobProcessor
    {
     
        readonly IAccountService accountService;
        readonly IUnitOfWork unitofWork;
        readonly IImportDataService importService;

        public NeverBounceFileProcessor(CronJobDb cronJob, JobService jobService, string neverBounceFileProcessorCacheName)
            : base(cronJob, jobService, neverBounceFileProcessorCacheName)
        {
            accountService = IoC.Container.GetInstance<IAccountService>();
            unitofWork = IoC.Container.GetInstance<IUnitOfWork>();
            importService = IoC.Container.GetInstance<IImportDataService>();
        }
        protected override void Execute()
        {
            try
            {
                Logger.Current.Informational("Inside NeverBounce CSV file preparation");
                var requests = accountService.GetAcceptedRequests(new GetNeverBounceAcceptedRequests() { Status = NeverBounceStatus.Accepted });
                if (requests != null && requests.Requests.IsAny())
                {
                    string serviceURL = ConfigurationManager.AppSettings["NeverBounce_API_URL"];
                    foreach (var request in requests.Requests)
                    {
                        try
                        {
                            Logger.Current.Informational("Generating CSV for NeverBounceRequestID : " + request.NeverBounceRequestID);
                            var contacts = importService.GetContactEmails(new GetContactEmailsRequest() { EntityIds = request.EntityIds, EntityType = request.EntityType, AccountId = request.AccountID }).Contacts;
                            request.EmailsCount = contacts.Count();
                            Logger.Current.Informational("Emails count for the above jobid : " + request.EmailsCount);
                            request.ScheduledPollingTime = contacts.Count() < 50000 ? DateTime.UtcNow.AddSeconds(10) : DateTime.UtcNow.AddSeconds(25);

                            request.FileName = request.NeverBounceRequestID + ".csv";
                            GenerateCSV(contacts, request.FileName);
                            string access_token = Utilities.GetOAuthToken();
                            if (!string.IsNullOrEmpty(access_token))
                            {
                                request.NeverBounceJobID = PostNeverBounce(access_token, request.FileName, serviceURL);
                                request.ServiceStatus = Entities.NeverBounceStatus.CSVGenerated;
                                request.Remarks = "Successfully generated CSV";
                                accountService.UpdateNeverBounceRequest(new UpdateNeverBounceRequest() { Request = request });
                            }
                            else
                                Logger.Current.Informational("Unable to fetch accesstoken");
                        }
                        catch (Exception e)
                        {
                            Logger.Current.Error("An error occured while generating CSV file for this NeverBounceRequestID : " + request.NeverBounceRequestID, e);
                            request.ServiceStatus = Entities.NeverBounceStatus.Failed;
                            request.Remarks = e.Message;
                            accountService.UpdateNeverBounceRequest(new UpdateNeverBounceRequest() { Request = request });
                            continue;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Current.Error("An error occured while generating CSV file", ex);
            }
        }

        private void GenerateCSV(IEnumerable<ReportContact> reportContacts, string fileName)
        {
            if (reportContacts.IsAny())
            {
                Logger.Current.Informational("Request received for generating CSV with filename as : " + fileName);
                string location = ConfigurationManager.AppSettings["NeverBounce_CSV_Files"];
                location += fileName.Replace(".xls", ".csv").Replace(".xlsx", ".csv").Replace(".xml", ".csv");

                DataTable table = ConvertToDataTable(reportContacts);
                ReadExcel ex = new ReadExcel();
                byte[] bytes = ex.ConvertDataSetToCSV(table, string.Empty);
                File.WriteAllBytes(location, bytes);
                Logger.Current.Informational("CSV generated successfully");
            }
            else
                throw new UnsupportedOperationException("Couldn't find contacts with email for filename : " + fileName);
        }

        private DataTable ConvertToDataTable(IEnumerable<ReportContact> contacts)
        {
            DataTable table = new DataTable();
            Logger.Current.Informational("Request received for converting contacts to DataTable");
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
            Logger.Current.Informational("Posting CSV location to NeverBounce");

            string baseURL = "bulk";
            var client = new RestClient(Path.Combine(url , baseURL));
            string localURL = ConfigurationManager.AppSettings["NeverBounce_CSV_URL"];
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
                Logger.Current.Informational("Response content : " + content);
                if(!string.IsNullOrEmpty(content))
                    response = JsonConvert.DeserializeObject<NeverBounceResponse>(content);
            }
            catch (Exception ex)
            {
                Logger.Current.Error("An exception occured while posting request to NeverBounce for filename : " + fileName, ex);
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

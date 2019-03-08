using CsvHelper;
using LandmarkIT.Enterprise.Utilities.ExceptionHandling;
using LandmarkIT.Enterprise.Utilities.Logging;
using SmartTouch.CRM.ApplicationServices.Messaging.Campaigns;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.Domain.Campaigns;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using LandmarkIT.Enterprise.CommunicationManager.Database;
using LandmarkIT.Enterprise.CommunicationManager.Operations;
using LandmarkIT.Enterprise.Utilities.Common;

namespace SmartTouch.CRM.JobProcessor.VMTALog
{
    public class FileReadProcessor : CronJobProcessor
    {        
        private static List<string> inProcessFileList = new List<string>();
        private static List<string> inProcessFeedBackLoopFileList = new List<string>();

        public FileReadProcessor(CronJobDb cronJob, JobService jobService, string cacheName) : base(cronJob, jobService, cacheName)
        {
        }
        protected override void Execute()
        {
            GetLogs();
        }
        public enum LogFileType
        {
            Diag,
            FeedBack
        }

        private static void GetLogs()
          {
            try
            {
                var path = System.Configuration.ConfigurationManager.AppSettings["MAIL_DELIVERY_PROCESS_PATH"];
                Log("Getting files from...", path);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                IEnumerable<string> files = Directory.GetFiles(path, "*.csv");
                files.ToList().ForEach(file =>
                {
                    if (!inProcessFileList.Contains(file))
                    {
                        inProcessFileList.Add(file);
                        OnCreated(file);
                    }
                });

                var feedBackLoopFilePath = ConfigurationManager.AppSettings["MAIL_FEEDBACKLOOP_DELIVER_PROCESS_PATH"];
                Log("Getting files from...,", feedBackLoopFilePath);
                if (!Directory.Exists(feedBackLoopFilePath))
                {
                    Directory.CreateDirectory(feedBackLoopFilePath);
                }
                IEnumerable<string> feedbackloopfiles = Directory.GetFiles(feedBackLoopFilePath, "*.csv");
                feedbackloopfiles.ToList().ForEach(file =>
                {
                    if (!inProcessFeedBackLoopFileList.Contains(file))
                    {
                        inProcessFeedBackLoopFileList.Add(file);
                        onFeedBackLoopFileCreated(file);
                    }
                });
            }
            catch (Exception ex)
            {
                ExceptionHandler.Current.HandleException(ex, DefaultExceptionPolicies.LOG_ONLY_POLICY);
            }
        }

        /// <summary>
        /// This event is fired on new file arrival to the watching path.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void OnCreated(string filePath)
        {
            var canbemoved = false;
            try
            {
                if (Path.GetFileName(filePath).Contains("diag"))
                {
                    Log("Processing...", Path.GetFileName(filePath));
                    var campaignService = IoC.Container.GetInstance<ICampaignService>();                 
                    long filelength = new System.IO.FileInfo(filePath).Length;
                    if (filelength > 0)
                    {
                        using (TextReader sr = new StreamReader(File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                        {
                            var csv = new CsvReader(sr);
                            csv.Configuration.RegisterClassMap<MailMapping>();
                            csv.Configuration.WillThrowOnMissingField = false;

                            IEnumerable<MailStat> records = csv.GetRecords<MailStat>().Where(i => i.CampaignId > 0).ToList();

                   
                            IEnumerable<CampaignLogDetails> filteredRecipients = records.Where(r => !string.IsNullOrEmpty(r.Recipient))
                                                            .GroupBy(m => new { m.Recipient, m.CampaignId }).
                                                             Select(g => g.Select(p=>  new CampaignLogDetails() {                                                             
                                                                BounceCategory = (int?)p.BounceCategory,
                                                                CampaignId = (int?)p.CampaignId,
                                                                CampaignRecipientId = p.CampaignRecipientId,
                                                                CreatedOn = DateTime.UtcNow,
                                                                DeliveryStatus = (short?)p.DeliveryStatus,
                                                                OptOutStatus = p.OptOutStatus,
                                                                Recipient = p.Recipient,
                                                                Remarks = Path.GetFileName(filePath) + "/" + p.BounceCategory.ToString() + "/",
                                                                TimeLogged = p.TimeLogged,
                                                                Status = 1,
                                                                FileType = 1
                                                            }).OrderByDescending(v => v.TimeLogged).First());

                            InsertCampaignLogDetailsResponce insertResponce = campaignService.InsertCampaignLogDetails(new InsertCampaignLogDetailsRequest() { CampaignLogDetails = filteredRecipients });
                            canbemoved = true;
                        }
                    }
                    else
                    {
                        inProcessFileList.Remove(filePath);
                    }
                }
                else
                {
                    canbemoved = true;
                    Log("File Not Processed...", Path.GetFileName(filePath));
                }
                if (canbemoved)
                    MoveFile(filePath, LogFileType.Diag);
            }
            catch (Exception ex)
            {
                inProcessFileList.Remove(filePath);
                ExceptionHandler.Current.HandleException(ex, DefaultExceptionPolicies.LOG_ONLY_POLICY, values: filePath);
            }
        }

        /// <summary>
        /// This event is fired on new file arrival to the watching path.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void onFeedBackLoopFileCreated(string filePath)
        {
            var canbemoved = false;
            try
            {
                if (Path.GetFileName(filePath).Contains("feedbackloop"))
                {
                    Log("Processing...", Path.GetFileName(filePath));
                    var campaignService = IoC.Container.GetInstance<ICampaignService>();

                    long filelength = new System.IO.FileInfo(filePath).Length;
                    if (filelength > 0)
                    {
                        using (TextReader sr = new StreamReader(File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                        {
                            var csv = new CsvReader(sr);
                            csv.Configuration.RegisterClassMap<FeedBackLoopMailMapping>();
                            csv.Configuration.WillThrowOnMissingField = false;

                            var records = csv.GetRecords<FeedBackLoopMailStat>().Where(i => i.CampaignRecipientId > 0).ToList();

                            //take campaignid and recipient as unique combination,
                            //take latest record.
                            var filteredRecipients = records.GroupBy(m => new { m.CampaignRecipientId })
                                                            .Select(g => g.Select(p=>  
                                                            new CampaignLogDetails() {                                                                                                                            
                                                                CampaignRecipientId = p.CampaignRecipientId,
                                                                CreatedOn = DateTime.UtcNow,
                                                                DeliveryStatus = (short?)p.DeliveryStatus,                                                                
                                                                Recipient = p.Recipient,
                                                                Remarks = Path.GetFileName(filePath) + "/" + (p.Remarks == null? "": p.Remarks.ToString()) + "/",
                                                                TimeLogged = p.TimeLogged,
                                                                Status = 1,
                                                                FileType = 2
                                                            }).OrderByDescending(v => v.TimeLogged).First());

                            InsertCampaignLogDetailsResponce insertResponce = campaignService.InsertCampaignLogDetails(new InsertCampaignLogDetailsRequest() { CampaignLogDetails = filteredRecipients });      
                            canbemoved = true;

                        }
                    }
                    else
                    {
                        inProcessFeedBackLoopFileList.Remove(filePath);
                    }
                }
                else
                {
                    canbemoved = true;
                    Log("File Not Processed...", Path.GetFileName(filePath));
                }
                if (canbemoved)
                    MoveFile(filePath, LogFileType.FeedBack);
            }
            catch (Exception ex)
            {
                inProcessFeedBackLoopFileList.Remove(filePath);
                ExceptionHandler.Current.HandleException(ex, DefaultExceptionPolicies.LOG_ONLY_POLICY, values: filePath);
            }
        }

        /// <summary>
        /// Move file to another location and delete from source location.
        /// </summary>
        /// <param name="source"></param>
        private static void MoveFile(string source, LogFileType LogType)
        {
            Func<bool> Move = () =>
                {
                    Log("Moving file from..", source);
                    var destinationFileName = Path.GetFileNameWithoutExtension(source) + "." + DateTime.Now.ToString("hhmmssfff") + Path.GetExtension(source);
                    string destination = string.Empty;
                    if (LogType == LogFileType.Diag)
                    {
                        destination = System.Configuration.ConfigurationManager.AppSettings["MAIL_DELIVERY_PROCESS_ARCHIVE_PATH"];
                    }
                    else
                    {
                        destination = System.Configuration.ConfigurationManager.AppSettings["MAIL_FEEDBACKLOOP_DELIVER_PROCESS_ARCHIVE_PATH"];
                    }

                    if (!Directory.Exists(destination))
                    {
                        Directory.CreateDirectory(destination);
                    }
                    if (File.Exists(source))
                    {
                        File.Copy(source, Path.Combine(destination, destinationFileName), true);
                        File.Delete(source);
                        //File.Move(source, destinationFileName);
                        if (LogType == LogFileType.Diag)
                            inProcessFileList.Remove(source);
                        else
                            inProcessFeedBackLoopFileList.Remove(source);
                    }
                    Log("Moved To..", Path.Combine(destination, destinationFileName));
                    return true;
                };
            try
            {
                Retry.Do<bool>(() => Move(), TimeSpan.FromSeconds(10));

            }
            catch (Exception ex)
            {
                ExceptionHandler.Current.HandleException(ex, DefaultExceptionPolicies.LOG_ONLY_POLICY, values: source);
            }
        }

        /// <summary>
        /// Log details to logger.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="args"></param>
        private static void Log(string message, params object[] args)
        {
            StringBuilder format = new StringBuilder();
            var idx = 0;
            var separator = ":";
            args.ToList().ForEach(s =>
            {
                if ((string)s == (string)args.Last())
                    separator = "";
                format.Append(" {" + (idx++) + "} " + separator);
            });
#if DEBUG
            Console.WriteLine(string.Format(message + format.ToString(), args));
#endif
            Logger.Current.Informational(string.Format(message + format.ToString(), args));
        }

        
    }
}

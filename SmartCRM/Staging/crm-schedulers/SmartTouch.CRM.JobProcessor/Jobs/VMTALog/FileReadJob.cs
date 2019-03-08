using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Quartz;
using CsvHelper;
using LandmarkIT.Enterprise.Utilities.Common;
using LandmarkIT.Enterprise.Utilities.ExceptionHandling;
using LandmarkIT.Enterprise.Utilities.Logging;
using SmartTouch.CRM.ApplicationServices.Messaging.Campaigns;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.Domain.Campaigns;
using SmartTouch.CRM.JobProcessor.QuartzScheduler.Scheduler;

namespace SmartTouch.CRM.JobProcessor.Jobs.VMTALog
{
    public class FileReadJob : BaseJob
    {
        public enum LogFileType
        {
            Diag,
            FeedBack
        }

        private readonly List<string> _inProcessFileList = new List<string>();
        private readonly List<string> _inProcessFeedBackLoopFileList = new List<string>();
        private readonly JobServiceConfiguration _jobConfig;
        

        public FileReadJob(JobServiceConfiguration jobConfig)
        {
            _jobConfig = jobConfig;
        }

        protected override void ExecuteInternal(IJobExecutionContext context)
        {
            var path = _jobConfig.MailDeliveryProcessPath; 
            var feedBackLoopFilePath = _jobConfig.MailFeedbackLoopDeliverProcessPath;

            try
            {
                Log("Getting files from...", path);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                IEnumerable<string> files = Directory.GetFiles(path, "*.csv");
                files.ToList().ForEach(file =>
                {
                    if (!_inProcessFileList.Contains(file))
                    {
                        _inProcessFileList.Add(file);
                        OnCreated(file);
                    }
                });

                Log("Getting files from...", feedBackLoopFilePath);
                if (!Directory.Exists(feedBackLoopFilePath))
                {
                    Directory.CreateDirectory(feedBackLoopFilePath);
                }
                IEnumerable<string> feedbackloopfiles = Directory.GetFiles(feedBackLoopFilePath, "*.csv");
                feedbackloopfiles.ToList().ForEach(file =>
                {
                    if (!_inProcessFeedBackLoopFileList.Contains(file))
                    {
                        _inProcessFeedBackLoopFileList.Add(file);
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
        private void OnCreated(string filePath)
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
                        _inProcessFileList.Remove(filePath);
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
                _inProcessFileList.Remove(filePath);
                ExceptionHandler.Current.HandleException(ex, DefaultExceptionPolicies.LOG_ONLY_POLICY, values: filePath);
            }
        }

        /// <summary>
        /// This event is fired on new file arrival to the watching path.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void onFeedBackLoopFileCreated(string filePath)
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

                            campaignService.InsertCampaignLogDetails(new InsertCampaignLogDetailsRequest() { CampaignLogDetails = filteredRecipients });      
                            canbemoved = true;

                        }
                    }
                    else
                    {
                        _inProcessFeedBackLoopFileList.Remove(filePath);
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
                _inProcessFeedBackLoopFileList.Remove(filePath);
                ExceptionHandler.Current.HandleException(ex, DefaultExceptionPolicies.LOG_ONLY_POLICY, values: filePath);
            }
        }

        /// <summary>
        /// Move file to another location and delete from source location.
        /// </summary>
        /// <param name="source"></param>
        private void MoveFile(string source, LogFileType LogType)
        {
            Func<bool> Move = () =>
                {
                    Log("Moving file from..", source);
                    var destinationFileName = Path.GetFileNameWithoutExtension(source) + "." + DateTime.Now.ToString("hhmmssfff") + Path.GetExtension(source);
                    string destination = string.Empty;
                    if (LogType == LogFileType.Diag)
                    {
                        destination = _jobConfig.MailDeliveryProcessArchivePath;
                    }
                    else
                    {
                        destination = _jobConfig.MailFeedbackLoopDeliveryProcessArchivePath;
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
                            _inProcessFileList.Remove(source);
                        else
                            _inProcessFeedBackLoopFileList.Remove(source);
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

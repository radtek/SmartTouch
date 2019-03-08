using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.FtpClient;
using System.IO;
using LandmarkIT.Enterprise.Utilities.Logging;
using LandmarkIT.Enterprise.CommunicationManager.Database;
using LandmarkIT.Enterprise.CommunicationManager.Operations;
using LandmarkIT.Enterprise.Utilities.Common;

namespace SmartTouch.CRM.JobProcessor.VMTALog
{
    public class FTPProcessor : CronJobProcessor
    {
        public static Dictionary<string, FTPElement> FTPElements
        {
            get
            {
                var elements = new Dictionary<string, FTPElement>();
                var section = System.Configuration.ConfigurationManager.GetSection("ftppaths") as FTPPathRetrieverSection;
                foreach (FTPElement fe in section.FTPElements)
                    elements.Add(fe.Name, fe);
                return elements;
            }
        }
        

        public FTPProcessor(CronJobDb cronJob, JobService jobService, string cacheName) : base(cronJob, jobService, cacheName)
        {
        }

        protected override void Execute()
        {
            var ftpElement = FTPElements["sourceftppath"];
            using (var client = new FtpClient())
            {
                client.Host = ftpElement.Host;
                client.Credentials = new NetworkCredential(ftpElement.Username, ftpElement.Password);
                client.DataConnectionType = FtpDataConnectionType.PASV;
                client.Connect();
                Logger.Current.Informational("Successfully elastblished connection to ftp.");
                var ftpActiveDirectory = client.GetWorkingDirectory();
                var diag_dir = Path.Combine(ftpActiveDirectory, System.Configuration.ConfigurationManager.AppSettings["DIAG_ACTIVE_DIRECTORY"]);
                var fbl_dir = Path.Combine(ftpActiveDirectory, System.Configuration.ConfigurationManager.AppSettings["FBL_ACTIVE_DIRECTORY"]);
                var diag_files = client.GetListing(diag_dir);
                var fbl_files = client.GetListing(fbl_dir);

                foreach (var file in diag_files.Union(fbl_files).Where(f => Path.GetExtension(f.Name) == ".csv"))
                {
                    Logger.Current.Informational("Started reading file: " + file.FullName);
                    using (var fileStream = client.OpenRead(file.FullName))
                    {
                        string directoryPath = string.Empty;
                        if (file.Name.Contains("diag"))
                            directoryPath = System.Configuration.ConfigurationManager.AppSettings["MAIL_DELIVERY_PROCESS_PATH"];
                        else
                            directoryPath = System.Configuration.ConfigurationManager.AppSettings["MAIL_FEEDBACKLOOP_DELIVER_PROCESS_PATH"];

                        Logger.Current.Informational("Copy to: " + directoryPath + " , file: " + file.FullName);

                        using (var ftpStream = File.OpenWrite(string.Format("{0}/{1}", directoryPath, file.Name)))
                        {
                            try
                            {
                                var buffer = new byte[8 * 1024];
                                int count = fileStream.Read(buffer, 0, buffer.Length);
                                while (count > 0)
                                {
                                    ftpStream.Write(buffer, 0, count);
                                    count = fileStream.Read(buffer, 0, buffer.Length);
                                }
                            }
                            catch (Exception ex)
                            {
                                ftpStream.Close();
                                ex.Data.Add("file", file.Name);
                                Logger.Current.Error("Error received while reading ftp file.", ex);
                            }

                        }
                        long filelength = new System.IO.FileInfo(string.Format("{0}/{1}", directoryPath, file.Name)).Length;
                        Logger.Current.Informational("File Name:" + file.Name + "," + "File Length: " + filelength.ToString());
                        if (filelength > 0)
                        {
                            var archiveFilePath = Path.GetFileNameWithoutExtension(file.Name) + "." + DateTime.Now.ToString("hhmmssfff") + Path.GetExtension(file.Name);
                            var archivepath = Path.Combine(ftpActiveDirectory, System.Configuration.ConfigurationManager.AppSettings["FTP_ARCHIVE_PATH"]);
                            Logger.Current.Informational("Moving file to archive, file: " + file.Name);
                            client.Rename(file.FullName, Path.Combine(archivepath, archiveFilePath));
                        }

                    }

                }
            }
        }
    }
}

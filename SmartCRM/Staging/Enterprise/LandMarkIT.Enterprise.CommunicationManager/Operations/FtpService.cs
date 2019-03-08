using System;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;
using System.IO;
using System.Net;
using System.Xml;
using LandmarkIT.Enterprise.CommunicationManager.Database;
using LandmarkIT.Enterprise.CommunicationManager.DatabaseEntities;
using LandmarkIT.Enterprise.CommunicationManager.Requests;
using LandmarkIT.Enterprise.Utilities.ExceptionHandling;
using LandmarkIT.Enterprise.Utilities.Logging;

namespace LandmarkIT.Enterprise.CommunicationManager.Operations
{
    public class FtpService : BaseService
    {
       //protected static string supportEmailId = ConfigurationManager.AppSettings["SupportEmailId"];
       // //  protected static string supportEmailId = "smartcrm1@gmail.com";
       
        
        public FtpRegistrationDb GetService(Guid token)
        {
            return this.unitOfWork.FtpRegistrationsRepository.FirstOrDefault(ft => ft.Guid == token);
        }

        public List<string> GetFiles(Guid tokenGuid, string folderPath, Guid emilProvider, string leadAdapterName, string accountName)
        {
            var filesList = new List<string>();
            var registration = GetService(tokenGuid);
            string host = registration.Host.Trim();
            if (!host.StartsWith("ftp://"))
                host = "ftp://" + host;
            try
            {
                string username = registration.UserName.Normalize();
                string password = registration.Password.Normalize();
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(Path.Combine(host, folderPath));
                request.Timeout = 30000;
                request.Method = WebRequestMethods.Ftp.ListDirectory;
                request.UsePassive = true;
                request.Credentials = new NetworkCredential(username, password);
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                
                Stream responseStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(responseStream);
                string line = string.Empty;

                while ((line = reader.ReadLine()) != null)
                {
                    try
                    {
                        var filename = Path.GetFileName(line);
                        if (Path.GetExtension(filename) == ".xml")
                        {
                            XmlDocument doc = new XmlDocument();
                            var ftpfilepath = Path.Combine(host, Path.GetFileName(filename));
                            ftpfilepath = ftpfilepath.Replace("#", "%23");
                            FtpWebRequest filerequest = (FtpWebRequest)WebRequest.Create(ftpfilepath);
                            filerequest.Method = WebRequestMethods.Ftp.DownloadFile;

                            filerequest.Credentials = new NetworkCredential(username, password);
                            FtpWebResponse fileresponse = (FtpWebResponse)filerequest.GetResponse();
                            Stream fileresponseStream = fileresponse.GetResponseStream();
                            StreamReader filereader = new StreamReader(fileresponseStream);
                            string xmlcontent = filereader.ReadToEnd();
                            filereader.Close();
                            fileresponseStream.Close();
                            fileresponse.Close();
                            doc.LoadXml(xmlcontent);

                            filesList.Add(Path.GetFileName(line));
                        }
                    }
                    catch (XmlException ex)
                    {
                        ServiceEventArgs args = new ServiceEventArgs();
                        args.ServiceStatus = ServiceErrorStatus.InvalidXMLFile;
                        OnFailure(this, args);
                        Logger.Current.Error("The xml file is invalid , account:" + accountName + " LeadAdapter : " + leadAdapterName, ex);
                        Sendmail("Invalid xml file details" + ex, "Invalid xml file from  " + leadAdapterName + "  LeadAdapter", emilProvider, accountName, leadAdapterName);
                    }
                    catch (WebException ex)
                    {
                        var errorResponse = (FtpWebResponse)ex.Response;
                        if (errorResponse.StatusCode == FtpStatusCode.NotLoggedIn)
                        {
                            ServiceEventArgs args = new ServiceEventArgs();
                            args.ServiceStatus = ServiceErrorStatus.InvalidCredentials;
                            OnFailure(this, args);
                            Logger.Current.Error("The credentials are invalid, account:" + accountName + " LeadAdapter : " + leadAdapterName, ex);
                            Sendmail("Credentials details" + ex, "Invalid credentials for  " + leadAdapterName + " LeadAdapter", emilProvider, accountName, leadAdapterName);
                        }
                        else
                        {
                            Logger.Current.Error("Web exception in the FTP Service, account:" + accountName + " LeadAdapter : " + leadAdapterName, ex);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Current.Error("The xml file is invalid , account:" + accountName + " LeadAdapter : " + leadAdapterName, ex);
                        Sendmail("Invalid xml file details" + ex, "Invalid xml file from  " + leadAdapterName + " LeadAdapter", emilProvider, accountName, leadAdapterName);
                    }
                }
                //Clean-up
                reader.Close();
                responseStream.Close(); //redundant
                response.Close();
            }
            catch (WebException ex)
            {
                var errorResponse = (FtpWebResponse)ex.Response;
                if (errorResponse.StatusCode == FtpStatusCode.NotLoggedIn)
                {
                    ServiceEventArgs args = new ServiceEventArgs();
                    args.ServiceStatus = ServiceErrorStatus.InvalidCredentials;
                    OnFailure(this, args);
                    Logger.Current.Error("The credentials are invalid, account:" + accountName, ex);
                    Sendmail("Credentials details" + ex, "Invalid credentials for " + leadAdapterName + " LeadAdapter", emilProvider, accountName, leadAdapterName);
                }
                else
                {
                    Logger.Current.Error("Exception in the get files method, token: " + tokenGuid + ",status: " + errorResponse.StatusCode + " Account name : " + accountName + " LeadAdapter : " +leadAdapterName, ex);
                }

            }
            catch (Exception ex)
            {
                Logger.Current.Error("Exception in the get files method, token: " + tokenGuid +" Account name : " + accountName + " LeadAdapter : " + leadAdapterName, ex);
            }
            return filesList;
        }

        public void Delete(Guid tokenGuid, string folderPath, List<string> fileNames)
        {
            var registration = GetService(tokenGuid);
            foreach (var item in fileNames)
            {
                string host = registration.Host;
                if (!host.StartsWith("ftp://"))
                    host = "ftp://" + host;
                var request = (FtpWebRequest)WebRequest.Create(Path.Combine(host, folderPath, item).Replace("#","%23"));
                request.Method = WebRequestMethods.Ftp.DeleteFile;
                request.Credentials = new NetworkCredential(registration.UserName, registration.Password);
                request.GetResponse();
            }

        }

        public bool MoveFile(Guid tokenGuid, string fileName, string localArchiveFilePath, int JobLogID)
        {
            try
            {
                var registration = GetService(tokenGuid);
                string extension = Path.GetExtension(localArchiveFilePath);
                string filenamewithoutextension = Path.GetFileNameWithoutExtension(fileName);
                string host = registration.Host;
                if (!host.StartsWith("ftp://"))
                    host = "ftp://" + host;
                string destinationFTpPath = host + "/" + "Archive" + "/" + filenamewithoutextension + JobLogID.ToString() + extension;
                
                bool isUploaded = UploadFile(destinationFTpPath, registration.UserName, registration.Password, localArchiveFilePath);
                Logger.Current.Verbose("File Upload status : " + isUploaded);
                bool isDelted = DeleteFile(host + "/" + fileName, registration.UserName, registration.Password);
                Logger.Current.Verbose("Is the file deleted : " + isDelted);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Current.Error("An exception occured in moving file method: ", ex);
               
                return false;
            }
        }

        void Sendmail(string message, string subject, Guid emilProvider, string accountName, string leadAdapterName)
        {
            try
            {
                string supportEmailId = ConfigurationManager.AppSettings["SupportEmailId"];
                string machine = ConfigurationManager.AppSettings["MachineName"];
                string subjct = accountName + " - " + subject;
                if (machine != "" && machine != null)
                    subjct = machine + " : " + subjct;


                List<string> To = new List<string>();
                Logger.Current.Verbose("Sending Email in LeadAdapter Engine :" + supportEmailId);
                supportEmailId = supportEmailId == null ? "smartcrm3@gmail.com" : supportEmailId;
                var body = " Error Message     : " + subject + ".\r\n Account Name     : " + accountName + ".\r\n LeadAdapter        : " + leadAdapterName + "\r\n Instance occured on  : " + DateTime.UtcNow + " (UTC).\r\n More Info            : " + message;


                To.Add(supportEmailId);
                SendMailRequest request = new SendMailRequest();
                request.To = To;
                request.From = supportEmailId;
                request.Body = body;
                request.Subject = subjct;
                request.TokenGuid = emilProvider;
                MailService service = new MailService();
                service.Send(request);
                Logger.Current.Verbose("Successfully sent mail from :" + supportEmailId);
            }
            catch (Exception ex)
            {
                Logger.Current.Error("An exception occured while sending email: ", ex);
            }
        }

        public bool UploadFile(string destFilePath, string username, string password, string localfilename)
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    client.Credentials = new NetworkCredential(username, password);
                    client.UploadFile(destFilePath, "STOR", localfilename);
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.Current.Error("Error in upload file method:", ex);
                return false;
            }
        }

        public bool DeleteFile(string destFilePath, string username, string password)
        {
            try
            {
                destFilePath = destFilePath.Replace("#", "%23");
                var request = (FtpWebRequest)WebRequest.Create(destFilePath);
                request.Method = WebRequestMethods.Ftp.DeleteFile;
                request.Credentials = new NetworkCredential(username, password);
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                Logger.Current.Informational("Ftp web response : " + response.StatusCode);
                return true;
            }
            catch (Exception ex)
            {
                ex.Data.Clear();
                ex.Data.Add("Destination File Path", destFilePath);
                ex.Data.Add("User Name", username);
                Logger.Current.Error("Error while deleting a file, chances of file being deleted by another account/ there is no permission :", ex);
                return false;
            }
        }

        public bool CreateFTPDirectory(Guid tokenGuid)
        {
            try
            {
                var registration = GetService(tokenGuid);
                string host = registration.Host;
                if (!host.StartsWith("ftp://"))
                    host = "ftp://" + host;
                //create the directory
                FtpWebRequest requestDir = (FtpWebRequest)FtpWebRequest.Create(host + "/" + "Archive");
                requestDir.Credentials = new NetworkCredential(registration.UserName, registration.Password);
                requestDir.Method = WebRequestMethods.Ftp.MakeDirectory;
                FtpWebResponse response = (FtpWebResponse)requestDir.GetResponse();
                response.Close();
                return true;
            }
            catch (WebException ex)
            {
                FtpWebResponse response = (FtpWebResponse)ex.Response;
                if (response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                {
                    response.Close();
                    return true;
                }
                else
                {
                    response.Close();
                    return false;
                }
            }
        }

        public void Download(Guid tokenGuid, string folderPath, string ftpFilePath, string localFileName)
        {
            string path = string.Empty;
            try
            {
                var registration = GetService(tokenGuid);
                var request = new WebClient();
                string host = registration.Host;
                if (!host.StartsWith("ftp://"))
                    host = "ftp://" + host;
                request.Credentials = new NetworkCredential(registration.UserName, registration.Password);
                path = Path.Combine(host, folderPath, ftpFilePath).Replace("#", "%23");
                request.DownloadFile(path, localFileName);
            }
            catch (Exception ex)
            {
                Logger.Current.Error("An exception occured in download method, ftp filepath: " + path, ex);
            }
        }

        public void DownloadFileAsync(Guid tokenGuid, string ftpFilePath, string localFileName)
        {
            var registration = GetService(tokenGuid);
            var request = new WebClient();
            string host = registration.Host;
            if (!host.StartsWith("ftp://"))
                host = "ftp://" + host;
            request.Credentials = new NetworkCredential(registration.UserName, registration.Password);
            var path = new Uri(Path.Combine(host, ftpFilePath).Replace("#", "%23"));
            localFileName = localFileName.Replace("#", "%23");
            request.DownloadFileAsync(path, localFileName);
        }

        public bool Update(string userName, string password, string url, int? port, bool enableSSL, Guid requestGuid)
        {
            var registration = GetService(requestGuid);
            registration.EnableSsl = enableSSL;
            registration.UserName = userName;
            registration.Password = password;
            registration.Host = url;
            registration.Port = port;
            this.unitOfWork.FtpRegistrationsRepository.Edit(registration);
            unitOfWork.Commit();
            return true;
        }

        public RegisterFtpRequest GetFtp(Guid token)
        {
            var ftpDetails = GetService(token);
            var registerFtpRequest = new RegisterFtpRequest
            {
                EnableSsl = ftpDetails.EnableSsl,
                Host = ftpDetails.Host,
                UserName = ftpDetails.UserName,
                Password = ftpDetails.Password,
                Port = ftpDetails.Port
            };
            return registerFtpRequest;
        }

        public IEnumerable<Guid> FindMatchGuids(Guid guid)
        {
            IEnumerable<Guid> guids = null;
            var registeration = this.GetService(guid);
            if (registeration != null)
                 guids = this.unitOfWork.FtpRegistrationsRepository.Find(f => f.Host == registeration.Host).Select(s => s.Guid);
            return guids;
        }
    }
}

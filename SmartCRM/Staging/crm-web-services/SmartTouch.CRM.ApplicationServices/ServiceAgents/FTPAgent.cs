using LandmarkIT.Enterprise.CommunicationManager.Operations;
using LandmarkIT.Enterprise.CommunicationManager.Requests;
using System;

namespace SmartTouch.CRM.ApplicationServices.ServiceAgents
{
    public class FTPAgent
    {
        public Guid FTPRegistration(string userName, string password, string url, int? port, bool enableSSL, Guid requestGuid)
        {
            var request = new RegisterFtpRequest
            {
                UserName = userName,
                Password = password,
                Host = url,
                Port = port,
                EnableSsl = enableSSL,
            };
            ServiceRegistration ftpRegistration = new ServiceRegistration();
            var response = ftpRegistration.RegisterFtp(request);
            return response.Token;
        }
        public bool UpdateFtpRegistration(string userName, string password, string url, int? port, bool enableSSL, Guid requestGuid)
        {
            FtpService ftp = new FtpService();
            return ftp.Update(userName, password, url, port, enableSSL, requestGuid);
        }
        public RegisterFtpRequest GetFtpRegistration(Guid token)
        {
            FtpService ftp = new FtpService();
            return ftp.GetFtp(token);
        }
    }
}

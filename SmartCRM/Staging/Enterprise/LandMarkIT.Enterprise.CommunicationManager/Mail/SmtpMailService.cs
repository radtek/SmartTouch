using LandmarkIT.Enterprise.CommunicationManager.DatabaseEntities;
using LandmarkIT.Enterprise.CommunicationManager.Operations;
using LandmarkIT.Enterprise.CommunicationManager.Repository;
using LandmarkIT.Enterprise.CommunicationManager.Requests;
using LandmarkIT.Enterprise.CommunicationManager.Responses;
using LandmarkIT.Enterprise.Logging;
using System;
using System.Net;
using System.Net.Mail;

namespace LandmarkIT.Enterprise.CommunicationManager.Mail
{
    public class SmtpMailService : IMailService
    {
        //TODO: implement reply to as well        
        SmtpClient smtpClient = default(SmtpClient);
        public SmtpMailService(Guid token)
        {
            var cmDataAccess = new CMDataAccess();
            var registration = cmDataAccess.GetMailRegistration(token);

            smtpClient = new SmtpClient(registration.Address);
            smtpClient.Credentials = new NetworkCredential(registration.UserName, registration.Password);
            if (registration.Port.HasValue) smtpClient.Port = registration.Port.Value;
            smtpClient.EnableSsl = registration.IsSSLEnabled;
        }

        public SendMailResponse Send(SendMailRequest request)
        {
            SendMailResponse result = new SendMailResponse();
            try
            {
                result.RequestGuid = Guid.NewGuid();
                smtpClient.Send(request.ToMailMessage());
                SendMailDetail details = new SendMailDetail();
               
                details.BCC = string.IsNullOrEmpty(Convert.ToString(request.BCC)) ? null : string.Join(";", request.BCC.ToArray());
                details.CC = string.IsNullOrEmpty(Convert.ToString(request.CC)) ? null : string.Join(";", request.CC.ToArray());
                details.From = request.From;
                details.Status = CommunicationStatus.Success;
                details.To = string.Join(";", request.To.ToArray());
                result.Details.Add(details);
                Response mailresponse = new Response();
                result.RequestGuid = mailresponse.MailResponse(result);
            }
            catch (Exception ex)
            {
                Logger.Current.Error("Exception occurred sending the mail..", ex);

            }
            return result;
        }

        public SendMailResponse SendIndependent(SendMailRequest request)
        {
            var result = new SendMailResponse { RequestGuid = Guid.NewGuid() };
            smtpClient.Send(request.ToMailMessage());
            return result;
        }
    }
}

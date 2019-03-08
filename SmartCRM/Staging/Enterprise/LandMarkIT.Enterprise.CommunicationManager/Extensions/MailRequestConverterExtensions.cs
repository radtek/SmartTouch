using LandmarkIT.Enterprise.CommunicationManager.Database;
using LandmarkIT.Enterprise.CommunicationManager.Operations;
using LandmarkIT.Enterprise.CommunicationManager.Requests;
using LandmarkIT.Enterprise.CommunicationManager.Responses;
using System;
using System.Net.Mail;

namespace LandmarkIT.Enterprise.CommunicationManager.Extensions
{
    public static class MailRequestConverterExtensions
    {
        public static MailMessage ToMailMessage(this SendMailRequest request)
        {
            var result = new MailMessage
            {
                From = string.IsNullOrWhiteSpace(request.DisplayName) ? new MailAddress(request.From) : new MailAddress(request.From, request.DisplayName),
                Priority = (System.Net.Mail.MailPriority)request.PriorityID,
                Subject = request.Subject,
                Body = request.Body,
                IsBodyHtml = request.IsBodyHtml
            };

            if (!string.IsNullOrWhiteSpace(request.ReplyTo)) result.ReplyToList.Add(request.ReplyTo);

            if (request.To != null && request.To.Count > 0) request.To.ForEach(item => { result.To.Add(item); });
            if (request.CC != null && request.CC.Count > 0) request.CC.ForEach(item => { result.CC.Add(item); });
            if (request.BCC != null && request.BCC.Count > 0) request.BCC.ForEach(item => { result.Bcc.Add(item); });
            //if (request.Attachments != null && request.Attachments.Count > 0) request.Attachments.ForEach(item => { result.Attachments.Add(new Attachment(item.ContentStream, item.FileName)); });
            return result;
        }

        public static RegisterMailRequest ToSmtpMailRegistrationRequest(this MailRegistrationDb mailRegistraion)
        {
            throw new NotImplementedException();
        }
        public static RegistrationResponse TextRegistrationRequest(RegisterTextRequest request)
        {
            ServiceRegistration registration = new ServiceRegistration();
            return registration.RegisterText(request);
        }
        public static RegistrationResponse EmailRegistrationRequest(RegisterMailRequest request)
        {
            ServiceRegistration registration = new ServiceRegistration();
            return registration.RegisterMail(request);
        }
        public static SendMailResponse ToSendMailResponse(this SendMailRequest request)
        {
            return new SendMailResponse
            {
                Token = request.TokenGuid,
                RequestGuid = request.RequestGuid,
                StatusID = CommunicationStatus.Queued,
                ServiceResponse = string.Empty
            };
        }
    }
}

using LandmarkIT.Enterprise.CommunicationManager.DatabaseEntities;
using LandmarkIT.Enterprise.CommunicationManager.Requests;
using System;
using System.Net.Mail;

namespace LandmarkIT.Enterprise.CommunicationManager
{
    internal static class CommunicationManagerExtensions
    {
        internal static MailMessage ToMailMessage(this SendMailRequest request)
        {
            var result = new MailMessage
            {
                From = string.IsNullOrWhiteSpace(request.DisplayName) ? new MailAddress(request.From) : new MailAddress(request.From, request.DisplayName),
                Priority = (System.Net.Mail.MailPriority)request.Priority,
                Subject = request.Subject,
                Body = request.Body,
                IsBodyHtml = request.IsBodyHtml
            };

            if (!string.IsNullOrWhiteSpace(request.ReplyTo)) result.ReplyToList.Add(request.ReplyTo);

            if (request.To != null && request.To.Count > 0) request.To.ForEach(item => { result.To.Add(item); });
            if (request.CC != null && request.CC.Count > 0) request.CC.ForEach(item => { result.CC.Add(item); });
            if (request.BCC != null && request.BCC.Count > 0) request.BCC.ForEach(item => { result.Bcc.Add(item); });
            if (request.Attachments != null && request.Attachments.Count > 0) request.Attachments.ForEach(item => { result.Attachments.Add(new Attachment(item.ContentStream, item.FileName)); });
            return result;
        }

        internal static SmtpMailRegistrationRequest ToSmtpMailRegistrationRequest(this MailRegistration mailRegistraion)
        {
            throw new NotImplementedException();
        }
        
    }
}

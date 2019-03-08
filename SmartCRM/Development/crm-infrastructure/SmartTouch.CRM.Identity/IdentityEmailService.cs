using LandmarkIT.Enterprise.CommunicationManager.Operations;
using LandmarkIT.Enterprise.CommunicationManager.Requests;
using LandmarkIT.Enterprise.Utilities.Logging;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Repository.Repositories;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using SmartTouch.CRM.Domain.Communication;

namespace SmartTouch.CRM.Identity
{
    public class IdentityEmailService : IIdentityMessageService
    {
        public IServiceProviderRepository ServiceProviderRepository { get; set; }

        public Task SendAsync(IdentityMessage message)
        {
            try
            {
                Logger.Current.Verbose("Request to send an email from Identity-Email service.");
                MailService mailService = new MailService();
                var subject = message.Subject.Split('|')[0];
                var from = message.Subject.Split('|')[1];
                var loginToken = message.Subject.Split('|')[3];
                SendMailRequest request = new SendMailRequest();
                request.TokenGuid = new Guid(loginToken);
                request.RequestGuid = Guid.NewGuid();
                request.ScheduledTime = DateTime.Now.ToUniversalTime();
                request.Subject = subject;
                request.Body = message.Body;
                request.To = new List<string>() { message.Destination };
                //request.To = message.Destination;
                request.IsBodyHtml = true;
                request.ServiceProviderEmail = from;
                request.AccountDomain = message.Subject.Split('|')[4];
                request.From = from;

                return mailService.SendAsync(request);
            }
            catch (Exception ex)
            {
                Logger.Current.Error("Exception occured while sending an email from Identity-Email service.",ex);
                throw;
            }
            
        }

        Guid GetAccountEmailProviderToken(int accountId)
        {
            IEnumerable<ServiceProvider> serviceProviders = ServiceProviderRepository.GetAccountCommunicationProviders(accountId, CommunicationType.Mail, Entities.MailType.TransactionalEmail);
            return serviceProviders.FirstOrDefault().LoginToken;
        }
    }
}

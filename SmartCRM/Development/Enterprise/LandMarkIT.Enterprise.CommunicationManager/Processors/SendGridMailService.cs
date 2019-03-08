using LandmarkIT.Enterprise.CommunicationManager.Contracts;
using System;

namespace LandmarkIT.Enterprise.CommunicationManager.Processors
{
    public class SendGridMailService : SmtpMailService, IMailService
    {
        public SendGridMailService(IUnitOfWork unitOfWork, Guid token, short parallelLoad = 30) : base(unitOfWork, token, parallelLoad) { }
    }
}

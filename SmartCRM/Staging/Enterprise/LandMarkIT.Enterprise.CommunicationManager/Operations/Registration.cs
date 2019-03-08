using LandmarkIT.Enterprise.CommunicationManager.Contracts;
using LandmarkIT.Enterprise.CommunicationManager.Database;
using LandmarkIT.Enterprise.CommunicationManager.Repositories;
using LandmarkIT.Enterprise.CommunicationManager.Requests;
using LandmarkIT.Enterprise.CommunicationManager.Responses;
using LandmarkIT.Enterprise.Utilities.Logging;
using System;

namespace LandmarkIT.Enterprise.CommunicationManager.Operations
{
    public class ServiceRegistration : BaseService
    {
        public RegistrationResponse RegisterMail(RegisterMailRequest request)
        {
            try
            {
                var dbRequest = new MailRegistrationDb
                {
                    Guid = Guid.NewGuid(),
                    Name = request.Name==null?"":request.Name,
                    Host = request.Host,
                    APIKey = request.APIKey,
                    UserName = request.UserName,
                    Password = request.Password,
                    Port = request.Port,
                    IsSSLEnabled = true,
                    MailProviderID = request.MailProviderID,
                    VMTA=request.VMTA,
                    MailChimpListID=request.MailChimpListID,
                    SenderDomain=request.SenderDomain,
                    ImageDomain=request.ImageDomain
                };

                unitOfWork.MailRegistrationsRepository.Add(dbRequest);
                unitOfWork.Commit();
                return new RegistrationResponse { Token = dbRequest.Guid };
            }
            catch (Exception ex)
            {
                Logger.Current.Error("Exception occurred while fetching the campaign templates.", ex);
                return null;
            }
        }
        public RegistrationResponse UpdateMailRegistration(RegisterMailRequest request)
        {
          //  var db = ObjectContextFactory.Create();
            var mailRegistration = unitOfWork.MailRegistrationsRepository.Single(mr => mr.Guid == request.RequestGuid);
            mailRegistration.APIKey = request.APIKey;
            mailRegistration.Host = request.Host;
            mailRegistration.MailProviderID = request.MailProviderID;
            mailRegistration.Password = request.Password;
            mailRegistration.UserName = request.UserName;
            mailRegistration.Name = request.Name;
            mailRegistration.VMTA = request.VMTA;
            mailRegistration.SenderDomain = request.SenderDomain;
            mailRegistration.ImageDomain = request.ImageDomain;
            mailRegistration.Port = request.Port;
            mailRegistration.MailChimpListID = request.MailChimpListID;
            unitOfWork.Commit();
            return new RegistrationResponse { Token = mailRegistration.Guid };
        }
        public RegistrationResponse UpdateTextRegistration(RegisterTextRequest request)
        {
            //  var db = ObjectContextFactory.Create();
            var textRegistration = unitOfWork.TextRegistrationsRepository.Single(mr => mr.Guid == request.RequestGuid);
            textRegistration.APIKey = request.Key;
            textRegistration.Token = request.Token;
            textRegistration.TextProviderID = request.TextProviderID;
            textRegistration.Password = request.Password;
            textRegistration.UserName = request.UserName;
            textRegistration.Name = request.Name;
            unitOfWork.Commit();
            return new RegistrationResponse { Token = textRegistration.Guid };
        }


        public RegistrationResponse RegisterText(RegisterTextRequest request)
        {
            var dbRequest = new TextRegistrationDb
            {
                Guid = Guid.NewGuid(),
                Name = request.Name,
                Address = request.Address,
                UserName = request.UserName,
                Password = request.Password,
                APIKey = request.Key,
                Token = request.Token,
                TextProviderID = request.TextProviderID
            };
            unitOfWork.TextRegistrationsRepository.Add(dbRequest);
            unitOfWork.Commit();
            return new RegistrationResponse { Token = dbRequest.Guid };
        }

        public RegistrationResponse RegisterFtp(RegisterFtpRequest request)
        {
            var dbRequest = new FtpRegistrationDb
            {
                Guid = Guid.NewGuid(),
                Host = request.Host,
                UserName = request.UserName,
                Password = request.Password,
                Port = request.Port,
            };
            unitOfWork.FtpRegistrationsRepository.Add(dbRequest);
            unitOfWork.Commit();
            return new RegistrationResponse { Token = dbRequest.Guid };
        }
    }
}

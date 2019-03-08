using AutoMapper;
using SmartTouch.CRM.ApplicationServices.Messaging.ThirdPartyClient;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.Login;
using SmartTouch.CRM.Domain.ThirdPartyAuthentication;
using System;
using System.Collections.Generic;

namespace SmartTouch.CRM.ApplicationServices.ServiceImplementations
{
    public class ThirdPartyClientService: IThirdPartyClientService
    {
       readonly IThirdPartyAuthenticationRepository thirdPartyClientRepository;
       //readonly IUnitOfWork unitOfWork;

       public ThirdPartyClientService(IThirdPartyAuthenticationRepository thirdPartyClientRepository)
       {
           if (thirdPartyClientRepository == null) throw new ArgumentNullException("thirdPartyClientRepository");
           //if (unitOfWork == null) throw new ArgumentNullException("unitOfWork");

           this.thirdPartyClientRepository = thirdPartyClientRepository;
           //this.unitOfWork = unitOfWork;
       }

       public GetThirdPartyClientResponse GetAllThirdPartyClients(GetThirdPartyClientRequest request)
       {
           GetThirdPartyClientResponse response = new GetThirdPartyClientResponse();
           IEnumerable<ThirdPartyClient> thirdPartyClients = thirdPartyClientRepository.GetAllThirdPartyClients(request.Name, request.Filter);

           IEnumerable<ThirdPartyClientViewModel> thirdPartyClientsList = Mapper.Map<IEnumerable<ThirdPartyClient>, IEnumerable<ThirdPartyClientViewModel>>(thirdPartyClients);
           response.ThirdPartyClientViewModel = thirdPartyClientsList;
           return response;

       }
      
       public InsertThirdPartyClientResponse  AddThirdPartyClient(InsertThirdPartyClientRequest request)
       {
           InsertThirdPartyClientResponse response = new InsertThirdPartyClientResponse();
           ThirdPartyClient thirdPartyClient = Mapper.Map<ThirdPartyClientViewModel, ThirdPartyClient>(request.ThirdPartyClientViewModel);
           thirdPartyClientRepository.AddThirdPartyClient(thirdPartyClient);
           return response;
       }

       public GetApiKeyByIDResponse GetApiKeyByID(GetApiKeyByIDRequest request)
       {
           GetApiKeyByIDResponse response = new GetApiKeyByIDResponse();
           ThirdPartyClient thirdPartyClients = thirdPartyClientRepository.GetApiKeyByID(request.ID);

           ThirdPartyClientViewModel thirdPartyClientsList = Mapper.Map<ThirdPartyClient, ThirdPartyClientViewModel>(thirdPartyClients);
           response.ThirdPartyClientViewModel = thirdPartyClientsList;
           return response;
       }


       public UpdateApiKeyResponse UpdateApiKey(UpdateApiKeyRequest request)
       {
           UpdateApiKeyResponse response = new UpdateApiKeyResponse();
           ThirdPartyClient thirdPartyClient = Mapper.Map<ThirdPartyClientViewModel, ThirdPartyClient>(request.ThirdPartyClientViewModel);
           thirdPartyClientRepository.UpdateThirdPartyClient(thirdPartyClient);
           return response;
       }


       public DeleteApiKeyResponse DeleteApiKey(DeleteApiKeyRequest request)
       {
           DeleteApiKeyResponse response = new DeleteApiKeyResponse();
           ThirdPartyClient thirpartyClient = Mapper.Map<ThirdPartyClientViewModel, ThirdPartyClient>(request.ThirdPartyClientViewModel);
           thirdPartyClientRepository.DeleteThirdPartyClient(thirpartyClient);
           return response;
       }

       public ThirdPartyClient GetThirdPartyClientIDByAccountName(string accountName)
       {
            return thirdPartyClientRepository.GetCLientSecretKeyByAccountName(accountName);
       }
        public ThirdPartyClient GetCLientSecretKeyByDomainName(string accountName)
        {
            return thirdPartyClientRepository.GetCLientSecretKeyByDomainName(accountName);
        }
    }
}

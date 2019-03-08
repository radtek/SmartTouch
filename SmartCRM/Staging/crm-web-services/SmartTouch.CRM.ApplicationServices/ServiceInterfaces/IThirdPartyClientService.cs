using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.ApplicationServices.Messaging.ThirdPartyClient;

namespace SmartTouch.CRM.ApplicationServices.ServiceInterfaces
{
    public interface IThirdPartyClientService
    {
        InsertThirdPartyClientResponse AddThirdPartyClient(InsertThirdPartyClientRequest request);
        GetThirdPartyClientResponse GetAllThirdPartyClients(GetThirdPartyClientRequest request);
        GetApiKeyByIDResponse GetApiKeyByID(GetApiKeyByIDRequest request);
        UpdateApiKeyResponse UpdateApiKey(UpdateApiKeyRequest request);
        DeleteApiKeyResponse DeleteApiKey(DeleteApiKeyRequest request);
    }
}

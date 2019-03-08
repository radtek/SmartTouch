using SmartTouch.CRM.Domain.Login;
using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.ThirdPartyAuthentication
{
    public interface IThirdPartyAuthenticationRepository : IRepository<ThirdPartyClient, string>
    {
        ClientRefreshToken GetRefreshToken(string Id);
        bool AddRefreshToken(ClientRefreshToken token);
        ClientRefreshToken FindRefreshToken(string token);
        bool RemoveRefreshToken(string token);
        void AddThirdPartyClient(ThirdPartyClient thirdPartyClient);
        void UpdateThirdPartyClient(ThirdPartyClient thirdPartyClient);
        IEnumerable<ThirdPartyClient> GetAllThirdPartyClients(string Name, string Filter);
        ThirdPartyClient GetApiKeyByID(string apiKeyID);
        void DeleteThirdPartyClient(ThirdPartyClient thirdPartyClient);

        ThirdPartyClient GetCLientSecretKeyByAccountName(string accountName);
        ThirdPartyClient GetCLientSecretKeyByDomainName(string accountName);

    }
}

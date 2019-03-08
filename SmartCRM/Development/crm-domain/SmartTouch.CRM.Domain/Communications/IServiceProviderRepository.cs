using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Communication
{
    public interface IServiceProviderRepository : IRepository<ServiceProvider, int>
    {
        ServiceProvider GetServiceProviders(int accountID, CommunicationType communicationType, MailType mailprovider);
        IEnumerable<ServiceProvider> GetAccountCommunicationProviders(int accountId, CommunicationType communicationType,MailType mailProvider);
        IEnumerable<Guid> GetEmailProviderTokens(int accountId);
        ServiceProvider GetSendTextServiceProviders(int accountId, CommunicationType communicationType);
        IEnumerable<ServiceProvider> AccountServiceProviders(int accountId);
        IEnumerable<ServiceProvider> AccountServiceProviders(int accountId, bool fromCache);
        ServiceProvider GetDefaultCampaignProvider(int accountId);
        ServiceProvider GetCampaignProvider(int accountId);
        Dictionary<Guid, string> GetTransactionalProviderDetails(int accountId);
        Email InsertServiceproviderEmail(ServiceProvider serviceprovider,  String email);
        Email UpdateServiceproviderEmail(ServiceProvider serviceprovider, String email);
        Email GetServiceProviderEmail(int serviceProviderId);
        ServiceProvider GetServiceProviderById(int accountId, int serviceProviderId);
        int FindByName(string name, int accountId);
        ServiceProvider GetAutomationCampaignProvider(Guid guid);
    }
}

using SmartTouch.CRM.ApplicationServices.Messaging.Communication;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ServiceInterfaces
{
    public interface ICommunicationProviderService
    {
        ServiceProviderResponse SaveServiceProvider(ServiceProviderRequest request);
        ServiceProviderResponse GetServiceProvider(ServiceProviderRequest request);
        GetServiceProviderResponse GetAccountServiceProviders(GetServiceProviderRequest request);
        ServiceProvider ServiceproviderToSendTransactionEmails(int accountID);
        ServiceProviderViewModel ServiceproviderToSendText(int accountID);
    }
}

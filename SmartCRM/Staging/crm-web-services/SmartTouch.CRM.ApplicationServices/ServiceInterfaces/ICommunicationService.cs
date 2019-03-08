using SmartTouch.CRM.ApplicationServices.Messaging.Campaigns;
using SmartTouch.CRM.ApplicationServices.Messaging.Communication;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ServiceInterfaces
{
    public interface ICommunicationService
    {
        //CommunicationTrackerResponse SaveCommunicationTrackerRequest(CommunicationTrackerRequest request);
        CommunicationTrackerResponse GetFindByContactId(CommunicationTrackerRequest request);
        SendMailResponse SendMail(SendMailRequest request);
        ScheduleMailResponse ScheduleEmail(ScheduleMailRequest request);
        SendTextResponse SendText(SendTextRequest request);
        LandmarkIT.Enterprise.CommunicationManager.Responses.RegistrationResponse CommunicationProviderRegistration(CommunicationProviderRegistrationRequest request);
        GetCommunicatioProvidersResponse GetCommunicationProviders(GetCommunicatioProvidersRequest request);
        GetEmailProvidersResponse GetEmailProviders(GetEmailProvidersRequest request);
        GetDefaultCampaignEmailProviderResponse GetDefaultCampaignEmailProvider(GetDefaultCampaignEmailProviderRequest request);
        CampaignUnsubscribeResponse UpdateContactEmailStatus(CampaignUnsubscribeRequest request);
        SendTextResponse GetSendTextviewModel(SendTextRequest request);
        InsertServiceProviderResponse AddServiceprovider(InsertServiceProviderRequest request);
        GetServiceProviderByIdResponse GetEmailProviderById(GetServiceProviderByIdRequest request);
        byte? GetServiceProviders(int accountID);
        GetEmailBodyResponse GetEmailBody(GetEmailBodyRequest request);
        GetServiceProviderImageDomainResponse GetServiceProviderImageDomain(GetServiceProviderImageDomainRequest request);
    }
}

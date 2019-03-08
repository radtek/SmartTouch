using LandmarkIT.Enterprise.CommunicationManager.Operations;
using LandmarkIT.Enterprise.Utilities.Logging;
using SmartTouch.CRM.ApplicationServices.Messaging.Communication;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Entities;
using System;
using System.Configuration;

namespace SmartTouch.CRM.ApplicationServices.ServiceImplementations
{
    public class UrlService : IUrlService
    {
        string imagHostingUrl;
        readonly ICommunicationProviderService communicationService;

        public UrlService(ICommunicationProviderService communicationService)
        {
            this.imagHostingUrl = ConfigurationManager.AppSettings["IMAGE_HOSTING_SERVICE_URL"] ?? "";
            this.communicationService = communicationService;
        }

        public string GetUrl(int accountId, ImageCategory category, string imageStorageName)
        {
            return this.imagHostingUrl + "/" + accountId + "/" + shortenCategory(category) + "/" + imageStorageName;
        }

        public string GetUrl(ImageCategory category, string imageStorageName)
        {
            return this.imagHostingUrl + "/SmartTouch/" + shortenCategory(category) + "/" + imageStorageName;
        }

        public string GetImageHostingUrl()
        {
            return this.imagHostingUrl;
        }

        string shortenCategory(ImageCategory category)
        {
            if (category == ImageCategory.ContactProfile || category == ImageCategory.OpportunityProfile)
                return "pi";
            else if (category == ImageCategory.Campaigns || category == ImageCategory.CampaignTemplateThumbnails)
                return "ci";
            else if (category == ImageCategory.AccountLogo)
                return "ai";
            else
                throw new InvalidOperationException("Mentioned category of image is not defined." + category.ToString());
        }

        public string GetImageDomain(int accountId)
        {
            string imageDomain = string.Empty;

            Logger.Current.Informational("Request received for getting image-domain");
            ServiceProviderResponse response = communicationService.GetServiceProvider(new ServiceProviderRequest()
              {
                  ServiceProviderViewModel = new ServiceProviderViewModel() { CommunicationTypeID = CommunicationType.Mail, MailType = MailType.BulkEmail },
                  AccountId = accountId
              });
            if (response.ServiceProviderViewModel != null)
            {
                Guid loginToken = response.ServiceProviderViewModel.LoginToken;
                MailService mailService = new MailService();
                var defaultProvider = mailService.GetMailRegistrationDetails(loginToken);
                imageDomain = defaultProvider.ImageDomain;
            }
            return imageDomain;
        }
    }
}

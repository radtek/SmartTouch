using AutoMapper;
using LandmarkIT.Enterprise.Utilities.ExceptionHandling;
using LandmarkIT.Enterprise.Utilities.Logging;
using Microsoft.Web.Administration;
using SmartTouch.CRM.ApplicationServices.Messaging.ImageDomain;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.ImageDomains;
using SmartTouch.CRM.Infrastructure.Domain;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace SmartTouch.CRM.ApplicationServices.ServiceImplementations
{
    public class ImageDomainService : IImageDomainService
    {
        readonly IImageDomainRepository imageDomainRepository;
        readonly IUnitOfWork unitOfWork;

        public ImageDomainService(IImageDomainRepository imageDomainRepository, IUnitOfWork unitOfWork)
        {
            this.imageDomainRepository = imageDomainRepository;
            this.unitOfWork = unitOfWork;
        }

        public GetImageDomainsResponse GetImageDomains(GetImageDomainsRequest request)
        {
            GetImageDomainsResponse response = new GetImageDomainsResponse();
            IEnumerable<ImageDomain> imageDomains = imageDomainRepository.FindAll(request.Query, request.Limit, request.PageNumber, request.Status);
            IEnumerable<ImageDomainViewModel> imageDomainViewModels = Mapper.Map<IEnumerable<ImageDomain>, IEnumerable<ImageDomainViewModel>>(imageDomains);
            response.ImageDomains = imageDomainViewModels;
            response.TotalHits = imageDomainRepository.GetImageDomainsCount();
            return response;
        }

        public GetImageDomainsResponse GetActiveImageDomains(GetImageDomainsRequest request)
        {
            GetImageDomainsResponse response = new GetImageDomainsResponse();
            IEnumerable<ImageDomain> imageDomains = imageDomainRepository.GetActiveImageDomains();
            response.ImageDomains = Mapper.Map<IEnumerable<ImageDomain>, IEnumerable<ImageDomainViewModel>>(imageDomains);

            return response;
        }

        public InsertImageDomainResponse InsertImageDomain(InsertImageDomainRequest request)
        {
            InsertImageDomainResponse response = new InsertImageDomainResponse();
            ImageDomain imageDomain = Mapper.Map<ImageDomainViewModel, ImageDomain>(request.ImageDomainViewModel);
            isValidImageDomain(imageDomain);
            bool isDuplicate = imageDomainRepository.IsDuplicateImageDomain(imageDomain);
            if (isDuplicate)
            {
                var message = "[|Image domain |] \"" + imageDomain.Domain + "\" [|already exists. Please choose a different domain.|]";
                throw new UnsupportedOperationException(message);
            }
            imageDomainRepository.Insert(imageDomain);
            ImageDomain newImageDomain = unitOfWork.Commit() as ImageDomain;
            ImageDomainViewModel imageDomainViewModel = Mapper.Map<ImageDomain, ImageDomainViewModel>(newImageDomain);
            AddImageDomainBinding(imageDomain.Domain);
            response.ImageDomainViewModel = imageDomainViewModel;
            return response;

        }

        public UpdateImageDomainResponse UpdateImageDomain(UpdateImageDomainRequest request)
        {
            UpdateImageDomainResponse response = new UpdateImageDomainResponse();
            Logger.Current.Verbose("Request received to update image domain with id " + request.ImageDomainViewModel.ImageDomainId);
            request.ImageDomainViewModel.LastModifiedBy = request.RequestedBy;
            request.ImageDomainViewModel.LastModifiedOn = DateTime.Now.ToUniversalTime();
            if (request.ImageDomainViewModel.Status == false)
            {
                bool isInvolvedInVMTA = imageDomainRepository.IsConfiguredWithVMTA(request.ImageDomainViewModel.ImageDomainId);
                if (isInvolvedInVMTA)
                    throw new UnsupportedOperationException("[|This Image Domain is being used by other accounts and could not be inactivated.|]");
            }

            response.ImageDomainViewModel = updateImageDomain(request.ImageDomainViewModel);
            return response;
        }

        ImageDomainViewModel updateImageDomain(ImageDomainViewModel imageDomainViewModel)
        {
            ImageDomain imageDomain = Mapper.Map<ImageDomainViewModel, ImageDomain>(imageDomainViewModel);
            isValidImageDomain(imageDomain);
            bool isDuplicate = imageDomainRepository.IsDuplicateImageDomain(imageDomain);
            if (isDuplicate)
            {
                var message = "[|Image domain |] \"" + imageDomain.Domain + "\" [|already exists. Please choose a different domain.|]";
                throw new UnsupportedOperationException(message);
            }

            imageDomainRepository.Update(imageDomain);
            ImageDomain updatedImageDomain = unitOfWork.Commit() as ImageDomain;

            Logger.Current.Informational("Image domain updated successfully.");
            AddImageDomainBinding(imageDomain.Domain);
            return Mapper.Map<ImageDomain, ImageDomainViewModel>(updatedImageDomain);
        }

        public GetImageDomainResponse GetImageDomain(GetImageDomainRequest request)
        {
            GetImageDomainResponse response = new GetImageDomainResponse();
            ImageDomain imageDomain = imageDomainRepository.GetImageDomain(request.ImageDomainId);
            if (imageDomain != null)
            {
                response.ImageDomainViewModel = Mapper.Map<ImageDomain, ImageDomainViewModel>(imageDomain);
            }
            return response;
        }

        void isValidImageDomain(ImageDomain imageDomain)
        {
            IEnumerable<BusinessRule> brokenRules = imageDomain.GetBrokenRules();

            if (brokenRules.Any())
            {
                StringBuilder brokenRulesBuilder = new StringBuilder();
                foreach (BusinessRule rule in brokenRules.Distinct())
                {
                    brokenRulesBuilder.AppendLine(rule.RuleDescription);
                }

                throw new UnsupportedOperationException(brokenRulesBuilder.ToString());
            }
        }

        public DeleteImageDomainResponse DeleteImageDomain(DeleteImageDomainRequest request)
        {
            DeleteImageDomainResponse response = new DeleteImageDomainResponse();
            bool isConfiguredWithVMTA = imageDomainRepository.IsConfiguredWithVMTA(request.ImageDomainId);
            if (isConfiguredWithVMTA)
                throw new UnsupportedOperationException("[|This Image Domain is being used by other accounts and could not be deleted.|]");
            imageDomainRepository.DeleteImageDomain(request.ImageDomainId);
            return response;
        }

        void AddImageDomainBinding(string domainName)
        {
            string smartTouchImageSiteName = System.Configuration.ConfigurationManager.AppSettings["SmarttouchImageSiteName"];
            string sslKey = System.Configuration.ConfigurationManager.AppSettings["SSLSerialNumber"];

            var port = 80;

            var serverManager = new ServerManager();
            var smartTouchImageSite = serverManager.Sites[smartTouchImageSiteName];
            string domainNameWithOutProtocol = domainName.Replace("http://", "").Replace("https://", "").Replace("www.", "");
            try
            {
                smartTouchImageSite.Bindings.Add(string.Format("*:{0}:{1}", port, domainNameWithOutProtocol), "http");
                Logger.Current.Informational("Added http binding successfully.");
                smartTouchImageSite.ServerAutoStart = false;
                serverManager.CommitChanges();
            }
            catch (Exception ex)
            {
                Logger.Current.Error("Error Logging While Adding Image Domain:" + domainName, ex);
            }

            bool isHttpsMode = false;//domainName.Contains("https");

            if (isHttpsMode)
            {
                X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
                store.Open(OpenFlags.ReadOnly);

                var certificate = store.Certificates.Find(X509FindType.FindBySerialNumber, sslKey, false).OfType<X509Certificate>().FirstOrDefault();
                if (certificate != null)
                {
                    Logger.Current.Informational("certificate found");
                }
                else
                {
                    Logger.Current.Informational("certificate not found");
                    foreach (X509Certificate2 objCert in store.Certificates)
                    {
                        string serialNumber = objCert.SerialNumber.Trim().ToString().ToUpper();
                        Logger.Current.Verbose("Certificate name" + objCert.FriendlyName + " Store serial number:" + objCert.SerialNumber.Trim());
                        string orgSerialNumber = sslKey.Trim().ToString().ToUpper();
                        if (String.Equals(serialNumber, orgSerialNumber, StringComparison.InvariantCulture))
                        {
                            certificate = objCert;
                        }
                    }
                    if (certificate != null)
                    {
                        Logger.Current.Informational("Certificate found.");
                    }
                    else
                        Logger.Current.Informational("Certificate not found.");
                }
                if (certificate != null)
                {
                    smartTouchImageSite.Bindings.Add("*:443:" + domainNameWithOutProtocol, certificate.GetCertHash(), store.Name);
                    Logger.Current.Informational("Added https binding successfully.");
                }
                else
                    Logger.Current.Informational("Https binding unsuccessful.");

                store.Close();
                smartTouchImageSite.ServerAutoStart = false;
                serverManager.CommitChanges();
                Logger.Current.Informational("Image Domain creation was successful for https protocol");

            }

        }
    }
}

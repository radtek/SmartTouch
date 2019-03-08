using AutoMapper;
using LandmarkIT.Enterprise.Utilities.Logging;
using SmartTouch.CRM.ApplicationServices.Messaging.Communication;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.Communication;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using System;
using System.Collections.Generic;

namespace SmartTouch.CRM.ApplicationServices.ServiceImplementations
{
    public class CommunicationProviderService : ICommunicationProviderService
    {
        readonly IServiceProviderRepository serviceProviderRepository;
        readonly IUnitOfWork unitOfWork;
        public CommunicationProviderService(IServiceProviderRepository serviceProviderRepository, IUnitOfWork unitOfWork)
        {
            if (serviceProviderRepository == null) throw new ArgumentNullException("serviceProviderRepository");
            if (unitOfWork == null) throw new ArgumentNullException("unitOfWork");
            this.serviceProviderRepository = serviceProviderRepository;
            this.unitOfWork = unitOfWork;
        }

        public ServiceProviderResponse SaveServiceProvider(ServiceProviderRequest request)
        {
            Logger.Current.Verbose("Request to insert communicationlogindetails for specific contact.");

            ServiceProvider serviceProviders = Mapper.Map<ServiceProviderViewModel, ServiceProvider>(request.ServiceProviderViewModel);
            serviceProviderRepository.Insert(serviceProviders);
            unitOfWork.Commit();
            return new ServiceProviderResponse();
        }

        public ServiceProviderResponse GetServiceProvider(ServiceProviderRequest request)
        {
            Logger.Current.Verbose("Request to fetch communication logindetails for specific contact.");
            ServiceProviderResponse response = new ServiceProviderResponse();
             ServiceProvider serviceProvider = serviceProviderRepository.GetServiceProviders(Convert.ToInt16(request.ServiceProviderViewModel.AccountID), request.ServiceProviderViewModel.CommunicationTypeID, request.ServiceProviderViewModel.MailType);
            if (serviceProvider != null)
            {
                ServiceProviderViewModel ServiceProviderViewModel = Mapper.Map<ServiceProvider, ServiceProviderViewModel>(serviceProvider as ServiceProvider);
                response.ServiceProviderViewModel = ServiceProviderViewModel;
            }

            return response;
        }

        public GetServiceProviderResponse GetAccountServiceProviders(GetServiceProviderRequest request)
        {
            GetServiceProviderResponse response = new GetServiceProviderResponse();
            Logger.Current.Verbose("Request to fetch communication logindetails for specific account.");
             IEnumerable<ServiceProvider> serviceProviders = serviceProviderRepository.GetAccountCommunicationProviders(request.AccountId, request.CommunicationTypeId, request.MailType);
            response.ServiceProviderViewModel = Mapper.Map<IEnumerable<ServiceProvider>, IEnumerable<ServiceProviderViewModel>>(serviceProviders);
            return response;
        }

        public ServiceProvider ServiceproviderToSendTransactionEmails(int accountID)
        {
            ServiceProvider serviceProvider = serviceProviderRepository.GetServiceProviders(accountID, CommunicationType.Mail, MailType.TransactionalEmail);
            return serviceProvider;
        }
        public ServiceProviderViewModel ServiceproviderToSendText(int accountID)
        {
            ServiceProvider serviceProvider = serviceProviderRepository.GetSendTextServiceProviders(accountID, CommunicationType.Text);

            ServiceProviderViewModel viewmodel = Mapper.Map<ServiceProvider, ServiceProviderViewModel>(serviceProvider);
            return viewmodel;
        }
    }
}

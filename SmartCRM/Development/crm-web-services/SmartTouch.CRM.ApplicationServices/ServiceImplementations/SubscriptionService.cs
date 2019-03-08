using AutoMapper;
using LandmarkIT.Enterprise.Extensions;
using SmartTouch.CRM.ApplicationServices.Messaging.Subscriptions;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.Subscriptions;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartTouch.CRM.ApplicationServices.ServiceImplementations
{
    public class SubscriptionService : ISubscriptionService
    {
        readonly ISubscriptionRepository subscriptionRepository;
        readonly IUnitOfWork unitOfWork;

        public SubscriptionService(ISubscriptionRepository subscriptionRepository, IUnitOfWork unitOfWork)
        {
            if (subscriptionRepository == null) throw new ArgumentNullException("subscriptionRepository");
            if (unitOfWork == null) throw new ArgumentNullException("unitOfWork");

            this.subscriptionRepository = subscriptionRepository;
            this.unitOfWork = unitOfWork;
        }

        public GetAllAccountSubscriptionTypesResponse GetAllAccountsSubscriptionTypes(GetAllAccountSubscriptionTypesRequest request)
        {
            GetAllAccountSubscriptionTypesResponse response = new GetAllAccountSubscriptionTypesResponse();
            IEnumerable<Subscription> subscriptions = subscriptionRepository.GetAllSubscriptions();

            IEnumerable<SubscriptionViewModel> subscriptionsList = MapDomainToVM(subscriptions);
            response.subscriptionViewModel = subscriptionsList;
            return response;
        }

        private IEnumerable<SubscriptionViewModel> MapDomainToVM(IEnumerable<Subscription> subscriptionDomain)
        {
            List<SubscriptionViewModel> subscriptions = new List<SubscriptionViewModel>();
            if (subscriptionDomain.IsAny())
            {
                foreach (var subscription in subscriptionDomain)
                {
                    SubscriptionViewModel details = new SubscriptionViewModel();
                    details.SubscriptionID = subscription.SubscriptionID;
                    details.SubscriptionName = subscription.SubscriptionName;
                    subscriptions.Add(details);
                }
            }
            return subscriptions;
        }
    }
}

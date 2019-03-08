using AutoMapper;
using SmartTouch.CRM.ApplicationServices.Messaging.AccountUnsubscribeView;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.AccountSettings;
using System;

namespace SmartTouch.CRM.ApplicationServices.ServiceImplementations
{
    public class AccountSettingsService : IAccountSettingsService
    {
        readonly IAccountSettingsRepository accountUnsubscribeViewRepository;
        public AccountSettingsService(IAccountSettingsRepository accountUnsubscribeViewRepository)
       {
           if (accountUnsubscribeViewRepository == null) throw new ArgumentNullException("accountUnsubscribeViewRepository");
           //if (unitOfWork == null) throw new ArgumentNullException("unitOfWork");

           this.accountUnsubscribeViewRepository = accountUnsubscribeViewRepository;
           //this.unitOfWork = unitOfWork;
       }

        public GetAccountUnsubscribeViewByAccountIdResponse GetAccountUnsubscribeView(GetAccountUnsubscribeViewByAccountIdRequest request)
        {
            GetAccountUnsubscribeViewByAccountIdResponse response = new GetAccountUnsubscribeViewByAccountIdResponse();
            AccountSettings accountUnsubcribeViewMap = accountUnsubscribeViewRepository.GetAccountUnsubcribeViewByAccountId(request.AccountID);
            AccountSettingsViewModel accountUnsubcribeView = Mapper.Map<AccountSettings, AccountSettingsViewModel>(accountUnsubcribeViewMap);
            response.accountUnsubscribeViewMap = accountUnsubcribeView;
            return response;
        }
    }
}

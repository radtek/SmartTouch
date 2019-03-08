using SmartTouch.CRM.ApplicationServices.ObjectMappers;
using AutoMapper;
using LandmarkIT.Enterprise.Utilities.ExceptionHandling;
using LandmarkIT.Enterprise.Utilities.Logging;
using SmartTouch.CRM.ApplicationServices.Messaging.Communication;
using SmartTouch.CRM.ApplicationServices.Messaging.Contacts;
using SmartTouch.CRM.ApplicationServices.Messaging.User;
using SmartTouch.CRM.ApplicationServices.Messaging.Accounts;
using SmartTouch.CRM.ApplicationServices.Messaging.WebAnalytics;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.Accounts;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.Roles;
using SmartTouch.CRM.Domain.Tours;
using SmartTouch.CRM.Domain.Users;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Domain.WebAnalytics;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.Domain;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DA = SmartTouch.CRM.Domain.Actions;
using SmartTouch.CRM.ApplicationServices.Messaging.ImplicitSync;
using SmartTouch.CRM.Repository.Database;
using LandmarkIT.Enterprise.CommunicationManager.Operations;
using System.Diagnostics;
using SmartTouch.CRM.Domain.Communication;
using System.Configuration;
using SmartTouch.CRM.ApplicationServices.ServiceAgents;
using LandmarkIT.Enterprise.Extensions;
using SmartTouch.CRM.Domain.Workflows;

namespace SmartTouch.CRM.ApplicationServices.ServiceImplementations
{
    public class UserService : IUserService
    {
        readonly ICachingService cachingService;
        readonly IUserRepository userRepository;
        readonly IUserSettingsRepository userSettingsRepository;
        readonly IUserActivitiesRepository userActivitiesRepository;
        readonly ITourRepository tourRepository;
        readonly DA.IActionRepository actionRepository;
        readonly IUnitOfWork unitOfWork;
        readonly IContactRepository contactRepository;
        readonly IWebAnalyticsProviderRepository webAnalyticsRepository;
        readonly IAccountRepository accountRepository;
        readonly IServiceProviderRepository serviceProviderRepository;
        readonly IMessageRepository messageRepository;

        public UserService(IUserRepository userRepository, IAccountRepository accountRepository,
            IUserSettingsRepository userSettingsRepository, ITourRepository tourRepository,
            DA.IActionRepository actionRepository, IUnitOfWork unitOfWork, IUserActivitiesRepository userActivitiesRepository
            , IContactRepository contactRepository, ICachingService cachingService, IWebAnalyticsProviderRepository webAnalyticsRepository
            , IServiceProviderRepository serviceProviderRepository, IMessageRepository messageRepository)
        {
            if (userRepository == null) throw new ArgumentNullException("userRepository");
            if (unitOfWork == null) throw new ArgumentNullException("unitOfWork");
            if (accountRepository == null) throw new ArgumentNullException("accountRepository");

            this.userRepository = userRepository;
            this.unitOfWork = unitOfWork;
            this.userSettingsRepository = userSettingsRepository;
            this.userActivitiesRepository = userActivitiesRepository;
            this.tourRepository = tourRepository;
            this.actionRepository = actionRepository;
            this.contactRepository = contactRepository;
            this.cachingService = cachingService;
            this.webAnalyticsRepository = webAnalyticsRepository;
            this.accountRepository = accountRepository;
            this.serviceProviderRepository = serviceProviderRepository;
            this.messageRepository = messageRepository;
        }

        public GetUserListResponse GetAllUsers(GetUserListRequest request)
        {
            Logger.Current.Verbose("Request for fetching all users");
            GetUserListResponse response = new GetUserListResponse();
            IEnumerable<User> users = userRepository.FindAll(request.Query, request.Limit, request.PageNumber, request.Status, request.Role, request.AccountID, request.IsSTAdmin);
            if (users == null)
                response.Exception = GetUserNotFoundException();
            else
            {
                IEnumerable<UserViewModel> list = Mapper.Map<IEnumerable<User>, IEnumerable<UserViewModel>>(users);
                response.Users = list;
                response.TotalHits = userRepository.FindAll(request.Query, request.Status, request.Role, request.AccountID, request.IsSTAdmin).Count();
                
            }
            return response;
        }

        public CheckIfLimitExceededResponse CheckUserLimit(CheckIfLimitExceededRequest request)
        {
            CheckIfLimitExceededResponse response = new CheckIfLimitExceededResponse();
            AccountSubscriptionData subscriptiondata = accountRepository.GetSubscriptionData(request.AccountId);
            if (subscriptiondata.Limit.HasValue)
            {
                int usercount = userRepository.GetUsersLimit(request.AccountId);
                if (usercount >= subscriptiondata.Limit)
                    response.IsLimitExceeded = true;
            }
            return response;
        }

        public GetUserListResponse GetUsers(GetUserListRequest request)
        {
            Logger.Current.Verbose("Request for fetching all users of account");
            Logger.Current.Informational("Request for fetching all users of an account ", request.AccountID.ToString());
            GetUserListResponse response = new GetUserListResponse();
            IEnumerable<User> users = userRepository.GetUsers(request.AccountID, request.IsSTAdmin);
            if (users == null)
            {
                response.Exception = GetUserNotFoundException();
            }
            else
            {
                IEnumerable<UserViewModel> list = Mapper.Map<IEnumerable<User>, IEnumerable<UserViewModel>>(users);
                response.Users = list;
            }
            return response;
        }

        public GetUserRoleResponse GetUserRole(GetUserRoleRequest request)
        {
            Logger.Current.Verbose("Request for fetching user's role");
            Logger.Current.Informational("Request for fetching user's role", request.UserId.ToString());
            Role role = userRepository.GetUserRole(request.UserId);
            return new GetUserRoleResponse() { Role = Mapper.Map<Role, RoleViewModel>(role) };
        }

        public GetCurrencyResponse GetCurrencies(GetCurrencyRequest request)
        {
            Logger.Current.Verbose("Request for getting the currencies for user");
            GetCurrencyResponse response = new GetCurrencyResponse();
            IEnumerable<dynamic> Currencies;
            Currencies = userRepository.GetCurrencies().ToList();
            if (Currencies == null)
                throw new UnsupportedOperationException("The requested dateformats list was not found.");
            response.Currencies = Currencies;
            return response;
        }

        public GetDateFormatResponse GetDateFormats(GetDateFormatRequest request)
        {
            Logger.Current.Verbose("Request for getting the date formats for user");
            GetDateFormatResponse response = new GetDateFormatResponse();
            IEnumerable<dynamic> DateFormats;
            DateFormats = userRepository.GetDateFormats().ToList();
            if (DateFormats == null)
                throw new UnsupportedOperationException("The requested dateformats list was not found.");
            response.DateFormats = DateFormats;
            return response;
        }

        public GetEmailResponse GetEmails(GetEmailRequest request)
        {
            GetEmailResponse response = new GetEmailResponse();
            List<Email> emails;
            var accountId = request.accountId;
            var userId = request.userId;
            emails = userRepository.GetEmail(accountId, userId).ToList();
            if (emails == null)
                throw new UnsupportedOperationException("The requested Email list was not found.");
            response.Emails = emails;
            return response;
        }

        public GetEmailResponse CampaignGetEmails(GetEmailRequest request)
        {
            GetEmailResponse response = new GetEmailResponse();
            List<Email> emails;
            var accountId = request.accountId;
            var userId = request.userId;
            var roleId = request.roleId;
            emails = userRepository.CampaignGetEmail(accountId, userId, roleId).ToList();
            if (emails == null)
                throw new UnsupportedOperationException("The requested email list was not found.");
            response.Emails = emails.Where(s => s.EmailId != null && s.EmailId != string.Empty).ToList();
            return response;
        }

        public InsertUserSettingsResponse InsertUserSettings(InsertUserSettingsRequest request)
        {
            Logger.Current.Verbose("Request for inserting an user");
            UserSettings setting = Mapper.Map<UserSettingsViewModel, UserSettings>(request.UserSettingsViewModel);
            isUserSettingValid(setting);
            bool isDuplicate = userSettingsRepository.IsDuplicateUserSetting(setting.AccountID, setting.UserID);

            if (isDuplicate)
            {
                Logger.Current.Verbose("updating user-settings for user ");
                Logger.Current.Informational("updating user-settings for user ", setting.UserID.ToString());
                userSettingsRepository.Update(setting);
                UserSettings newUserSettings = unitOfWork.Commit() as UserSettings;
                UserSettingsViewModel userSettingsViewModel = Mapper.Map<UserSettings, UserSettingsViewModel>(newUserSettings);
                InsertUserSettingsResponse userSettingsResponse = new InsertUserSettingsResponse();
                userSettingsResponse.UserSettingsViewModel = userSettingsViewModel;
                return userSettingsResponse;
            }
            else
            {
                Logger.Current.Verbose("inserting user-settings for user ");
                Logger.Current.Informational("inserting user-settings for user ", setting.UserID.ToString());
                userSettingsRepository.Insert(setting);
                UserSettings newUserSettings = unitOfWork.Commit() as UserSettings;
                UserSettingsViewModel userSettingsViewModel = Mapper.Map<UserSettings, UserSettingsViewModel>(newUserSettings);
                InsertUserSettingsResponse userSettingsResponse = new InsertUserSettingsResponse();
                userSettingsResponse.UserSettingsViewModel = userSettingsViewModel;
                return userSettingsResponse;
            }
        }

        public InsertUserResponse InsertUser(InsertUserRequest request)
        {
            Logger.Current.Verbose("Request for inserting an user");
            User user = Mapper.Map<UserViewModel, User>(request.UserViewModel);
            user.Status = Status.Inactive;
            isUserValid(user);
            var UserId = default(int);
            int.TryParse(user.Id, out UserId);
            bool isDuplicate = userRepository.IsDuplicateUser(user.Email.EmailId, UserId, user.AccountID);
            if (isDuplicate)
                throw new UnsupportedOperationException("[|User already exists.|]");

            AccountSubscriptionData subscriptiondata = accountRepository.GetSubscriptionData(request.UserViewModel.AccountID);
            if (subscriptiondata.Limit.HasValue && !userRepository.IsRoleExcludedFromLimit(request.UserViewModel.AccountID, request.UserViewModel.RoleID))
            {
                int usercount = userRepository.GetUsersLimit(request.UserViewModel.AccountID);
                if (usercount > subscriptiondata.Limit)
                {
                    UserLimitEmailNotification(request.UserViewModel.AccountID);
                    throw new UnsupportedOperationException("[|Maximum number of Users reached. Please contact Help Desk for assistance at helpdesk@smarttouchinteractive.com.|]");
                }
            }
            this.GetLocationsByZip(user.Addresses);
            userRepository.Insert(user);
            User newUser = unitOfWork.Commit() as User;
            UserViewModel userViewModel = Mapper.Map<User, UserViewModel>(newUser);
            InsertUserResponse userResponse = new InsertUserResponse();
            userResponse.UserViewModel = userViewModel;
            return userResponse;
        }

        public bool GetAccountSubscription(int accountId)
        {
            AccountSubscriptionData subscriptiondata = accountRepository.GetSubscriptionData(accountId);
            if (subscriptiondata.SubscriptionID == (int)AccountSubscription.BDX)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public UpdateUserResponse UpdateUser(UpdateUserRequest request)
        {
            Logger.Current.Verbose("Request for updating a user");
            if (request.UserViewModel.SocialMediaUrls.Where(p => p.MediaType == "Facebook").Count() > 1 ||
                request.UserViewModel.SocialMediaUrls.Where(p => p.MediaType == "Twitter").Count() > 1 ||
                request.UserViewModel.SocialMediaUrls.Where(p => p.MediaType == "Website").Count() > 1 ||
                request.UserViewModel.SocialMediaUrls.Where(p => p.MediaType == "LinkedIn").Count() > 1 ||
                request.UserViewModel.SocialMediaUrls.Where(p => p.MediaType == "Google+").Count() > 1 ||
                request.UserViewModel.SocialMediaUrls.Where(p => p.MediaType == "Blog").Count() > 1 ||
                request.UserViewModel.SocialMediaUrls.Where(p => p.MediaType == "Skype").Count() > 1
                )
            {
                throw new UnsupportedOperationException("[|Multiple web & social media URLs of similar type are not accepted.|]");
            };

            User user = Mapper.Map<UserViewModel, User>(updateViewModel(request.UserViewModel));
            isUserValid(user);
            var viewModel = request.UserViewModel;
            if (viewModel.Phones.Where(p => p.PhoneType == (short)DropdownValueTypes.Homephone).Count() > 1 ||
                viewModel.Phones.Where(p => p.PhoneType == (short)DropdownValueTypes.WorkPhone).Count() > 1 ||
                viewModel.Phones.Where(p => p.PhoneType == (short)DropdownValueTypes.MobilePhone).Count() > 1)
            {
                throw new UnsupportedOperationException("[|Multiple phone numbers of similar type are not accepted.|]");
            }

            List<string> emailsList = new List<string>();
            foreach (Email email in request.UserViewModel.Emails)
            {
                emailsList.Add(email.EmailId);
            }

            if (emailsList.Distinct().Count() != emailsList.Count)
                throw new UnsupportedOperationException("[|User can not have duplicate Emails|]");

            var UserId = default(int);
            int.TryParse(user.Id, out UserId);

            List<int> userIds = new List<int>();
            userIds.Add(UserId);

            AccountSubscriptionData subscriptiondata = accountRepository.GetSubscriptionData(request.AccountId);
            if (subscriptiondata.SubscriptionID == (int)AccountSubscription.BDX && user.Status == Status.Inactive)
            {
                bool isAccountAdminExists = userRepository.IsAccountAdminExistsInActiveUsers(userIds.ToArray(), request.AccountId);
                if (isAccountAdminExists == false)
                    throw new UnsupportedOperationException("[|Minimum one Active Account Administrator is required.|]");
            }

            if (subscriptiondata.Limit.HasValue && !userRepository.IsRoleExcludedFromLimit(request.AccountId, request.UserViewModel.RoleID) && user.Status == Status.Active &&
                userRepository.GetUserStatusByUserId(UserId, request.AccountId) == (byte)Status.Inactive)
            {
                int usercount = userRepository.GetUsersLimit(request.UserViewModel.AccountID);
                if (usercount >= subscriptiondata.Limit)
                {
                    UserLimitEmailNotification(request.UserViewModel.AccountID);
                    throw new UnsupportedOperationException("[|Maximum number of Users reached. Please contact Help Desk for assistance at helpdesk@smarttouchinteractive.com.|]");
                }
            }

            IEnumerable<Address> Addresses = user.Addresses;
            foreach (Address address in Addresses)
            {
                address.ApplyNation(contactRepository.GetTaxRateBasedOnZipCode(address.ZipCode));
            }
            //this.GetLocationsByZip(user.Addresses);
            userRepository.Update(user);
            unitOfWork.Commit();
            return new UpdateUserResponse();
        }

        IEnumerable<Address> GetLocationsByZip(IEnumerable<Address> addresses)
        {
            foreach (Address address in addresses)
            {
                if (string.IsNullOrEmpty(address.State.Code) && string.IsNullOrEmpty(address.Country.Code) && string.IsNullOrEmpty(address.City) && !string.IsNullOrEmpty(address.ZipCode))
                {
                    TaxRate taxRate = contactRepository.GetTaxRateBasedOnZipCode(address.ZipCode);
                    if (taxRate != null)
                    {
                        string countryCode = taxRate.CountryID == 1 ? "US" : "CA";
                        Country country = new Country()
                        {
                            Code = countryCode.Trim()
                        };
                        string stateCode = taxRate.CountryID == 1 ? "US-" + taxRate.StateCode : "CA-" + taxRate.StateCode;
                        State state = new State()
                        {
                            Code = stateCode.Trim()
                        };
                        address.City = taxRate.CityName;
                        address.State = state;
                        address.Country = country;
                    }
                }
            }
            return addresses;
        }

        UserViewModel updateViewModel(UserViewModel viewModel)
        {
            var emptyAddresses = viewModel.Addresses.Where(a => a.AddressID == 0
                && string.IsNullOrEmpty(a.AddressLine1) && string.IsNullOrEmpty(a.AddressLine2)
                && string.IsNullOrEmpty(a.City) && string.IsNullOrEmpty(a.ZipCode)
                && (a.Country == null || a.Country.Code.Equals("US") || a.Country.Code.Equals(""))
                && (a.State == null || a.State.Code.Equals("US-AK") || a.State.Code.Equals(""))).ToList();
            foreach (AddressViewModel addressViewModel in emptyAddresses)
            {
                viewModel.Addresses.Remove(addressViewModel);
            }
            return viewModel;
        }

        void isUserValid(User user)
        {
            IEnumerable<BusinessRule> brokenRules = user.GetBrokenRules();

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

        void isUserSettingValid(UserSettings userSettings)
        {
            IEnumerable<BusinessRule> brokenRules = userSettings.GetBrokenRules();

            if (brokenRules.Any())
            {
                StringBuilder brokenRulesBuilder = new StringBuilder();
                foreach (BusinessRule rule in brokenRules)
                {
                    brokenRulesBuilder.AppendLine(rule.RuleDescription);
                }

                throw new UnsupportedOperationException(brokenRulesBuilder.ToString());
            }
        }

        public GetRoleResponse GetRoles(GetRoleRequest request)
        {
            Logger.Current.Verbose("Request for getting roles of an Account");
            GetRoleResponse response = new GetRoleResponse();
            IEnumerable<dynamic> roles;

            roles = userRepository.GetRoles(request.AccountId).ToList();
            if (roles == null)
                throw new UnsupportedOperationException("Roles was not found for this Account. ");
            response.Roles = roles;
            response.SubscriptionId = accountRepository.GetSubscriptionIdByAccountId(request.AccountId);
            return response;
        }

        public GetUserSettingResponse GetUserSetting(GetUserSettingRequest request)
        {
            GetUserSettingResponse response = new GetUserSettingResponse();
            Logger.Current.Verbose("Request for fetching User-Setting details.");
            UserSettings usersetting = userRepository.FindUserSettingBy(request.AccountID, request.UserID);
            if (usersetting == null)
            {
                Logger.Current.Informational("No record exist in user-setting for user : " + request.UserID);
                response.UserSettingsViewModel = null;
            }
            else
            {
                UserSettingsViewModel userSettingsViewModel = Mapper.Map<UserSettings, UserSettingsViewModel>(usersetting);
                response.UserSettingsViewModel = userSettingsViewModel;
            }
            return response;
        }

        public GetUserResponse GetUser(GetUserRequest request)
        {
            GetUserResponse response = new GetUserResponse();
            Logger.Current.Verbose("Request received for getting user");
            User user = userRepository.FindBy(request.Id);
            if (user == null)
            {
                Logger.Current.Verbose("Requested user was not found");
                response.Exception = GetUserNotFoundException();
            }
            else
            {
                UserViewModel userViewModel = Mapper.Map<User, UserViewModel>(user);
                if (userViewModel.SocialMediaUrls == null || !userViewModel.SocialMediaUrls.Any())
                    userViewModel.SocialMediaUrls.Add(new { MediaType = "Facebook", Url = "" });
                response.User = userViewModel;
                Logger.Current.Verbose("Converting user from domain to userviewmodel is successful");
            }
            return response;
        }

        public GetUserResponse GetUserByUserName(GetUserRequest request)
        {
            Logger.Current.Verbose("Request received by UserService/GetUsesrByUserName method with parameters: " + request.DomainUrl + ", " + request.UserName);
            GetUserResponse response = new GetUserResponse();
            User user = userRepository.FindByEmailAndDomainUrl(request.UserName, request.DomainUrl);
            if (user != null)
                response.User = new UserViewModel()
                {
                    AccountID = user.AccountID,
                    UserID = int.Parse(user.Id),
                    Password = user.Password
                };
            return response;
        }

        public GetLoginInfoByUsernameResponse GetSuperAdminByEmail(GetLoginInfoByUsernameRequest request)
        {
            Logger.Current.Verbose("Request received by UserService/GetSuperAdminByEmail method with parameters: " + request.UserName);
            GetLoginInfoByUsernameResponse response = new GetLoginInfoByUsernameResponse();
            var user = userRepository.GetSuperAdminPasswordByEmail(request.UserName);
            response.UserInfo = user;
            return response;
        }

        private UnsupportedOperationException GetUserNotFoundException()
        {
            throw new UnsupportedOperationException("The requested user was not found.");
        }

        public UserStatusResponse UpdateUsersStatus(UserStatusRequest request)
        {
            Logger.Current.Verbose("Request for user change the status");

            AccountSubscriptionData subscriptiondata = accountRepository.GetSubscriptionData(request.AccountId);
            if (subscriptiondata.SubscriptionID == (int)AccountSubscription.BDX && request.Status == (byte)Status.Inactive)
            {
                bool isAccountAdminExists = userRepository.IsAccountAdminExistsInActiveUsers(request.UserID, request.AccountId);
                if (isAccountAdminExists == false)
                    throw new UnsupportedOperationException("[|Minimum one Active Account Administrator is required.|]");
            }
            if (subscriptiondata.Limit.HasValue && request.Status == 1) //Making user active
            {
                int usercount = userRepository.GetAllActiveUserIds(request.AccountId, request.UserID);
                if (usercount > subscriptiondata.Limit)
                {
                    UserLimitEmailNotification(request.AccountId);
                    throw new UnsupportedOperationException("[|Maximum number of Users reached. Please contact Help Desk for assistance at helpdesk@smarttouchinteractive.com.|]");
                }
            }
            userRepository.UpdateUserStatus(request.UserID, request.Status);

            return new UserStatusResponse();
        }

        public ChangeRoleResponse ChangeRole(ChangeRoleRequest request)
        {
            Logger.Current.Verbose("Request for user change the role");
            AccountSubscriptionData subscriptiondata = accountRepository.GetSubscriptionData(request.AccountId);
            if (subscriptiondata.Limit.HasValue && !userRepository.IsRoleExcludedFromLimit(request.AccountId, request.RoleID))
            {
                bool limitExceeded = userRepository.IsLimitReached(request.AccountId,request.UserID,request.RoleID, subscriptiondata.Limit.Value);
                if (limitExceeded)
                {
                    UserLimitEmailNotification(request.AccountId);
                    throw new UnsupportedOperationException("[|Maximum number of Users reached. Please contact Help Desk for assistance at helpdesk@smarttouchinteractive.com.|]");
                }
            }
            userRepository.ChangeRole(request.RoleID, request.UserID);

            return new ChangeRoleResponse();
        }

        public DeactivateUserResponse DeactivateUser(DeactivateUserRequest request)
        {
            Logger.Current.Verbose("Request for user deactivate");
            Logger.Current.Informational("UserId to deactivate :" + request.UserID);
            AccountSubscriptionData subscriptiondata = accountRepository.GetSubscriptionData(request.AccountId);
            if (subscriptiondata.SubscriptionID == (int)AccountSubscription.BDX && request.UserID.Length < subscriptiondata.Limit)
            {
                bool isAccountAdminExists = userRepository.IsAccountAdminExistsInUsers(request.UserID, request.AccountId);
                if (isAccountAdminExists == false)
                    throw new UnsupportedOperationException("[|Minimum one Account Administrator is required.|]");
            }
            userRepository.DeactivateUsers(request.UserID, (int)request.RequestedBy);
            return new DeactivateUserResponse();
        }

        public GetUserNotificationsResponse GetImpendingReminderNotifications(GetUserNotificationsRequest request)
        {
            Logger.Current.Verbose("Request for getting impending user notifications.");
            Logger.Current.Informational("UserId to get the impending notifications :" + string.Join(",", request.UserIds));
            IEnumerable<Notification> notifications = userRepository.GetImpendingReminderNotifications(request.UserIds);
            Logger.Current.Informational("Retreived Impending Notifications. Count: " + (notifications != null ? notifications.Count() : 0));
            return new GetUserNotificationsResponse() { Notifications = notifications };
        }

        public GetUserNotificationsResponse GetWebVisitNotifications(GetUserNotificationsRequest request)
        {
            Logger.Current.Informational("Request for getting recent web visits for User :" + string.Join(",", request.UserIds));

            IEnumerable<Notification> webVisitNotifications = userRepository.GetImpendingReminderWebVisitNotifications(request.UserIds, request.AccountId);
            Logger.Current.Informational("Retreived webVisitNotifications . Count: " + (webVisitNotifications != null ? webVisitNotifications.Count() : 0));

            return new GetUserNotificationsResponse() { WebVisitNotifications = webVisitNotifications };
        }

        public GetUserNotificationsResponse GetReminderNotifications(GetUserNotificationsRequest request)
        {
            Logger.Current.Verbose("Request for getting user notifications.");
            Logger.Current.Informational("UserId to get the notifications :" + string.Join(",", request.UserIds));
            IEnumerable<Notification> notifications = userRepository.GetReminderNotifications(request.UserIds, request.ModuleIds, request.TodayNotifications);
            return new GetUserNotificationsResponse() { Notifications = notifications };
        }

        public GetUserNotificationsResponse GetNotifications(GetUserNotificationsRequest request)
        {
            Logger.Current.Verbose("Request for getting user notifications.");
            Logger.Current.Informational("UserId to get the notifications :" + string.Join(",", request.UserIds));
            IEnumerable<Notification> notifications = userRepository.GetViewedNotifications(request.UserIds, request.ModuleIds, request.TodayNotifications);
            return new GetUserNotificationsResponse() { Notifications = notifications };
        }

        public DeleteNotificationResponse DeleteNotification(DeleteNotificationRequest request)
        {
            Logger.Current.Informational("Requet received for deleting a notification");
            DeleteNotificationResponse response = new DeleteNotificationResponse();
            response.DeletedCount = userRepository.DeleteNotification(request.NotificationIds, request.RequestedBy.Value, request.ModuleId, request.ArePreviousNotifications, request.IsBulkDelete);
            return response;
        }

        public DeleteBulkNotificationsResponse DeleteBulkNotifications(DeleteBulkNotificationsRequest request)
        {
            Logger.Current.Informational("Requet received for deleting bulk notification");
            DeleteBulkNotificationsResponse response = new DeleteBulkNotificationsResponse();
            var usersPermissions = cachingService.GetUserPermissions(request.AccountId);
            var accountPermissions = cachingService.GetAccountPermissions(request.AccountId);

            var userModules = usersPermissions.Where(s => s.RoleId == request.RoleId &&
                              accountPermissions.Contains(s.ModuleId)).Select(r => r.ModuleId).ToList();

            if (userModules != null)
                userModules.Add((byte)AppModules.Download);

            response.DeletedCount = userRepository.DeleteBulkNotifications(request.RequestedBy.Value, request.ArePreviousNotifications, userModules);
            return response;
        }

        public GetUserNotificationCountResponse GetUnReadNotificationsCount(GetUserNotificationCountRequest request)
        {
            Logger.Current.Verbose("Request for getting unread user notifications count.");
            Logger.Current.Informational("UserId to get the unread notifications :" + request.RequestedBy.Value);

            var usersPermissions = cachingService.GetUserPermissions(request.AccountId);
            var accountPermissions = cachingService.GetAccountPermissions(request.AccountId);

            var userModules = usersPermissions != null ? usersPermissions.Where(s => s.RoleId == request.RoleId &&
                              accountPermissions.Contains(s.ModuleId)).Select(r => r.ModuleId).ToList() : null;

            if (userModules != null)
                userModules.Add((byte)AppModules.Download);

            int notificationsCount = userRepository.GetNotificationsCount(request.RequestedBy.Value, NotificationStatus.New, userModules);
            return new GetUserNotificationCountResponse() { Count = notificationsCount };
        }

        public GetUserNotificationCountResponse GetUnReadWebVisitNotificationsCount(GetUserNotificationCountRequest request)
        {
            Logger.Current.Verbose("Request for getting unread user notifications count.");
            Logger.Current.Informational("UserId to get the unread notifications :" + request.RequestedBy.Value);

            IEnumerable<int> notificationIds = userRepository.GetWebVisitNotificationsCount(request.RequestedBy.Value, request.AccountId, NotificationStatus.New);
            return new GetUserNotificationCountResponse() { NotificationIds = notificationIds };
        }

        public GetNotificationsCountByDateResponse GetNotificationsCountByDate(GetNotificationsCountByDateRequest request)
        {
            Logger.Current.Verbose("Request received for getting notifications count by date.");

            NotificationsCount count = userRepository.GetNotificationsCountByDate(request.RequestedBy.Value, request.ModuleIds, NotificationStatus.New);
            return new GetNotificationsCountByDateResponse() { Count = count };
        }

        public GetRecentWebVisitsResponse GetRecentWebVisits(GetRecentWebVisitsRequest request)
        {
            Logger.Current.Verbose("Request for getting web visits. Requested by: " + request.RequestedBy);
            GetRecentWebVisitsResponse response = new GetRecentWebVisitsResponse();
            var recentWebVisits = webAnalyticsRepository.FindWebVisitsByOwner(request.PageNumber, request.Limit, request.AccountId, (int)request.RequestedBy, request.IncludePreviouslyRead);
            response.RecentWebVisits = recentWebVisits;
            return response;
        }

        public GetUserActivitiesResponse GetUserActivities(GetUserActivitiesRequest request)
        {
            Logger.Current.Verbose("Request for getting user-activities");
            GetUserActivitiesResponse response = new GetUserActivitiesResponse();

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var userActivities = userActivitiesRepository.FindAll(request.UserId, request.PageNumber, request.AccountId, request.ModuleIds);
            if (userActivities != null)
            {
                UserActivitiesListViewModel viewModel = new UserActivitiesListViewModel() { };

                var stopwatch1 = new Stopwatch();
                stopwatch1.Start();
                UserActivityAnalyzer analyzer = new UserActivityAnalyzer(userActivities);
                var userActivityViewModels = analyzer.GenerateAnalysis(request.DateFormat);
                stopwatch1.Stop();
                var elapsed_time1 = stopwatch1.Elapsed;
                Logger.Current.Informational("Time Taken to complete analysis of user-activities :" + elapsed_time1);

                viewModel.UserActivities = userActivityViewModels;
                response.UserActivities = viewModel;
            }
            stopwatch.Stop();
            var elapsed_time = stopwatch.Elapsed;
            Logger.Current.Informational("Time Taken to fetch user-activities :" + elapsed_time);
            return response;
        }

        public GetUserActivityDetailResponse GetUserActivityDetails(GetUserActivityDetailRequest request)
        {
            Logger.Current.Verbose("Request for getting user-activity details");
            GetUserActivityDetailResponse response = new GetUserActivityDetailResponse();
            if (request.EntityIds != null)
            {
                List<UserActivityEntityDetail> entityDetail = new List<UserActivityEntityDetail>();
                foreach (var Id in request.EntityIds)
                {
                    var EntityDetail = userActivitiesRepository.GetUserActivityEntityDetails(request.ModuleId, Id, request.AccountId);
                    if (EntityDetail != null)
                        entityDetail.Add(EntityDetail);
                }
                response.EntityDetails = entityDetail;
            }
            return response;
        }

        public DeleteUserActivityResponse DeleteUserActivity(DeleteUserActivityRequest request)
        {
            Logger.Current.Verbose("Request for deleting user-activity");
            DeleteUserActivityResponse response = new DeleteUserActivityResponse();
            userActivitiesRepository.DeleteActivity(request.userID, request.activityLogID, request.AccountId);
            return response;
        }

        public UserReadActivityResponse InsertReadActivity(UserReadActivityRequest request)
        {
            Logger.Current.Verbose("Request for inserting Contact Read activity in UserLog");
            userActivitiesRepository.InsertContactReadActivity(request.EntityId, request.EntityName, request.UserId, request.ModuleName, request.ActivityName, request.AccountId);
            return new UserReadActivityResponse();
        }

        public ChangeOwnerLogResponse InsertChangeOwnerActivity(ChangeOwnerLogRequest request)
        {
            Logger.Current.Verbose("Request for log entry for change owner activity");
            userActivitiesRepository.InsertChangeOwnerActivity(request.EntityId, request.UserId, request.ModuleName, request.ActivityName);
            return new ChangeOwnerLogResponse();
        }

        public GetRecentViwedContactsResponse GetRecentlyViewedContacts(GetRecentViwedContactsRequest request)
        {
            Logger.Current.Verbose("Request for fetching recently-viewed contacts by user");
            GetRecentViwedContactsResponse response = new GetRecentViwedContactsResponse();
            IList<int> contactsIdList = userActivitiesRepository.GetContactsByActivity(request.UserId, request.ModuleName, request.ActivityName, request.ContactIDs, request.sort, request.AccountId);
            if (contactsIdList != null)
            {
                response.ContactIdList = contactsIdList;
                return response;
            }
            return null;
        }

        public MarkNotificationAsReadResponse MarkNotificationAsRead(MarkNotificationAsReadRequest request)
        {
            Logger.Current.Verbose("Request received to mark a notification as read.");
            Notification notification = request.Notification;
            if (notification.Source == Entities.NotificationSource.Tour && notification.EntityId.HasValue)
            {
                Tour tour = tourRepository.GetTourByID(notification.EntityId.Value);
                if (tour != null)
                {
                    tour.NotificationStatus = NotificationStatus.Viewed;
                    tourRepository.Update(tour);
                    unitOfWork.Commit();
                }
            }
            else if (notification.Source == Entities.NotificationSource.Action && notification.EntityId.HasValue)
            {
                DA.Action action = actionRepository.FindBy(notification.EntityId.Value);
                if (action != null)
                {
                    action.NotificationStatus = NotificationStatus.Viewed;
                    actionRepository.Update(action);
                    unitOfWork.Commit();
                }
            }
            else if (notification.Source == Entities.NotificationSource.System || notification.Source == Entities.NotificationSource.LitmusResults)
            {
                notification.Status = NotificationStatus.Viewed;
                userRepository.UpdateNotification(notification);
            }
            return new MarkNotificationAsReadResponse();
        }

        public GetRequestUserResponse GetUsersList(GetUserListRequest request)
        {
            GetRequestUserResponse response = new GetRequestUserResponse();
            IEnumerable<User> users;
            users = userRepository.GetUsers(request.AccountID, request.IsSTAdmin).ToList();
            if (users == null)
            {
                throw new UnsupportedOperationException("The requested users list was not found.");
            }
            else
            {
                IEnumerable<UserEntryViewModel> userslist = Mapper.Map<IEnumerable<User>, IEnumerable<UserEntryViewModel>>(users);
                response.Users = userslist;
            }
            return response;
        }

        public IsAccountAdminExistResponse IsAccountAdminExist(IsAccountAdminExistRequest request)
        {
            IsAccountAdminExistResponse response = new IsAccountAdminExistResponse();
            Logger.Current.Verbose("Request for checking if Account-Admin exist or not : " + request.AccountId);
            response.AccountAdminExist = userRepository.IsAccountAdminExist(request.AccountId);
            return response;
        }

        public InsertProfileAuditResponse InsertProfileAudit(InsertProfileAuditRequest request)
        {
            Logger.Current.Verbose("Request received for inserting profile audit");
            Logger.Current.Informational("Request received for inserting profile audit ", request.UserId.ToString());
            foreach (int userId in request.UserId)
            {
                userRepository.InsertUserProfileAudit(userId, request.AuditType, request.AuditedBy, request.Password);
            }
            InsertProfileAuditResponse response = new InsertProfileAuditResponse();
            return response;
        }

        public GetUserFullNameResponse GetUserFullName(GetUserFullNameRequest request)
        {
            Logger.Current.Verbose("Request for getting user fullname");
            Logger.Current.Informational("Request for getting user fullname ", request.Id.ToString());
            GetUserFullNameResponse response = new GetUserFullNameResponse();
            Logger.Current.Verbose("Request for fetching the user full name with ID : " + request.Id);
            response.UserFullName = userRepository.GetUserName(request.Id);
            return response;
        }

        private UnsupportedOperationException GetUserActivitiesNotFoundException()
        {
            return new UnsupportedOperationException("The requested user activities was not found.");
        }

        public GetUserCalenderResponse GetUserCalender(GetUserCalenderRequest request)
        {
            Logger.Current.Informational("Request for getting user calender");
            GetUserCalenderResponse response = new GetUserCalenderResponse();
            response.CalenderSlots = new List<CalenderTimeSlotViewModel>();
            var calenderTimeSlots = userRepository.GetUserCalender(request.AccountId, request.RequestedBy, request.StartDate, request.EndDate);
            response.CalenderSlots = Mapper.Map<IList<CalenderTimeSlot>, IList<CalenderTimeSlotViewModel>>(calenderTimeSlots);
            return response;
        }

        public UpdateFacebookConnectionResponse UpdateFacebookAccessToken(UpdateFacebookConnectionRequest request)
        {
            userRepository.UpdateFacebookAccessToken(request.UserId, request.FacebookAccessToken);
            return new UpdateFacebookConnectionResponse();
        }

        public UpdateTwitterConnectionResponse UpdateTwitterOAuthTokens(UpdateTwitterConnectionRequest request)
        {
            userRepository.UpdateTwitterOAuthTokens(request.UserId, request.TwitterOAuthToken, request.TwitterOAuthTokenSecret);
            return new UpdateTwitterConnectionResponse();
        }

        public InsertUserPasswordResponse InsertUserResetPassword(InsertUserPasswordRequest request)
        {
            Logger.Current.Verbose("Request for insert user password");
            AccountSubscriptionData subscriptiondata = accountRepository.GetSubscriptionData(request.AccountId);
            if (subscriptiondata.Limit.HasValue)
            {
                int usercount = userRepository.GetAllActiveUserIds(request.AccountId, request.UserIDs);
                if (usercount > subscriptiondata.Limit)
                {
                    UserLimitEmailNotification(request.AccountId);
                    throw new UnsupportedOperationException("[|Maximum number of Users reached. Please contact Help Desk for assistance at helpdesk@smarttouchinteractive.com.|]");
                }
            }
            userRepository.UpdateUserResetPassword(request.UserIDs, request.Password, request.RequestedBy);
            return new InsertUserPasswordResponse();
        }

        public GetContactWebVisitReportResponse GetWebVisitDetailsByVisitReference(GetContactWebVisitReportRequest request)
        {
            Logger.Current.Verbose("Request for fetching web visit details for visit reference: " + request.VisitReference);
            GetContactWebVisitReportResponse response = new GetContactWebVisitReportResponse();
            var webVisits = contactRepository.GetWebVisitDetailsByVisitReference(request.VisitReference, (int)request.RequestedBy);
            response.WebVisits = Mapper.Map<IEnumerable<WebVisit>, IEnumerable<WebVisitViewModel>>(webVisits);
            return response;
        }

        public GetUsersByUserIDsResponse GetUsersByUserIDs(GetUsersByUserIDsRequest request)
        {
            Logger.Current.Verbose("Request received to GetUsersByUserIDs:" + string.Join(",", request.UserIDs));
            GetUsersByUserIDsResponse response = new GetUsersByUserIDsResponse();
            response.Users = userRepository.GetUsersByUserIDs(request.UserIDs);
            Logger.Current.Informational("Received UsersByUserIDs:" + response.Users.Count());
            return response;
        }

        public GetUsersOptedInstantWebVisitEmailResponse GetUsersOptedInstantWebVisitEmail(GetUsersOptedInstantWebVisitEmailRequest request)
        {
            GetUsersOptedInstantWebVisitEmailResponse response = new GetUsersOptedInstantWebVisitEmailResponse();
            response.Users = userRepository.GetUsersOptedInstantWebVisitEmail(request.AccountId);
            return response;
        }

        public GetUsersOptedWebVisitSummaryEmailResponse GetUsersOptedWebVisitSummaryEmail(GetUsersOptedWebVisitSummaryEmailRequest request)
        {
            GetUsersOptedWebVisitSummaryEmailResponse response = new GetUsersOptedWebVisitSummaryEmailResponse();
            response.UsersOpted = userRepository.GetUsersOptedWebVisitSummaryEmail(request.AccountId);
            response.AllUsers = userRepository.GetAllUsersBasicInfo(request.AccountId);
            return response;
        }

        public GetUserTimeZoneResponse GetUserTimeZoneByUserID(GetUserTimeZoneRequest request)
        {
            GetUserTimeZoneResponse response = new GetUserTimeZoneResponse();
            response.TimeZone = userRepository.GetUserTimeZone((int)request.RequestedBy, request.AccountId);
            return response;
        }

        public InserImplicitSyncEmailInfoResponse TrackReceivedEmail(InserImplicitSyncEmailInfoRequest request)
        {
            InserImplicitSyncEmailInfoResponse response = new InserImplicitSyncEmailInfoResponse();
            var emailsToBeTracked = new List<ReceivedMailAudit>();
            var referenceId = Guid.NewGuid();
            foreach (var recepient in request.EmailInfo.To)
            {
                ReceivedMailAudit receivedEmail = new ReceivedMailAudit();
                receivedEmail.SentByContactID = request.SentByContactID;
                receivedEmail.UserID = request.Users.Where(u => u.Email == recepient).Select(u => u.UserID).FirstOrDefault();
                receivedEmail.ReceivedOn = request.EmailInfo.SentDate;
                receivedEmail.ReferenceID = referenceId;
                emailsToBeTracked.Add(receivedEmail);
                
            }
            Dictionary<Guid,int> receivedMailAuditIds= userRepository.TrackReceivedEmail(emailsToBeTracked);
            InsertEmailReceived(request, referenceId.ToString());
            addToTopicEmailReceived(emailsToBeTracked, receivedMailAuditIds,request.Users.Select(s=> s.AccountID).FirstOrDefault());
            response.ResponseGuid = referenceId;
            return response;
        }

        void addToTopicEmailReceived(List<ReceivedMailAudit> receivedEmailAudit, Dictionary<Guid, int> receivedMailAuditIds,int accountId)
        {
            var messages = new List<TrackMessage>();
            if (receivedEmailAudit.IsAny())
            {
                receivedEmailAudit.Each(rea =>
                {
                    int receivedEmailAuditId = receivedMailAuditIds.Where(s => s.Key == rea.ReferenceID).Select(s => s.Value).FirstOrDefault();
                    var message = new TrackMessage()
                    {
                        EntityId = receivedEmailAuditId,
                        AccountId = accountId,
                        ContactId = rea.SentByContactID,
                        LeadScoreConditionType = (int)LeadScoreConditionType.ContactEmailReceived
                    };

                    messages.Add(message);

                });
            }
            messageRepository.SendMessages(messages);
        }

        public FindUsersByEmailsResponse FindUsersByEmails(FindUsersByEmailsRequest request)
        {
            FindUsersByEmailsResponse response = new FindUsersByEmailsResponse();
            response.Users = userRepository.FindUsersByEmails(request.UserEmails, request.AccountId);
            return response;
        }

        public GetUsersByUserIDsResponse GetUserBasicInfo(GetUsersByUserIDsRequest request)
        {
            GetUsersByUserIDsResponse response = new GetUsersByUserIDsResponse();
            UserBasicInfo userInfo = userRepository.GetUserBasicDetails(request.UserIDs.FirstOrDefault().Value);
            response.Users = new List<UserBasicInfo>() { userInfo };
            return response;
        }

        void InsertEmailReceived(InserImplicitSyncEmailInfoRequest request, string referenceId)
        {
            MailService mailService = new MailService();
            mailService.InsertEmailReceived(request.EmailInfo, referenceId);
            //InserImplicitSyncEmailInfoResponse response = new InserImplicitSyncEmailInfoResponse()
            //{
            //    ResponseGuid = referenceId
            //};
            //return response;
        }

        public void AddNotification(AddNotificationRequest request)
        {
            userRepository.AddNotification(request.Notification);
        }

        public UpdateTCAcceptanceResponse UpdateTCAcceptance(UpdateTCAcceptanceRequest request)
        {
            if (request != null && request.RequestedBy.HasValue)
                userSettingsRepository.UpdateTCAcceptance(request.RequestedBy.Value, request.AccountId);
            return new UpdateTCAcceptanceResponse();
        }

        public GetFirstLoginUserSettingsResponse GetFirstLoginUserSettings(GetFirstLoginUserSettingsRequest request)
        {
            GetFirstLoginUserSettingsResponse response = new GetFirstLoginUserSettingsResponse();
            if (request != null && request.RequestedBy.HasValue)
                response.UserSettings = userSettingsRepository.GetFirstLoginSettings(request.RequestedBy.Value);
            return response;
        }

        public GetAccountAddressResponse GetAccountAddress(GetAccountAddressRequest request)
        {
            GetAccountAddressResponse response = new GetAccountAddressResponse();
            if (request.AccountId != 0)
                response.Address = accountRepository.GetAccountAddress(request.AccountId);
            return response;
        }

        public bool IsIncludeSignatureByDefaultOrNot(int userId)
        {
            bool isIncludeSignature = userSettingsRepository.IsIncludeSignatureByDefault(userId);
            return isIncludeSignature;
        }

        public int? GetDefaultAccountAdmin()
        {
            return userRepository.GetDefaultAdmin();
        }

        public int GetActiveUserIds(int account, int[] userIds)
        {
            return userRepository.GetAllActiveUserIds(account, userIds);
        }

        public AccountSubscriptionData GetSubscriptionData(int account)
        {
            return accountRepository.GetSubscriptionData(account);
        }

        public void UserLimitEmailNotification(int accountId)
        {
            try
            {
                ServiceProvider ServiceProviders = serviceProviderRepository
                   .GetServiceProviders(1, CommunicationType.Mail, MailType.TransactionalEmail);

                LandmarkIT.Enterprise.CommunicationManager.Requests.SendMailRequest sendMailRequest = new LandmarkIT.Enterprise.CommunicationManager.Requests.SendMailRequest();

                Logger.Current.Verbose("Account Id in LeadAdapter:" + accountId);
                Logger.Current.Verbose("Email Guid in LeadAdapter :" + ServiceProviders.LoginToken);
                Account account = accountRepository.GetAccountMinDetails(accountId);

                string _supportEmailId = ConfigurationManager.AppSettings["SupportEmailId"];
                string subjct = account.AccountName + " - Maximum user limit reached";

                var body = "Maximum number of Users reached. Please contact Help Desk for assistance at helpdesk@smarttouchinteractive.com.";

                Logger.Current.Verbose("Sending Email in for user limit :" + _supportEmailId);
                _supportEmailId = _supportEmailId == null ? "smartcrm3@gmail.com" : _supportEmailId;
                List<string> To = userRepository.GetAccountAdminEmails(accountId);
                EmailAgent agent = new EmailAgent();
                sendMailRequest.TokenGuid = ServiceProviders.LoginToken;
                sendMailRequest.From = _supportEmailId;
                sendMailRequest.IsBodyHtml = true;
                sendMailRequest.DisplayName = "";
                sendMailRequest.To = To;
                sendMailRequest.Subject = subjct;
                sendMailRequest.Body = body;
                sendMailRequest.RequestGuid = Guid.NewGuid();
                var varsendMailresponse = agent.SendEmail(sendMailRequest);
                if (varsendMailresponse.StatusID == LandmarkIT.Enterprise.CommunicationManager.Responses.CommunicationStatus.Success)
                {
                    Logger.Current.Informational("Support mail sent successfully");
                }
                Logger.Current.Verbose("Sending Email for user limit reached :" + _supportEmailId);
            }
            catch (Exception ex)
            {
                Logger.Current.Error("An exception occured while sending email for user limit : ", ex);
            }
        }

        public MyCommunicationResponse GetMyCommunicationDetails(MyCommunicationRequest request)
        {
            MyCommunicationResponse response = new MyCommunicationResponse();
            response.CommunicationDetails = userRepository.GetMyCommunicationDetails(request.UserId, request.AccountId, request.StartDate, request.EndDate);
            return response;
        }

        public GetMyCommunicationContactsResponse GetMyCommunicationContacts(GetMyCommunicationContactsRequest request)
        {
            GetMyCommunicationContactsResponse response = new GetMyCommunicationContactsResponse();
            response.ContactIdList = userRepository.GetMyCommunicationContacts(request.UserId, request.AccountId, request.StartDate, request.EndDate, request.Activity, request.ActivityType);
            return response;
        }

        public GetUserResponse GetUserDetailsByEmailAndAccountId(GetUserRequest request)
        {
            GetUserResponse response = new GetUserResponse();
            User user = userRepository.FindByEmailAndAccountId(request.UserName, request.AccountId);
            response.User = Mapper.Map<User, UserViewModel>(user);
            return response;
        }
    }
}

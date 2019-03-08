using Newtonsoft.Json;
using System.Linq;
using SmartTouch.CRM.ApplicationServices.Messaging.Campaigns;
using SmartTouch.CRM.ApplicationServices.Messaging.User;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.WebService.Helpers;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;
using LandmarkIT.Enterprise.Extensions;
using SmartTouch.CRM.Entities;
using LandmarkIT.Enterprise.Utilities.Logging;
using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using SmartTouch.CRM.Identity;
using SmartTouch.CRM.ApplicationServices.Messaging.Communication;
using SmartTouch.CRM.ApplicationServices.Messaging.Geo;

namespace SmartTouch.CRM.WebService.Controllers
{
    /// <summary>
    /// Creating users controller for users module
    /// </summary>
    public class UserController : SmartTouchApiController
    {
        readonly IUserService userService;
        readonly ITourService tourService;
        readonly IActionService actionService;
        readonly ICachingService cachingService;
        readonly ICampaignService campaignService;
        readonly IGeoService geoService;

        /// <summary>
        /// Creating constructor for users controller for accessing
        /// </summary>
        /// <param name="userService">userService</param>
        /// <param name="tourService">tourService</param>
        /// <param name="actionService">actionService</param>
        /// <param name="cachingService">cachingService</param>
        public UserController(IUserService userService, ITourService tourService,
            IActionService actionService, ICachingService cachingService, ICampaignService campaignService, IGeoService geoService)
        {
            this.userService = userService;
            this.tourService = tourService;
            this.actionService = actionService;
            this.cachingService = cachingService;
            this.campaignService = campaignService;
            this.geoService = geoService;
        }

        /// <summary>
        /// Inser a new user.
        /// </summary>
        /// <param name="viewModel">Properties of a new form.</param>
        /// <returns>User Insertion Details response</returns>
        [Route("User")]
        [HttpPost]
        public HttpResponseMessage PostUser(UserViewModel viewModel)
        {
            InsertUserResponse response = userService.InsertUser(new InsertUserRequest() { UserViewModel = viewModel });
            return Request.BuildResponse(response);
        }

        /// <summary>
        /// Get remainders of a user.
        /// </summary>
        /// <param name="userId">user Id</param>
        /// <param name="moduleIds">module Ids</param>
        /// <param name="todayNotifications">today Notifications</param>
        /// <returns>All Reminders</returns>
        [Route("Reminders")]
        [HttpGet]
        [Authorize]
        public HttpResponseMessage GetReminders(int userId, string moduleIds, bool todayNotifications)
        {
            int[] moduleIDs = null;
            if (!string.IsNullOrEmpty(moduleIds))
                moduleIDs = JsonConvert.DeserializeObject<int[]>(moduleIds);
            GetUserNotificationsResponse response = userService.GetReminderNotifications(
                new GetUserNotificationsRequest() { UserIds = new List<int>() { userId }, ModuleIds = moduleIDs, TodayNotifications = todayNotifications });
            return Request.BuildResponse(response);
        }

        /// <summary>
        /// Get notifications of a user.
        /// </summary>
        /// <param name="userId">user Id</param>
        /// <param name="moduleIds">module Ids</param>
        /// <param name="todayNotifications">today Notifications</param>
        /// <returns>User Notifications</returns>

        [Route("Notifications")]
        [HttpGet]
        [Authorize]
        public HttpResponseMessage GetNotifications(int userId, string moduleIds, bool todayNotifications)
        {
            int[] moduleIDs = null;
            if (!string.IsNullOrEmpty(moduleIds))
                moduleIDs = JsonConvert.DeserializeObject<int[]>(moduleIds);
            GetUserNotificationsResponse response = userService.GetNotifications(
                new GetUserNotificationsRequest() { UserIds = new List<int>() { userId }, ModuleIds = moduleIDs, TodayNotifications = todayNotifications });
            return Request.BuildResponse(response);
        }

        /// <summary>
        /// Mark notification as read.
        /// </summary>
        /// <param name="notification">Properties of notification.</param>
        /// <returns>Notifications which are read</returns>

        [Route("MarkNotificationAsRead")]
        [HttpPost]
        [Authorize]
        public HttpResponseMessage MarkNotificationAsRead(Notification notification)
        {
            MarkNotificationAsReadResponse response =
                userService.MarkNotificationAsRead(new MarkNotificationAsReadRequest() { Notification = notification });
            return Request.BuildResponse(response);
        }

        /// <summary>
        /// Delete a notification
        /// </summary>
        /// <param name="deleteViewModel">notifications delete viewmodel</param>
        /// <returns>Notification deletion details response</returns>

        [Route("DeleteNotification")]
        [HttpGet]
        [Authorize]
        public HttpResponseMessage DeleteNotification(string deleteViewModel)
        {
            DeleteNotificationViewModel notificationsViewModel = new DeleteNotificationViewModel();
            if (deleteViewModel != null)
                notificationsViewModel = JsonConvert.DeserializeObject<DeleteNotificationViewModel>(deleteViewModel);
            DeleteNotificationResponse response =
                userService.DeleteNotification(new DeleteNotificationRequest()
                {
                    NotificationIds = notificationsViewModel.NotificationIds,
                    IsBulkDelete = notificationsViewModel.IsBulkDelete,
                    ModuleId = notificationsViewModel.ModuleId,
                    ArePreviousNotifications = notificationsViewModel.ArePreviousNotifications,
                    RequestedBy = this.UserId
                });
            return Request.BuildResponse(response);
        }

        /// <summary>
        /// For Deleting Notification.
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        [Route("NotificationDelete")]
        [HttpPost]
        [Authorize]
        public HttpResponseMessage NotificationDelete(DeleteNotificationViewModel viewModel)
        {
            DeleteNotificationResponse response =
                userService.DeleteNotification(new DeleteNotificationRequest()
                {
                    NotificationIds = viewModel.NotificationIds,
                    IsBulkDelete = viewModel.IsBulkDelete,
                    ModuleId = viewModel.ModuleId,
                    ArePreviousNotifications = viewModel.ArePreviousNotifications,
                    RequestedBy = this.UserId
                });
            return Request.BuildResponse(response);
        }

        /// <summary>
        /// Get Campaign Litmus Results
        /// </summary>
        /// <param name="campaignId"></param>
        /// <returns></returns>

        [Route("litmusresults")]
        [HttpGet]
        [Authorize]
        public HttpResponseMessage GetCampaignLitmusResults(int campaignId)
        {
            GetCampaignLitmusResponse response = campaignService.GetCampaignLitmusMap(new GetCampaignLitmusMapRequest()
            {
                AccountId = this.AccountId,
                CampaignId = campaignId,
                RequestedBy = 0
            });

            if(response.CampaignLitmusMaps.IsAny())
                response.LitmusId = response.CampaignLitmusMaps.OrderByDescending(m => m.LastModifiedOn).FirstOrDefault().LitmusId;

            return Request.BuildResponse(response);
        }

        /// <summary>
        /// Get Notifications Count
        /// </summary>
        /// <returns></returns>
        [Route("NotificationsCount")]
        [HttpGet]
        public HttpResponseMessage GetNotificationsCount()
        {
            var accountId = this.AccountId;
            var userId = this.UserId;
            var roleId = this.RoleId;
            var usersPermissions = cachingService.GetUserPermissions(accountId);
            var accountPermissions = cachingService.GetAccountPermissions(accountId);
            var userModules = usersPermissions.Where(s => s.RoleId == roleId && accountPermissions.Contains(s.ModuleId)).Select(r => r.ModuleId).ToList();
            if (accountId != 1)
                userModules.Add((byte)AppModules.Download);
            else
                userModules = userModules.Where(m => m != 79).Select(s => s).ToList();

            var response = userService.GetNotificationsCountByDate(new GetNotificationsCountByDateRequest()
            {
                RequestedBy = userId,
                ModuleIds = userModules
            });
            if (userModules != null)
                response.PermissionModuleIds = userModules;

            return Request.BuildResponse(response);
        }

        /// <summary>
        /// Get All Users of Logged in Account.
        /// </summary>
        /// <returns></returns>
        [Route("GetUsers")]
        [HttpGet]
        public HttpResponseMessage GetUsers()
        {
            GetRequestUserResponse response = userService.GetUsersList(new GetUserListRequest()
            {
                AccountID = this.AccountId,
                IsSTAdmin = this.IsSTAdmin
            });

            return Request.BuildResponse(response);
        }

        /// <summary>
        /// For Logged In User Basic Information.
        /// </summary>
        /// <returns></returns>
        [Route("GetUserBasicInfo")]
        [HttpGet]
        public HttpResponseMessage GetUserBasicInfo()
        {
            GetUsersByUserIDsResponse response = userService.GetUserBasicInfo(new GetUsersByUserIDsRequest()
            {
                UserIDs = new List<int?>() { this.UserId }
            });

            return Request.BuildResponse(response);

        }

        /// <summary>
        /// For My Profile
        /// </summary>
        /// <returns></returns>
        [Route("UserProfile")]
        [HttpGet]
        public HttpResponseMessage MyProfile()
        {
            int userId = this.UserId;
            int accountId = this.IsSTAdmin ? 1 : this.AccountId;
            GetUserResponse response = userService.GetUser(new GetUserRequest(userId));
            MyProfileViewModel model = new MyProfileViewModel();
            GetUserSettingResponse userSettingResp = new GetUserSettingResponse();
            UserSettingsViewModel settings = new UserSettingsViewModel();
            settings.AccountId = accountId;
            var dropdownValues = cachingService.GetDropdownValues(null);
            var defaultAddressFields = dropdownValues.Where(s => s.DropdownID == (byte)DropdownFieldTypes.AddressType).Select(s => s.DropdownValuesList).ToList().FirstOrDefault();
            var defaultPhoneFields = dropdownValues.Where(s => s.DropdownID == (byte)DropdownFieldTypes.PhoneNumberType).Select(s => s.DropdownValuesList).ToList().FirstOrDefault();
            var defaultAddressType = defaultAddressFields.Where(d => d.IsDefault == true).FirstOrDefault();
            var defaultPhoneType = defaultPhoneFields.Where(d => d.IsDefault == true).FirstOrDefault();
            IList<Email> EmailList = new List<Email>();
            settings.DateFormat = 2;
            settings.ItemsPerPage = 25;
            settings.TimeZone = this.TimeZone;
            settings.CountryId = "CA";
            settings.CurrencyID = 1;
            settings.UserId = userId;
            settings.EmailSignature = "";
            settings.Emails = EmailList;
            if (response.User != null)
            {
                Logger.Current.Verbose("User found");
                response.User.AddressTypes = defaultAddressFields.Where(d => d.IsActive == true);
                response.User.PhoneNumberTypes = defaultPhoneFields.Where(d => d.IsActive == true);
                if (response.User.Phones.Count == 0)
                {
                    Logger.Current.Verbose("No phones found for this user");
                    response.User.Phones = new List<Phone>() {
                        new Phone () {
                            Number = "",
                            PhoneType = defaultPhoneType.DropdownValueTypeID,
                            IsPrimary = true,
                            ContactPhoneNumberID = 0
                        }
                    };
                }
                if (response.User.Addresses.Count == 0)
                {
                    Logger.Current.Verbose("No address found for this user");
                    Address accAddress = userService.GetAccountAddress(new GetAccountAddressRequest() { AccountId = accountId }).Address;
                    if (accAddress != null)
                    {
                        response.User.Addresses = new List<AddressViewModel>() {
                        new AddressViewModel () {
                            AddressID = 0,
                            AddressTypeID = defaultAddressType.DropdownValueID,
                            State = new State {
                                Code = accAddress.State.Code,
                                Name = accAddress.State.Name
                            },
                            Country = new Country {
                                Code = accAddress.Country.Code,
                                Name = accAddress.Country.Name
                            },
                            IsDefault = true
                            }
                        };
                    }
                }
                userSettingResp = userService.GetUserSetting(new GetUserSettingRequest()
                {
                    AccountID = accountId,
                    UserID = userId
                });
                if (userSettingResp.UserSettingsViewModel != null)
                    userSettingResp.UserSettingsViewModel.Emails = response.User.Emails;
                else
                    settings.Emails = response.User.Emails;
                model.userViewModel = response.User;
                model.userSettingsViewModel = userSettingResp.UserSettingsViewModel == null ? settings : userSettingResp.UserSettingsViewModel;
                Logger.Current.Verbose("viewmodel is initialized");
            }
            else
            {
                Logger.Current.Verbose("user not found");
                model.userSettingsViewModel = settings;
                var userViewModel = new UserViewModel();
                userViewModel.Phones = new List<Phone>() {
                    new Phone () {
                        Number = "",
                        PhoneType = defaultPhoneType.DropdownValueTypeID,
                        IsPrimary = true,
                        ContactPhoneNumberID = 0
                    }
                };
                userViewModel.Addresses = new List<AddressViewModel>() {
                    new AddressViewModel () {
                        AddressID = 0,
                        AddressTypeID = defaultAddressType.DropdownValueID,
                        Country = new Country {
                            Code = ""
                        },
                        State = new State {
                            Code = ""
                        },
                        IsDefault = true
                    }
                };
                model.userViewModel = userViewModel;
            }
            Logger.Current.Verbose("Loading my-profile page");
           return Request.CreateResponse(model);
        }

        /// <summary>
        /// User Profile Saving.
        /// </summary>
        /// <param name="userViewModel"></param>
        /// <returns></returns>
        [Route("UserProfileSaving")]
        [HttpPost]
        public HttpResponseMessage UpdateUser(UserViewModel userViewModel)
        {
            int userId = this.UserId;
            userViewModel.ModifiedBy = userId;
            userViewModel.ModifiedOn = DateTime.Now.ToUniversalTime();
            UpdateUserResponse response = new UpdateUserResponse();
            try
            {
                UpdateUserRequest request = new UpdateUserRequest()
                {
                    UserViewModel = userViewModel,
                    AccountId = this.AccountId
                };
                response = userService.UpdateUser(request);
                IEnumerable<int> userIds = new List<int>() {
                userViewModel.UserID
                  };

                InsertProfileAudit(userIds, UserAuditType.ProfileUpdate, null, userId);
            }
            catch(Exception ex)
            {
                response.ErrorMessage = ex.Message;
            }

            return Request.BuildResponse(response);
        }

        void InsertProfileAudit(IEnumerable<int> userId, UserAuditType auditType, string password, int auditedBy)
        {
            InsertProfileAuditRequest profileRequest = new InsertProfileAuditRequest()
            {
                UserId = userId,
                AuditType = auditType,
                Password = password,
                AuditedBy = auditedBy
            };
            userService.InsertProfileAudit(profileRequest);
        }

        //[Route("UserSettingsSaving")]
        //[HttpPost]
        //public async Task<HttpResponseMessage> SaveSettings(string userSettingsViewModel)
        //{
        //    UserSettingsViewModel viewModel = JsonConvert.DeserializeObject<UserSettingsViewModel>(userSettingsViewModel);
        //    bool changePassword = false;
        //    InsertUserSettingsRequest request = new InsertUserSettingsRequest()
        //    {
        //        UserSettingsViewModel = viewModel
        //    };
        //    InsertUserSettingsResponse resp = userService.InsertUserSettings(request);
        //    if (viewModel.newPassword == viewModel.confirmPassword && viewModel.confirmPassword != "" && viewModel.confirmPassword != null)
        //    {
        //        try
        //        {
        //            await ChangePassword(viewModel.currentPassword, viewModel.newPassword);
        //            changePassword = true;
        //        }
        //        catch (Exception ex)
        //        {
        //            Logger.Current.Error("An error occured while changing password.", ex);
        //            return Request.CreateResponse(ex.Message);
        //        }
        //    }
        //    IEnumerable<int> userIds = new List<int>() {
        //        viewModel.UserId
        //    };
        //    if (!changePassword)
        //        InsertProfileAudit(userIds, UserAuditType.ProfileUpdate, null, this.UserId);

        //   return  Request.BuildResponse(resp);
        //}

        //[HttpPost]
        //public async Task<HttpResponseMessage> ChangePassword(string currentPassword, string newPassword)
        //{
        //    string email = this.UserName;
        //    int accountId = this.AccountId;
        //    var user = await UserManager.FindByIdAsync(this.UserId.ToString());
        //    if (user == null)
        //    {
        //        var jsonmodel= Json(new
        //        {
        //            success = false,
        //            response = "[|User does not exist|]"
        //        });

        //        return Request.CreateResponse(jsonmodel);
        //    }
        //    IdentityResult result = await UserManager.ChangePasswordAsync(user.Id, currentPassword, newPassword, email, accountId);
        //    if (result.Succeeded)
        //    {
        //        /*await UserManager.UpdateSecurityStampAsync(user.Id);*/
        //        var jsonModel = Json(new
        //        {
        //            success = true,
        //            response = "Successfully Changes Password"
        //        });
        //        return Request.CreateResponse(jsonModel);
        //    }
        //    else
        //    {
        //        string responseJson = string.Join(", ", result.Errors);
        //        throw new Exception(responseJson);
        //    }
        //}

        /// <summary>
        /// For Deleting Users.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("DeleteUsers")]
        [HttpPost]
        public HttpResponseMessage DeleteUsers(string id)
        {
            int[] userIDs = null;
            if (!string.IsNullOrEmpty(id))
                userIDs = JsonConvert.DeserializeObject<int[]>(id);
            DeactivateUserRequest request = new DeactivateUserRequest();
            request.UserID = userIDs;
            request.RequestedBy = this.UserId;
            request.AccountId = this.AccountId;
            DeactivateUserResponse response = userService.DeactivateUser(request);
            return Request.BuildResponse(response);
        }

        /// <summary>
        /// For Getting Currencies.
        /// </summary>
        /// <returns></returns>
        [Route("GetCurrencies")]
        [HttpGet]
        public HttpResponseMessage GetCurrencies()
        {
            GetCurrencyResponse response = userService.GetCurrencies(new GetCurrencyRequest()
            {

            });

            return Request.BuildResponse(response);
        }

        /// <summary>
        /// For Getting Date Formarts.
        /// </summary>
        /// <returns></returns>
        [Route("GetDateFormats")]
        [HttpGet]
        public HttpResponseMessage GetDateFormats()
        {
            GetDateFormatResponse response = userService.GetDateFormats(new GetDateFormatRequest()
            {

            });
            return Request.BuildResponse(response);
        }

        /// <summary>
        /// For Getting Countries.
        /// </summary>
        /// <returns></returns>
        [Route("GetCountries")]
        [HttpGet]
        public HttpResponseMessage GetCountries()
        {
            GetCountriesResponse response = geoService.GetCountries(new GetCountriesRequest()
            {

            });
            return Request.BuildResponse(response);
        }

        /// <summary>
        /// For Getting States.
        /// </summary>
        /// <param name="countryCode"></param>
        /// <returns></returns>
        [Route("GetStates")]
        [HttpGet]
        public HttpResponseMessage GetStates(string countryCode)
        {
            GetStatesResponse response = geoService.GetStates(new GetStatesRequest(countryCode));
            return Request.BuildResponse(response) ;
        }
    }
}
using Kendo.Mvc.UI;
using LandmarkIT.Enterprise.Utilities.ExceptionHandling;
using LandmarkIT.Enterprise.Utilities.Logging;
using LinqToTwitter;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Newtonsoft.Json;
using SmartTouch.CRM.ApplicationServices.Messaging.Accounts;
using SmartTouch.CRM.ApplicationServices.Messaging.Communication;
using SmartTouch.CRM.ApplicationServices.Messaging.User;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.Accounts;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Identity;
using SmartTouch.CRM.Web.Utilities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.SessionState;

namespace SmartTouch.CRM.Web.Controllers
{
    public class UserController : SmartTouchController
    {
        private ApplicationUserManager _userManager;

        readonly IUserService userService;

        readonly ICommunicationProviderService serviceProviderService;

        ICachingService cachingService;

        readonly ISocialIntegrationService socialIntegrationService;

        readonly IAccountService accountService;

        readonly IUrlService urlService;

        public UserController(IUserService userService, ICachingService cachingService, ICommunicationProviderService serviceProviderService, ISocialIntegrationService socialIntegrationService, IAccountService accountService, IUrlService urlService)
        {
            this.userService = userService;
            this.cachingService = cachingService;
            this.serviceProviderService = serviceProviderService;
            this.socialIntegrationService = socialIntegrationService;
            this.accountService = accountService;
            this.urlService = urlService;
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        #region User
        [Route("users")]
        [SmarttouchAuthorize(AppModules.Users, AppOperations.Read)]
        [MenuType(MenuCategory.Users, MenuCategory.LeftMenuAccountConfiguration)]
        [OutputCache(Duration = 30)]
        public ActionResult UserList()
        {
            ViewBag.userId = 0;
            short ItemsPerPage = default(short);
            short.TryParse(this.Identity.ToItemsPerPage(), out ItemsPerPage);
            ViewBag.ItemsPerPage = ItemsPerPage;
            ViewBag.IsLimitExceeded = userService.CheckUserLimit(new CheckIfLimitExceededRequest() { AccountId = this.Identity.ToAccountID() }).IsLimitExceeded;
            UserViewModel viewModel = new UserViewModel();
            return View("UserList", viewModel);
        }

        [Route("users/search")]
        [SmarttouchAuthorize(AppModules.Users, AppOperations.Read)]
        [MenuType(MenuCategory.Users, MenuCategory.LeftMenuAccountConfiguration)]
        public ActionResult UserSearch()
        {
            ViewBag.userId = 1;
            short ItemsPerPage = default(short);
            short.TryParse(this.Identity.ToItemsPerPage(), out ItemsPerPage);
            ViewBag.ItemsPerPage = ItemsPerPage;
            UserViewModel viewModel = new UserViewModel();
            return View("UserList", viewModel);
        }

        [SmarttouchAuthorize(AppModules.Users, AppOperations.Read)]
        public ActionResult UsersViewRead([DataSourceRequest] DataSourceRequest request, string name, string status, string role)
        {
            byte userstatus = 0;
            short userrole = 0;
            if (!String.IsNullOrEmpty(status))
                byte.TryParse(status, out userstatus);/*userstatus = Convert.ToByte(status);*/
            if (!String.IsNullOrEmpty(role))
                short.TryParse(role, out userrole);/*userrole = Convert.ToByte(role);*/
            AddCookie("userpagesize", request.PageSize.ToString(), 1);
            AddCookie("userpagenumber", request.Page.ToString(), 1);
            GetUserListResponse response = userService.GetAllUsers(new GetUserListRequest()
            {
                Query = name,
                Limit = request.PageSize,
                PageNumber = request.Page,
                Status = userstatus,
                Role = userrole,
                AccountID = UserExtensions.ToAccountID(this.Identity),
                IsSTAdmin = this.Identity.IsSTAdmin()
            });
            
            return Json(new DataSourceResult
            {
                Data = response.Users,
                Total = response.TotalHits
            }, JsonRequestBehavior.AllowGet);
        }

        [SmarttouchAuthorize(AppModules.Users, AppOperations.Create)]
        public ActionResult AddUser()
        {
            UserViewModel viewModel = new UserViewModel();
            return PartialView("AddUser", viewModel);
        }

        [SmarttouchAuthorize(AppModules.Users, AppOperations.Edit)]
        public ActionResult _ChangeUserStatus()
        {
            UserViewModel viewModel = new UserViewModel();
            return PartialView("_ChangeUserStatus", viewModel);
        }

        [SmarttouchAuthorize(AppModules.Users, AppOperations.Edit)]
        public ActionResult _ChangeUserRole()
        {
            UserViewModel viewModel = new UserViewModel();
            return PartialView("_ChangeUserRole", viewModel);
        }

        [SmarttouchAuthorize(AppModules.Users, AppOperations.Edit)]
        public ActionResult _ResetUserPassword()
        {
            ResetPasswordViewModel viewModel = new ResetPasswordViewModel();
            return PartialView("_ResetUserPassword", viewModel);
        }

        public JsonResult GetEmails()
        {
            int userId = this.Identity.ToUserID();
            int accountId = this.Identity.ToAccountID();
            GetEmailResponse response = userService.GetEmails(new GetEmailRequest()
            {
                userId = userId,
                accountId = accountId
            });
            return Json(new
            {
                success = true,
                response = response.Emails.ToArray()
            }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult CampaignGetEmails()
        {
            int userId = this.Identity.ToUserID();
            int accountId = this.Identity.ToAccountID();
            int roleId = this.Identity.ToRoleID();
            GetEmailResponse response = userService.CampaignGetEmails(new GetEmailRequest()
            {
                userId = userId,
                accountId = accountId,
                roleId = roleId
            });
            return Json(new
            {
                success = true,
                response = response.Emails.ToArray()
            }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetDateFormats()
        {
            GetDateFormatResponse response = userService.GetDateFormats(new GetDateFormatRequest()
            {

            });
            return Json(new
            {
                success = true,
                response = response.DateFormats
            }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetCurrencies()
        {
            GetCurrencyResponse response = userService.GetCurrencies(new GetCurrencyRequest()
            {

            });
            return Json(new
            {
                success = true,
                response = response.Currencies
            }, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> SaveSettings(string userSettingsViewModel)
        {
            UserSettingsViewModel viewModel = JsonConvert.DeserializeObject<UserSettingsViewModel>(userSettingsViewModel);
            bool changePassword = false;
            InsertUserSettingsRequest request = new InsertUserSettingsRequest()
            {
                UserSettingsViewModel = viewModel
            };
            InsertUserSettingsResponse resp = userService.InsertUserSettings(request);
            if (viewModel.newPassword == viewModel.confirmPassword && viewModel.confirmPassword != "" && viewModel.confirmPassword != null)
            {
                try
                {
                    await ChangePassword(viewModel.currentPassword, viewModel.newPassword);
                    changePassword = true;
                }
                catch (Exception ex)
                {
                    Logger.Current.Error("An error occured while changing password.", ex);
                    throw new UnsupportedOperationException(ex.Message);
                }
            }
            IEnumerable<int> userIds = new List<int>() {
                viewModel.UserId
            };
            if (!changePassword)
                InsertProfileAudit(userIds, UserAuditType.ProfileUpdate, null, this.Identity.ToUserID());
            return Json(new
            {
                success = true,
                response = resp.UserSettingsViewModel.UserSettingId
            }, JsonRequestBehavior.AllowGet);
        }

        [SmarttouchAuthorize(AppModules.Users, AppOperations.Edit)]
        public ActionResult _ResendInvite()
        {
            UserViewModel viewModel = new UserViewModel();
            return PartialView("_ResendInvite", viewModel);
        }

        [SmarttouchAuthorize(AppModules.Users, AppOperations.Create)]
        public ActionResult InsertUser(string userViewModel)
        {
            UserViewModel viewModel = JsonConvert.DeserializeObject<UserViewModel>(userViewModel);
            viewModel.AccountID = this.Identity.ToAccountID();
            viewModel.CreatedBy = this.Identity.ToUserID();
            viewModel.CreatedOn = DateTime.Now.ToUniversalTime();
            viewModel.IsDeleted = false;
            InsertUserRequest request = new InsertUserRequest()
            {
                UserViewModel = viewModel
            };
            var serviceprovider = serviceProviderService.ServiceproviderToSendTransactionEmails(this.Identity.ToAccountID());
            if (serviceprovider.LoginToken == new Guid())
            {
                throw new UnsupportedOperationException("[|Communication Providers are not configured for this Account.|]");
            }
            InsertUserResponse response = userService.InsertUser(request);
            var name = response.UserViewModel.FirstName + " " + response.UserViewModel.LastName;
            string filename = EmailTemplate.UserRegistration.ToString() + ".txt";
            if (viewModel.DoNotEmail == false)
                SendEmail(response.UserViewModel.UserID.ToString(), response.UserViewModel.AccountID.ToString(), response.UserViewModel.PrimaryEmail, name, filename, this.Identity.ToAccountPrimaryEmail(), string.Empty);
            return Json(new
            {
                success = true,
                response = ""
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SendEmail(string userID, string accountID, string emailID, string name, string filename, string accountEmail, string resendInvite)
        {
            string code = UserManager.GenerateEmailConfirmationToken(userID);
            var body = "";
            string savedFileName = string.Empty;
            string imagesUrl = ConfigurationManager.AppSettings["IMAGE_HOSTING_SERVICE_URL"];
            int accountId = 0;
            int.TryParse(accountID, out accountId);
            string accountAddress = accountService.GetPrimaryAddress(new GetAddressRequest()
            {
                AccountId = accountId
            }).Address;
            string accountPhoneNumber = accountService.GetPrimaryPhone(new GetPrimaryPhoneRequest()
            {
                AccountId = accountId
            }).PrimaryPhone;
            string accountLogo = string.Empty;
            string accountName = string.Empty;
            string subject = string.Empty;
            ApplicationServices.Messaging.Accounts.GetAccountImageStorageNameResponse response = accountService.GetStorageName(new ApplicationServices.Messaging.Accounts.GetAccountImageStorageNameRequest()
            {
                AccountId = accountId
            });
            if (response.AccountLogoInfo.SubscriptionId == 3)
            {
                string templateName = string.Empty;
                if (string.IsNullOrEmpty(resendInvite))
                    templateName = EmailTemplate.BDXUserRegistration.ToString() + ".txt";
                else
                    templateName = EmailTemplate.BDXResendInvite.ToString() + ".txt";
                savedFileName = Path.Combine(System.Configuration.ConfigurationManager.AppSettings["EMAILTEMPLATES_PHYSICAL_PATH"].ToString(), templateName);
                subject = "BDX Lead Management - New User Creation Notification – " + response.AccountLogoInfo.AccountName + " : " + name;
            }
            else
            {
                savedFileName = Path.Combine(System.Configuration.ConfigurationManager.AppSettings["EMAILTEMPLATES_PHYSICAL_PATH"].ToString(), filename);
            }
            if (!String.IsNullOrEmpty(response.AccountLogoInfo.StorageName))
                accountLogo = urlService.GetUrl(accountId, ImageCategory.AccountLogo, response.AccountLogoInfo.StorageName);
            else
                accountLogo = "";
            accountName = response.AccountLogoInfo.AccountName;
            if (response.AccountLogoInfo.SubscriptionId != 3)
                subject = "SmartTouch New User Creation Notification – " + accountName + " : " + name;
            string accountImage = string.Empty;
            if (!string.IsNullOrEmpty(accountLogo))
            {
                accountImage = accountImage + "<td align='right' valign='center' style='margin:0px;padding:0px 0px 25px 0px;'><img src='" + accountLogo + "' alt='" + accountName + "' style='width:100px;' width='100'> </td>";
            }
            else
            {
                accountImage = "";
            }
            using (StreamReader reader = new StreamReader(savedFileName))
            {
                do
                {
                    var protectedUserId = MachineKey.Protect(GetBytes(userID));
                    var protectedAccountId = MachineKey.Protect(GetBytes(accountID));
                    var protectedEmailId = MachineKey.Protect(GetBytes(emailID));
                    var securedUserID = Convert.ToBase64String(protectedUserId);
                    var securedAccountID = Convert.ToBase64String(protectedAccountId);
                    var securedEmailID = Convert.ToBase64String(protectedEmailId);
                    var user = UserExtensions.ToFirstName(this.Identity) + " " + UserExtensions.ToLastName(this.Identity);
                    body = reader.ReadToEnd().Replace("[USERNAME]", UserExtensions.ToUserName(this.Identity)).Replace("[ACCOUNTNAME]", accountName).Replace("[USER]", user).Replace("[ACCOUNTID]", Url.Encode(securedAccountID)).Replace("[NAME]", name).Replace("[EMAILID]", Url.Encode(securedEmailID)).Replace("[NEWUSEREMAIL]", emailID).Replace("[GUID]", code).Replace("[USERID]", Url.Encode(securedUserID)).Replace("[STURL]", Request.Url.Authority.ToLower()).Replace("[ADDRESS]", accountAddress).Replace("[PHONE]", accountPhoneNumber).Replace("[IMAGES_URL]", imagesUrl).Replace("[AccountImage]", accountImage);
                }
                while (!reader.EndOfStream);
            }
            var result = default(int);
            int.TryParse(accountID, out result);
            Dictionary<Guid, string> providerDetails = accountService.GetTransactionalProviderDetails(this.Identity.ToAccountID());
            string fromEmail = string.IsNullOrEmpty(providerDetails.FirstOrDefault().Value) ? accountEmail : providerDetails.FirstOrDefault().Value;
            if (providerDetails.FirstOrDefault().Key != null)
                UserManager.SendEmail(userID, subject + "|" + fromEmail + "|" + result + "|" + providerDetails.FirstOrDefault().Key + "|" + Request.Url.Authority.ToLower(), body);
            return Json(new
            {
                success = true,
                response = "[|Email has been sent to you Please check your Email.|]"
            }, JsonRequestBehavior.AllowGet);
        }

        static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        static string GetString(byte[] bytes)
        {
            char[] chars = new char[bytes.Length / sizeof(char)];
            System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }

        [Route("edituser")]
        [SmarttouchAuthorize(AppModules.Users, AppOperations.Edit)]
        [MenuType(MenuCategory.UsersEdiAction, MenuCategory.LeftMenuAccountConfiguration)]
        public ActionResult AddEditUser(int userId)
        {
            GetUserResponse response = userService.GetUser(new GetUserRequest(userId));
            var dropdownValues = cachingService.GetDropdownValues(null);
            response.User.AddressTypes = dropdownValues.Where(s => s.DropdownID == (byte)DropdownFieldTypes.AddressType).Select(s => s.DropdownValuesList).ToList().FirstOrDefault().Where(d => d.IsActive == true);
            response.User.PhoneNumberTypes = dropdownValues.Where(s => s.DropdownID == (byte)DropdownFieldTypes.PhoneNumberType).Select(s => s.DropdownValuesList).ToList().FirstOrDefault().Where(d => d.IsActive == true);
            var defaultAddressType = response.User.AddressTypes.SingleOrDefault(a => a.IsDefault == true);
            Address accAddress = userService.GetAccountAddress(new GetAccountAddressRequest() { AccountId = this.Identity.ToAccountID() }).Address;
            if (response.User.Addresses.Count == 0)
            {
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
            ViewBag.PageName = "EditUser";
            ViewBag.NewAddress = new AddressViewModel()
            {
                AddressID = 0,
                AddressTypeID = defaultAddressType.DropdownValueID,
                Country = new Country
                {
                    Code = accAddress.Country.Code,
                    Name = accAddress.Country.Name
                },
                State = new State
                {
                    Code = accAddress.State.Code,
                    Name = accAddress.State.Name
                },
                IsDefault = true
            };
            var defaultPhoneType = response.User.PhoneNumberTypes.SingleOrDefault(a => a.IsDefault == true);
            if (response.User.Phones.Count == 0)
            {
                response.User.Phones = new List<Phone>() {
                    new Phone () {
                        Number = "",
                        PhoneType = defaultPhoneType.DropdownValueTypeID,
                        IsPrimary = true,
                        ContactPhoneNumberID = 0
                    }
                };
            }
            return View("AddEditUser", response.User);
        }

        [Route("myprofile")]
        [MenuType(MenuCategory.Settings, MenuCategory.LeftMenuAccountConfiguration)]
        public ActionResult MyProfile()
        {
            int userId = this.Identity.ToUserID();
            int accountId = this.Identity.IsSTAdmin() ? 1 : this.Identity.ToAccountID();
            GetUserResponse response = userService.GetUser(new GetUserRequest(userId));
            MyProfileViewModel model = new MyProfileViewModel();
            GetUserSettingResponse userSettingResp = new GetUserSettingResponse();
            ViewBag.PageName = "MyProfile";
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
            settings.TimeZone = this.Identity.ToTimeZone();
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
            ViewBag.NewAddress = new AddressViewModel()
            {
                AddressID = 0,
                AddressTypeID = defaultAddressType.DropdownValueID,
                Country = new Country
                {
                    Code = ""
                },
                State = new State
                {
                    Code = ""
                },
                IsDefault = false
            };
            ViewBag.NewPhone = new Phone()
            {
                Number = "",
                PhoneType = defaultPhoneType.DropdownValueID,
                IsPrimary = true,
                ContactPhoneNumberID = 0
            };
            ViewBag.IsAccountStAdmin = this.Identity.IsSTAdmin();
            Logger.Current.Verbose("Loading my-profile page");
            return View("MyProfile", model);
        }

        [HttpPost]
        [MenuType(MenuCategory.ContactAction, MenuCategory.LeftMenuAccountConfiguration)]
        public JsonResult UpdateUser(string userViewModel)
        {
            UserViewModel viewModel = JsonConvert.DeserializeObject<UserViewModel>(userViewModel);
            int userId = this.Identity.ToUserID();
            viewModel.ModifiedBy = userId;
            viewModel.ModifiedOn = DateTime.Now.ToUniversalTime();
            UpdateUserRequest request = new UpdateUserRequest()
            {
                UserViewModel = viewModel,
                AccountId = this.Identity.ToAccountID()
            };
            userService.UpdateUser(request);
            IEnumerable<int> userIds = new List<int>() {
                viewModel.UserID
            };
            InsertProfileAudit(userIds, UserAuditType.ProfileUpdate, null, userId);
            return Json(new
            {
                success = true,
                response = ""
            }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetRoles()
        {
            Logger.Current.Verbose("Request for getting roles");
            GetRoleResponse response = userService.GetRoles(new GetRoleRequest()
            {
                AccountId = this.Identity.ToAccountID()
            });
            if (!this.Identity.IsSTAdmin())
            {
                if(response.SubscriptionId == 2)
                    response.Roles = response.Roles.Where(s => s.RoleName != "Account Administrator");
                else
                    response.Roles = response.Roles.Where(s => s.RoleName != "Account Administrator" && s.RoleName != "Marketing Administrator" && s.RoleName != "Sales Administrator" && s.RoleName != "Marketing");
            }
                
            return Json(new
            {
                success = true,
                response = response.Roles
            }, JsonRequestBehavior.AllowGet);
        }

        public bool IsAccountAdminExist()
        {
            IsAccountAdminExistResponse response = userService.IsAccountAdminExist(new IsAccountAdminExistRequest()
            {
                AccountId = this.Identity.ToAccountID()
            });
            return response.AccountAdminExist;
        }

        [SmarttouchAuthorize(AppModules.Users, AppOperations.Delete)]
        public ActionResult DeleteUsers(int[] id)
        {
            DeactivateUserRequest request = new DeactivateUserRequest();
            request.UserID = id;
            request.RequestedBy = this.Identity.ToUserID();
            request.AccountId = this.Identity.ToAccountID();
            userService.DeactivateUser(request);
            return Json(new
            {
                success = true,
                response = ""
            }, JsonRequestBehavior.AllowGet);
        }

        [SmarttouchAuthorize(AppModules.Users, AppOperations.Edit)]
        public ActionResult UpdateUserStatus(string userData)
        {
            UserStatusRequest request = JsonConvert.DeserializeObject<UserStatusRequest>(userData);
            request.AccountId = this.Identity.ToAccountID();
            userService.UpdateUsersStatus(request);
            InsertProfileAudit(request.UserID, UserAuditType.ProfileUpdate, null, this.Identity.ToUserID());
            return Json(new
            {
                success = true,
                response = ""
            }, JsonRequestBehavior.AllowGet);
        }

        [SmarttouchAuthorize(AppModules.Users, AppOperations.Edit)]
        public ActionResult UpdateUserRole(string userData)
        {
            ChangeRoleRequest request = JsonConvert.DeserializeObject<ChangeRoleRequest>(userData);
            request.AccountId = this.Identity.ToAccountID();
            userService.ChangeRole(request);
            InsertProfileAudit(request.UserID, UserAuditType.ProfileUpdate, null, this.Identity.ToUserID());
            return Json(new
            {
                success = true,
                response = ""
            }, JsonRequestBehavior.AllowGet);
        }

        [SmarttouchAuthorize(AppModules.Users, AppOperations.Edit)]
        public ActionResult ResendInvite(string userData)
        {
            int nUsers = 0;
            int existActive = 0;
            int accountId = this.Identity.ToAccountID();
            ChangeRoleRequest request = JsonConvert.DeserializeObject<ChangeRoleRequest>(userData);
            string filename = EmailTemplate.ResendInvite.ToString() + ".txt";
            for (int i = 0; i < request.UserID.Count(); i++)
            {
                GetUserResponse response = userService.GetUser(new GetUserRequest(request.UserID[i]));
                if (string.IsNullOrEmpty(response.User.Password) && response.User.Status == 2)
                {
                    AccountSubscriptionData userSubscription = userService.GetSubscriptionData(accountId);
                    if (userSubscription.Limit.HasValue)
                    {
                        int usercount = userService.GetActiveUserIds(accountId, request.UserID);
                        if (usercount > userSubscription.Limit)
                        {
                            userService.UserLimitEmailNotification(accountId);
                            throw new UnsupportedOperationException("[|Maximum number of Users reached. Please contact Help Desk for assistance at helpdesk@smarttouchinteractive.com.|]");
                        }
                    }

                    var name = response.User.FirstName + " " + response.User.LastName;
                    SendEmail(response.User.UserID.ToString(), response.User.AccountID.ToString(), response.User.PrimaryEmail, name, filename, this.Identity.ToAccountPrimaryEmail(), "ResendInvite");
                    nUsers++;
                }
                else
                {
                    existActive = 1;
                }
            }
            if (nUsers > 0)
            {
                if (existActive != 0)
                    return Json(new
                    {
                        success = true,
                        response = "[|Email successfully sent to|] " + nUsers + "[|User(s)|]" + "</br>" + "[|Email can not be sent to Active User(s)|]"
                    }, JsonRequestBehavior.AllowGet);
                else
                    return Json(new
                    {
                        success = true,
                        response = "[|Email successfully sent to|] " + nUsers + " [|User(s)|]"
                    }, JsonRequestBehavior.AllowGet);
            }
            else
                throw new UnsupportedOperationException("[|Please select Unregistered User(s)|]");
        }

        public void AddCookie(string cookieName, string Value, int days)
        {
            HttpCookie CartCookie = new HttpCookie(cookieName, Value);
            CartCookie.Expires = DateTime.Now.AddDays(days);
            Response.Cookies.Add(CartCookie);
        }

        [HttpPost]
        public async Task<JsonResult> ChangePassword(string currentPassword, string newPassword)
        {
            string email = this.Identity.ToUserEmail();
            int accountId = this.Identity.ToAccountID();
            var user = await UserManager.FindByIdAsync(this.Identity.ToUserID().ToString());
            if (user == null)
            {
                return Json(new
                {
                    success = false,
                    response = "[|User does not exist|]"
                }, JsonRequestBehavior.AllowGet);
            }
            IdentityResult result = await UserManager.ChangePasswordAsync(user.Id, currentPassword, newPassword, email, accountId);
            if (result.Succeeded)
            {
                /*await UserManager.UpdateSecurityStampAsync(user.Id);*/
                return Json(new
                {
                    success = true,
                    response = ""
                }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                string responseJson = string.Join(", ", result.Errors);
                throw new Exception(responseJson);
            }
        }

        [HttpPost]
        public async Task<JsonResult> CheckPassword(string password)
        {
            var user = await UserManager.FindByIdAsync(this.Identity.ToUserID().ToString());
            if (user == null)
            {
                return Json(new
                {
                    success = false,
                    response = "[|User does not exist|]"
                }, JsonRequestBehavior.AllowGet);
            }
            bool isPasswordCorrect = await UserManager.CheckPasswordAsync(user, password);
            if (isPasswordCorrect)
            {
                return Json(new
                {
                    success = true,
                    response = ""
                }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new
                {
                    success = true,
                    response = "[|Please enter correct password|]"
                }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetUserTimeZone()
        {
            return Json(Identity.ToIanaTimeZone(), JsonRequestBehavior.AllowGet);
        }

        #endregion

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

        [SmarttouchAuthorize(AppModules.Users, AppOperations.Edit)]
        public ActionResult SaveUserPassword(string userData)
        {
            InsertUserPasswordRequest request = JsonConvert.DeserializeObject<InsertUserPasswordRequest>(userData);
            request.RequestedBy = this.Identity.ToUserID();
            request.AccountId = this.Identity.ToAccountID();
            ValidatePassword(request.Password);
            if (!string.IsNullOrEmpty(request.Password))
                request.Password = GetPasswordHash(request.Password);
            userService.InsertUserResetPassword(request);
            return Json(new
            {
                success = true,
                response = ""
            }, JsonRequestBehavior.AllowGet);
        }

        private static string GetPasswordHash(string password)
        {
            string hashedPassword = string.Empty;
            if (!string.IsNullOrEmpty(password))
            {
                PasswordHasher passwordHasher = new PasswordHasher();
                hashedPassword = passwordHasher.HashPassword(password);
            }
            return hashedPassword;
        }

        private static string ValidatePassword(string password)
        {
            int RequiredLength = 6;
            if (String.IsNullOrEmpty(password) || password.Length < RequiredLength)
            {
                throw new UnsupportedOperationException("Password should be of length 6 characters");
            }
            if (!Regex.IsMatch(password, @"\d"))
            {
                throw new UnsupportedOperationException("Password should contain at least one digit.");
            }
            if (!Regex.IsMatch(password, @"(?=.*[a-z])"))
            {
                throw new UnsupportedOperationException("Password should contain at least one lower-case letter.");
            }
            if (!Regex.IsMatch(password, @"(?=.*[A-Z])"))
            {
                throw new UnsupportedOperationException("Password should contain at least one upper-case letter.");
            }
            if (!Regex.IsMatch(password, @"(?=.*[_\W])"))
            {
                throw new UnsupportedOperationException("Password should contain at least one special character.");
            }
            return null;
        }

        #region Social Media Actions
        private Uri GetReidrectUri(CommunicationType type)
        {
            var uriBuilder = new UriBuilder(Request.Url);
            uriBuilder.Fragment = null;
            uriBuilder.Query = uriBuilder.Fragment;
            if (type == CommunicationType.Facebook)
            {
                uriBuilder.Path = Url.Action("FacebookCallback");
            }
            else if (type == CommunicationType.Twitter)
            {
                uriBuilder.Path = Url.Action("TwitterCallback");
            }
            return uriBuilder.Uri;
        }

        #region Facebook Actions
        /// <summary>
        /// Authorize Facebook APP
        /// </summary>
        /// <returns></returns>
        [SmarttouchSessionStateBehaviour(SessionStateBehavior.Required)]
        public ActionResult AuthorizeFacebook()
        {
            Session["buster"] = Guid.NewGuid();
            var loginUrl = socialIntegrationService.GetFacebookLoginUri(GetReidrectUri(CommunicationType.Facebook), Session["buster"].ToString(), this.Identity.ToUserID());
            return Redirect(loginUrl.AbsoluteUri);
        }

        /// <summary>
        /// Capture the response from Facebook Callback
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult FacebookCallback(string code)
        {
            if (!string.IsNullOrEmpty(code))
            {
                string accessToken = socialIntegrationService.GetFacebookAccessTokenByCode(GetReidrectUri(CommunicationType.Facebook), code, Session["buster"].ToString(), this.Identity.ToUserID());
                userService.UpdateFacebookAccessToken(new UpdateFacebookConnectionRequest()
                {
                    UserId = this.Identity.ToUserID(),
                    FacebookAccessToken = accessToken
                });
            }
            return RedirectToAction("MyProfile");
        }

        /// <summary>
        /// Disconnect user from Facebook App
        /// </summary>
        /// <returns></returns>
        public ActionResult DisconnectFacebook()
        {
            socialIntegrationService.RevokeFacebookConnection(this.Identity.ToUserID());
            userService.UpdateFacebookAccessToken(new UpdateFacebookConnectionRequest()
            {
                UserId = this.Identity.ToUserID(),
                FacebookAccessToken = null
            });
            return Json(new
            {
                Data = true
            }, JsonRequestBehavior.AllowGet);
        }

        #endregion
        #region Twitter Actions
        /// <summary>
        /// Get Twitter login uri async
        /// </summary>
        /// <param name="redirectUri"></param>
        /// <returns></returns>
        private async Task<ActionResult> GetTwitterLoginUriAsync(Uri redirectUri, int uid)
        {
            var userResponse = userService.GetUser(new GetUserRequest(uid));
            var user = userResponse.User;
            var auth = new MvcAuthorizer
            {
                CredentialStore = new SessionStateCredentialStore
                {
                    ConsumerKey = user.Account.TwitterAPIKey,
                    ConsumerSecret = user.Account.TwitterAPISecret,
                    OAuthToken = null,
                    OAuthTokenSecret = null
                }
            };
            return await auth.BeginAuthorizationAsync(redirectUri);
            ;
        }

        /// <summary>
        /// Authorize Twitter App
        /// </summary>
        /// <returns></returns>
        public Task<ActionResult> AuthorizeTwitter()
        {
            return GetTwitterLoginUriAsync(GetReidrectUri(CommunicationType.Twitter), this.Identity.ToUserID());
        }

        /// <summary>
        /// Capture the response coming from Twitter Callback
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult TwitterCallback()
        {
            var auth = socialIntegrationService.GetTwitterTokens(Request.Url);
            userService.UpdateTwitterOAuthTokens(new UpdateTwitterConnectionRequest()
            {
                UserId = this.Identity.ToUserID(),
                TwitterOAuthToken = auth.CredentialStore.OAuthToken,
                TwitterOAuthTokenSecret = auth.CredentialStore.OAuthTokenSecret
            });
            return RedirectToAction("MyProfile");
        }

        /// <summary>
        /// Disconnect user from Twitter App
        /// </summary>
        /// <returns></returns>
        public ActionResult DisconnectTwitter()
        {
            userService.UpdateTwitterOAuthTokens(new UpdateTwitterConnectionRequest()
            {
                UserId = this.Identity.ToUserID(),
                TwitterOAuthToken = null,
                TwitterOAuthTokenSecret = null
            });
            return Json(new
            {
                Data = true
            }, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #endregion
    }
}

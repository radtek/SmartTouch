using LandmarkIT.Enterprise.Utilities.Caching;
using LandmarkIT.Enterprise.Utilities.ExceptionHandling;
using LandmarkIT.Enterprise.Utilities.Logging;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using SmartTouch.CRM.ApplicationServices.Messaging.Accounts;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.Users;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Identity;
using SmartTouch.CRM.Web.Utilities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;
using Thinktecture.IdentityModel.Client;

namespace SmartTouch.CRM.Web.Controllers
{
    public class LoginController : SmartTouchController
    {
        private ApplicationUserManager _userManager;

        readonly IAccountService accountService;

        readonly IUrlService urlService;

        readonly ICachingService cachingService;

        public LoginController(IAccountService accountService, IUrlService urlService, ICachingService cachingService)
        {
            this.accountService = accountService;
            this.urlService = urlService;
            this.cachingService = cachingService;
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

        #region Login
        [AllowAnonymous]
        [OutputCache(Duration = 30)]
        public ActionResult Login(string returnUrl, string message, string modelMessage)
        {
            Logger.Current.Informational("In Login returnUrl " + returnUrl + ", message:" + message);
            if (Request.IsAuthenticated)
            {
                GetAccountAuthorizationRequest request = new GetAccountAuthorizationRequest();
                request.name = DomainName;
                GetAccountAuthorizationResponse response = accountService.GetAccountByDomainUrl(request);
                if (response.Exception != null)
                {
                    ExceptionHandler.Current.HandleException(response.Exception, DefaultExceptionPolicies.LOG_ONLY_POLICY);
                    ModelState.AddModelError("", "[|Invalid Account.|]");
                    return View();
                }
                cachingService.AddAccountPermissions(response.AccountId);
                cachingService.AddUserPermissions(response.AccountId);
                return RedirectToLocal("", Thread.CurrentPrincipal.Identity.ToRoleID(), Thread.CurrentPrincipal.Identity.ToAccountID(), "", "");
            }
            if (!String.IsNullOrEmpty(modelMessage))
                ModelState.AddModelError("", modelMessage);
            var defaultHelpURL = ConfigurationManager.AppSettings["helpURL"].ToString();
            ViewBag.SecurityMessage = message;
            if (!string.IsNullOrEmpty(returnUrl) && returnUrl.ToLower().Contains("logoff"))
                returnUrl = null;
            Logger.Current.Verbose("Request for login using domainurl:" + DomainName);
            ViewBag.Page = "Login";
            string loginPage = "Login";
            string masterUrl = DomainName;
            if (!string.IsNullOrWhiteSpace(DomainName))
            {
                GetAccountAuthorizationRequest request = new GetAccountAuthorizationRequest();
                request.name = DomainName;
                var accountID = default(int);
                GetAccountAuthorizationResponse response = accountService.GetAccountByDomainUrl(request);
                GetSubscriptionSettingsRequest ssRequest = new GetSubscriptionSettingsRequest();
                if (response != null)
                {
                    ssRequest.SubscriptionId = response.SubscriptionId;
                }
                GetSubscriptionSettingsResponse ssResponse = accountService.GetSubscriptionSettings(ssRequest);
                masterUrl = ssResponse.SubscriptionSettings.Where(p => p.SubscriptionSettingType == SubscriptionSettingTypes.Master).Select(p => p.Value).FirstOrDefault();
                if (masterUrl != null && masterUrl != DomainName)
                    return Redirect("https://" + masterUrl + "/Login/?modelMessage=" + modelMessage);
                loginPage = ssResponse.SubscriptionSettings.Where(p => p.SubscriptionSettingType == SubscriptionSettingTypes.Login).Select(p => p.Value).FirstOrDefault();
                ViewBag.LoginUrl = masterUrl == null ? DomainName : masterUrl;
                ViewBag.LoginPage = loginPage;
                if (response.Exception != null)
                {
                    ExceptionHandler.Current.HandleException(response.Exception, DefaultExceptionPolicies.LOG_ONLY_POLICY);
                    ModelState.AddModelError("", "[|Invalid Account.|]");
                    return View();
                }
                if (response != null)
                {
                    cachingService.AddAccountPermissions(response.AccountId);
                    Logger.Current.Informational("AccountId :" + response.AccountId);
                    Logger.Current.Informational("Account Name :" + response.AccountName);
                    accountID = response.AccountId;
                    this.Response.Cookies.Add(new HttpCookie("helpURL", !string.IsNullOrEmpty(response.HelpURL) ? response.HelpURL : defaultHelpURL));
                }
                LoginViewModel loginViewModel = new LoginViewModel();
                loginViewModel.AccountId = accountID;
                if (response.SubscriptionId == (int)AccountSubscription.Standard || response.SubscriptionId == (int)AccountSubscription.STAdmin)
                {
                    loginViewModel.AccountName = response.AccountName;
                }
                if (returnUrl != null)
                {
                    ViewBag.ReturnUrl = returnUrl;
                }
                ViewBag.AccountID = accountID;
                ViewBag.AccountName = response.AccountName;
                if (response.Status == 3)
                {
                    AccountViewModel account = cachingService.GetAccount(accountID);
                    ViewBag.AccountName = account.AccountName;
                    ViewBag.ImageSrc = account.Image == null ? "" : account.Image.ImageContent;
                    ViewBag.StatusMessage = account.StatusMessage;
                    return View("~/Views/Error/Suspended.cshtml");
                }
                else if (response.Status == 5)
                {
                    AccountViewModel account = cachingService.GetAccount(accountID);
                    ViewBag.AccountName = account.AccountName;
                    ViewBag.ImageSrc = account.Image == null ? "" : account.Image.ImageContent;
                    ViewBag.StatusMessage = account.StatusMessage;
                    return View("~/Views/Error/Maintenance.cshtml");
                }
                return View(loginPage, loginViewModel);
            }
            else
            {
                return View(loginPage);
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            Logger.Current.Informational("In Login View Model");
            if (!string.IsNullOrEmpty(returnUrl) && returnUrl.ToLower().Contains("logoff"))
                returnUrl = null;
            LoginViewModel loginViewModel = model;
            ViewBag.Page = "Login";
            Logger.Current.Informational("Are these LoginViewModel fields valid : " + ModelState.IsValid);
            if (ModelState.IsValid)
            {
                try
                {
                    var userName = loginViewModel.Email + "|" + loginViewModel.AccountId;
                    var user = await UserManager.FindAsync(userName, loginViewModel.Password);
                    if (user != null)
                    {
                        Logger.Current.Informational("Based on credentials UserId is :" + user.Id);
                        var tokenEndpoint = string.Format("{0}/token", ConfigurationManager.AppSettings["WEBSERVICE_URL"]);
                        var sw = new System.Diagnostics.Stopwatch();
                        sw.Start();
                        var clientId = System.Web.Configuration.WebConfigurationManager.AppSettings["SMARTTOUCH_APIKEY"].ToString();
                        var client = new OAuth2Client(new Uri(tokenEndpoint), clientId, "");
                        var accessToken = client.RequestResourceOwnerPasswordAsync(userName, loginViewModel.Password).Result.AccessToken;
                        sw.Stop();
                        var timeelapsed = sw.Elapsed;
                        Logger.Current.Informational("time elapsed to fetch token:" + timeelapsed);
                        if (!string.IsNullOrEmpty(accessToken))
                        {
                            if (this.Response.Cookies["accessToken"] == null)
                                this.Response.Cookies.Add(new HttpCookie("accessToken", accessToken));
                            else
                                this.Response.Cookies.Set(new HttpCookie("accessToken", accessToken));
                            await SignInAsync(user, loginViewModel.RememberMe);
                            int userID;
                            int.TryParse(user.Id, out userID);
                            string IP = Request.UserHostAddress;
                            UserManager.InsertLoginAudit(userID, (int)model.AccountId, IP, SignInActivity.SignIn);
                            cachingService.AddAccountPermissions((int)model.AccountId);
                            cachingService.AddUserPermissions((int)model.AccountId);
                            cachingService.AddDropdownValues(model.AccountId);
                            bool showTC = accountService.ShowTC(new ShowTCRequest()
                            {
                                AccountId = (int)model.AccountId
                            }).ShowTC;
                            UserSettings userSettings = accountService.GetFirstLoginUserSettings(new GetFirstLoginUserSettingsRequest()
                            {
                                RequestedBy = userID
                            }).UserSettings;
                            if (userSettings != null && !userSettings.HasAcceptedTC && showTC)
                                AddCookie("ShowTC", "1", 1);
                            else
                                AddCookie("ShowTC", "0", 1);
                            if (user.HasTourCompleted.HasValue && user.HasTourCompleted.Value)
                                AddCookie("IsFirstLogin", 1.ToString(), 1);
                            else if (user.HasTourCompleted == null || (user.HasTourCompleted.HasValue && !user.HasTourCompleted.Value))
                                AddCookie("IsFirstLogin", 0.ToString(), 1);
                            return RedirectToLocal(returnUrl, user.RoleID, (int)model.AccountId, model.Email, model.Password);
                        }
                        else
                        {
                            Logger.Current.Informational("Invalid API Key.");
                            ModelState.AddModelError("", "[|Invalid API Key.|]");
                            return RedirectToAction("Login", new RouteValueDictionary(new
                            {
                                controller = "Login",
                                action = "Login",
                                modelMessage = "[|Invalid API Key.|]"
                            }));
                        }
                    }
                    else
                    {
                        Logger.Current.Informational("Requested user not found");
                        ModelState.AddModelError("", "[|Invalid username or password.|]");
                        return RedirectToAction("Login", new RouteValueDictionary(new
                        {
                            controller = "Login",
                            action = "Login",
                            modelMessage = "[|Invalid username or password.|]"
                        }));
                    }
                }
                catch (Exception ex)
                {
                    ExceptionHandler.Current.HandleException(ex, DefaultExceptionPolicies.LOG_ONLY_POLICY);
                    ModelState.AddModelError("", "[|An error has occurred, please try again later.|]");
                    return RedirectToAction("Login", new RouteValueDictionary(new
                    {
                        controller = "Login",
                        action = "Login",
                        modelMessage = "[|An error has occurred, please try again later.|]"
                    }));
                }
            }
            else
            {
                ModelState.AddModelError("", "[|Invalid details|]");
                return RedirectToAction("Login", new RouteValueDictionary(new
                {
                    controller = "Login",
                    action = "Login",
                    modelMessage = "[|Invalid details|]"
                }));
            }
        }

        #endregion
        #region Register
        [Route("register")]
        [AllowAnonymous]
        public ActionResult Register(string userId, string accountId, string emailId)
        {
            try
            {
                if (String.IsNullOrEmpty(userId))
                    userId = TempData["UserId"] as string;
                if (String.IsNullOrEmpty(accountId))
                    accountId = TempData["AccountId"] as string;
                if (String.IsNullOrEmpty(emailId))
                    emailId = TempData["EmailId"] as string;
                if (userId == null || accountId == null)
                {
                    Logger.Current.Informational("While registering we got userId or accountID as null.");
                    ModelState.AddModelError("", "[|Invalid details|]");
                    return View();
                }
                RegisterViewModel registerViewModel = new RegisterViewModel();
                registerViewModel.AccountId = Convert.ToInt32(accountId);
                registerViewModel.UserId = userId;
                registerViewModel.Email = emailId;
                string masterUrl = string.Empty;
                string registerPage = string.Empty;
                GetAccountSubscriptionDataRequest datarequest = new GetAccountSubscriptionDataRequest();
                datarequest.AccountId = Convert.ToInt32(accountId);
                GetAccountSubscriptionDataResponse response = accountService.GetSubscriptionDataByAccountID(datarequest);
                GetSubscriptionSettingsRequest ssRequest = new GetSubscriptionSettingsRequest();
                ssRequest.SubscriptionId = response.AccountSubscriptionData.SubscriptionID;
                GetSubscriptionSettingsResponse ssResponse = accountService.GetSubscriptionSettings(ssRequest);
                masterUrl = ssResponse.SubscriptionSettings.Where(p => p.SubscriptionSettingType == SubscriptionSettingTypes.Master).Select(p => p.Value).FirstOrDefault();
                AddCookie("accountUrl", response.AccountSubscriptionData.AccountUrl.ToString(), 1);
                if (masterUrl != null && masterUrl != DomainName)
                {
                    return Redirect("https://" + masterUrl + "/Register?userId=" + userId + "&accountId=" + accountId + "&emailId=" + emailId);
                }
                registerPage = ssResponse.SubscriptionSettings.Where(p => p.SubscriptionSettingType == SubscriptionSettingTypes.Register).Select(p => p.Value).FirstOrDefault();
                ViewBag.Page = "Register";
                return View(registerPage, registerViewModel);
            }
            catch (Exception ex)
            {
                ExceptionHandler.Current.HandleException(ex, DefaultExceptionPolicies.LOG_ONLY_POLICY);
                ModelState.AddModelError("", "[|An error has occurred, please try again later.|]");
                return View();
            }
        }

        [Route("register"), HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> Register(RegisterViewModel model, string returnUrl)
        {
            RegisterViewModel registerViewModel = model;
            Logger.Current.Informational("Are these RegisterViewModel fields valid : " + ModelState.IsValid);
            if (ModelState.IsValid)
            {
                try
                {
                    var user = new IdentityUser()
                    {
                        Id = registerViewModel.UserId,
                        Password = registerViewModel.Password,
                        UserName = registerViewModel.Email,
                        Email = new Email()
                        {
                            EmailId = registerViewModel.AccountId + "|" + registerViewModel.Email,
                            IsVerified = false
                        },
                        AccountID = registerViewModel.AccountId
                    };
                    IdentityResult result = await UserManager.AddPasswordAsync(user.Id, user.Password);
                    if (result.Succeeded)
                    {
                        var userName = registerViewModel.Email + "|" + registerViewModel.AccountId;
                        var newUser = await UserManager.FindByNameAsync(userName);
                        int userID;
                        int.TryParse(model.UserId, out userID);
                        string IP = Request.UserHostAddress;
                        UserManager.InsertLoginAudit(userID, model.AccountId, IP, SignInActivity.SignIn);
                        UserManager.InsertUserProfileAudit(userID, UserAuditType.PasswordChange, userID, model.Password);
                        AddCookie("IsFirstLogin", 0.ToString(), 1);
                        bool showTC = accountService.ShowTC(new ShowTCRequest()
                        {
                            AccountId = (int)model.AccountId
                        }).ShowTC;
                        UserSettings userSettings = accountService.GetFirstLoginUserSettings(new GetFirstLoginUserSettingsRequest()
                        {
                            RequestedBy = userID
                        }).UserSettings;
                        if (userSettings != null && !userSettings.HasAcceptedTC && showTC)
                            AddCookie("ShowTC", "1", 1);
                        else
                            AddCookie("ShowTC", "0", 1);
                        await SignInAsync(user, registerViewModel.RememberMe);
                        return RedirectToLocal(returnUrl, newUser.RoleID, model.AccountId, "", "");
                    }
                    else
                    {
                        Logger.Current.Informational("Adding password for the user failed" + result.Errors);
                        AddErrors(result);
                    }
                }
                catch (Exception ex)
                {
                    ExceptionHandler.Current.HandleException(ex, DefaultExceptionPolicies.LOG_ONLY_POLICY);
                    ModelState.AddModelError("", "[|An error has occurred, please try again later.|]");
                    return View(model);
                }
            }
            return View(model);
        }

        #endregion
        #region ForgotPassword
        [Route("forgotpassword")]
        [AllowAnonymous]
        public ActionResult ForgotPassword(string email)
        {
            ForgotPasswordViewModel forgotPasswordViewModel = new ForgotPasswordViewModel();
            string forgotPasswordPage = "";
            try
            {
                Logger.Current.Verbose("Request for Forgot Password using domainurl:" + DomainName);
                GetAccountAuthorizationRequest request = new GetAccountAuthorizationRequest();
                request.name = DomainName;
                var accountID = default(int);
                GetAccountAuthorizationResponse response = accountService.GetAccountByDomainUrl(request);
                GetSubscriptionSettingsRequest ssRequest = new GetSubscriptionSettingsRequest();
                if (response != null)
                {
                    ssRequest.SubscriptionId = response.SubscriptionId;
                }
                GetSubscriptionSettingsResponse ssResponse = accountService.GetSubscriptionSettings(ssRequest);
                forgotPasswordPage = ssResponse.SubscriptionSettings.Where(p => p.SubscriptionSettingType == SubscriptionSettingTypes.ForgotPassword).Select(p => p.Value).FirstOrDefault();
                ViewBag.PageName = forgotPasswordPage;
                if (response.Exception != null)
                {
                    ExceptionHandler.Current.HandleException(response.Exception, DefaultExceptionPolicies.LOG_ONLY_POLICY);
                    ModelState.AddModelError("", "Invalid Account.");
                    return View();
                }
                if (response != null)
                {
                    Logger.Current.Informational("AccountId :" + response.AccountId);
                    Logger.Current.Informational("Account Name :" + response.AccountName);
                    accountID = response.AccountId;
                }
                forgotPasswordViewModel.AccountId = accountID;
                if (response.SubscriptionId == (int)AccountSubscription.Standard || response.SubscriptionId == (int)AccountSubscription.STAdmin)
                    forgotPasswordViewModel.AccountName = response.AccountName;
                forgotPasswordViewModel.AccountPrimaryEmail = response.PrimaryEmail;
                forgotPasswordViewModel.Email = email;
            }
            catch (Exception ex)
            {
                ExceptionHandler.Current.HandleException(ex, DefaultExceptionPolicies.LOG_ONLY_POLICY);
                ModelState.AddModelError("", "[|An error has occurred, please try again later.|]");
                return View(forgotPasswordPage, forgotPasswordViewModel);
            }
            return View(forgotPasswordPage, forgotPasswordViewModel);
        }

        [Route("forgotpassword"), HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model, string ReturnUrl)
        {
            try
            {
                Logger.Current.Informational("Are these ForgotPasswordViewModel fields valid : " + ModelState.IsValid);
                if (ModelState.IsValid)
                {
                    ForgotPasswordViewModel forgotPasswordViewModel = model;
                    var userName = forgotPasswordViewModel.Email + "|" + forgotPasswordViewModel.AccountId;
                    var user = await UserManager.FindByNameAsync(userName);
                    if (user != null)
                    {
                        int userID;
                        int.TryParse(user.Id, out userID);
                        string IP = Request.UserHostAddress;
                        UserManager.InsertLoginAudit(userID, model.AccountId, IP, SignInActivity.ForgotPassword);
                        Logger.Current.Informational("Based on details the UserId is:" + user.Id);
                        string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                        var body = "";
                        string filename = EmailTemplate.ForgotPassword.ToString() + ".txt";
                        string savedFileName = string.Empty;
                        Logger.Current.Informational("The path for ForgotPassword Email template is :" + savedFileName);
                        string imagesUrl = ConfigurationManager.AppSettings["IMAGE_HOSTING_SERVICE_URL"];
                        string accountLogo = string.Empty;
                        string accountName = string.Empty;
                        string subject = string.Empty;
                        ApplicationServices.Messaging.Accounts.GetAccountImageStorageNameResponse response = accountService.GetStorageName(new ApplicationServices.Messaging.Accounts.GetAccountImageStorageNameRequest()
                        {
                            AccountId = model.AccountId
                        });
                        if (response.AccountLogoInfo.SubscriptionId == 3)
                        {
                            string templateName = EmailTemplate.BDXForgotPassword.ToString() + ".txt";
                            savedFileName = Path.Combine(System.Configuration.ConfigurationManager.AppSettings["EMAILTEMPLATES_PHYSICAL_PATH"].ToString(), templateName);
                            subject = "BDX Lead Management - Password Reset Alert - " + response.AccountLogoInfo.AccountName + " : " + user.FirstName + " " + user.LastName;
                        }
                        else
                        {
                            savedFileName = Path.Combine(System.Configuration.ConfigurationManager.AppSettings["EMAILTEMPLATES_PHYSICAL_PATH"].ToString(), filename);
                        }
                        if (!String.IsNullOrEmpty(response.AccountLogoInfo.StorageName))
                            accountLogo = urlService.GetUrl(model.AccountId, ImageCategory.AccountLogo, response.AccountLogoInfo.StorageName);
                        else
                            accountLogo = "";
                        accountName = response.AccountLogoInfo.AccountName;
                        string accountImage = string.Empty;
                        if (!string.IsNullOrEmpty(accountLogo))
                        {
                            accountImage = accountImage + "<td align='right' valign='center' style='margin:0px;padding:0px 0px 25px 0px;'><img src='" + accountLogo + "' alt='" + accountName + "' style='width:100px;' width='100'> </td>";
                        }
                        else
                        {
                            accountImage = "";
                        }
                        if (response.AccountLogoInfo.SubscriptionId != 3)
                            subject = "SmartTouch Password Reset Alert - " + accountName + " : " + user.FirstName + " " + user.LastName;
                        var protectedUserEmail = MachineKey.Protect(GetBytes(user.Email.EmailId));
                        var securedUserEmail = Convert.ToBase64String(protectedUserEmail);
                        var protectedAccountName = MachineKey.Protect(GetBytes(model.AccountName));
                        var securedAccountName = Convert.ToBase64String(protectedAccountName);
                        string accountAddress = accountService.GetPrimaryAddress(new GetAddressRequest()
                        {
                            AccountId = model.AccountId
                        }).Address;
                        string accountPhoneNumber = accountService.GetPrimaryPhone(new GetPrimaryPhoneRequest()
                        {
                            AccountId = model.AccountId
                        }).PrimaryPhone;
                        using (StreamReader reader = new StreamReader(savedFileName))
                        {
                            do
                            {
                                body = reader.ReadToEnd().Replace("[ACCOUNT]", Url.Encode(securedAccountName)).Replace("[GUID]", code).Replace("[FNAME]", user.FirstName).Replace("[LNAME]", user.LastName).Replace("[UNAME]", user.Email.EmailId).Replace("[USERID]", Url.Encode(securedUserEmail)).Replace("[STURL]", Request.Url.Authority.ToLower()).Replace("[ADDRESS]", accountAddress).Replace("[PHONE]", accountPhoneNumber).Replace("[IMAGES_URL]", imagesUrl).Replace("[AccountImage]", accountImage).Replace("[AccountName]", accountName);
                            }
                            while (!reader.EndOfStream);
                        }
                        Dictionary<Guid, string> providerDetails = accountService.GetTransactionalProviderDetails(model.AccountId);
                        if (providerDetails.FirstOrDefault().Key != null)
                            await UserManager.SendEmailAsync(user.Id, subject + "|" + model.AccountPrimaryEmail + "|" + model.AccountId + "|" + providerDetails.FirstOrDefault().Key.ToString() + "|" + Request.Url.Host.ToLower(), body);
                        UserManager.UpdateResetFlag(userID, true);
                        ViewBag.Message = "Email has been sent to you. Please check your Email.";
                        return View(ReturnUrl, model);
                    }
                    if (user == null || !(await UserManager.ConfirmEmailAsync(user.Id, string.Empty)).Succeeded)
                    {
                        ModelState.AddModelError("", "The user either does not exist or is not confirmed.");
                        return View(ReturnUrl, model);
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionHandler.Current.HandleException(ex, DefaultExceptionPolicies.LOG_ONLY_POLICY);
                ModelState.AddModelError("", "[|An error has occurred, please try again later.|]");
                return View(model);
            }
            return View(model);
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

        #endregion
        #region ResetPassword
        [Route("resetpassword")]
        [AllowAnonymous]
        public ActionResult ResetPassword(string userEmail, string code, string accountname)
        {
            ResetPasswordViewModel resetPasswordViewModel = new ResetPasswordViewModel();
            var resetpasswordpage = "ResetPassword";
            try
            {
                var securedUserEmail = Convert.FromBase64String(userEmail);
                var decryptedUserId = MachineKey.Unprotect(securedUserEmail);
                var securedAccountName = Convert.FromBase64String(accountname);
                var decryptedAccountName = MachineKey.Unprotect(securedAccountName);
                userEmail = GetString(decryptedUserId);
                accountname = GetString(decryptedAccountName);
                if (userEmail == null || code == null || accountname == null)
                {
                    Logger.Current.Informational("Any one or more of the parameters  is null.");
                    ModelState.AddModelError("", "Invalid details");
                    return View(resetPasswordViewModel);
                }
                string modifiedCode = code.Replace(" ", "+");
                GetAccountAuthorizationRequest request = new GetAccountAuthorizationRequest();
                request.name = DomainName;
                var accountID = default(int);
                GetAccountAuthorizationResponse response = accountService.GetAccountByDomainUrl(request);
                if (response.Exception != null)
                {
                    ExceptionHandler.Current.HandleException(response.Exception, DefaultExceptionPolicies.LOG_ONLY_POLICY);
                    ModelState.AddModelError("", "Invalid Account.");
                    return View(resetPasswordViewModel);
                }
                GetSubscriptionSettingsRequest ssRequest = new GetSubscriptionSettingsRequest();
                if (response != null)
                {
                    accountID = response.AccountId;
                    ssRequest.SubscriptionId = response.SubscriptionId;
                }
                GetSubscriptionSettingsResponse ssResponse = accountService.GetSubscriptionSettings(ssRequest);
                resetpasswordpage = ssResponse.SubscriptionSettings.Where(p => p.SubscriptionSettingType == SubscriptionSettingTypes.ResetPassword).Select(p => p.Value).FirstOrDefault();
                resetPasswordViewModel.AccountId = accountID;
                resetPasswordViewModel.AccountName = accountname;
                resetPasswordViewModel.Email = userEmail;
                resetPasswordViewModel.Code = modifiedCode;
            }
            catch (Exception ex)
            {
                ExceptionHandler.Current.HandleException(ex, DefaultExceptionPolicies.LOG_ONLY_POLICY);
                ModelState.AddModelError("", "[|An error has occurred, please try again later.|]");
                return View(resetpasswordpage, resetPasswordViewModel);
            }
            return View(resetpasswordpage, resetPasswordViewModel);
        }

        [Route("resetpassword"), HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model, string returnUrl)
        {
            Logger.Current.Informational("Are these ResetPasswordViewModel fields valid : " + ModelState.IsValid);
            try
            {
                if (ModelState.IsValid)
                {
                    string modifiedCode = model.Code.Replace(" ", "+");
                    var userName = model.Email + "|" + model.AccountId;
                    var user = await UserManager.FindByNameAsync(userName);
                    if (user != null)
                    {
                        int userID;
                        int.TryParse(user.Id, out userID);
                        bool flag = UserManager.CheckResetFlag(userID, model.AccountId.ToString());
                        if (flag)
                        {
                            if (user == null)
                            {
                                ModelState.AddModelError("", "The user either does not exist or is not confirmed.");
                                return View(model);
                            }
                            var result = await UserManager.ResetPasswordAsync(user.Id, modifiedCode, model.NewPassword, model.Email, model.AccountId);
                            if (result.Succeeded)
                            {
                                string IP = Request.UserHostAddress;
                                UserManager.InsertLoginAudit(userID, model.AccountId, IP, SignInActivity.ResetPassword);
                                UserManager.InsertLoginAudit(userID, model.AccountId, IP, SignInActivity.SignIn);
                                await SignInAsync(user, isPersistent: false);
                                return RedirectToLocal(returnUrl, user.RoleID, model.AccountId, "", "");
                            }
                            Logger.Current.Informational("Adding password for the user failed" + result.Errors);
                            AddErrors(result);
                            return View(model);
                        }
                    }
                    if (user == null)
                    {
                        ModelState.AddModelError("", "The user either does not exist or is not confirmed.");
                        return View(model);
                    }
                    ModelState.AddModelError("", "Your password reset link has expired.");
                    return View(model);
                }
                else
                    return View(model);
            }
            catch (Exception ex)
            {
                ExceptionHandler.Current.HandleException(ex, DefaultExceptionPolicies.LOG_ONLY_POLICY);
                ModelState.AddModelError("", "[|An error has occurred, please try again later.|]");
                return View(model);
            }
        }

        #endregion
        #region ChangePassword
        [AllowAnonymous]
        public async Task<JsonResult> ChangePassword(int userId, string currentPassword, string newPassword)
        {
            var user = await UserManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return Json(new
                {
                    success = false,
                    response = "User does not exist"
                }, JsonRequestBehavior.AllowGet);
            }
            IdentityResult result = await UserManager.ChangePasswordAsync(user.Id, currentPassword, newPassword);
            if (result.Succeeded)
            {
                return Json(new
                {
                    success = true,
                    response = ""
                }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                string responseJson = result.ToString();
                return Json(new
                {
                    success = false,
                    response = responseJson
                }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion
        public async Task<JsonResult> CheckPassword(int userId, string password)
        {
            var user = await UserManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return Json(new
                {
                    success = false,
                    response = "User does not exist"
                }, JsonRequestBehavior.AllowGet);
            }
            bool isPasswordCorrect = await UserManager.CheckPasswordAsync(user, password);
            return Json(new
            {
                success = true,
                response = isPasswordCorrect ? "" : "Please enter correct password"
            }, JsonRequestBehavior.AllowGet);
        }

        [Route("confirmemail")]
        [AllowAnonymous]
        [SmarttouchSessionStateBehaviour(SessionStateBehavior.Required)]
        public async Task<ActionResult> ConfirmEmail(string userId, string code, string accountID, string emailId)
        {
            Logger.Current.Verbose("Request received for validating code with the given userId");
            try
            {
                var securedUserId = Convert.FromBase64String(userId);
                var decryptedUserId = MachineKey.Unprotect(securedUserId);
                userId = GetString(decryptedUserId);
                var securedAccountId = Convert.FromBase64String(accountID);
                var decryptedAccountId = MachineKey.Unprotect(securedAccountId);
                accountID = GetString(decryptedAccountId);
                var securedEmailId = Convert.FromBase64String(emailId);
                var decryptedEmailId = MachineKey.Unprotect(securedEmailId);
                emailId = GetString(decryptedEmailId);
                string modifiedCode = code.Replace(" ", "+");
                if (userId == null || code == null || accountID == null || emailId == null)
                {
                    Logger.Current.Informational("One or more of the parameters is null.");
                    RegisterViewModel model = new RegisterViewModel();
                    ModelState.AddModelError("", "[|Invalid details|]");
                    return View("Register", model);
                }
                var result = await UserManager.ConfirmEmailAsync(userId, modifiedCode);
                if (result.Succeeded)
                {
                    TempData["UserId"] = userId;
                    TempData["AccountId"] = accountID;
                    TempData["EmailId"] = emailId;
                    return RedirectToAction("Register", "Login");
                }
                else
                {
                    Logger.Current.Informational("An error occured while confirming your email :" + result.Errors);
                    RegisterViewModel model = new RegisterViewModel();
                    ModelState.AddModelError("", "[|Invalid link|]");
                    return View("Register", model);
                }
            }
            catch (Exception ex)
            {
                ExceptionHandler.Current.HandleException(ex, DefaultExceptionPolicies.LOG_ONLY_POLICY);
                RegisterViewModel model = new RegisterViewModel();
                ModelState.AddModelError("", "[|Invalid link|]");
                return View("Register", model);
            }
        }

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private async Task SignInAsync(IdentityUser user, bool isPersistent)
        {
            try
            {
                Logger.Current.Informational("Logging out..user id: " + user.Id);
                AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
                HttpCookie CartCookie = new HttpCookie("IsPersistent", isPersistent.ToString());
                CartCookie.Expires = DateTime.Now.AddDays(1);
                Response.Cookies.Add(CartCookie);
                string accountLogoUrl = "false";
                ClaimsIdentity claimsIdentity = await user.GenerateUserIdentityAsync(UserManager);
                GetAccountImageStorageNameResponse response = accountService.GetStorageName(new GetAccountImageStorageNameRequest()
                {
                    AccountId = user.AccountID
                });
                if (response != null)
                {
                    accountLogoUrl = response.AccountLogoInfo.StorageName != null ? urlService.GetUrl(user.AccountID, ImageCategory.AccountLogo, response.AccountLogoInfo.StorageName) : accountLogoUrl;
                    claimsIdentity.AddClaim(new Claim("AccountLogoUrl", accountLogoUrl));
                    claimsIdentity.AddClaim(new Claim("AccountName", response.AccountLogoInfo.AccountName));
                    HttpCookie helpURL = new HttpCookie("helpURL", response.AccountLogoInfo.HelpURL);
                    helpURL.Expires = DateTime.Now.AddDays(1);
                    Response.Cookies.Add(helpURL);
                }
                AuthenticationManager.SignIn(new AuthenticationProperties()
                {
                    IsPersistent = isPersistent
                }, claimsIdentity);
            }
            catch (Exception ex)
            {
                ExceptionHandler.Current.HandleException(ex, DefaultExceptionPolicies.LOG_ONLY_POLICY);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl, short roleId, int accountId, string uName, string pWord)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                if (roleId == 1)
                    return RedirectToRoute("accounts");
                else
                {
                    var usersPermissions = cachingService.GetUserPermissions(accountId);
                    List<byte> userModules = usersPermissions.Where(s => s.RoleId == roleId).Select(s => s.ModuleId).ToList();
                    if (userModules.Contains((byte)AppModules.Dashboard))
                    {
                        return RedirectToAction("DashboardList", "Dashboard");
                    }
                    else
                        return RedirectToRoute("contacts");
                }
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        [HttpGet]
        public ActionResult LogOff(string logoffmessage)
        {
            var sortType = ReadCookie("LogoutToken");
            int userID = this.Identity.ToUserID();
            int accountID = this.Identity.ToAccountID();
            string IP = Request.UserHostAddress;
            AuthenticationManager.SignOut();
            UserManager.InsertLoginAudit(userID, accountID, IP, SignInActivity.SignOut);
            if (logoffmessage != null)
            {
                return RedirectToAction("Login", new
                {
                    message = logoffmessage
                });
            }
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetExpires(DateTime.UtcNow.AddHours(-1));
            Response.Cache.SetNoStore();
            RemoveCookie("MessagesCookie");
            Response.Cookies.Add(new HttpCookie("LogoutTokenResult", sortType));
            return RedirectToAction("Login");
        }

        public string ReadCookie(string strValue)
        {
            string strValues = string.Empty;
            if (Request.Cookies[strValue] != null)
            {
                strValues = Request.Cookies[strValue].Value;
            }
            return strValues;
        }

        /// <summary>
        /// Adds the cookie.
        /// </summary>
        /// <param name="cookieName">Name of the cookie.</param>
        /// <param name="Value">The value.</param>
        /// <param name="days">The days.</param>
        public void AddCookie(string cookieName, string Value, int days, string subDomain = "")
        {
            System.Web.HttpCookie CartCookie = new System.Web.HttpCookie(cookieName, Value);
            CartCookie.Expires = DateTime.Now.AddDays(days);
            if (!string.IsNullOrEmpty(subDomain))
            {
                string[] hostParts = new System.Uri(subDomain).Host.Split('.');
                string domain = String.Join(".", hostParts.Skip(Math.Max(0, hostParts.Length - 2)).Take(2));
                CartCookie.Domain = domain;
            }
            var cookie = Response.Cookies.Get(cookieName);
            if (cookie == null)
                Response.Cookies.Add(CartCookie);
            else
                Response.Cookies.Set(CartCookie);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _userManager != null)
            {
                _userManager.Dispose();
                _userManager = null;
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Remove Cookie
        /// </summary>
        /// <param name="cookieName"></param>
        public void RemoveCookie(string cookieName)
        {
            if (Request.Cookies[cookieName] != null)
            {
                System.Web.HttpCookie myCookie = new System.Web.HttpCookie(cookieName);
                myCookie.Expires = DateTime.Now.AddDays(-1d);
                Response.Cookies.Add(myCookie);
            }
        }
    }
}

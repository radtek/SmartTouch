using Kendo.Mvc.UI;
using Newtonsoft.Json;
using SmartTouch.CRM.ApplicationServices.Messaging.Accounts;
using SmartTouch.CRM.ApplicationServices.Messaging.LeadScore;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Web.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Mvc;
using SmartTouch.CRM.ApplicationServices.Messaging.LeadAdapters;
using System.Net;
using SmartTouch.CRM.ApplicationServices.Messaging.Role;
using System.Configuration;
using System.Linq;
using SmartTouch.CRM.ApplicationServices.Messaging.Communication;
using LandmarkIT.Enterprise.Utilities.Logging;
using LandmarkIT.Enterprise.Utilities.ExceptionHandling;
using SmartTouch.CRM.ApplicationServices.Messaging.ImageDomain;
using LandmarkIT.Enterprise.Extensions;
using System.Text.RegularExpressions;
using System.Net.Http;
using SmartTouch.CRM.Domain.Accounts;
using SmartTouch.CRM.ApplicationServices.Messaging.User;
using SmartTouch.CRM.Domain.Roles;

namespace SmartTouch.CRM.Web.Controllers
{
    public class AccountController : SmartTouchController
    {
        IAccountService accountService;
        ILeadScoreRuleService leadScoreService;
        ILeadAdapterService leadAdapterService;
        IRoleService roleService;
        IDropdownValuesService dropDownValuesService;
        ICachingService cachingService;
        ICommunicationService communicationService;
        IUrlService urlService;
        IImageDomainService imageDomainService;
        IUserService userService;

        public AccountController(IAccountService accountService, ILeadScoreRuleService leadScoreService, ILeadAdapterService leadAdapterService, IRoleService roleService, IDropdownValuesService dropDownValuesService, ICachingService cachingService, ICommunicationService communicationService, IUrlService urlService, IImageDomainService imageDomainService, IUserService userService)
        {
            this.accountService = accountService;
            this.leadScoreService = leadScoreService;
            this.leadAdapterService = leadAdapterService;
            this.roleService = roleService;
            this.urlService = urlService;
            this.dropDownValuesService = dropDownValuesService;
            this.cachingService = cachingService;
            this.communicationService = communicationService;
            this.imageDomainService = imageDomainService;
            this.userService = userService;
        }

        #region Account
        /// <summary>
        /// Accounts the list.
        /// </summary>
        /// <returns></returns>
        [Route("accounts")]
        [SmarttouchAuthorize(AppModules.Accounts, AppOperations.Read)]
        [MenuType(MenuCategory.Accounts, MenuCategory.LeftMenuAccountConfiguration)]
        [OutputCache(Duration = 30)]
        public ActionResult AccountList()
        {
            AccountViewModel viewModel = new AccountViewModel();
            ViewBag.accountId = 0;
            short ItemsPerPage = default(short);
            short.TryParse(this.Identity.ToItemsPerPage(), out ItemsPerPage);
            ViewBag.ItemsPerPage = ItemsPerPage;
            return View("AccountList", viewModel);
        }

        /// <summary>
        /// Gets the reputation count.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns></returns>
        public ActionResult GetReputationCount(int accountId, DateTime startDate, DateTime endDate)
        {
            GetSenderReputationCountResponse response = accountService.GetReputationCount(new GetSenderReputationCountRequest()
            {
                AccountId = accountId,
                StartDate = ToUserUtcDateTime(startDate),
                EndDate = ToUserUtcDateTime(endDate)
            });
            return Json(new
            {
                success = true,
                response = response
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// To the user UTC date time.
        /// </summary>
        /// <param name="d">The d.</param>
        /// <returns></returns>
        private static DateTime ToUserUtcDateTime(DateTime d)
        {
            return d.ToJsonSerailizedDate().ToUserUtcDateTime();
        }

        /// <summary>
        /// Accounts the search.
        /// </summary>
        /// <returns></returns>
        [Route("accounts/search")]
        [SmarttouchAuthorize(AppModules.Accounts, AppOperations.Read)]
        [MenuType(MenuCategory.Accounts, MenuCategory.LeftMenuAccountConfiguration)]
        public ActionResult AccountSearch()
        {
            ViewBag.accountId = 1;
            short ItemsPerPage = default(short);
            short.TryParse(this.Identity.ToItemsPerPage(), out ItemsPerPage);
            ViewBag.ItemsPerPage = ItemsPerPage;
            AccountViewModel viewModel = new AccountViewModel();
            return View("AccountList", viewModel);
        }

        /// <summary>
        /// Gets the time zones.
        /// </summary>
        /// <returns></returns>
        public JsonResult GetTimeZones()
        {
            var tzs = TimeZoneInfo.GetSystemTimeZones();
            var timeZones = new List<TimeZoneViewModel>();
            var pattern = "\\w+(\\-|\\+)\\d+:\\d+|UTC";
            var regex = new Regex(pattern);
            foreach (var tz in tzs)
            {
                TimeZoneViewModel tzsInfo = new TimeZoneViewModel();
                if (tz.IsDaylightSavingTime(DateTime.Now))
                {
                    var name = tz.DisplayName;
                    var utcoffset = tz.GetUtcOffset(DateTime.Now);
                    var hours = utcoffset.Hours.ToString("+00;-00");
                    var minutes = utcoffset.Minutes.ToString("00").Replace("-", "");
                    string displayUtc = hours + ":" + minutes;
                    if (utcoffset.Ticks == 0)
                    {
                        tzsInfo.DisplayName = regex.Replace(name, "UTC");
                    }
                    else
                    {
                        tzsInfo.DisplayName = regex.Replace(name, "UTC" + displayUtc);
                    }
                    tzsInfo.Id = tz.Id;
                }
                else
                {
                    tzsInfo.DisplayName = tz.DisplayName;
                    tzsInfo.Id = tz.Id;
                }
                timeZones.Add(tzsInfo);
            }
            return Json(new
            {
                success = true,
                response = timeZones
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Accountses the view read.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="name">The name.</param>
        /// <param name="status">The status.</param>
        /// <returns></returns>
        [SmarttouchAuthorize(AppModules.Accounts, AppOperations.Read)]
        public ActionResult AccountsViewRead([DataSourceRequest] DataSourceRequest request, string name, string status)
        {
            byte accountstatus = 0;
            if (!String.IsNullOrEmpty(status))
                accountstatus = Convert.ToByte(status);
            AddCookie("accountpagesize", request.PageSize.ToString(), 1);
            AddCookie("accountpagenumber", request.Page.ToString(), 1);
            var sortField = request.Sorts.Count > 0 ? request.Sorts.First().Member : "AccountName";
            var direction = request.Sorts.Count > 0 ? request.Sorts.First().SortDirection : System.ComponentModel.ListSortDirection.Ascending;
            GetAccountsResponse response = accountService.GetAllAccounts(new GetAccountsRequest()
            {
                Query = name,
                Limit = request.PageSize,
                PageNumber = request.Page,
                Status = accountstatus,
                RequestedBy = this.Identity.ToUserID(),
                SortField = sortField,
                SortDirection = direction
            });
            return Json(new DataSourceResult
            {
                Data = response.Accounts,
                Total = response.TotalHits
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Adds the cookie.
        /// </summary>
        /// <param name="cookieName">Name of the cookie.</param>
        /// <param name="Value">The value.</param>
        /// <param name="days">The days.</param>
        public void AddCookie(string cookieName, string Value, int days)
        {
            HttpCookie CartCookie = new HttpCookie(cookieName, Value);
            CartCookie.Expires = DateTime.Now.AddDays(days);
            Response.Cookies.Add(CartCookie);
        }

        /// <summary>
        /// Adds the account.
        /// </summary>
        /// <returns></returns>
        [Route("addaccount")]
        [SmarttouchAuthorize(AppModules.Accounts, AppOperations.Create)]
        [MenuType(MenuCategory.AccountAddAction, MenuCategory.LeftMenuAccountConfiguration)]
        public ActionResult AddAccount()
        {
            AccountViewModel viewModel = new AccountViewModel();
            IList<dynamic> phones = new List<dynamic>();
            IList<dynamic> socialMediaUrls = new List<dynamic>();
            IList<dynamic> dateFormat = new List<dynamic>();
            IList<dynamic> currency = new List<dynamic>();
            var dropdownValues = dropDownValuesService.GetAllByAccountID("", null).DropdownValuesViewModel.AsQueryable();
            viewModel.AddressTypes = dropdownValues.Where(s => s.DropdownID == 2).Select(s => s.DropdownValuesList).ToList().FirstOrDefault();
            var defaultAddresstypeID = viewModel.AddressTypes.Where(d => d.IsDefault).SingleOrDefault();
            viewModel.Addresses = new List<AddressViewModel>() {
                new AddressViewModel () {
                    AddressID = 0,
                    AddressTypeID = defaultAddresstypeID.DropdownValueID,
                    Country = new Country {
                        Code = "US"
                    },
                    State = new State {
                        Code = ""
                    },
                    IsDefault = true
                }
            };
            viewModel.Image = new ImageViewModel();
            phones.Add(new
            {
                PhoneType = "Mobile",
                PhoneNumber = ""
            });
            socialMediaUrls.Add(new
            {
                MediaType = "Website",
                Url = ""
            });
            viewModel.Phones = phones;
            viewModel.SocialMediaUrls = socialMediaUrls;
            viewModel.DateFormats = dateFormat;
            viewModel.Currency = currency;
            viewModel.CountryID = "US";
            viewModel.CurrencyID = 1;
            viewModel.CurrencyFormat = "$X,XXX.XX";
            viewModel.DateFormatID = 2;
            viewModel.Status = (byte)AccountStatus.Draft;
            viewModel.TimeZone = "Central Standard Time";
            viewModel.ShowTC = false;
            ViewBag.page = "Accounts";
            viewModel.OpportunityCustomers = (byte)OpportunityCustomers.People;
            viewModel.Modules = GetModulesList(null);
            viewModel.HelpURL = ConfigurationManager.AppSettings["helpURL"].ToString();
            GetRoleResponse response = userService.GetRoles(new GetRoleRequest()
            {
                AccountId = this.Identity.ToAccountID()
            });
            viewModel.Roles = (IEnumerable<Role>)response.Roles;
            ViewBag.AccountCreation = true;
            ViewBag.AccountsTitle = "Add Account";
            ViewBag.Domain = System.Configuration.ConfigurationManager.AppSettings["SMARTCRMDOMAIN"].ToString();
            ViewBag.NewAddress = new AddressViewModel()
            {
                AddressID = 0,
                AddressTypeID = defaultAddresstypeID.DropdownValueID,
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
            return View("AddEditAccount", viewModel);
        }

        /// <summary>
        /// Edits the account.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        [Route("editaccount")]
        [SmarttouchAuthorize(AppModules.Accounts, AppOperations.Edit)]
        [MenuType(MenuCategory.AccountEditAction, MenuCategory.LeftMenuAccountConfiguration)]
        public ActionResult EditAccount(int accountId)
        {
            ViewBag.AccountsTitle = "Edit Account";
            ViewBag.Domain = System.Configuration.ConfigurationManager.AppSettings["SMARTCRMDOMAIN"].ToString();
            ViewBag.Mode = "E";
            GetAccountResponse response = accountService.GetAccount(new GetAccountRequest(accountId)
            {
                RequestedBy = this.Identity.ToUserID(),
                AccountId = this.Identity.ToAccountID()
            });
            if (response.AccountViewModel != null)
            {
                response.AccountViewModel.Modules = GetModulesList(null);
                response.AccountViewModel.SubscribedModules = GetModulesList(accountId);
                response.AccountViewModel.CreatedOn = response.AccountViewModel.CreatedOn.ToUserUtcDateTimeV2();
                response.AccountViewModel.DateFormat = this.Identity.ToDateFormat();
                var dropdownValues = dropDownValuesService.GetAllByAccountID("", null).DropdownValuesViewModel.AsQueryable();
                response.AccountViewModel.AddressTypes = dropdownValues.Where(s => s.DropdownID == 2).Select(s => s.DropdownValuesList).ToList().FirstOrDefault();
                if (response.AccountViewModel.Addresses.Count == 0)
                {
                    response.AccountViewModel.Addresses = new List<AddressViewModel>() {
                        new AddressViewModel () {
                            AddressID = 0,
                            AddressTypeID = response.AccountViewModel.AddressTypes.Where (s => s.IsDefault).FirstOrDefault ().DropdownValueID,
                            State = new State {
                                Code = ""
                            },
                            Country = new Country {
                                Code = ""
                            }
                        }
                    };
                }
                ViewBag.page = "Accounts";
                if (response.AccountViewModel != null)
                {
                    if (response.AccountViewModel.OpportunityCustomers == null)
                        response.AccountViewModel.OpportunityCustomers = (byte)OpportunityCustomers.People;
                    if (response.AccountViewModel.Status == (byte)AccountStatus.Draft)
                        ViewBag.AccountCreation = true;
                    else
                        ViewBag.AccountCreation = false;
                }
                if (response.AccountViewModel.DomainURL != null && response.AccountViewModel.DomainURL.Contains('.'))
                    response.AccountViewModel.DomainURL = response.AccountViewModel.DomainURL.Substring(0, response.AccountViewModel.DomainURL.IndexOf('.'));
                ViewBag.NewAddress = new AddressViewModel()
                {
                    AddressID = 0,
                    AddressTypeID = response.AccountViewModel.AddressTypes.Where(s => s.IsDefault).FirstOrDefault().DropdownValueID,
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
                GetRoleResponse rolesResponse = userService.GetRoles(new GetRoleRequest()
                {
                    AccountId = accountId
                });
                response.AccountViewModel.Roles = (IEnumerable<Role>)rolesResponse.Roles;
            }
            return View("AddEditAccount", response.AccountViewModel);
        }

        /// <summary>
        /// Copies the account.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        [Route("copyaccount")]
        [SmarttouchAuthorize(AppModules.Accounts, AppOperations.Edit)]
        [MenuType(MenuCategory.AccountAddAction, MenuCategory.LeftMenuAccountConfiguration)]
        public ActionResult CopyAccount(int accountId)
        {
            ViewBag.Domain = System.Configuration.ConfigurationManager.AppSettings["SMARTCRMDOMAIN"].ToString();
            ViewBag.AccountCreation = true;
            ViewBag.AccountsTitle = "Copy Account";
            ViewBag.Mode = "C";
            GetAccountResponse response = accountService.GetAccount(new GetAccountRequest(accountId)
            {
                RequestedBy = this.Identity.ToUserID()
            });
            ViewBag.page = "Accounts";
            AccountViewModel viewmodel = accountService.CopyAccount(response.AccountViewModel);
            viewmodel.Image = new ImageViewModel();
            if (viewmodel != null)
            {
                viewmodel.SubscribedModules = null;
                viewmodel.Modules = GetModulesList(null);
                var dropdownValues = dropDownValuesService.GetAllByAccountID("", null).DropdownValuesViewModel.AsQueryable();
                viewmodel.AddressTypes = dropdownValues.Where(s => s.DropdownID == 2).Select(s => s.DropdownValuesList).ToList().FirstOrDefault();
                if (response.AccountViewModel.DomainURL != null && response.AccountViewModel.DomainURL.Contains('.'))
                    response.AccountViewModel.DomainURL = response.AccountViewModel.DomainURL.Substring(0, response.AccountViewModel.DomainURL.IndexOf('.'));
                if (response.AccountViewModel.Addresses.Count == 0)
                {
                    response.AccountViewModel.Addresses = new List<AddressViewModel>() {
                        new AddressViewModel () {
                            AddressID = 0,
                            AddressTypeID = response.AccountViewModel.AddressTypes.Where (s => s.IsDefault).FirstOrDefault ().DropdownValueID,
                            State = new State {
                                Code = ""
                            },
                            Country = new Country {
                                Code = ""
                            }
                        }
                    };
                }
            }
            return View("AddEditAccount", response.AccountViewModel);
        }

        /// <summary>
        /// Inserts the account.
        /// </summary>
        /// <param name="accountViewModel">The account view model.</param>
        /// <returns></returns>
        [SmarttouchAuthorize(AppModules.Accounts, AppOperations.Create)]
        public ActionResult InsertAccount(string accountViewModel)
        {
            AccountViewModel viewModel = JsonConvert.DeserializeObject<AccountViewModel>(accountViewModel);
            var user = this.Identity.ToUserID();
            viewModel.CreatedBy = user;
            viewModel.CreatedOn = DateTime.Now.ToUniversalTime();
            viewModel.ModifiedBy = user;
            viewModel.ModifiedOn = DateTime.Now.ToUniversalTime();
            if (viewModel.DomainURL != null)
                viewModel.DomainURL = viewModel.DomainURL + "" + System.Configuration.ConfigurationManager.AppSettings["SMARTCRMDOMAIN"].ToString();
            InsertAccountRequest request = new InsertAccountRequest()
            {
                AccountViewModel = viewModel,
                RequestedBy = this.Identity.ToUserID(),
                SSLKey = System.Configuration.ConfigurationManager.AppSettings["SSLSerialNumber"].ToString()
            };
            if (viewModel.Modules.Where(m => m.ModuleId == (byte)AppModules.Opportunity && m.IsSelected == false).Any())
                viewModel.OpportunityCustomers = null;
            InsertAccountResponse response = accountService.InsertAccount(request);
            var accountmodel = response.AccountViewModel;
            if (accountmodel != null && request.AccountViewModel.Status == (byte)AccountStatus.Active)
            {
                var name = accountmodel.FirstName + " " + accountmodel.LastName;
                string filename = EmailTemplate.AccountRegistration.ToString() + ".txt";
                SendEmail(accountmodel.AccountName, accountmodel.PrimaryEmail, name, filename, "SmartTouch New Account Creation Notification: " + accountmodel.AccountName + "", accountmodel.DomainURL,(byte)EmailNotificationsCategory.AccountRegistration);
            }
            return Json(new
            {
                success = true,
                response = ""
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Updates the account.
        /// </summary>
        /// <param name="accountViewModel">The account view model.</param>
        /// <returns></returns>
        /// <exception cref="LandmarkIT.Enterprise.Utilities.ExceptionHandling.UnsupportedOperationException">Account created successfully. +   + responseJson</exception>
        [HttpPost]
        [SmarttouchAuthorize(AppModules.Accounts, AppOperations.Edit)]
        [MenuType(MenuCategory.Accounts, MenuCategory.LeftMenuAccountConfiguration)]
        public JsonResult UpdateAccount(string accountViewModel)
        {
            AccountViewModel viewModel = JsonConvert.DeserializeObject<AccountViewModel>(accountViewModel);
            viewModel.ModifiedBy = this.Identity.ToUserID();
            viewModel.ModifiedOn = DateTime.Now.ToUniversalTime();
            viewModel.CreatedOn = viewModel.CreatedOn.ToUniversalTime();
            if (viewModel.DomainURL != null)
            {
                viewModel.DomainURL = viewModel.DomainURL + "" + System.Configuration.ConfigurationManager.AppSettings["SMARTCRMDOMAIN"].ToString();
                viewModel.PreviousDomainURL = viewModel.PreviousDomainURL + "" + System.Configuration.ConfigurationManager.AppSettings["SMARTCRMDOMAIN"].ToString();
            }
            if (viewModel.Modules.Where(m => m.ModuleId == (byte)AppModules.Opportunity && m.IsSelected == false).Any())
                viewModel.OpportunityCustomers = null;
            if (viewModel.Status == 3)
            {
                var name = viewModel.FirstName + " " + viewModel.LastName;
                string filename = string.Format("{0}.txt", EmailTemplate.AccountPause.ToString());
                string subject = string.Format("SmartTouch  Account Paused Notification: {0}", viewModel.AccountName);
                SendEmail(viewModel.AccountName, viewModel.PrimaryEmail, name, filename, subject, viewModel.DomainURL,(byte)EmailNotificationsCategory.AccountPause);
            }
            UpdateAccountRequest request = new UpdateAccountRequest()
            {
                AccountViewModel = viewModel,
                RequestedBy = this.Identity.ToUserID()
            };
            UpdateAccountResponse response = accountService.UpdateAccount(request);
            if (response.Exception == null)
            {
                cachingService.AddUserPermissions(viewModel.AccountID);
                cachingService.AddAccountPermissions(viewModel.AccountID);
                cachingService.AddDataSharingPermissions(viewModel.AccountID);
                if (viewModel.PreviousStatus == (byte)AccountStatus.Draft && viewModel.Status == (byte)AccountStatus.Active)
                {
                    string name = string.Format("{0} {1}", viewModel.FirstName, viewModel.LastName);
                    string filename = string.Format("{0}.txt", EmailTemplate.AccountRegistration.ToString());
                    string subject = string.Format("SmartTouch  Account Paused Notification : {0}", viewModel.AccountName);
                    try
                    {
                        SendEmail(viewModel.AccountName, viewModel.PrimaryEmail, name, filename, subject, viewModel.DomainURL, (byte)EmailNotificationsCategory.AccountPause);
                    }
                    catch (Exception ex)
                    {
                        Logger.Current.Error(string.Format("An error occurred while sending an email :{0}", ex));
                        response.Exception = ex;
                        string responseJson = response.Exception.Message.Replace("\r\n", "</br>").Replace("\n", "</br>");
                        throw new UnsupportedOperationException("Account created successfully." + " " + responseJson);
                    }
                }
            }
            return Json(new
            {
                success = true,
                response = ""
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Leads the score view read.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        [SmarttouchAuthorize(AppModules.LeadScore, AppOperations.Read)]
        public ActionResult LeadScoreViewRead([DataSourceRequest] DataSourceRequest request, string name)
        {
            AddCookie("leadscorepagesize", request.PageSize.ToString(), 1);
            AddCookie("leadscorepagenumber", request.Page.ToString(), 1);
            GetLeadScoreListResponse response = leadScoreService.GetLeadScoresList(new GetLeadScoreListRequest()
            {
                Query = name,
                Limit = request.PageSize,
                PageNumber = request.Page,
                AccountID = UserExtensions.ToAccountID(this.Identity)
            });
            return Json(new DataSourceResult
            {
                Data = response.LeadScoreViewModel,
                Total = response.TotalHits
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Deletes the account.
        /// </summary>
        /// <param name="accountData">The account data.</param>
        /// <returns></returns>
        /// <exception cref="LandmarkIT.Enterprise.Utilities.ExceptionHandling.UnsupportedOperationException">Account updated successfully. +   + responseJson</exception>
        [SmarttouchAuthorize(AppModules.Accounts, AppOperations.Delete)]
        public ActionResult DeleteAccount(string accountData)
        {
            AccountStatusUpdateRequest request = JsonConvert.DeserializeObject<AccountStatusUpdateRequest>(accountData);
            request.RequestedBy = this.Identity.ToUserID();
            AccountStatusUpdateResponse response = new AccountStatusUpdateResponse();
            response = accountService.UpdateAccountStatus(request);
            byte category = (byte)EmailNotificationsCategory.Default;
            if (response.Toemails.Count > 0)
            {
                string filename = "";
                string subject = "";
                if (request.StatusID == 3)
                {
                    filename = EmailTemplate.AccountPause.ToString() + ".txt";
                    subject = "SmartTouch Account Paused Notification:";
                    category = (byte)EmailNotificationsCategory.AccountPause;
                }
                else if (request.StatusID == 4)
                {
                    filename = EmailTemplate.AccountClose.ToString() + ".txt";
                    subject = "SmartTouch Account Closed Notification:";
                    category = (byte)EmailNotificationsCategory.AccountClose;
                }
                else if (request.StatusID == 6)
                {
                    filename = EmailTemplate.AccountClose.ToString() + ".txt";
                    subject = "SmartTouch Account Deleted Notification:";
                    category = (byte)EmailNotificationsCategory.AccountClose;
                }
                for (int i = 0; i < response.Toemails.Count; i++)
                {
                    var accountName = response.Toemails[i].Split('|')[1];
                    var emailID = response.Toemails[i].Split('|')[0];
                    try
                    {
                        if (request.StatusID != 5)
                        {
                            SendEmail(accountName, emailID, "", filename, subject + accountName, Request.Url.Authority.ToLower(), category);
                        }
                    }
                    catch (Exception ex)
                    {
                        response.Exception = ex;
                        string responseJson = response.Exception.Message.Replace("\r\n", "</br>").Replace("\n", "</br>");
                        throw new UnsupportedOperationException("Account updated successfully." + " " + responseJson);
                    }
                }
            }
            return Json(new
            {
                success = true,
                response = ""
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Sends the email.
        /// </summary>
        /// <param name="account">The account.</param>
        /// <param name="emailID">The email identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="filename">The filename.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="domainurl">The domainurl.</param>
        /// <returns></returns>
        /// <exception cref="LandmarkIT.Enterprise.Utilities.ExceptionHandling.UnsupportedOperationException">
        /// Transactional service providers are not configured for this account
        /// or
        /// An error occured while sending an email
        /// </exception>
        public LandmarkIT.Enterprise.CommunicationManager.Responses.SendMailResponse SendEmail(string account, string emailID, string name, string filename, string subject, string domainurl, byte category = (byte)EmailNotificationsCategory.Default)
        {
            var body = "";
            string savedFileName = Path.Combine(System.Configuration.ConfigurationManager.AppSettings["EMAILTEMPLATES_PHYSICAL_PATH"].ToString(), filename);
            string imagesUrl = ConfigurationManager.AppSettings["IMAGE_HOSTING_SERVICE_URL"];
            string accountLogo = string.Empty;
            using (StreamReader reader = new StreamReader(savedFileName))
            {
                do
                {
                    body = reader.ReadToEnd().Replace("[ACCOUNT]", account).Replace("[NAME]", name).Replace("[EMAILID]", emailID).Replace("[DOMAINURL]", domainurl).Replace("[IMAGES_URL]", imagesUrl).Replace("[AccountImage]", accountLogo);
                }
                while (!reader.EndOfStream);
            }
            LandmarkIT.Enterprise.CommunicationManager.Operations.MailService mailService = new LandmarkIT.Enterprise.CommunicationManager.Operations.MailService();
            LandmarkIT.Enterprise.CommunicationManager.Requests.SendMailRequest request = new LandmarkIT.Enterprise.CommunicationManager.Requests.SendMailRequest();
            try
            {
                string fromEmail = this.Identity.ToAccountPrimaryEmail();
                Dictionary<Guid, string> providerDetails = accountService.GetTransactionalProviderDetails(this.Identity.ToAccountID());
                if (providerDetails != null && providerDetails.Count > 0)
                {
                    var fromEmailId = !string.IsNullOrEmpty(providerDetails.FirstOrDefault().Value) ? providerDetails.FirstOrDefault().Value : fromEmail;
                    request.TokenGuid = providerDetails.FirstOrDefault().Key;
                    request.RequestGuid = Guid.NewGuid();
                    request.ScheduledTime = DateTime.Now.ToUniversalTime();
                    request.Subject = subject;
                    request.Body = body;
                    request.To = new List<string>() {
                        emailID
                    };
                    request.IsBodyHtml = true;
                    request.From = fromEmailId;
                    request.ServiceProviderEmail = fromEmailId;
                    request.AccountDomain = Request.Url.Host;
                    request.CategoryID = (byte)category;
                    request.AccountID = 0;
                    return mailService.Send(request);
                }
                else
                    throw new UnsupportedOperationException("Transactional service providers are not configured for this account");
            }
            catch (Exception ex)
            {
                throw new UnsupportedOperationException(string.Format("An error occurred while sending an email: {0}", ex));
            }
        }

        /// <summary>
        /// Checks the domain URL.
        /// </summary>
        /// <param name="domainURL">The domain URL.</param>
        /// <returns></returns>
        [Route("checkdomainurl")]
        [HttpGet]
        public ActionResult CheckDomainURL(string domainURL)
        {
            if (domainURL != null)
            {
                CheckDomainURLAvailabilityRequest request = new CheckDomainURLAvailabilityRequest()
                {
                    DomainURL = domainURL
                };
                CheckDomainURLAvailabilityResponse response = accountService.IsDomainURLExist(request);
                return Json(new
                {
                    success = true,
                    response = response
                }, JsonRequestBehavior.AllowGet);
            }
            else
                return null;
        }

        /// <summary>
        /// Gets the account privacy policy.
        /// </summary>
        /// <returns></returns>
        public JsonResult GetAccountPrivacyPolicy()
        {
            string privacyPolicy = accountService.GetAccountPrivacyPolicy(this.Identity.ToAccountID());
            return Json(privacyPolicy, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Gets the active image domains.
        /// </summary>
        /// <returns></returns>
        public ActionResult GetActiveImageDomains()
        {
            GetImageDomainsRequest imageDomainsRequest = new GetImageDomainsRequest();
            GetImageDomainsResponse response = imageDomainService.GetActiveImageDomains(imageDomainsRequest);
            return Json(new
            {
                success = true,
                response = response.ImageDomains
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Senders the reputation.
        /// </summary>
        /// <param name="reportType">Type of the report.</param>
        /// <param name="reportId">The report identifier.</param>
        /// <param name="runReportResults">if set to <c>true</c> [run report results].</param>
        /// <param name="AccountId">The account identifier.</param>
        /// <returns></returns>
        [Route("senderreputation")]
        public ActionResult SenderReputation(byte reportType, int reportId, bool runReportResults, int AccountId)
        {
            ViewBag.DateFormat = this.Identity.ToDateFormat();
            short ItemsPerPage = default(short);
            short.TryParse(this.Identity.ToItemsPerPage(), out ItemsPerPage);
            ViewBag.ItemsPerPage = ItemsPerPage;
            if (runReportResults == true)
                ViewBag.RunReportResults = true;
            else
                ViewBag.RunReportResults = false;
            ReportViewModel reportviewModel = new ReportViewModel();
            reportviewModel.AccountId = AccountId;
            reportviewModel.ReportId = reportId;
            ViewBag.ReportType = null;
            ViewBag.ReportId = null;
            reportviewModel.ReportName = "Campaign List";
            return View("senderreputation", reportviewModel);
        }

        /// <summary>
        /// Gets the health report.
        /// </summary>
        /// <returns></returns>
        [Route("healthreport")]
        [SmarttouchAuthorize(AppModules.Accounts, AppOperations.Read)]
        public ActionResult GetHealthReport()
        {
            List<ContactGroup> contactsList = accountService.GetAllContactsByAccount(this.Identity.ToAccountID());
            using (HttpClient client = new HttpClient())
            {
                UriBuilder url = new UriBuilder(ConfigurationManager.AppSettings["WEBSERVICE_URL"].ToString());
                HttpCookie seenAlertsCookie = Request.Cookies["accessToken"];
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + seenAlertsCookie.Value);
                HttpResponseMessage httpResponse = client.GetAsync(url + "/accounts").Result;
                string status = httpResponse.StatusCode.ToString() == "OK" ? "Success" : "Failure";
                ViewBag.ApiResponce = status;
                AccountHealthReport result = accountService.GetAccountReportData();
                ViewBag.Contacts = contactsList;
                ViewBag.Campaigns = result.Campaigns;
                ViewBag.Forms = result.Forms;
                ViewBag.Emails = result.Emails;
                ViewBag.CampaignSent = result.CampaignSent;
                ViewBag.FailedImports = result.FailedImports;
                ViewBag.SucceededImports = result.SucceededImports;
                ViewBag.InProgressImport = result.InProgressImport;
                ViewBag.FailedLeadAdapter = result.FailedLeadAdapter;
                ViewBag.SucceededLeadAdapters = result.SucceededLeadAdapters;
                ViewBag.ContactLeadScore = result.ContactLeadScore;
                ViewBag.WorkFlow = result.HealthWorkflowReport;
                ViewBag.WebsiteResponce = CheckWebsite();
                return View("HealthReport");
            }
        }

        /// <summary>
        /// Checks the website.
        /// </summary>
        /// <returns></returns>
        private static string CheckWebsite()
        {
            string str = ConfigurationManager.AppSettings["WEB_URL"];
            UriBuilder builder = new UriBuilder(str);
            Uri resultUri = builder.Uri;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(resultUri);
            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
            string status = response.StatusCode.ToString() == "OK" ? "Success" : "Failure";
            return status;
        }

        public JsonResult GetTC(int accountId)
        {
            string terms = string.Empty;
            if (accountId != 0)
                terms = accountService.GetTermsAndConditions(new GetTermsAndConditionsRequest()
                {
                    AccountId = accountId
                }).TC;
            return Json(terms, JsonRequestBehavior.AllowGet);
        }

        public JsonResult UpdateTCAcceptance(int userId)
        {
            int userID = userId != 0 ? userId : this.Identity.ToUserID();
            accountService.UpdateTCAcceptance(new UpdateTCAcceptanceRequest()
            {
                RequestedBy = userID,
                AccountId = this.Identity.ToAccountID()
            });
            return Json(new
            {
                success = true,
                response = ""
            }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Getusers(int accountId)
        {
            GetUserListResponse response = userService.GetAllUsers(new GetUserListRequest()
            {
                Query = string.Empty,
                Limit = 100,
                PageNumber = 1,
                Status = 1,
                Role = 0,
                AccountID = accountId,
                IsSTAdmin = true
            });
            return Json(new DataSourceResult
            {
                Data = response.Users,
                Total = response.TotalHits
            }, JsonRequestBehavior.AllowGet);
        }

        #region leadadapter
        /// <summary>
        /// Adds the lead adapter.
        /// </summary>
        /// <returns></returns>
        [SmarttouchAuthorize(AppModules.LeadAdapter, AppOperations.Create)]
        public ActionResult AddLeadAdapter()
        {
            LeadAdapterViewModel viewModel = new LeadAdapterViewModel();
            viewModel.AccountID = UserExtensions.ToAccountID(this.Identity);
            return View("AddEditLeadAdapter", viewModel);
        }

        /// <summary>
        /// Edits the lead adapter.
        /// </summary>
        /// <param name="leadAdapterID">The lead adapter identifier.</param>
        /// <returns></returns>
        [SmarttouchAuthorize(AppModules.LeadAdapter, AppOperations.Edit)]
        public ActionResult EditLeadAdapter(int leadAdapterID)
        {
            ViewBag.Mode = "E";
            LeadAdapterViewModel viewModel = new LeadAdapterViewModel();
            GetLeadAdapterResponse response = leadAdapterService.GetLeadAdapter(new GetLeadAdapterRequest(leadAdapterID));
            if (response != null)
            {
                viewModel = response.LeadAdapterViewModel;
            }
            return View("AddEditLeadAdapter", viewModel);
        }

        /// <summary>
        /// Views the lead adapter.
        /// </summary>
        /// <param name="leadAdapterID">The lead adapter identifier.</param>
        /// <returns></returns>
        [SmarttouchAuthorize(AppModules.LeadAdapter, AppOperations.Read)]
        public ActionResult ViewLeadAdapter(int leadAdapterID)
        {
            ViewBag.Mode = "E";
            LeadAdapterViewModel viewModel = new LeadAdapterViewModel();
            GetLeadAdapterResponse response = leadAdapterService.GetLeadAdapter(new GetLeadAdapterRequest(leadAdapterID));
            if (response != null)
            {
                viewModel = response.LeadAdapterViewModel;
            }
            return View("ViewLeadAdapter", viewModel);
        }

        /// <summary>
        /// Inserts the lead adapter.
        /// </summary>
        /// <param name="leadAdapterViewModel">The lead adapter view model.</param>
        /// <returns></returns>
        [SmarttouchAuthorize(AppModules.LeadAdapter, AppOperations.Create)]
        public ActionResult InsertLeadAdapter(string leadAdapterViewModel)
        {
            LeadAdapterViewModel viewModel = JsonConvert.DeserializeObject<LeadAdapterViewModel>(leadAdapterViewModel);
            viewModel.AccountID = this.Identity.ToAccountID();
            viewModel.CreatedBy = this.Identity.ToUserID();
            viewModel.CreatedDateTime = DateTime.Now.ToUniversalTime();
            viewModel.ModifiedBy = this.Identity.ToUserID();
            viewModel.ModifiedDateTime = DateTime.Now.ToUniversalTime();
            InsertLeadAdapterRequest request = new InsertLeadAdapterRequest()
            {
                LeadAdapterViewModel = viewModel
            };
            InsertLeadAdapterResponse response = leadAdapterService.InsertLeadAdapter(request);
            if (response.Exception != null)
            {
                string responseJson = response.Exception.Message.Replace("\r\n", "</br>").Replace("\n", "</br>");
                return Json(new
                {
                    success = false,
                    response = responseJson
                }, JsonRequestBehavior.AllowGet);
            }
            return Json(new
            {
                success = true,
                response = ""
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Updates the lead adapter.
        /// </summary>
        /// <param name="leadAdapterViewModel">The lead adapter view model.</param>
        /// <returns></returns>
        [SmarttouchAuthorize(AppModules.LeadAdapter, AppOperations.Edit)]
        public ActionResult UpdateLeadAdapter(string leadAdapterViewModel)
        {
            LeadAdapterViewModel viewModel = JsonConvert.DeserializeObject<LeadAdapterViewModel>(leadAdapterViewModel);
            viewModel.ModifiedBy = this.Identity.ToUserID();
            viewModel.ModifiedDateTime = DateTime.Now.ToUniversalTime();
            UpdateLeadAdapterRequest request = new UpdateLeadAdapterRequest()
            {
                LeadAdapterViewModel = viewModel
            };
            UpdateLeadAdapterResponse response = leadAdapterService.UpdateLeadAdapter(request);
            if (response.Exception != null)
            {
                string responseJson = response.Exception.Message.Replace("\r\n", "</br>").Replace("\n", "</br>");
                return Json(new
                {
                    success = false,
                    response = responseJson
                }, JsonRequestBehavior.AllowGet);
            }
            return Json(new
            {
                success = true,
                response = ""
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Gets the lead adapter types.
        /// </summary>
        /// <returns></returns>
        public JsonResult GetLeadAdapterTypes()
        {
            return Json(new
            {
                success = true,
                reponse = ""
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Leads the adapters view read.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        [SmarttouchAuthorize(AppModules.LeadAdapter, AppOperations.Read)]
        public ActionResult LeadAdaptersViewRead([DataSourceRequest] DataSourceRequest request, string name)
        {
            AddCookie("leadadapterpagesize", request.PageSize.ToString(), 1);
            AddCookie("leadadapterpagenumber", request.Page.ToString(), 1);
            GetLeadAdapterListResponse response = leadAdapterService.GetAllLeadAdapters(new GetLeadAdapterListRequest()
            {
                Query = name,
                Limit = request.PageSize,
                PageNumber = request.Page,
                AccountID = UserExtensions.ToAccountID(this.Identity)
            });
            return Json(new DataSourceResult
            {
                Data = response.LeadAdapters,
                Total = response.TotalHits
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Deletes the lead adapter.
        /// </summary>
        /// <param name="leadAdapterID">The lead adapter identifier.</param>
        /// <returns></returns>
        [SmarttouchAuthorize(AppModules.LeadAdapter, AppOperations.Delete)]
        public JsonResult DeleteLeadAdapter(int leadAdapterID)
        {
            DeleteLeadAdapterRequest request = new DeleteLeadAdapterRequest(leadAdapterID);
            DeleteLeadAdapterResponse response = leadAdapterService.DeleteLeadAdapter(request);
            if (response.Exception != null)
            {
                string responseJson = response.Exception.Message.Replace("\r\n", "</br>").Replace("\n", "</br>");
                return Json(new
                {
                    success = false,
                    response = responseJson
                }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new
                {
                    success = true,
                    reponse = ""
                }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion
        #endregion

        #region AccountSettings
        /// <summary>
        /// Accounts the settings.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="LandmarkIT.Enterprise.Utilities.ExceptionHandling.UnsupportedOperationException">An error occured</exception>
        [Route("accountsettings")]
        [SmarttouchAuthorize(AppModules.AccountSettings, AppOperations.Read)]
        [MenuType(MenuCategory.SaveSettings, MenuCategory.LeftMenuAccountSettings)]
        public ActionResult AccountSettings()
        {
            try
            {
                ViewBag.Domain = System.Configuration.ConfigurationManager.AppSettings["SMARTCRMDOMAIN"].ToString();
                int accountId = this.Identity.ToAccountID();
                ViewBag.Mode = "E";
                GetAccountRequest request = new GetAccountRequest(accountId)
                {
                    RequestedBy = this.Identity.ToUserID(),
                    AccountId = this.Identity.ToAccountID(),
                    RoleId = this.Identity.ToRoleID(),
                    RequestBySTAdmin = this.Identity.IsSTAdmin()
                };
                GetAccountResponse response = accountService.GetAccount(request);
                if (response.AccountViewModel != null)
                {
                    var dropdownValues = dropDownValuesService.GetAllByAccountID("", null).DropdownValuesViewModel.AsQueryable();
                    response.AccountViewModel.AddressTypes = dropdownValues.Where(s => s.DropdownID == 2).Select(s => s.DropdownValuesList).ToList().FirstOrDefault();
                    if (response.AccountViewModel.Addresses.Count == 0)
                    {
                        response.AccountViewModel.Addresses = new List<AddressViewModel>() {
                            new AddressViewModel () {
                                AddressID = 0,
                                AddressTypeID = response.AccountViewModel.AddressTypes.Where (s => s.IsDefault).FirstOrDefault ().DropdownValueID,
                                State = new State {
                                    Code = ""
                                },
                                Country = new Country {
                                    Code = ""
                                }
                            }
                        };
                    }
                    response.AccountViewModel.Modules = GetModulesList(null);
                    response.AccountViewModel.SubscribedModules = GetModulesList(accountId);
                    GetCommunicatioProvidersRequest providerRequest = new GetCommunicatioProvidersRequest
                    {
                        AccountId = this.Identity.ToAccountID()
                    };
                    response.AccountViewModel.DefaultVMTA = System.Configuration.ConfigurationManager.AppSettings["VMTA_DEFAULT"].ToString();
                    GetCommunicatioProvidersResponse providerResponse = communicationService.GetCommunicationProviders(providerRequest);
                    response.AccountViewModel.ServiceProviderRegistrationDetails = providerResponse.RegistrationListViewModel;
                    response.AccountViewModel.CampaignProviders = providerResponse.campaignProviderViewModel;
                    ViewBag.page = "AccountSettings";
                    ViewBag.IsAccountStAdmin = this.Identity.IsSTAdmin();
                    ViewBag.AccountCreation = false;
                    if (response.AccountViewModel.DomainURL != null && response.AccountViewModel.DomainURL.Contains('.'))
                        response.AccountViewModel.DomainURL = response.AccountViewModel.DomainURL.Substring(0, response.AccountViewModel.DomainURL.IndexOf('.'));
                    ViewBag.NewAddress = new AddressViewModel()
                    {
                        AddressID = 0,
                        AddressTypeID = response.AccountViewModel.AddressTypes.Where(s => s.IsDefault).FirstOrDefault().DropdownValueID,
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
                    List<AppModules> appModules = new List<AppModules>() {
                        AppModules.WebAnalytics
                    };
                    var WebAnalyticsPermission = MenuHelper.CheckPermission(appModules).FirstOrDefault();
                    if (WebAnalyticsPermission != null)
                        ViewBag.WebAnalyticsPermission = WebAnalyticsPermission.HasPermission;
                    else
                        ViewBag.WebAnalyticsPermission = false;
                    GetImageDomainsResponse imageDomainResponse = imageDomainService.GetActiveImageDomains(new GetImageDomainsRequest()
                    {

                    });
                    response.AccountViewModel.ImageDomains = imageDomainResponse.ImageDomains;
                    UpdateUserActivityLog(response.AccountViewModel.AccountID, response.AccountViewModel.AccountName, UserActivityType.Read);
                }
                return View("AccountSettings", response.AccountViewModel);
            }
            catch (Exception ex)
            {
                throw new UnsupportedOperationException("An error occurred", ex);
            }
        }

        /// <summary>
        /// Gets the account logo.
        /// </summary>
        /// <returns></returns>
        public JsonResult GetAccountLogo()
        {
            GetAccountIdRequest request = new GetAccountIdRequest()
            {
                accountId = this.Identity.ToAccountID(),
                RequestedBy = this.Identity.ToUserID()
            };
            GetAccountResponse AccountData = accountService.GetAccountById(request);
            if (AccountData.AccountViewModel.Image != null)
            {
                if (!string.IsNullOrEmpty(AccountData.AccountViewModel.Image.StorageName))
                {
                    switch (AccountData.AccountViewModel.Image.ImageCategoryID)
                    {
                        case ImageCategory.AccountLogo:
                            AccountData.AccountViewModel.Image.ImageContent = urlService.GetUrl(this.Identity.ToAccountID(), ImageCategory.AccountLogo, AccountData.AccountViewModel.Image.StorageName);
                            break;
                        default:
                            AccountData.AccountViewModel.Image.ImageContent = string.Empty;
                            break;
                    }
                }
                return Json(new
                {
                    ImageContent = AccountData.AccountViewModel.Image.ImageContent,
                    AccountName = AccountData.AccountViewModel.AccountName
                }, JsonRequestBehavior.AllowGet);
            }
            return Json(new
            {
                ImageContent = "",
                AccountName = AccountData.AccountViewModel.AccountName
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Updates the account settings.
        /// </summary>
        /// <param name="accountViewModel">The account view model.</param>
        /// <returns></returns>
        [HttpPost]
        [SmarttouchAuthorize(AppModules.AccountSettings, AppOperations.Edit)]
        [MenuType(MenuCategory.AccountSettings, MenuCategory.LeftMenuAccountConfiguration)]
        public JsonResult UpdateAccountSettings(string accountViewModel)
        {
            AccountViewModel viewModel = JsonConvert.DeserializeObject<AccountViewModel>(accountViewModel);
            viewModel.ModifiedBy = this.Identity.ToUserID();
            viewModel.ModifiedOn = DateTime.Now.ToUniversalTime();
            if (viewModel.DomainURL != null)
            {
                viewModel.DomainURL = viewModel.DomainURL + "" + System.Configuration.ConfigurationManager.AppSettings["SMARTCRMDOMAIN"].ToString();
                viewModel.PreviousDomainURL = viewModel.PreviousDomainURL + "" + System.Configuration.ConfigurationManager.AppSettings["SMARTCRMDOMAIN"].ToString();
            }
            UpdateAccountRequest request = new UpdateAccountRequest()
            {
                AccountViewModel = viewModel,
                RequestedBy = this.Identity.ToUserID(),
                IsSettingsUpdate = true
            };
            CommunicationProviderRegistrationRequest registrationRequest = new CommunicationProviderRegistrationRequest();
            registrationRequest.ProviderRegistrationViewModels = viewModel.ServiceProviderRegistrationDetails;
            registrationRequest.RequestedBy = this.Identity.ToUserID();
            registrationRequest.AccountId = this.Identity.ToAccountID();
            ////request.AccountViewModel.LogoExists = imageController.SaveAccountLogo(request.AccountViewModel.ImageContent);
            UpdateAccountResponse response = accountService.UpdateAccount(request);
            communicationService.CommunicationProviderRegistration(registrationRequest);
            if (response.Exception == null)
            {
                cachingService.AddUserPermissions(viewModel.AccountID);
                cachingService.AddAccountPermissions(viewModel.AccountID);
                cachingService.AddDataSharingPermissions(viewModel.AccountID);
            }
            UpdateUserActivityLog(viewModel.AccountID, viewModel.AccountName, UserActivityType.Update);
            return Json(new
            {
                success = true,
                response = ""
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Gets the modules list.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        IEnumerable<ModuleViewModel> GetModulesList(int? accountId)
        {
            GetModulesResponse response = new GetModulesResponse();
            if (accountId.HasValue)
            {
                response = roleService.GetModules(new GetModulesRequest()
                {
                    AccountID = accountId,
                    RequestedBy = this.Identity.ToUserID()
                });
                GetDataAccessPermissionsResponse sharingResponse = accountService.GetSharingPermissions(new GetDataAccessPermissionsRequest()
                {
                    accountId = accountId.Value,
                    Modules = (List<ModuleViewModel>)response.ModuleViewModel
                });
                response.ModuleViewModel = sharingResponse.Modules;
            }
            else
            {
                response = roleService.GetModules(new GetModulesRequest());
            }
            if (response.Exception != null)
            {
                return null;
            }
            else
                return response.ModuleViewModel;
        }

        /// <summary>
        /// Adds the new service provider.
        /// </summary>
        /// <param name="providerId">The provider identifier.</param>
        /// <returns></returns>
        public ActionResult AddNewServiceProvider(int providerId)
        {
            ViewBag.ProviderId = providerId;
            return PartialView("_AddNewServiceProvider");
        }

        void UpdateUserActivityLog(int entityId, string entityName, UserActivityType activityType)
        {
            int userId = this.Identity.ToUserID();
            UserReadActivityRequest request = new UserReadActivityRequest();
            request.ActivityName = activityType;
            request.EntityId = entityId;
            request.ModuleName = AppModules.AccountSettings;
            request.UserId = userId;
            request.AccountId = this.Identity.ToAccountID();
            request.EntityName = entityName;
            userService.InsertReadActivity(request);
        }
        #endregion
    }
}

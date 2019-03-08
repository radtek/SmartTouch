using Newtonsoft.Json;
using SmartTouch.CRM.ApplicationServices.Messaging;
using SmartTouch.CRM.ApplicationServices.Messaging.Accounts;
using SmartTouch.CRM.ApplicationServices.Messaging.WebAnalytics;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.WebService.Helpers;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;

namespace SmartTouch.CRM.WebService.Controllers
{
    /// <summary>
    /// Creating accounts controller for accounts module
    /// </summary>
    public class AccountsController : SmartTouchApiController
    {
        readonly IAccountService accountService;
        readonly IWebAnalyticsProviderService webAnalyticsProviderService;
        /// <summary>
        /// Creating constructor for account controller for accessing
        /// </summary>
        /// <param name="accountService">accountService </param>
        /// <param name="webAnalyticsProviderService">webAnalyticsProviderService</param>
        public AccountsController(IAccountService accountService, IWebAnalyticsProviderService webAnalyticsProviderService)
        {
            this.accountService = accountService;
            this.webAnalyticsProviderService = webAnalyticsProviderService;
        }

        /// <summary>
        /// Gets Account by AcccountName in the system.
        /// </summary>
        /// <param name="accountName">Name of the account. It works as a search string, name for account.</param>
        /// <returns></returns>
        public HttpResponseMessage GetAccountByName(string accountName)
        {
            ServiceResponseBase response = accountService.GetAccountByName(new GetAccountNameRequest() { name = accountName });
            return Request.BuildResponse(response);
        }

        /// <summary>
        /// Get Account by id.
        /// </summary>
        /// <param name="id">The Id value that uniquely identifies a account.</param>
        /// <returns>Account Details by Id</returns>
        [Authorize]
        public HttpResponseMessage GetAccountById(int id)
        {
            //This method is being used only in the Unit Testing. If using in any other areas, please pass the RequestedBy value.
            ServiceResponseBase response = accountService.GetAccount(new GetAccountRequest(id));
            return Request.BuildResponse(response);
        }

        /// <summary>
        /// Inserts a new account.
        /// </summary>
        /// <param name="viewModel">Properties of a new account</param>
        /// <returns>Tag insert details response </returns>
        [Route("Tag/InsertAccount")]
        [HttpPost]
        public HttpResponseMessage PostTag(AccountViewModel viewModel)
        {
            InsertAccountResponse response = accountService.InsertAccount(new InsertAccountRequest() { AccountViewModel = viewModel });
            return Request.BuildResponse(response);
        }

        /// <summary>
        /// Updates an account.
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns>Tag updated details</returns>
        [Route("Tag/UpdateAccount")]
        [HttpPut]
        public HttpResponseMessage PutTag(AccountViewModel viewModel)
        {
            UpdateAccountResponse response = accountService.UpdateAccount(new UpdateAccountRequest() { AccountViewModel = viewModel });
            return Request.BuildResponse(response);
        }

        /// <summary>
        /// Deletes an account.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Tag deletion deatails</returns>
        [Route("Tag/DeleteAccount")]
        [HttpDelete]
        [ApiExplorerSettings(IgnoreApi=true)]
        public HttpResponseMessage Delete(int id)
        {
            DeleteAccountResponse response = accountService.DeleteAccount(new DeleteAccountRequest(id));
            return Request.BuildResponse(response);
        }

        /// <summary>
        /// Delete a account by Id.
        /// </summary>
        /// <param name="accountId">The Id value that uniquely identifies a account.</param>
        /// <param name="StatusID">Status id of a account.</param>
        /// <returns>Account deletion details</returns>

        [HttpDelete]
        [ApiExplorerSettings(IgnoreApi = true)]
        public HttpResponseMessage DeleteAccount(int[] accountId, byte StatusID)
        {
            AccountStatusUpdateResponse response = accountService.UpdateAccountStatus(new AccountStatusUpdateRequest() { AccountID = accountId , StatusID = StatusID});
            return Request.BuildResponse(response);
        }

        /// <summary>
        /// Check domail url exist or not.
        /// </summary>
        /// <param name="domainURL">Domain url of a account.</param>        
        /// <returns>Domain Url details</returns>

        [Route("checkdomainurl")]
        [HttpGet]
        public HttpResponseMessage CheckDomainURL(string domainURL)
        {
            if (domainURL != null)
            {
                CheckDomainURLAvailabilityRequest request = new CheckDomainURLAvailabilityRequest() { DomainURL = domainURL };
                CheckDomainURLAvailabilityResponse response = accountService.IsDomainURLExist(request);
                return Request.BuildResponse(response);
            }
            else return null;
        }

        [System.Web.Mvc.AllowAnonymous]
        [Route("GetImportRowData")]
        public HttpResponseMessage GetImportRowData(int newDataId, int oldDataId)
        {
            GetImportRowDataResponce responce = new GetImportRowDataResponce();
            responce.RowData = accountService.GetRowData(newDataId, oldDataId);
            return Request.BuildResponse(responce);
        }

        /// <summary>
        /// Get account web analytics providers.
        /// </summary>
        /// <param name="accountId">Id of a account.</param>        
        /// <returns>Account web analytics providers</returns>

        [Route("getaccountwebanalyticsproviders")]
        [HttpGet]
        public HttpResponseMessage GetAccountWebAnalyticsProviders(int accountId)
        {
            if (accountId > 0)
            {
                GetWebAnalyticsProvidersRequest request = new GetWebAnalyticsProvidersRequest() { AccountId = accountId};
                GetWebAnalyticsProvidersResponse response = accountService.GetAccountWebAnalyticsProviders(request);
                return Request.BuildResponse(response);
            }
            else return null;
        }


        /// <summary>
        /// Getting Bdx Accounts
        /// </summary>
        /// <param name="accountName"></param>
        /// <returns></returns>
        [Route("getbdxaccounts")]
        [System.Web.Mvc.AllowAnonymous]
        [HttpGet]
        public HttpResponseMessage GetBdxAccounts(string accountName)
        {
            GetBdxAccountsRequest request = new GetBdxAccountsRequest() { AccountName = accountName };
            GetBdxAccountsResponse response = accountService.GetBdxAccounts(request);          
            return Request.BuildResponse(response);
          
        }

        /// <summary>
        /// Validate VisiStat Key
        /// </summary>
        /// <param name="visiStatData">VisiStat Key</param>
        /// <returns>VisiStat Data</returns>
        [Route("validatevisitatkey")]
        [HttpGet]
        public HttpResponseMessage ValidateVisiStatKey(string visiStatData)
        {
            ValidateVisiStatKeyRequest request = JsonConvert.DeserializeObject<ValidateVisiStatKeyRequest>(visiStatData);

            if (!string.IsNullOrEmpty(request.VisiStatKey) && !string.IsNullOrEmpty(request.TrackingDomain))
            {
                ValidateVisiStatKeyResponse response = webAnalyticsProviderService.ValidateVisiStatKey(request);
                return Request.BuildResponse(response);
            }
            else return null;
        }

        /// <summary>
        /// Gets all accounts.
        /// </summary>
        /// <returns></returns>
        [Route("accounts")]
        [HttpGet]
        public HttpResponseMessage GetAllAccounts()
        {
            ServiceResponseBase response = accountService.GetAllAccounts();
            return Request.BuildResponse(response);
        }
    }
}
using Kendo.Mvc.UI;
using Newtonsoft.Json;
using SmartTouch.CRM.ApplicationServices.Messaging.Accounts;
using SmartTouch.CRM.ApplicationServices.Messaging.ThirdPartyClient;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Web.Utilities;
using System;
using System.Web.Mvc;

namespace SmartTouch.CRM.Web.Controllers
{
    public class APIKeysController : SmartTouchController
    {
        private readonly IThirdPartyClientService thirdPartyClientService;

        private readonly IAccountService accountService;

        public APIKeysController(IThirdPartyClientService thirdPartyClientService, IAccountService accountService)
        {
            this.thirdPartyClientService = thirdPartyClientService;
            this.accountService = accountService;
        }

        /// <summary>
        /// Gets all API keys.
        /// </summary>
        /// <returns></returns>
        [Route("apikeys")]
        [SmarttouchAuthorize(AppModules.ApiKeys, AppOperations.Read)]
        [MenuType(MenuCategory.ApiKeys, MenuCategory.LeftMenuAccountConfiguration)]
        [SmarttouchSessionStateBehaviour(System.Web.SessionState.SessionStateBehavior.Required)]
        public ActionResult GetAllApiKeys()
        {
            int userID = UserExtensions.ToUserID(this.Identity);
            ViewBag.userid = userID;
            return View("ApiKeysList");
        }

        /// <summary>
        /// Gets all API keys list.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="name">The name.</param>
        /// <param name="filterdata">The filterdata.</param>
        /// <returns></returns>
        public ActionResult GetAllApiKeysList([DataSourceRequest] DataSourceRequest request, string name, string filterdata)
        {
            int RoleId = UserExtensions.ToRoleID(this.Identity);
            GetThirdPartyClientResponse response = thirdPartyClientService.GetAllThirdPartyClients(new GetThirdPartyClientRequest()
            {
                RequestedBy = RoleId,
                Name = name,
                Filter = filterdata
            });
            return Json(new DataSourceResult
            {
                Data = response.ThirdPartyClientViewModel
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Adds the API key.
        /// </summary>
        /// <returns></returns>
        [Route("addapikey")]
        [SmarttouchAuthorize(AppModules.ApiKeys, AppOperations.Read)]
        [MenuType(MenuCategory.Undefined, MenuCategory.LeftMenuAccountConfiguration)]
        [SmarttouchSessionStateBehaviour(System.Web.SessionState.SessionStateBehavior.Required)]
        public ActionResult AddAPIKey()
        {
            Guid guid = Guid.NewGuid();
            string myGuid = Convert.ToString(guid);
            ViewBag.guid = myGuid.ToUpper();
            ViewBag.mode = "add";
            ThirdPartyClientViewModel viewModel = new ThirdPartyClientViewModel();
            viewModel.IsActive = true;
            return View("addAPIKey", viewModel);
        }

        /// <summary>
        /// Gets all accounts.
        /// </summary>
        /// <returns></returns>
        public JsonResult GetAllAccounts()
        {
            GetAccountListResponse response = accountService.GetAllAccounts();
            return Json(new
            {
                success = true,
                response = response.Accounts
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Inserts the API keys.
        /// </summary>
        /// <param name="apiKeys">The API keys.</param>
        /// <returns></returns>
        public ActionResult InsertApiKeys(string apiKeys)
        {
            int userID = UserExtensions.ToUserID(this.Identity);
            ThirdPartyClientViewModel thirdPartyClientViewmodel = JsonConvert.DeserializeObject<ThirdPartyClientViewModel>(apiKeys);
            thirdPartyClientViewmodel.LastUpdatedBy = userID;
            thirdPartyClientViewmodel.LastUpdatedOn = DateTime.Now.ToUniversalTime();
            InsertThirdPartyClientResponse response = thirdPartyClientService.AddThirdPartyClient(new InsertThirdPartyClientRequest()
            {
                RequestedBy = userID,
                ThirdPartyClientViewModel = thirdPartyClientViewmodel
            });
            return Json(new
            {
                success = true,
                response = response
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Edits the API key.
        /// </summary>
        /// <param name="apiKeyID">The API key identifier.</param>
        /// <returns></returns>
        [Route("editapikey")]
        [SmarttouchAuthorize(AppModules.ApiKeys, AppOperations.Read)]
        [MenuType(MenuCategory.Undefined, MenuCategory.LeftMenuAccountConfiguration)]
        [SmarttouchSessionStateBehaviour(System.Web.SessionState.SessionStateBehavior.Required)]
        public ActionResult EditApiKey(string apiKeyID)
        {
            ViewBag.mode = "edit";
            GetApiKeyByIDResponse response = thirdPartyClientService.GetApiKeyByID(new GetApiKeyByIDRequest()
            {
                ID = apiKeyID
            });
            return View("addAPIKey", response.ThirdPartyClientViewModel);
        }

        /// <summary>
        /// Updates the API key.
        /// </summary>
        /// <param name="apiKeys">The API keys.</param>
        /// <returns></returns>
        public JsonResult UpdateApiKey(string apiKeys)
        {
            int UserID = this.Identity.ToUserID();
            int userID = UserExtensions.ToUserID(this.Identity);
            ThirdPartyClientViewModel thirdPartyClientViewmodel = JsonConvert.DeserializeObject<ThirdPartyClientViewModel>(apiKeys);
            thirdPartyClientViewmodel.LastUpdatedOn = DateTime.Now.ToUniversalTime();
            thirdPartyClientViewmodel.LastUpdatedBy = userID;
            UpdateApiKeyResponse response = thirdPartyClientService.UpdateApiKey(new UpdateApiKeyRequest()
            {
                ThirdPartyClientViewModel = thirdPartyClientViewmodel,
                RequestedBy = UserID,
                AccountId = this.Identity.ToAccountID()
            });
            return Json(new
            {
                success = true,
                response = response
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Deletes the rule.
        /// </summary>
        /// <param name="apiKey">The API key.</param>
        /// <returns></returns>
        [Route("deleterapikey/")]
        [SmarttouchAuthorize(AppModules.ApiKeys, AppOperations.Delete)]
        public ActionResult DeleteRule(string apiKey)
        {
            int UserID = this.Identity.ToUserID();
            ThirdPartyClientViewModel thirdPartyClientViewmodel = JsonConvert.DeserializeObject<ThirdPartyClientViewModel>(apiKey);
            thirdPartyClientService.DeleteApiKey(new DeleteApiKeyRequest()
            {
                ThirdPartyClientViewModel = thirdPartyClientViewmodel,
                RequestedBy = UserID,
                AccountId = this.Identity.ToAccountID()
            });
            return Json(new
            {
                success = true,
                response = ""
            }, JsonRequestBehavior.AllowGet);
        }
    }
}

using LandmarkIT.Enterprise.Utilities.ExceptionHandling;
using SmartTouch.CRM.ApplicationServices.Messaging.Accounts;
using SmartTouch.CRM.ApplicationServices.Messaging.Action;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.Accounts;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.WebService.Helpers;
using System;
using System.Net.Http;
using System.Web.Http;

namespace SmartTouch.CRM.WebService.Controllers
{
    public class ActionController : SmartTouchApiController
    {
        readonly IActionService actionService;
        readonly IAccountService accountService;

        /// <summary>
        /// Creating constructor for Actions controller for accessing
        /// </summary>
        /// <param name="ActionService">ActionService</param>
        public ActionController(IActionService actionService, IAccountService accountService)
        {
            this.actionService = actionService;
            this.accountService = accountService;
        }

        /// <summary>
        /// Inserts a new Action.
        /// </summary>
        /// <param name="viewModel">Properties of a new Action</param>
        /// <returns>Action Insertion Details Response</returns>
        [Route("AddAction")]
        [HttpPost]
        public HttpResponseMessage PostAction(ActionViewModel viewModel)
        {
            InsertActionResponse response = new InsertActionResponse();
            var accountAddress = accountService.GetPrimaryAddress(new GetAddressRequest()
            {
                AccountId = (viewModel.AccountId.HasValue&& viewModel.AccountId.Value > 0)? viewModel.AccountId.Value : 1
            });
            string accountPhoneNumber = accountService.GetPrimaryPhone(new GetPrimaryPhoneRequest()
            {
                AccountId = (viewModel.AccountId.HasValue && viewModel.AccountId.Value > 0) ? viewModel.AccountId.Value : 1
            }).PrimaryPhone;
            Account account = accountService.GetAccountMinDetails(this.AccountId);
            try
            {
                response = actionService.InsertAction(new InsertActionRequest()
                {
                    ActionViewModel = viewModel,
                    RequestedBy = viewModel.CreatedBy,
                    AccountId = (viewModel.AccountId.HasValue && viewModel.AccountId.Value > 0) ? viewModel.AccountId.Value : 1,
                    RequestedFrom = RequestOrigin.API,
                    AccountAddress = accountAddress.Address,
                    AccountPhoneNumber = accountPhoneNumber,
                    AccountPrimaryEmail = account.Email.EmailId,
                    AccountDomain = Request.RequestUri.Host
                });
            }
            catch (Exception ex)
            {
                response.Exception = ex;
            }
            if (response.Exception != null)
            {
                var message = response.Exception.Message.Replace("[|", "").Replace("|]", "");
                response.Exception = new UnsupportedOperationException(message);
            }
            return Request.BuildResponse(response);
        }

        /// <summary>
        /// Updates a Action.
        /// </summary>
        /// <param name="viewModel">Properties of a Action</param>
        /// <returns>Action Updation Details Response</returns>
        [Route("UpdateAction")]
        [HttpPut]
        public HttpResponseMessage PutAction(ActionViewModel viewModel)
        {
            UpdateActionResponse response = actionService.UpdateAction(new UpdateActionRequest() { ActionViewModel = viewModel });
            return Request.BuildResponse(response);
        }

        /// <summary>
        /// Deletes a Action By ActionId.
        /// </summary>
        /// <param name="ActionId">Id of a Action</param>
        /// <returns>Action Deletion Details Response</returns>
        [Route("Action/Delete")]
        [HttpDelete]
        public HttpResponseMessage DeleteAction(int actionId)
        {
            DeleteActionResponse response = actionService.DeleteAction(new DeleteActionRequest() { ActionId = actionId});
            return Request.BuildResponse(response);
        }

        /// <summary>
        /// Gets a Action by ActionId.
        /// </summary>
        /// <param name="ActionId">Id of a Action</param>
        /// <returns>Action Details</returns>
        public HttpResponseMessage GetAction(int actionId)
        {
            GetActionResponse response = actionService.GetAction(new GetActionRequest() { Id = actionId });
            return Request.BuildResponse(response);
        }
    }
}
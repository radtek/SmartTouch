using LandmarkIT.Enterprise.Utilities.ExceptionHandling;
using SmartTouch.CRM.ApplicationServices.Messaging.Accounts;
using SmartTouch.CRM.ApplicationServices.Messaging.Tour;
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
    /// <summary>
    /// Creating tours controller for tours module
    /// </summary>
    public class ToursController : SmartTouchApiController
    {
        readonly ITourService tourService;
        readonly IAccountService accountService;

        /// <summary>
        /// Creating constructor for tours controller for accessing
        /// </summary>
        /// <param name="tourService">tourService</param>
        public ToursController(ITourService tourService, IAccountService accountService)
        {
            this.tourService = tourService;
            this.accountService = accountService;
        }

        /// <summary>
        /// Inserts a new Tour.
        /// </summary>
        /// <param name="viewModel">Properties of a new Tour</param>
        /// <returns>Tour Insertion Details Response</returns>
        [Route("Tours")]
        [HttpPost]
        public HttpResponseMessage PostTour(TourViewModel viewModel)
        {
            InsertTourResponse response = new InsertTourResponse();
            viewModel.CreatedBy = viewModel.CreatedBy;
            viewModel.CreatedOn = DateTime.Now.ToUniversalTime();
            var accountAddress = accountService.GetPrimaryAddress(new GetAddressRequest()
            {
                AccountId = (viewModel.AccountId.HasValue && viewModel.AccountId.Value>0)? viewModel.AccountId.Value:1
            });
            string accountPhoneNumber = accountService.GetPrimaryPhone(new GetPrimaryPhoneRequest()
            {
                AccountId = (viewModel.AccountId.HasValue && viewModel.AccountId.Value > 0) ? viewModel.AccountId.Value : 1
            }).PrimaryPhone;
            Account account = accountService.GetAccountMinDetails((viewModel.AccountId.HasValue && viewModel.AccountId.Value > 0) ? viewModel.AccountId.Value : 1);
            try
            {
                response = tourService.InsertTour(new InsertTourRequest()
                {
                    TourViewModel = viewModel,
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
        /// Updates a Tour.
        /// </summary>
        /// <param name="viewModel">Properties of a Tour</param>
        /// <returns>Tour Updation Details Response</returns>
        [Route("Tours")]
        [HttpPut]
        public HttpResponseMessage PutTour(TourViewModel viewModel)
        {
            UpdateTourResponse response = tourService.UpdateTour(new UpdateTourRequest() { TourViewModel = viewModel });
            return Request.BuildResponse(response);
        }

        /// <summary>
        /// Deletes a Tour By tourId.
        /// </summary>
        /// <param name="tourId">Id of a tour</param>
        /// <returns>Tour Deletion Details Response</returns>
        [Route("Tours/Delete")]
        [HttpDelete]
        public HttpResponseMessage DeleteTour(int tourId)
        {
            DeleteTourResponse response = tourService.DeleteTour(tourId,0,0);
            return Request.BuildResponse(response);
        }

        /// <summary>
        /// Gets a Tour by tourId.
        /// </summary>
        /// <param name="tourId">Id of a tour</param>
        /// <returns>Tour Details</returns>
        public HttpResponseMessage GetTour(int tourId)
        {
            GetTourResponse response = tourService.GetTour(tourId);
            return Request.BuildResponse(response);
        }
	}
}
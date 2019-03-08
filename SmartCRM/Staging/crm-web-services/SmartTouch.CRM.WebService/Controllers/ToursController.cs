using SmartTouch.CRM.ApplicationServices.Messaging.Tour;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.WebService.Helpers;
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

        /// <summary>
        /// Creating constructor for tours controller for accessing
        /// </summary>
        /// <param name="tourService">tourService</param>
        public ToursController(ITourService tourService)
        {
            this.tourService = tourService;
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
            InsertTourResponse response = tourService.InsertTour(new InsertTourRequest() { TourViewModel = viewModel});
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
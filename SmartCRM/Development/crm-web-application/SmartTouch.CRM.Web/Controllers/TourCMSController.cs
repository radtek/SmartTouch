using Newtonsoft.Json;
using SmartTouch.CRM.ApplicationServices.Messaging.TourCMS;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Web.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SmartTouch.CRM.Web.Controllers
{
    public class TourCMSController : SmartTouchController
    {
        readonly IApplicationTourService applicationTourService;

        public TourCMSController(IApplicationTourService applicationTourService)
        {
            this.applicationTourService = applicationTourService;
        }

        [Route("apptour")]
        [SmarttouchAuthorize(AppModules.Accounts, AppOperations.Read)]
        [MenuType(MenuCategory.TourCMS, MenuCategory.LeftMenuAccountConfiguration)]
        public ActionResult TourCMS()
        {
            GetAllTourDetailsResponse response = applicationTourService.FindAll(new GetAllTourDetailsRequest() { });
            return View("TourCMS", response.ApplicationTours);
        }

        public JsonResult UpdateTourCMS(string viewModel)
        {
            ApplicationTourViewModel applicationTourViewModel = JsonConvert.DeserializeObject<ApplicationTourViewModel>(viewModel);
            UpdateTourCMSResponse response = new UpdateTourCMSResponse();
            if (applicationTourViewModel != null)
                response = applicationTourService.Update(new UpdateTourCMSRequest() { ViewModel = applicationTourViewModel });
            return Json(new { success = true, response = response }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateInput(false)]
        public JsonResult UpdateTour(string updateCMSViewModel)
        {
            UpdateApplicationTourResponse response = new UpdateApplicationTourResponse();
            if (!string.IsNullOrEmpty(updateCMSViewModel))
            {
                ApplicationTourViewModel applicationTourViewModel = JsonConvert.DeserializeObject<ApplicationTourViewModel>(updateCMSViewModel);
                response = applicationTourService.UpdateDetails(new UpdateApplicationTourRequest()
                {
                    ApplicationTourId = applicationTourViewModel.ApplicationTourDetailsID,
                    Content = applicationTourViewModel.Content,
                    Title = applicationTourViewModel.Title,
                    RequestedBy = this.Identity.ToUserID()
                });
            }
            return Json(new { success = true, response = response }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetAppTourByDivision(int divisionId)
        {
            GetByDivisionResponse response = new GetByDivisionResponse();
            if (divisionId != 0)
                response = applicationTourService.GetByDivision(new GetByDivisionRequest() { DivisionId = divisionId });
            return Json(new { success = true, response = response.AppTourViewModel }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult UpdateTourVisit(int userId)
        {
            UpdateTourVisitResponse response = new UpdateTourVisitResponse();
            if (userId != 0)
                applicationTourService.UpdateTourVisit(new UpdateTourVisitRequest() { UserId = userId });
            return Json(new { success = true, response = response }, JsonRequestBehavior.AllowGet);
        }
    }
}
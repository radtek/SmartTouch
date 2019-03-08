using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Web.Utilities;
using System.Threading;
using System.Web.Mvc;

namespace SmartTouch.CRM.Web.Controllers
{
    public class ErrorController : SmartTouchController
    {

        readonly ICachingService cacheService;
        public ErrorController(ICachingService cacheService)
        {
            this.cacheService = cacheService;
        }

        // [Route("error")]
        public ViewResult Index()
        {
            return View("Error");
        }
        [AllowAnonymous]
        public ViewResult AnonymousError()
        {
            return View("AnonymousError");
        }

        // [Route("notfound")]
        public ViewResult NotFound(ErrorViewModel errorViewModel)
        {
            // Response.StatusCode = 404;  //you may want to set this to 200
            if(errorViewModel.Message != null)
            {
                ViewBag.ErrorMessage = errorViewModel.Message;
                return View("NotFound");
            }
            else
                return View("NotFound");
        }

        public ViewResult AccessDenied()
        {
            return View("AccessDenied");
        }

        [Route("suspended")]
        public ActionResult Suspended()
        {
            int accountId = Thread.CurrentPrincipal.Identity.ToAccountID();
            AccountViewModel account = cacheService.GetAccount(accountId);
            ViewBag.AccountName = account.AccountName;
            ViewBag.ImageSrc = account.Image == null ? "" : account.Image.ImageContent;
            ViewBag.StatusMessage = account.StatusMessage;

            return View("Suspended");
        }

        [Route("maintenance")]
        public ActionResult Maintanance()
        {
            int accountId = Thread.CurrentPrincipal.Identity.ToAccountID();
            AccountViewModel account = cacheService.GetAccount(accountId);
            ViewBag.AccountName = account.AccountName;
            ViewBag.ImageSrc = account.Image == null ? "" : account.Image.ImageContent;
            ViewBag.StatusMessage = account.StatusMessage;
            return View("Maintenance");
        }

        public ActionResult EntityNotFound(ErrorViewModel errorViewModel)
        {
            if (errorViewModel.Message != null)
            {
                ViewBag.ErrorMessage = errorViewModel.Message;
                return View("EntityNotFound");
            }
            else
                return View("EntityNotFound");
        }
    }
}
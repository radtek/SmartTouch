using Kendo.Mvc.UI;
using Newtonsoft.Json;
using SmartTouch.CRM.ApplicationServices.Messaging.ImageDomain;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Web.Utilities;
using System;
using System.Web;
using System.Web.Mvc;

namespace SmartTouch.CRM.Web.Controllers
{
    public class ImageDomainController : SmartTouchController
    {
        IImageDomainService imageDomainService;
        public ImageDomainController(IImageDomainService imageDomainService) 
        {
            this.imageDomainService = imageDomainService;
        }

        [SmarttouchAuthorize(AppModules.Accounts, AppOperations.Read)]
        public ActionResult AddImageDomain()
        {
            ImageDomainViewModel viewModel = new ImageDomainViewModel();
            viewModel.Status = true;
            return PartialView("AddImageDomain", viewModel);
        }

        [SmarttouchAuthorize(AppModules.Accounts, AppOperations.Read)]
        public ActionResult InsertImageDomain(string imageDomainViewModel)
        {
            ImageDomainViewModel viewModel = JsonConvert.DeserializeObject<ImageDomainViewModel>(imageDomainViewModel);
            viewModel.CreatedBy= this.Identity.ToUserID();
            viewModel.CreatedOn = DateTime.Now.ToUniversalTime();
            InsertImageDomainRequest request = new InsertImageDomainRequest() { ImageDomainViewModel = viewModel };
            imageDomainService.InsertImageDomain(request);
            return Json(new { success = true, response = "" }, JsonRequestBehavior.AllowGet);
        }

        [Route("editimagedomain")]
        [SmarttouchAuthorize(AppModules.Accounts, AppOperations.Read)]
        public ActionResult EditImageDomain(byte imageDomainId)
        {
            GetImageDomainResponse response = imageDomainService.GetImageDomain(new GetImageDomainRequest() { ImageDomainId = imageDomainId});
            ViewBag.IsModal = true;
            ViewBag.EditView = true;
            return PartialView("AddImageDomain", response.ImageDomainViewModel);
        }


        [SmarttouchAuthorize(AppModules.Accounts, AppOperations.Read)]
        public ActionResult UpdateImageDomain(string imageDomainViewModel)
        {
            ImageDomainViewModel viewModel = JsonConvert.DeserializeObject<ImageDomainViewModel>(imageDomainViewModel);
            viewModel.LastModifiedBy = this.Identity.ToUserID();
            viewModel.LastModifiedOn = DateTime.Now.ToUniversalTime();
            UpdateImageDomainRequest request = new UpdateImageDomainRequest() { ImageDomainViewModel = viewModel, RequestedBy=this.Identity.ToUserID() };
            imageDomainService.UpdateImageDomain(request);
            return Json(new { success = true, response = "" }, JsonRequestBehavior.AllowGet);
        }

        [SmarttouchAuthorize(AppModules.Accounts, AppOperations.Read)]
        public ActionResult DeleteImageDomain(byte imageDomainId)
        {
            DeleteImageDomainRequest request = new DeleteImageDomainRequest() { ImageDomainId = imageDomainId };
            request.RequestedBy = this.Identity.ToUserID();
            request.AccountId = this.Identity.ToAccountID();
            imageDomainService.DeleteImageDomain(request);
            return Json(new { success = true, response = "[|Successfully deleted image domain|]" }, JsonRequestBehavior.AllowGet);

        }
        [Route("imagedomains")]
        [SmarttouchAuthorize(AppModules.Accounts, AppOperations.Read)]
        [MenuType(MenuCategory.ImageDomain, MenuCategory.LeftMenuAccountConfiguration)]
        [OutputCache(Duration = 30)]
        public ActionResult ImageDomainsList()
        {
            ImageDomainViewModel viewModel = new ImageDomainViewModel();
            ViewBag.accountId = 0;
            short ItemsPerPage = default(short);
            short.TryParse(this.Identity.ToItemsPerPage(), out ItemsPerPage);
            ViewBag.ItemsPerPage = ItemsPerPage;
            return View("ImageDomainsList", viewModel);
        }

        [Route("imagedomains/search")]        
        public ActionResult ImageDomains([DataSourceRequest] DataSourceRequest request, string name, string status)
        {
            AddCookie("accountpagesize", request.PageSize.ToString(), 1);
            AddCookie("accountpagenumber", request.Page.ToString(), 1);
            GetImageDomainsRequest imageDomainsRequest = new GetImageDomainsRequest(){
                Query = name,
                Limit = request.PageSize,
                PageNumber = request.Page,
                Status = true,
                RequestedBy = null

            };
            GetImageDomainsResponse response = imageDomainService.GetImageDomains(imageDomainsRequest);
            
            return Json(new DataSourceResult
            {
                Data = response.ImageDomains,Total = response.TotalHits               
            }, JsonRequestBehavior.AllowGet);
        }

        [Route("activeImagedomains")]        
        public ActionResult GetActiveImageDomains()
        {
            GetImageDomainsRequest imageDomainsRequest = new GetImageDomainsRequest();
            GetImageDomainsResponse response = imageDomainService.GetActiveImageDomains(imageDomainsRequest);
            return Json(new {success = true, Data = response.ImageDomains}, JsonRequestBehavior.AllowGet);
        }

        public void AddCookie(string cookieName, string Value, int days)
        {
            HttpCookie CartCookie = new HttpCookie(cookieName, Value);
            CartCookie.Expires = DateTime.Now.AddDays(days);
            Response.Cookies.Add(CartCookie);
        }
    }
}
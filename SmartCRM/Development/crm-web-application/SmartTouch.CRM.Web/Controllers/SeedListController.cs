using Newtonsoft.Json;
using SmartTouch.CRM.ApplicationServices.Messaging.SeedList;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Web.Utilities;
using System.Collections.Generic;
using System.Web.Mvc;

namespace SmartTouch.CRM.Web.Controllers
{
    public class SeedListController : SmartTouchController
    {
        ISeedEmailService seedListService;

        public SeedListController(ISeedEmailService seedListService)
        {
            this.seedListService = seedListService;
        }

        [Route("seedlist")]
        [SmarttouchAuthorize(AppModules.Accounts, AppOperations.Read)]
        [MenuType(MenuCategory.SeedList, MenuCategory.LeftMenuAccountConfiguration)]
        public ActionResult GetSeedList()
        {
            int RoleId = UserExtensions.ToRoleID(this.Identity);
            GetSeedListResponse response = seedListService.GetSeedList(new GetSeedListRquest() { RequestedBy = RoleId });
            return View("SeedList",response.SeedEmailViewModel);
        }

        [SmarttouchAuthorize(AppModules.Accounts, AppOperations.Read)]
        public JsonResult InsertSeedList(string SeedEmail)
        {           
            int userID = UserExtensions.ToUserID(this.Identity);

            IEnumerable<SeedEmailViewModel> viewModel = JsonConvert.DeserializeObject<IEnumerable<SeedEmailViewModel>>(SeedEmail);
            InsertSeedListResponse response = seedListService.InsertSeedList(new InsertSeedListRequest() { RequestedBy= userID,SeedEmailViewModel = viewModel });
            return Json(new { success = true, response = response }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetList()
        {
            int RoleId = UserExtensions.ToRoleID(this.Identity);
            GetSeedListResponse response = seedListService.GetSeedList(new GetSeedListRquest() { RequestedBy = RoleId });
            var data = response.SeedEmailViewModel;
            return Json(new { success = true, response = data }, JsonRequestBehavior.AllowGet);
        }
    }   
}
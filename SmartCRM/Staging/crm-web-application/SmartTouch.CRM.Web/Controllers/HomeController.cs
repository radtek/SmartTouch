using LandmarkIT.Enterprise.Extensions;
using Newtonsoft.Json;
using SmartTouch.CRM.ApplicationServices.Messaging.Contacts;
using SmartTouch.CRM.ApplicationServices.Messaging.User;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Web.Utilities;
using System.Linq;
using System.Web.Mvc;

namespace SmartTouch.CRM.Web.Controllers
{
    public class HomeController : SmartTouchController
    {
        IUserService userService;

        ICachingService cachingService;

        public HomeController(IUserService userService, ICachingService cachingService)
        {
            this.userService = userService;
            this.cachingService = cachingService;
        }

        public ActionResult Index()
        {
            return View();
        }

        public JsonResult KeepAlive()
        {
            return new JsonResult
            {
                Data = "Success",
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        [HttpGet]
        public ActionResult RecentActivities(string moduleIds, int pageNo)
        {
            int[] moduleIDs = null;
            if (moduleIds != null)
                moduleIDs = JsonConvert.DeserializeObject<int[]>(moduleIds);
            string dateFormat = this.Identity.ToDateFormat();
            int userId = this.Identity.ToUserID();
            int accountId = this.Identity.ToAccountID();
            int roleId = this.Identity.ToRoleID();
            var usersPermissions = cachingService.GetUserPermissions(accountId);
            var accountPermissions = cachingService.GetAccountPermissions(accountId);
            var userModules = usersPermissions.Where(s => s.RoleId == (short)roleId && accountPermissions.Contains(s.ModuleId)).Select(r => r.ModuleId).ToList();
            GetUserActivitiesResponse response = userService.GetUserActivities(new GetUserActivitiesRequest()
            {
                PageNumber = pageNo,
                UserId = userId,
                DateFormat = dateFormat,
                AccountId = accountId,
                ModuleIds = moduleIDs
            });
            if (response != null)
                response.UserModules = userModules;
            return new JsonResult
            {
                Data = response,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public ActionResult ActivityDetail(byte moduleId, int[] entityIds)
        {
            GetUserActivityDetailResponse response = userService.GetUserActivityDetails(new GetUserActivityDetailRequest()
            {
                ModuleId = moduleId,
                EntityIds = entityIds,
                AccountId = this.Identity.ToAccountID()
            });
            return new JsonResult
            {
                Data = response.EntityDetails,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        [Route("webnotificationdetails")]
        public ActionResult WebVistiDetails(string webVisitReference)
        {
            GetContactWebVisitReportResponse response = userService.GetWebVisitDetailsByVisitReference(new GetContactWebVisitReportRequest()
            {
                VisitReference = webVisitReference,
                RequestedBy = this.Identity.ToUserID()
            });
            response.WebVisits.ToList().ForEach(w => w.VisitedOn = w.VisitedOn.ToUtcBrowserDatetime());
            ViewBag.DateFormat = this.Identity.ToDateFormat();
            return PartialView("_WebVisitDetails", response.WebVisits);
        }

        public ActionResult DeleteUserActivity(int userActivityLogId)
        {
            int userId = this.Identity.ToUserID();
            int accountId = this.Identity.ToAccountID();
            DeleteUserActivityRequest request = new DeleteUserActivityRequest();
            request.userID = userId;
            request.activityLogID = 1;
            request.AccountId = accountId;
            userService.DeleteUserActivity(request);
            return View();
        }

        [HttpGet]
        public ActionResult RecentWebVisits(string parameters)
        {
            GetRecentWebVisitsRequest request = JsonConvert.DeserializeObject<GetRecentWebVisitsRequest>(parameters);
            request.RequestedBy = this.Identity.ToUserID();
            request.AccountId = this.Identity.ToAccountID();
            request.RoleId = this.Identity.ToRoleID();
            cachingService.GetUserPermissions(request.AccountId);
            cachingService.GetAccountPermissions(request.AccountId);
            GetRecentWebVisitsResponse response = userService.GetRecentWebVisits(request);
            return new JsonResult
            {
                Data = response,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        [HttpGet]
        public JsonResult NotificationsCountByDate()
        {
            var accountId = this.Identity.ToAccountID();
            var userId = this.Identity.ToUserID();
            var roleId = this.Identity.ToRoleID();
            var usersPermissions = cachingService.GetUserPermissions(accountId);
            var accountPermissions = cachingService.GetAccountPermissions(accountId);
            var userModules = usersPermissions.Where(s => s.RoleId == roleId && accountPermissions.Contains(s.ModuleId)).Select(r => r.ModuleId).ToList();
            if (accountId != 1)
                userModules.Add((byte)AppModules.Download);
            else
                userModules = userModules.Where(m => m != 79).Select(s => s).ToList();

            var response = userService.GetNotificationsCountByDate(new GetNotificationsCountByDateRequest()
            {
                RequestedBy = userId,
                ModuleIds = userModules
            });
            if (userModules != null)
                response.PermissionModuleIds = userModules;
            return new JsonResult
            {
                Data = response,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        [HttpGet]
        public JsonResult DeleteBulkNotifications(bool arePreviousNotifications)
        {
            var accountId = this.Identity.ToAccountID();
            var userId = this.Identity.ToUserID();
            var roleId = this.Identity.ToRoleID();
            DeleteBulkNotificationsResponse response = userService.DeleteBulkNotifications(new DeleteBulkNotificationsRequest()
            {
                AccountId = accountId,
                ArePreviousNotifications = arePreviousNotifications,
                RequestedBy = userId,
                RoleId = roleId
            });
            return new JsonResult
            {
                Data = response.DeletedCount,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }
    }
}

using Newtonsoft.Json;
using System.Linq;
using SmartTouch.CRM.ApplicationServices.Messaging.Campaigns;
using SmartTouch.CRM.ApplicationServices.Messaging.User;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.WebService.Helpers;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;
using LandmarkIT.Enterprise.Extensions;

namespace SmartTouch.CRM.WebService.Controllers
{
    /// <summary>
    /// Creating users controller for users module
    /// </summary>
    public class UserController : SmartTouchApiController
    {
        readonly IUserService userService;
        readonly ITourService tourService;
        readonly IActionService actionService;
        readonly ICachingService cachingService;
        readonly ICampaignService campaignService;

        /// <summary>
        /// Creating constructor for users controller for accessing
        /// </summary>
        /// <param name="userService">userService</param>
        /// <param name="tourService">tourService</param>
        /// <param name="actionService">actionService</param>
        /// <param name="cachingService">cachingService</param>
        public UserController(IUserService userService, ITourService tourService,
            IActionService actionService, ICachingService cachingService, ICampaignService campaignService)
        {
            this.userService = userService;
            this.tourService = tourService;
            this.actionService = actionService;
            this.cachingService = cachingService;
            this.campaignService = campaignService;
        }


        /// <summary>
        /// Inser a new user.
        /// </summary>
        /// <param name="viewModel">Properties of a new form.</param>
        /// <returns>User Insertion Details response</returns>
        [Route("User")]
        [HttpPost]
        public HttpResponseMessage PostUser(UserViewModel viewModel)
        {
            InsertUserResponse response = userService.InsertUser(new InsertUserRequest() { UserViewModel = viewModel });
            return Request.BuildResponse(response);
        }

        /// <summary>
        /// Get remainders of a user.
        /// </summary>
        /// <param name="userId">user Id</param>
        /// <param name="moduleIds">module Ids</param>
        /// <param name="todayNotifications">today Notifications</param>
        /// <returns>All Reminders</returns>
        [Route("Reminders")]
        [HttpGet]
        [Authorize]
        public HttpResponseMessage GetReminders(int userId, string moduleIds, bool todayNotifications)
        {
            int[] moduleIDs = null;
            if (!string.IsNullOrEmpty(moduleIds))
                moduleIDs = JsonConvert.DeserializeObject<int[]>(moduleIds);
            GetUserNotificationsResponse response = userService.GetReminderNotifications(
                new GetUserNotificationsRequest() { UserIds = new List<int>() { userId }, ModuleIds = moduleIDs, TodayNotifications = todayNotifications });
            return Request.BuildResponse(response);
        }

        /// <summary>
        /// Get notifications of a user.
        /// </summary>
        /// <param name="userId">user Id</param>
        /// <param name="moduleIds">module Ids</param>
        /// <param name="todayNotifications">today Notifications</param>
        /// <returns>User Notifications</returns>

        [Route("Notifications")]
        [HttpGet]
        [Authorize]
        public HttpResponseMessage GetNotifications(int userId, string moduleIds, bool todayNotifications)
        {
            int[] moduleIDs = null;
            if (!string.IsNullOrEmpty(moduleIds))
                moduleIDs = JsonConvert.DeserializeObject<int[]>(moduleIds);
            GetUserNotificationsResponse response = userService.GetNotifications(
                new GetUserNotificationsRequest() { UserIds = new List<int>() { userId }, ModuleIds = moduleIDs, TodayNotifications = todayNotifications });
            return Request.BuildResponse(response);
        }

        /// <summary>
        /// Mark notification as read.
        /// </summary>
        /// <param name="notification">Properties of notification.</param>
        /// <returns>Notifications which are read</returns>

        [Route("MarkNotificationAsRead")]
        [HttpPost]
        [Authorize]
        public HttpResponseMessage MarkNotificationAsRead(Notification notification)
        {
            MarkNotificationAsReadResponse response =
                userService.MarkNotificationAsRead(new MarkNotificationAsReadRequest() { Notification = notification });
            return Request.BuildResponse(response);
        }

        /// <summary>
        /// Delete a notification
        /// </summary>
        /// <param name="deleteViewModel">notifications delete viewmodel</param>
        /// <returns>Notification deletion details response</returns>

        [Route("DeleteNotification")]
        [HttpGet]
        [Authorize]
        public HttpResponseMessage DeleteNotification(string deleteViewModel)
        {
            DeleteNotificationViewModel notificationsViewModel = new DeleteNotificationViewModel();
            if (deleteViewModel != null)
                notificationsViewModel = JsonConvert.DeserializeObject<DeleteNotificationViewModel>(deleteViewModel);
            DeleteNotificationResponse response =
                userService.DeleteNotification(new DeleteNotificationRequest()
                {
                    NotificationIds = notificationsViewModel.NotificationIds,
                    IsBulkDelete = notificationsViewModel.IsBulkDelete,
                    ModuleId = notificationsViewModel.ModuleId,
                    ArePreviousNotifications = notificationsViewModel.ArePreviousNotifications,
                    RequestedBy = this.UserId
                });
            return Request.BuildResponse(response);
        }

        /// <summary>
        /// Get Campaign Litmus Results
        /// </summary>
        /// <param name="campaignId"></param>
        /// <returns></returns>

        [Route("litmusresults")]
        [HttpGet]
        [Authorize]
        public HttpResponseMessage GetCampaignLitmusResults(int campaignId)
        {
            GetCampaignLitmusResponse response = campaignService.GetCampaignLitmusMap(new GetCampaignLitmusMapRequest()
            {
                AccountId = this.AccountId,
                CampaignId = campaignId,
                RequestedBy = 0
            });

            if(response.CampaignLitmusMaps.IsAny())
                response.LitmusId = response.CampaignLitmusMaps.OrderByDescending(m => m.LastModifiedOn).FirstOrDefault().LitmusId;

            return Request.BuildResponse(response);
        }
    }
}
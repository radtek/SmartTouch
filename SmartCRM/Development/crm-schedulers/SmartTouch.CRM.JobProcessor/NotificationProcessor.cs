using LandmarkIT.Enterprise.CommunicationManager.Database;
using LandmarkIT.Enterprise.CommunicationManager.Operations;
using LandmarkIT.Enterprise.Extensions;
using LandmarkIT.Enterprise.Utilities.Logging;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.Domain.Notifications;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace SmartTouch.CRM.JobProcessor
{
    public class NotificationProcessor : CronJobProcessor
    {
        readonly IPushNotificationService pushNotificationService;

        public NotificationProcessor(CronJobDb cronJob, JobService jobService, string cacheName)
            : base(cronJob, jobService, cacheName)
        {
            pushNotificationService = IoC.Container.GetInstance<IPushNotificationService>();
        }

        protected override void Execute()
        {

            try
            {
                //Get push notification which is ready to process.
                var basicAuthenticationKey = ConfigurationManager.AppSettings["BasicAPPBasicAuthenticationKey"].ToString();
                var mobileAppKey = ConfigurationManager.AppSettings["MobileAPPKey"].ToString();
                var notificationLimit = ConfigurationManager.AppSettings["NotificationLimit"].ToString();
                IEnumerable<PushNotification> pushNotifications = pushNotificationService.GetAllPushNotificationToProcess();
                if (pushNotifications.IsAny())
                {
                        foreach(var item in pushNotifications)
                        {
                            try
                            {
                                IEnumerable<Notification> notifications = pushNotificationService.GetAllUserNotifications(item.UserId, Convert.ToInt32(notificationLimit));
                                if (notifications.IsAny())
                                {
                                    foreach (var notification in notifications)
                                    {
                                        //push notification to Mobile Started
                                        try
                                        {

                                            var request = WebRequest.Create("https://onesignal.com/api/v1/notifications") as HttpWebRequest;
                                            request.KeepAlive = true;
                                            request.Method = "POST";
                                            request.ContentType = "application/json; charset=utf-8";
                                            request.Headers.Add("authorization", "Basic" + basicAuthenticationKey);
                                            var serializer = new JavaScriptSerializer();

                                            var obj = new
                                            {
                                                app_id = mobileAppKey,
                                                contents = new { en = notification.Details },
                                                include_player_ids = new string[] { item.SubscriptionID  }
                                            };

                                            var param = serializer.Serialize(obj);
                                            byte[] byteArray = Encoding.UTF8.GetBytes(param);
                                            string responseContent = null;

                                            try
                                            {
                                                using (var writer = request.GetRequestStream())
                                                {
                                                    writer.Write(byteArray, 0, byteArray.Length);
                                                }

                                                using (var response = request.GetResponse() as HttpWebResponse)
                                                {
                                                    using (var reader = new StreamReader(response.GetResponseStream()))
                                                    {
                                                        responseContent = reader.ReadToEnd();
                                                        Logger.Current.Informational("Push Notification Response: " + responseContent);
                                                    }
                                                }
                                            }
                                            catch (WebException ex)
                                            {
                                              Logger.Current.Error("Error while Pushing notification to Mobile Using API,", ex);
                                            }
                                            // Updating Notification Status to Completed
                                            pushNotificationService.UpdateNotificationStatus(notification.NotificationID, PushNotificationStatus.Completed);
                                        }
                                        catch (Exception ex)
                                        {
                                            Logger.Current.Error("Error while Pushing notification to Mobile,", ex);
                                            pushNotificationService.UpdateNotificationStatus(notification.NotificationID, PushNotificationStatus.Failed);
                                        }
                                    }

                                }

                            }
                            catch (Exception Ex)
                            {
                                Logger.Current.Error("Error while PushNotification Processing", Ex);
                                pushNotifications = pushNotificationService.GetAllPushNotificationToProcess();
                            }
                        }
                    pushNotifications = pushNotificationService.GetAllPushNotificationToProcess();
                }

            }
            catch (Exception ex)
            {
                Logger.Current.Error("Error While Processing Notification Messages -", ex);
            }
        }

    }
}

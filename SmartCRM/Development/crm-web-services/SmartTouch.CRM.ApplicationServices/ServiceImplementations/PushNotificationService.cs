using SmartTouch.CRM.ApplicationServices.Messaging.Notifications;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.Domain.Notifications;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ServiceImplementations
{
    public class PushNotificationService: IPushNotificationService
    {
        readonly IPushNotificationsRepository pushNotificationRepository;

        /// <summary>
        /// Creating Constructor to Access Methods
        /// </summary>
        /// <param name="pushNotificationRepository"></param>
        public PushNotificationService(IPushNotificationsRepository pushNotificationRepository)
        {
            if (pushNotificationRepository == null) throw new ArgumentNullException("pushNotificationRepository");

            this.pushNotificationRepository = pushNotificationRepository;
        }

        /// <summary>
        /// For Pushing Notifications
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PushNotificationResponse PushNotification(PushNotificationRequest request)
        {
            PushNotificationResponse response = new PushNotificationResponse();
            pushNotificationRepository.PushNotifications(request.PushNotification);
            return response;
        }

        /// <summary>
        /// Get all push notification which is ready to process.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<PushNotification> GetAllPushNotificationToProcess()
        {
            return pushNotificationRepository.GetAllPushNotificationToProcess();
        }

        /// <summary>
        /// Updating Push Notification Staus
        /// </summary>
        /// <param name="notificationId"></param>
        /// <param name="allow"></param>
        public void UpdatePushtification(int notificationId, bool allow)
        {
            pushNotificationRepository.UpdatePushtification(notificationId, allow);
        }

        /// <summary>
        /// Get All User Notifications to push
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public IEnumerable<Notification> GetAllUserNotifications(int userId,int limit)
        {
           return pushNotificationRepository.GetAllUserNotifications(userId, limit);
        }

        /// <summary>
        /// Updating Notification Staus
        /// </summary>
        /// <param name="notificationId"></param>
        /// <param name="Status"></param>
        public void UpdateNotificationStatus(int notificationId, PushNotificationStatus Status)
        {
            pushNotificationRepository.UpdateNotificationStatus(notificationId, Status);
        }

    }
}

using SmartTouch.CRM.ApplicationServices.Messaging.Notifications;
using SmartTouch.CRM.Domain.Notifications;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ServiceInterfaces
{
    public interface IPushNotificationService
    {
        PushNotificationResponse PushNotification(PushNotificationRequest request);
        IEnumerable<PushNotification> GetAllPushNotificationToProcess();
        void UpdatePushtification(int notificationId, bool allow);
        IEnumerable<Notification> GetAllUserNotifications(int userId,int limit);
        void UpdateNotificationStatus(int notificationId, PushNotificationStatus Status);
    }
}

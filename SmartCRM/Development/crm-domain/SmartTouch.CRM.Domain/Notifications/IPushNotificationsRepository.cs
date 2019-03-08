using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Notifications
{
    public interface IPushNotificationsRepository: IRepository<PushNotification, int>
    {
        void PushNotifications(PushNotification notification);
        IEnumerable<PushNotification> GetAllPushNotificationToProcess();
        void UpdatePushtification(int notificationId, bool allow);
        IEnumerable<Notification> GetAllUserNotifications(int userId,int limit);
        void UpdateNotificationStatus(int notificationId, PushNotificationStatus Status);

    }
}

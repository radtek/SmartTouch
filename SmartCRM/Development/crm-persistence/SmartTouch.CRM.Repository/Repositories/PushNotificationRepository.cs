using SmartTouch.CRM.Domain.Notifications;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using SmartTouch.CRM.Repository.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Repositories
{
    public class PushNotificationRepository: Repository<PushNotification, int, PushNotificationsBb>, IPushNotificationsRepository
    {
        /// <summary>
        /// Creating Constructor to Access 
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <param name="objectContextFactory"></param>
        public PushNotificationRepository(IUnitOfWork unitOfWork, IObjectContextFactory objectContextFactory)
            : base(unitOfWork, objectContextFactory)
        {

        }

        public override PushNotificationsBb ConvertToDatabaseType(PushNotification domainType, CRMDb context)
        {
            throw new NotImplementedException();
        }

        public override PushNotification ConvertToDomain(PushNotificationsBb databaseType)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<PushNotification> FindAll()
        {
            throw new NotImplementedException();
        }

        public override PushNotification FindBy(int id)
        {
            throw new NotImplementedException();
        }

        public override void PersistValueObjects(PushNotification domainType, PushNotificationsBb dbType, CRMDb context)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// For Inserting PushNotifications.
        /// </summary>
        /// <param name="notification"></param>
        public void PushNotifications(PushNotification notification)
        {
            using(var db = ObjectContextFactory.Create())
            {
                int id = GetExistingPushNotificationId(notification.AccountId, notification.UserId, notification.SubscriptionID);
                if (id == 0)
                {
                    var sql = @"INSERT INTO PushNotifications(Device,SubscriptionID,AccountID,UserId,Allow,CreatedDate)
                            VALUES(@Device,@SubscriptionID,@AccountID,@UserId,@Allow,@CreatedDate)";
                    db.Execute(sql, new
                    {
                        Device = notification.Device,
                        SubscriptionID = notification.SubscriptionID,
                        AccountID = notification.AccountId,
                        UserId = notification.UserId,
                        Allow = notification.Allow,
                        CreatedDate = notification.CreatedDate
                    });
                }
                else
                    UpdatePushtification(id, notification.Allow);
            }
        }

        /// <summary>
        /// Get all push notification which is ready to process.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<PushNotification> GetAllPushNotificationToProcess()
        {
            using(var db= ObjectContextFactory.Create())
            {
                var sql = @"SELECT PushNotificationID as Id,AccountID as AccountId,SubscriptionID,UserId FROM PushNotifications (NOLOCK) WHERE Allow=@Allow ORDER BY CreatedDate DESC";
                return db.Get<PushNotification>(sql, new { Allow=1 }).ToList();
            }
        }

        /// <summary>
        /// Updating Push Notification Staus
        /// </summary>
        /// <param name="notificationId"></param>
        /// <param name="Status"></param>
        public void UpdatePushtification(int notificationId, bool allow)
        {
            using (var db = ObjectContextFactory.Create())
            {
                var sql = @"UPDATE PushNotifications SET Allow=@StatusId WHERE PushNotificationID=@PushNoticationId";
                db.Execute(sql, new { StatusId = allow, PushNoticationId = notificationId });
            }
        }

        /// <summary>
        /// Get All User Notifications to push
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public IEnumerable<Notification> GetAllUserNotifications(int userId,int limit)
        {
            using (var db = ObjectContextFactory.Create())
            {
                IEnumerable<Notification> notifications = new List<Notification>() { };
                db.QueryStoredProc("GetNotificationsToProcess", (reader) =>
                {
                    notifications = reader.Read<Notification>().ToList();
                }, new
                {
                    UserID = userId,
                    NotificationLmit = limit
                });

                return notifications;
            }
        }

        /// <summary>
        /// Updating Notification Staus
        /// </summary>
        /// <param name="notificationId"></param>
        /// <param name="Status"></param>
        public void UpdateNotificationStatus(int notificationId, PushNotificationStatus Status)
        {
            using (var db = ObjectContextFactory.Create())
            {
                var sql = @"UPDATE Notifications SET PushNotificationStatusID=@PushNotificationStatusId WHERE NotificationID=@NotificationId";
                db.Execute(sql, new { PushNotificationStatusId = Status, NotificationId = notificationId });
            }
        }

        private int GetExistingPushNotificationId(int accountId,int userId,string subscription)
        {
            using(var db= ObjectContextFactory.Create())
            {
                var sql = @"SELECT PushNotificationID as Id FROM PushNotifications (nolock) WHERE AccountID=@AccountID AND UserId=@UserId and SubscriptionID=@SubscriptionID";
                return db.Get<int>(sql, new { AccountID = accountId, UserId = userId, SubscriptionID = subscription }).FirstOrDefault();
               
            }
        }
    }
}

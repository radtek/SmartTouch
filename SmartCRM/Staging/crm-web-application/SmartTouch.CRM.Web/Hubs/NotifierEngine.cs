using LandmarkIT.Enterprise.Extensions;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using SmartTouch.CRM.ApplicationServices.Messaging.User;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.Domain.ValueObjects;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SmartTouch.CRM.Web.Hubs
{
    public class NotifierEngine : IDisposable
    {
        private readonly static Lazy<NotifierEngine> _instance = new Lazy<NotifierEngine>(
           () => new NotifierEngine(GlobalHost.ConnectionManager.GetHubContext<NotificationHub>().Clients));

        private readonly ConcurrentDictionary<int, Notification> _notifications = new ConcurrentDictionary<int, Notification>();
        private readonly ConcurrentDictionary<int, Notification> _webVisitNotifications = new ConcurrentDictionary<int, Notification>();

        private readonly Dictionary<string, HashSet<string>> _connections =
           new Dictionary<string, HashSet<string>>();

        public int Count
        {
            get
            {
                return _connections.Count;
            }
        }

        public void Add(string userId, string connectionId)
        {
            lock (_connections)
            {
                HashSet<string> connections;
                if (!_connections.TryGetValue(userId, out connections))
                {
                    connections = new HashSet<string>();
                    _connections.Add(userId, connections);
                }

                lock (connections)
                {
                    connections.Add(connectionId);
                }
            }
        }

        public IEnumerable<string> GetConnections(string key)
        {
            HashSet<string> connections;
            if (_connections.TryGetValue(key, out connections))
            {
                return connections;
            }

            return Enumerable.Empty<string>();
        }

        public void Remove(string userId, string connectionId)
        {
            lock (_connections)
            {
                HashSet<string> connections;
                if (!_connections.TryGetValue(userId, out connections))
                {
                    return;
                }

                lock (connections)
                {
                    connections.Remove(connectionId);

                    if (connections.Count == 0)
                    {
                        _connections.Remove(userId);
                    }
                }
            }
        }

        private IHubConnectionContext<dynamic> Clients
        {
            get;
            set;
        }

        public static NotifierEngine Instance
        {
            get
            {
                return _instance.Value;
            }
        }

        private static double notificationInterval = 60000d;
        private readonly TimeSpan _realTimeInterval = TimeSpan.FromMilliseconds(20000);
        private readonly TimeSpan _notificationLoadInterval = TimeSpan.FromMilliseconds(notificationInterval);
        private readonly TimeSpan _webVisitNotificationLoadInterval = TimeSpan.FromMilliseconds(notificationInterval);
        
        private Timer _timer;
        private Timer _newReminderTimer;
        private Timer _newWebVisitTimer;

        readonly IUserService userService;
        private NotifierEngine(IHubConnectionContext<dynamic> clients)
        {
            Clients = clients;
            userService = IoC.Container.GetInstance<IUserService>();
            _timer = new Timer(broadcastRemiders, null, _realTimeInterval, _realTimeInterval);
            _newReminderTimer = new Timer(loadNotifications, null, _notificationLoadInterval, _notificationLoadInterval);
            _newWebVisitTimer = new Timer(loadWebVisitNotifications, null, _webVisitNotificationLoadInterval, _webVisitNotificationLoadInterval);
            string interval = System.Configuration.ConfigurationManager.AppSettings["NotificationInterval"];
            double.TryParse(interval, out notificationInterval);
        }

        public void LoadReminders(IEnumerable<Notification> notifications)
        {
            foreach (var notification in notifications)
                _notifications.TryAdd(notification.GetHashCode(), notification);
        }

        public void LoadWebVisits(IEnumerable<Notification> notifications)
        {
            foreach (var notification in notifications)
                _webVisitNotifications.TryAdd(notification.GetHashCode(), notification);
        }

        public int GetNewNotifications(int userId, int accountId, short roleId)
        {
            GetUserNotificationCountResponse response = userService.GetUnReadNotificationsCount(
               new GetUserNotificationCountRequest() { RequestedBy = userId, AccountId = accountId, RoleId = roleId });

            return response.Count;
        }

        public IEnumerable<int> GetNewWebVisitNotifications(int userId, int accountId)
        {
            GetUserNotificationCountResponse response = userService.GetUnReadWebVisitNotificationsCount(
               new GetUserNotificationCountRequest() { RequestedBy = userId, AccountId = accountId });

            return response.NotificationIds;
        }

        private void loadNotifications(object state)
        {
            var userIds = _connections.Keys.Select(s =>
            {
                int userId = 0;
                int.TryParse(s, out userId);
                return userId;
            });

            GetUserNotificationsResponse response = userService.GetImpendingReminderNotifications(
              new GetUserNotificationsRequest() { UserIds = userIds });

            if (response.Notifications.IsAny())
                this.LoadReminders(response.Notifications);
        }

        private void loadWebVisitNotifications(object state)
        {
            var userIds = _connections.Keys.Select(s =>
            {
                int userId = 0;
                int.TryParse(s, out userId);
                return userId;
            });


            GetUserNotificationsResponse response = userService.GetWebVisitNotifications(
                 new GetUserNotificationsRequest() { UserIds = userIds });

            if (response.WebVisitNotifications.IsAny())
                this.LoadWebVisits(response.WebVisitNotifications);
        }

        private void broadcastRemiders(object state)
        {
            foreach (var notification in _notifications.Values)
            {
                if (notification.Time.AddSeconds(-1) <= DateTime.Now.ToUniversalTime())
                {
                    if (notification.UserID.HasValue)
                        Clients.User(notification.UserID.Value.ToString()).notifyUser(notification);
                    else
                        Clients.All.notifyUser(notification);

                    Notification notificationRemoved = null;
                    _notifications.TryRemove(notification.GetHashCode(), out notificationRemoved);
                }
            }
            foreach (var notification in _webVisitNotifications.Values)
            {
                if (notification.Time.AddSeconds(-1) <= DateTime.Now.ToUniversalTime())
                {
                    if (notification.UserID.HasValue)
                        Clients.User(notification.UserID.Value.ToString()).notifyWebVisit(notification);
                    else
                        Clients.All.notifyWebVisit(notification);

                    Notification notificationRemoved = null;
                    _webVisitNotifications.TryRemove(notification.GetHashCode(), out notificationRemoved);
                }
            }
        }

        public void Dispose()
        {
            if (_timer != null) _timer.Dispose();
            if (_newReminderTimer != null) _newReminderTimer.Dispose();
            if (_newWebVisitTimer != null) _newWebVisitTimer.Dispose();
        }
    }
}
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartTouch.CRM.SignalR
{
    [HubName("notifierEngine")]
    public class NotificationHub : Hub
    {
        private readonly NotifierEngine notifier;

        public int userId { get; set; }
        public int accountId { get; set; }
        public int roleId { get; set; }

        public NotificationHub()
            : this(NotifierEngine.Instance)
        {
        }

        public NotificationHub(NotifierEngine notifier)
        {
            this.notifier = notifier;
        }

        public int GetUnreadNotificationCount()
        {
            this.InitializeProperties();
            return notifier.GetNewNotifications(userId, accountId, (short)roleId);
        }

        public IEnumerable<int> GetUnreadWebVisitNotificationCount()
        {
            this.InitializeProperties();
            return notifier.GetNewWebVisitNotifications(userId, accountId);
        }

        public override System.Threading.Tasks.Task OnConnected()
        {
            string name = userId.ToString();
            notifier.Add(name, Context.ConnectionId);
            return base.OnConnected();
        }

        public override System.Threading.Tasks.Task OnDisconnected(bool stopCalled)
        {
            string name = userId.ToString();
            notifier.Remove(name, Context.ConnectionId);
            return base.OnDisconnected(stopCalled);
        }

        public override System.Threading.Tasks.Task OnReconnected()
        {
            string name = userId.ToString();
            if (!notifier.GetConnections(name).Contains(Context.ConnectionId))
            {
                notifier.Add(name, Context.ConnectionId);
            }
            return base.OnReconnected();
        }

        private void InitializeProperties()
        {
            int userID, accountID, roleID;
            int.TryParse(Context.QueryString["userid"], out userID);
            int.TryParse(Context.QueryString["accountid"], out accountID);
            int.TryParse(Context.QueryString["roleid"], out roleID);
            userId = userID;
            accountId = accountID;
            roleId = roleID;
        }
    }
}

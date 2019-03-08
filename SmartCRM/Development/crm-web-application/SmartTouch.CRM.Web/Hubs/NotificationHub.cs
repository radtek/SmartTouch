using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using SmartTouch.CRM.Web.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace SmartTouch.CRM.Web.Hubs
{
    [HubName("notifierEngine")]
    public class NotificationHub : Hub
    {
        private readonly NotifierEngine notifier;

        public NotificationHub() : this(NotifierEngine.Instance)
        {
        }

        public NotificationHub(NotifierEngine notifier)
        {
            this.notifier = notifier;
        }

        public int GetUnreadNotificationCount()
        {
            var userId = Context.User.Identity.ToUserID();/*Thread.CurrentPrincipal.Identity.ToUserID();*/
            var acconutId = Context.User.Identity.ToAccountID();
            var roleId = Context.User.Identity.ToRoleID();
            return notifier.GetNewNotifications(userId, acconutId, roleId);
        }

        public IEnumerable<int> GetUnreadWebVisitNotificationCount()
        {
            var userId = Context.User.Identity.ToUserID();/*Thread.CurrentPrincipal.Identity.ToUserID();*/
            var accountId = Context.User.Identity.ToAccountID();
            return notifier.GetNewWebVisitNotifications(userId, accountId);
        }

        public override System.Threading.Tasks.Task OnConnected()
        {
            string name = Context.User.Identity.ToUserID().ToString();
            notifier.Add(name, Context.ConnectionId);
            return base.OnConnected();
        }

        public override System.Threading.Tasks.Task OnDisconnected(bool stopCalled)
        {
            string name = Context.User.Identity.ToUserID().ToString();
            notifier.Remove(name, Context.ConnectionId);
            return base.OnDisconnected(stopCalled);
        }

        public override System.Threading.Tasks.Task OnReconnected()
        {
            string name = Context.User.Identity.ToUserID().ToString();
            if (!notifier.GetConnections(name).Contains(Context.ConnectionId))
            {
                notifier.Add(name, Context.ConnectionId);
            }
            return base.OnReconnected();
        }
    }
    public class CustomUserIdProvider : IUserIdProvider
    {
        public string GetUserId(IRequest request)
        {
            return request.User.Identity.ToUserID().ToString();
        }

        public static string GetAccountId(IRequest request)
        {
            return request.User.Identity.ToAccountID().ToString();
        }
    }
}

//using System;
//using System.Linq;
//using System.Collections.Generic;
//using XSockets.Core.Utility.Storage;
//using XSockets.Core.XSocket;
//using XSockets.Core.XSocket.Helpers;
//using SmartTouch.CRM.Domain.ValueObjects;
//using SmartTouch.CRM.Entities;
//using SmartTouch.CRM.ApplicationServices.ViewModels;

//namespace SmartTouch.CRM.Web.Utilities.Sockets
//{
//    /// <summary>
//    /// A controller (plugin) that will take care of all notificaitons.   
//    /// 
//    /// Mission
//    /// Users create appointments and should be notified x minutes before the appointment
//    /// 
//    /// Challenges
//    /// 1 Keep track of appointments and when they occur
//    /// 2 Keep state between connections for all appointments per client
//    /// 3 When an reminder fires send the message to the correct client
//    /// 4 If the client is offline (maybe between pages) the message should be stored and sent when the client gets back online.
//    /// </summary>
//    public class Notifier : XSocketController
//    {
//        public Notifier()
//        {
//            this.OnOpen += Notifier_OnOpen;
//            this.OnClose += Notifier_OnClose;
//        }

//        void Notifier_OnClose(object sender, XSockets.Core.Common.Socket.Event.Arguments.OnClientDisconnectArgs e)
//        {
//            //Tell XSockets to store messages for me while I am offline
//            //We store them for 5 minutes and the messages we store is only for the Notify event
//            this.OfflineSubscribe(300000, "Notify");
//        }

//        void Notifier_OnOpen(object sender, XSockets.Core.Common.Socket.Event.Arguments.OnClientConnectArgs e)
//        {
//            //Get messages that arrived while I was offline
//            this.OnlinePublish();
//        }     

//        /// <summary>
//        /// Register a new reminder that will send a notification to the client x minutes before
//        /// </summary>
//        /// <param name="appointment"></param>
//        public void AddReminder(Notification reminder)
//        {
//            //Just add id´s...
//            reminder.StorageGuid = this.StorageGuid;
//            if(reminder.Id == null)
//                reminder.Id = Guid.NewGuid();
//            var notifs = Repository<Guid, Notification>.Find(n => n.Source == reminder.Source 
//                && n.EntityId == reminder.EntityId && n.StorageGuid == reminder.StorageGuid);

//            Notification existingNotification = notifs.FirstOrDefault();

//            if (existingNotification != null)
//                reminder.Id = existingNotification.Id;
//            //Add to storage so that the NotifierEngine can find it...
//            Repository<Guid, Notification>.AddOrUpdate(reminder.Id, reminder);
//        }

//        /// <summary>
//        /// Send a notification to the client, will be called from the notifierengine...
//        /// </summary>
//        /// <param name="appointment"></param>
//        public void Notify(Notification appointment)
//        {
//            //Use a custom extension to also queue messages if the user is offline
//            //this.SendAndQueue(p => p.StorageGuid == appointment.StorageGuid, appointment, "Notify");
//            //appointment.Status = NotificationStatus.New;
//            this.SendToAndQueue(appointment, "Notify", p => p.StorageGuid == appointment.StorageGuid);
//        }
//    }
//}
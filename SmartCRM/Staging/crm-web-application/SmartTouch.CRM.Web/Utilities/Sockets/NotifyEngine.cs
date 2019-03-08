//using System;
//using System.Timers;
//using XSockets.Core.Common.Globals;
//using XSockets.Core.Common.Socket;
//using XSockets.Core.Utility.Storage;
//using XSockets.Core.XSocket;
//using XSockets.Plugin.Framework;

//using SmartTouch.CRM.Domain.ValueObjects;


//namespace SmartTouch.CRM.Web.Utilities.Sockets
//{
//    /// <summary>
//    /// This is a longrunning contorller in XSockets...
//    /// In here we can handle stuff like polling for data or longrunning tasks..
//    /// Clients cant connect to this controller....
//    /// </summary>
//    [XSocketMetadata("NotifyEngine", Constants.GenericTextBufferSize, PluginRange.Internal)]
//    public class NotifyEngine : XSocketController
//    {
//        private static Notifier notifier;
//        private static Timer timer;
//        static NotifyEngine()
//        {
//            //Initialize your long running stuff here  
//            notifier = new Notifier();
            
//            //Start a timer thar ticks every second...
//            timer = new Timer(1000);
//            timer.Elapsed += timer_Elapsed;
//            timer.Start();
//        }

//        /// <summary>
//        /// On tick we check if there are any appointments that needs to be sent...
//        /// We will send a notification 10 seconds before the meeting.. Hurry up  ;)
//        /// </summary>
//        /// <param name="sender"></param>
//        /// <param name="e"></param>
//        static void timer_Elapsed(object sender, ElapsedEventArgs e)
//        {
//            var notifications = Repository<Guid, Notification>.Find(p => p.Time.AddSeconds(-1) <= DateTime.Now.ToUniversalTime());
//            foreach (var notification in notifications)
//            {
//                //Notify client
//                notifier.Notify(notification);
//                //Remove appointment
//                Repository<Guid, Notification>.Remove(notification.Id);
//            }
//        }
//    }
//}
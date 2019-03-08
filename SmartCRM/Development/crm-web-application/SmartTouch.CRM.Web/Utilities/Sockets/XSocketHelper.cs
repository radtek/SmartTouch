//using System;
//using XSockets.Core.Common.Socket;
//using XSockets.Core.XSocket;
//using XSockets.Core.XSocket.Helpers;

//namespace SmartTouch.CRM.Web.Utilities.Sockets
//{
//    /// <summary>
//    /// Custom extension for this sample
//    /// </summary>
//    public static class XSocketsHelper
//    {
//        /// <summary>
//        /// A helper for both sending to those online and storing for those offline...
//        /// </summary>
//        /// <typeparam name="T"></typeparam>
//        /// <param name="socket"></param>
//        /// <param name="condition"></param>
//        /// <param name="o"></param>
//        /// <param name="eventname"></param>
//        //public static void SendAndQueue<T>(this T socket, Func<T, bool> condition, object o, string eventname)
//        //    where T : XBaseSocket, IXBaseSocket
//        //{
//        //    //Send to onliners
//        //    socket.SendTo(condition, o, eventname);
//        //    //Queue message for offliners
//        //    socket.Queue(socket.ToTextArgs(o, eventname), condition);
//        //}
//    }
//}
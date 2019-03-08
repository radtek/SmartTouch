using LandmarkIT.Enterprise.CommunicationManager.Database;
using LandmarkIT.Enterprise.CommunicationManager.Operations;
using LandmarkIT.Enterprise.Utilities.Logging;

namespace LandmarkIT.Enterprise.Scheduler
{
    public static class JobProcessor
    {
        public static void SendQueueMail(int instance = 1)
        {            
            //action
            Logger.Current.Verbose("Request received for sending queue mails");
            new JobService().ProcessMailQueue(instance);
        }

        public static void SendQueueText()
        {
            Logger.Current.Verbose("Request received for sending queue text messages");
            new JobService().ProcessTextQueue();
        }
    }
}

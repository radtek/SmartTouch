using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.MessageQueues
{
    public interface IPublishSubscribeService
    {
        void Initialize();
        void SendMessage(Message message);
        void SendScheduledMessage(Message message, DateTime scheduleDate);
        Message RecieveMessage(Subscriber subscriber);
        void SendMessages(IEnumerable<Message> messages);
        void SendMessageWithOutTracking(Message message);
    }

    public enum Subscriber
    {
        LeadScoreEngine,
        AutomationEngine
    }
}

using SmartTouch.CRM.ApplicationServices.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ServiceImplementations
{
    public interface IMessageService
    {
        void SendMessages(SendMessagesRequest request);
        GetLeadScoreMessagesResponse GetLeadScoreMessages();
        void UpdateLeadScoreMessage(UpdateLeadScoreMessage request);
    }
}

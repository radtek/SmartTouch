using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.MessageQueues;

namespace SmartTouch.CRM.LeadScoringEngine.MessageHandlers
{
    internal interface IMessageHandler
    {
        MessageHandlerStatus Handle(Message message);        
    }
}

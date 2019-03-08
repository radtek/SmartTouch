using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.MessageQueues
{
    public interface IMessageQueuingService
    {
        void EnQueue(Message message);
        Message DeQueue();        
    }
}

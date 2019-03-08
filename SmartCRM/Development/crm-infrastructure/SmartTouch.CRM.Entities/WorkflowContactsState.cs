using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Entities
{
    public enum WorkflowContactsState : byte
    {
        ContactsStarted = 1,
        ContactsInProgress = 2,
        ContactsCompleted = 3,
        ContactsOptedOut = 4
    }
}

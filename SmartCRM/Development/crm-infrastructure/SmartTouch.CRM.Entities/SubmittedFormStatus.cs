using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Entities
{
    public enum SubmittedFormStatus:byte
    {
        ReadyToProcess = 1,
        Completed = 2,
        Fail = 3,
        Spam = 4
    }
}

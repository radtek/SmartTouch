using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Entities
{
    public enum ReminderType : byte
    {
        [Description("No Reminder")] NoReminder = 0,
        Email = 1,
        [Description("Pop Up")] PopUp = 2,
        [Description("Text Message")] TextMessage = 3
    }
}

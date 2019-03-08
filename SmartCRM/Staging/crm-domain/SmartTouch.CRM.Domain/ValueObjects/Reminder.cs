using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SmartTouch.CRM.Infrastructure.Domain;
using SmartTouch.CRM.Entities;

namespace SmartTouch.CRM.Domain.ValueObjects
{
    public class Reminder : ValueObjectBase
    {
        public ReminderType ReminderType { get; set; }
        public DateTime RemindOn { get; set; }

        protected override void Validate()
        {
           
        }

    }
}

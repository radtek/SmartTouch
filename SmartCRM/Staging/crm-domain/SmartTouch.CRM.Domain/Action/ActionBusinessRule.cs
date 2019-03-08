using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Action
{
    public static class ActionBusinessRule
    {
        public static readonly BusinessRule ActionMessageRequired = new BusinessRule("Action Details is mandatory.");
        public static readonly BusinessRule ContactsRequired = new BusinessRule("Include atleast one contact.");
        public static readonly BusinessRule RemindOndateInvalid = new BusinessRule("Remind on date should be later than today.");
        public static readonly BusinessRule ActionMessageLengthNotExceed1000 = new BusinessRule("Action Message length should not exceed 1000.");
        public static readonly BusinessRule RemindOnInvalid = new BusinessRule("RemindOn is in invalid format");
    }
}

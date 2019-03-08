using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SmartTouch.CRM.Infrastructure.Domain;

namespace SmartTouch.CRM.Domain.Note
{
    public static class NoteBusinessRule
    {
        public static readonly BusinessRule NoteDetailsRequired = new BusinessRule("Note Details is mandatory.");
        public static readonly BusinessRule ContactsRequired = new BusinessRule("Include atleast one contact.");
        public static readonly BusinessRule NoteDetailsNotMoreThan1000Characters = new BusinessRule("Note Details should not exceed More Than 1000 Characters");
    }
}

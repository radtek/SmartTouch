using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SmartTouch.CRM.Infrastructure.Domain;

namespace SmartTouch.CRM.Domain.Notes
{
    public static class NoteBusinessRule
    {
        public static readonly BusinessRule NoteDetailsRequired = new BusinessRule("[|Note details is mandatory.|]");
        public static readonly BusinessRule ContactsRequired = new BusinessRule("[|Include at least one contact.|]");
        public static readonly BusinessRule NoteDetailsNotMoreThan1000Characters = new BusinessRule("[|Note details should not exceed more than 1000 characters|]");
    }
}

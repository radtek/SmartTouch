using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Entities;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    public class ChangeOwnerViewModel
    {
        public int? OwnerId { get; set; }
        public string OwnerName { get; set; }
        public IEnumerable<ContactEntry> Contacts { get; set; }
        public bool SelectAll { get; set; }
    }
}

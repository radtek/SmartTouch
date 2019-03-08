using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    public class CustomContactViewModel
    {
        public int DuplicateContactId { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public bool HasPermission { get; set; }
    }
}

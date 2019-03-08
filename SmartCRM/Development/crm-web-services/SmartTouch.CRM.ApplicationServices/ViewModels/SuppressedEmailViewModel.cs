using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    public class SuppressedEmailViewModel
    {
        public int SuppressedEmailID { get; set; }
        public string Email { get; set; }
        public int AccountID { get; set; }
    }
}

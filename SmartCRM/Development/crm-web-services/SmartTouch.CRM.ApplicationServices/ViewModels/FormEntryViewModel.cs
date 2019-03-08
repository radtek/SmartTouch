using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    public class FormEntryViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int AccountId { get; set; }
        public int CreatedBy { get; set; }
        public int LastModifiedBy { get; set; }
    }
}

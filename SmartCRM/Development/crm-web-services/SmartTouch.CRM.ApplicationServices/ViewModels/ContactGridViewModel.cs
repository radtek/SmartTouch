using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    public class ContactGridViewModel
    {
        public string SearchString { get; set; }
        public string ShowingField { get; set; }
        public string SortingField { get; set; }
        public int PageNo { get; set; }
        public int PageCount { get; set; }
    }
}

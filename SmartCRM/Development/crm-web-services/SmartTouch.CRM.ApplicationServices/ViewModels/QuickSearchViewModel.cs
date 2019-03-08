using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    public class QuickSearchViewModel
    {
        public string Query { get; set; }
        public int[] SearchableEntities { get; set; }
        public int PageNumber { get; set; }
        public int Limit { get; set; }
    }
}

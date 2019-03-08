using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    public class GridEntryViewModel
    {
        public string SearchBy { get; set; }
        public int PageNumber { get; set; }
        public int PazeSize { get; set; }
        public string SortField { get; set; }
        public string SortDirection { get; set; }
        public string FilterBy { get; set; }
        public string ShowingType { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public int? UserId { get; set; }
        public bool IsFromDashboard { get; set; }
        public string DrildownType { get; set; }
        public List<int> ContactIds { get; set; }
    }
}

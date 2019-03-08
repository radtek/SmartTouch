using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    public class DashboardSettingViewModel
    {
        public byte Id { get; set; }
        public string Report { get; set; }
        public bool Value { get; set; }
    }

    public class DashboardViewModel
    {
        public int UserId { get; set; }
        public IEnumerable<DashboardSettingViewModel> Settings { get; set; }
    }
}

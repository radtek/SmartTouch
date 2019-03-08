using SmartTouch.CRM.ApplicationServices.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Role
{
    public class GetModulesRequest : ServiceRequestBase
    {
        public int? AccountID { get; set; }
    }

    public class GetModulesResponse : ServiceResponseBase
    {
        public IEnumerable<ModuleViewModel> ModuleViewModel { get; set; }
    }
}

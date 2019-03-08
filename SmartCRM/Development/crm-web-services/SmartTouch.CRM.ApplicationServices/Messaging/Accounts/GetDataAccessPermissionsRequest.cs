using SmartTouch.CRM.ApplicationServices.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Accounts
{
    public class GetDataAccessPermissionsRequest : ServiceRequestBase
    {
        public int accountId { get; set; }
        public List<ModuleViewModel> Modules { get; set; }
        public SettingsScreen Screen { get; set; }
    }

    public class GetDataAccessPermissionsResponse
    {
        public List<ModuleViewModel> Modules { get; set; }
    }

    public enum SettingsScreen : byte
    { 
        SharingScreen = 1,
        ConfigurationScreen = 2
    }
}

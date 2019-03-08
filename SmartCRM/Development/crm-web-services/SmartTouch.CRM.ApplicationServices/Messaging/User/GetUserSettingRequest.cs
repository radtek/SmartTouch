using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.ApplicationServices.ViewModels;

namespace SmartTouch.CRM.ApplicationServices.Messaging.User
{
    public class GetUserSettingRequest : ServiceRequestBase
    {
        public int AccountID { get; set; }
        public int UserID { get; set; }
        public UserSettingsViewModel UserSettingsViewModel { get; set; }
    }

    public class GetUserSettingResponse : ServiceResponseBase
    {      
        public virtual UserSettingsViewModel UserSettingsViewModel { get; set; }
    }
}

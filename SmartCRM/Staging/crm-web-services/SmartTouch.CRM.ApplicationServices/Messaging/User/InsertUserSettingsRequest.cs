using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.ApplicationServices.ViewModels;

namespace SmartTouch.CRM.ApplicationServices.Messaging.User
{
    public class InsertUserSettingsRequest : ServiceRequestBase
    {
        public UserSettingsViewModel UserSettingsViewModel { get; set; }
    }

    public class InsertUserSettingsResponse : ServiceResponseBase
    {
        public virtual UserSettingsViewModel UserSettingsViewModel { get; set; }
    }
}

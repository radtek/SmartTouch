using SmartTouch.CRM.Domain.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Accounts
{
    public class GetFirstLoginUserSettingsRequest : ServiceRequestBase
    {

    }

    public class GetFirstLoginUserSettingsResponse : ServiceResponseBase
    {
        public UserSettings UserSettings { get; set; }
    }
}

using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.AccountSettings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.AccountUnsubscribeView
{
   public class GetAccountUnsubscribeViewByAccountIdRequest : ServiceRequestBase
    {
        public int AccountID { get; set; }
    }

    public class GetAccountUnsubscribeViewByAccountIdResponse :ServiceResponseBase
    {
        public AccountSettingsViewModel accountUnsubscribeViewMap { get; set; }
    }
}

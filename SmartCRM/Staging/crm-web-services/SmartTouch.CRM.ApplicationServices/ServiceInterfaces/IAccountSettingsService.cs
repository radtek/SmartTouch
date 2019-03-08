using SmartTouch.CRM.ApplicationServices.Messaging.AccountUnsubscribeView;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ServiceInterfaces
{
   public  interface IAccountSettingsService
    {
       GetAccountUnsubscribeViewByAccountIdResponse GetAccountUnsubscribeView(GetAccountUnsubscribeViewByAccountIdRequest request); 
    }
}

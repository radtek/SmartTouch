using RestSharp;
using SmartTouch.CRM.ApplicationServices.Messaging.MailGun;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ServiceInterfaces
{
   public interface IMailGunService
    {
       GetRestResponse EmailValidate(GetRestRequest request);
       GetRestResponse BulkEmailValidate(GetRestRequest request);
    }
}

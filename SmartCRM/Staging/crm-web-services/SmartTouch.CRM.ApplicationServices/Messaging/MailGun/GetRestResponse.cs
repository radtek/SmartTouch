using RestSharp;
using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.MailGun
{
   public class GetRestResponse :ServiceResponseBase
    {
       public IRestResponse RestResponse { get; set; }
    }
    public class GetRestRequest :ServiceRequestBase
    {
        public string Email { get; set; }
    }
}

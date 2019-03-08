using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using SmartTouch.CRM.ApplicationServices.ServiceImplementations;
using SmartTouch.CRM.ApplicationServices.Messaging.MailGun;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.WebService.Helpers;
using System.Net.Http;
using SmartTouch.CRM.ApplicationServices.Messaging;

namespace SmartTouch.CRM.WebService.Controllers
{
    /// <summary>
    /// Creating MailGun controller 
    /// </summary>
    public class MailGunController : SmartTouchApiController
    {
        readonly  IMailGunService mailGunService;
        /// <summary>
        /// Creating constructor for maigun controller for accessing
        /// </summary>
        /// <param name="mailGunService"></param>
        public MailGunController(IMailGunService mailGunService)
        {
            this.mailGunService = mailGunService;
        }

        /// <summary>
        /// Get Email Status
        /// </summary>
        /// <param name="email">Email</param>
        /// <returns>Email Status </returns>
        [Route("EmailValidate")]
        [HttpGet]
        public HttpResponseMessage GetEmailStatus(string email)
        {
            ServiceResponseBase response = mailGunService.EmailValidate(new GetRestRequest() { Email = email });
            return Request.BuildResponse(response);
        }
    }
}
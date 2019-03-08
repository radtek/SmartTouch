using SmartTouch.CRM.ApplicationServices.Messaging.Enterprices;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;
using SmartTouch.CRM.WebService.Helpers;
using SmartTouch.CRM.ApplicationServices.Messaging;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using Newtonsoft.Json;
using LandmarkIT.Enterprise.Utilities.Logging;

namespace SmartTouch.CRM.WebService.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public class EnterpriseServicesController : SmartTouchApiController
    {

        readonly IEnterpriseService enterpriseService;
        /// <summary>
        /// Creating Constructor For EnterpriseServices
        /// </summary>
        /// <param name="enterpriseService"></param>
        public EnterpriseServicesController(IEnterpriseService enterpriseService)
        {
            this.enterpriseService = enterpriseService;
        }

       /// <summary>
       /// Fetching all reported coupons
       /// </summary>
       /// <param name="pagenumber"></param>
       /// <param name="pagesize"></param>
       /// <returns></returns>
        [Route("")]
       // [Route("{pagenumber}/{pagesize}")]
        [HttpPost]
        public HttpResponseMessage ReportedCouponsData(int pagenumber, int pagesize)
        {
            GetAllReportedCouponsRequest request = new GetAllReportedCouponsRequest() { PageNumber= pagenumber,Pagesize = pagesize };
            GetAllReportedCouponsResponse response = enterpriseService.GetAllCoupons(request);
            return Request.BuildResponse(response);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contactData"></param>
        /// <returns></returns>
        [Route("")]
        [Route("grabonemail")]
        [HttpPost]
        public HttpResponseMessage SendEmail(ContactDataViewModel contactData)
        { 
            SendEmailRequest request = new SendEmailRequest() {
                Contacts = contactData.Contacts,
                FormSubmissionIds = contactData.FormSubmissionIds,
                EmailBody = contactData.EmailBody,
                AccountId =this.AccountId,
                RequestedBy = this.UserId
            };
            SendEmailResponse response = enterpriseService.SendEmail(request);
            return Request.BuildResponse(response);
        }

    }
}
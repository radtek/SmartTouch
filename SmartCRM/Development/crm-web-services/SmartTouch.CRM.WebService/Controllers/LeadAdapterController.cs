using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SmartTouch.CRM.ApplicationServices.Messaging.LeadAdapters;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.WebService.Helpers;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Linq;
using LandmarkIT.Enterprise.Utilities.Logging;
using System;
using System.Configuration;

namespace SmartTouch.CRM.WebService.Controllers
{
    /// <summary>
    /// Creating leadadapter controller for leadadapter module
    /// </summary>
    public class LeadAdapterController : SmartTouchApiController
    {
        readonly ILeadAdapterService leadAdapterService;

        /// <summary>
        /// Creating constructor for leadadapter controller for accessing
        /// </summary>
        /// <param name="leadAdapterService">leadAdapterService</param>
        public LeadAdapterController(ILeadAdapterService leadAdapterService)
        {
            this.leadAdapterService = leadAdapterService;
        }

        /// <summary>
        /// To insert a new leadadapter
        /// </summary>
        /// <param name="viewModel">All the properties related to the leadadapter</param>
        /// <returns>Leadadapter Insertion Details</returns>
        [Route("InsertLeadadapter")]
        [HttpPost]
        public HttpResponseMessage InsertLeadadapter(LeadAdapterViewModel viewModel)
        {
            InsertLeadAdapterRequest request = new InsertLeadAdapterRequest() { LeadAdapterViewModel = viewModel };
            InsertLeadAdapterResponse response = leadAdapterService.InsertLeadAdapter(request);
            return Request.BuildResponse(response);
        }

        /// <summary>
        /// Update the leadadapter
        /// </summary>
        /// <param name="viewModel">All the properties related to leadadapter</param>
        /// <returns>Leadadapter Updation Details </returns>
        [Route("UpdateLeadadapter")]
        [HttpPost]
        public HttpResponseMessage UpdateLeadadapter(LeadAdapterViewModel viewModel)
        {
            UpdateLeadAdapterRequest request = new UpdateLeadAdapterRequest() { LeadAdapterViewModel = viewModel };
            UpdateLeadAdapterResponse response = leadAdapterService.UpdateLeadAdapter(request);
            return Request.BuildResponse(response);
        }

        /// <summary>
        /// To fetch the leadapter with id
        /// </summary>
        /// <param name="request">Id of the leadadapter</param>
        /// <returns>Leadadapter Details</returns>
        [Route("GetLeadadapter")]
        [HttpGet]
        public HttpResponseMessage GetLeadadapter(GetLeadAdapterRequest request)
        {
            GetLeadAdapterResponse response = leadAdapterService.GetLeadAdapter(request);
            return Request.BuildResponse(response);
        }

        /// <summary>
        /// To delete the leadadapter with the leadadapterid
        /// </summary>
        /// <param name="leadAdapterID"></param>
        /// <returns>Deleted Leadadapter Details</returns>
        [Route("DeleteLeadadapter")]
        [HttpPost]
        public HttpResponseMessage DeleteLeadadapter(int leadAdapterID)
        {
            DeleteLeadAdapterRequest request = new DeleteLeadAdapterRequest(leadAdapterID);
            DeleteLeadAdapterResponse response = leadAdapterService.DeleteLeadAdapter(request);
            return Request.BuildResponse(response);
        }

        [AllowAnonymous]
        [HttpGet]
        [HttpPost]
        [Route("leadgen")]
        public HttpResponseMessage LeadGen()
        {
            HttpResponseMessage msg = new HttpResponseMessage();
            try
            {
                if (HttpContext.Current.Request.HttpMethod == "GET")
                {
                    NameValueCollection nvcData = HttpUtility.ParseQueryString(Request.RequestUri.Query);
                    string ch = nvcData["hub.challenge"];
                    Logger.Current.Informational(ch);
                    Dictionary<string, string> queryStrings = new Dictionary<string, string>();

                    if (!string.IsNullOrEmpty(ch))
                        msg.Content = new StringContent(ch);
                }
                else if (HttpContext.Current.Request.HttpMethod == "POST")
                {
                    HttpContent requestContent = Request.Content;
                    string jsonContent = requestContent.ReadAsStringAsync().Result;
                    var parsed = JObject.Parse(jsonContent);
                    FacebookLeadGen leadGen = new FacebookLeadGen();
                    var entryObj = parsed.SelectToken("entry").Value<IEnumerable<object>>();
                    Logger.Current.Informational("json content:" + jsonContent);
                    if (entryObj != null && entryObj.Any())
                    {
                        var changesObj = parsed.SelectToken("entry[0].changes").Value<IEnumerable<object>>();
                        if (changesObj != null && changesObj.Any())
                        {
                            try
                            {
                                leadGen.AdGroupId = parsed.SelectToken("entry[0].changes[0].value.adgroup_id").Value<long>();
                                leadGen.AdID = parsed.SelectToken("entry[0].changes[0].value.ad_id").Value<long>();
                            }
                            catch
                            {
                                leadGen.AdGroupId = 0;
                                var adId = ConfigurationManager.AppSettings["FacebookAdID"].ToString();
                                leadGen.AdID = long.Parse(adId);
                            }
                            leadGen.FormID = parsed.SelectToken("entry[0].changes[0].value.form_id").Value<long>();
                            leadGen.LeadGenID = parsed.SelectToken("entry[0].changes[0].value.leadgen_id").Value<long>();
                            leadGen.PageID = parsed.SelectToken("entry[0].changes[0].value.page_id").Value<long>();
                            leadAdapterService.InsertFacebookLeadGen(new InsertFacebookLeadGenRequest() { FacebookLeadGen = leadGen });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Current.Error("error in facebook webhook", ex);
            }

            return msg;
        }
    }
}

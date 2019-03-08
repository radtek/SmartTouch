using SmartTouch.CRM.ApplicationServices.Messaging.DropdownValues;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.WebService.Helpers;
using System.Net.Http;
using System.Web.Http;

namespace SmartTouch.CRM.WebService.Controllers
{
    /// <summary>
    /// Creating dropdownvalues controller for dropdownvalues module
    /// </summary>
    public class DropdownValuesController : SmartTouchApiController
    {
        private readonly IDropdownValuesService dropdownValuesService;

        /// <summary>
        /// Creating constructor for dropdownvalues controller for accessing
        /// </summary>
        /// <param name="dropdownValuesService">dropdownValuesService</param>
        public DropdownValuesController(IDropdownValuesService dropdownValuesService)
        {
            this.dropdownValuesService = dropdownValuesService;

        }

        /// <summary>
        /// Gets Dropdown Values by AccountId.
        /// </summary>
        /// <param name="accountId">Id of an Account.</param>
        /// <returns>All Dropdown values</returns>
        [Route("DropDownValueFields")]
        [HttpGet]
        public HttpResponseMessage GetDropdownValues(int accountId)
        {
            GetDropdownListResponse response = dropdownValuesService.GetAll(new GetDropdownListRequest() { AccountID = accountId, Limit = 10 });
            return Request.BuildResponse(response);
        }

        /// <summary>
        /// Gets a Dropdown Value by DropdownId.
        /// </summary>
        /// <param name="dropdownId">Id of a Dropdown.</param>
        /// <returns>Dropdown Value</returns>
        [HttpGet]
        public HttpResponseMessage GetDropdownValue(byte dropdownId)
        {
            GetDropdownValueResponse response = dropdownValuesService.GetDropdownValue(new GetDropdownValueRequest() { DropdownID = dropdownId, AccountId = 1 });
            return Request.BuildResponse(response);
        }

        /// <summary>
        /// Get Template Data
        /// </summary>
        /// <param name="filename">Code </param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage GetTemplateDataByFileName(string filename)
        {
            GetTemplateDataResponse response = dropdownValuesService.GetTemplateData(new GetTemplateDataRequest() { FileName = filename });
            return Request.BuildResponse(response);
        }
    }
}
using System.Net.Http;
using System.Web.Http;
using SmartTouch.CRM.ApplicationServices.Messaging.CustomFields;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.WebService.Helpers;

namespace SmartTouch.CRM.WebService.Controllers
{
    /// <summary>
    /// Creating customfields controller for customfields module
    /// </summary>
    public class CustomFieldsController : SmartTouchApiController
    {
      private readonly ICustomFieldService customFieldService;
      /// <summary>
      /// Creating constructor for customfields controller for accessing
      /// </summary>
      /// <param name="customFieldService">customFieldService</param>
      public CustomFieldsController(ICustomFieldService customFieldService)
        {
            this.customFieldService = customFieldService;
        }

        /// <summary>
        /// Add a new custom field tab
        /// </summary>
        /// <param name="viewModel"></param>
      /// <returns>CustomFieldTab insertion details</returns>
        [Route("customtab")][HttpPost]
        public HttpResponseMessage PostCustomFieldTab(CustomFieldTabViewModel viewModel)
        {
            InsertCustomFieldTabRequest request = new InsertCustomFieldTabRequest() { CustomFieldTabViewModel = viewModel };
            InsertCustomFieldTabResponse response = customFieldService.InsertCustomFieldTab(request);
            return Request.BuildResponse(response);
        }


        /// <summary>
        /// Edit an existing custom field tab
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns>CustomFieldTab Updation details</returns>
        [Route("customtab")][HttpPut]
        public HttpResponseMessage PutCustomFieldTab(CustomFieldTabViewModel viewModel)
        {
            UpdateCustomFieldTabRequest request = new UpdateCustomFieldTabRequest() { CustomFieldTabViewModel = viewModel };
            UpdateCustomFieldTabResponse response = ModelState.IsValid ? customFieldService.UpdateCustomFieldTab(request) : null;
            return Request.BuildResponse(response);

        }

        /// <summary>
        /// Add a custom field
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns>CustomField insertion details</returns>
        [Route("insertcustomfield")][HttpPost]
        public HttpResponseMessage PostCustomField(FieldViewModel viewModel)
        {
            InsertCustomFieldRequest request = new InsertCustomFieldRequest() { CustomFieldViewModel = viewModel };
            InsertCustomFieldResponse response = customFieldService.InsertCustomField(request);
            return Request.BuildResponse(response);
        }


        /// <summary>
        /// Edit a custom field
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns>CustomField updation details</returns>
        [Route("CustomFields")][HttpPut]
        public HttpResponseMessage PutCustomField(FieldViewModel viewModel)
        {
            UpdateCustomFieldRequest request = new UpdateCustomFieldRequest() { CustomFieldViewModel = viewModel };
            UpdateCustomFieldResponse response = customFieldService.UpdateCustomField(request);
            return Request.BuildResponse(response);
        }


        /// <summary>
        /// Get all custom field tabs of an account
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns>All CustomFieldTabs Details(</returns>
        [Route("CustomFieldTab")][HttpGet]
        public HttpResponseMessage GetCustomFieldTabs(int accountId)
        {
            GetAllCustomFieldTabsRequest request = new GetAllCustomFieldTabsRequest(accountId);
            GetAllCustomFieldTabsResponse response = customFieldService.GetAllCustomFieldTabs(request);
            return Request.BuildResponse(response);
        }


        /// <summary>
        /// Saves all custom field tabs
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns>All CustomFieldTabs Saving Details Response</returns>
        [Route("customtabs/save")][HttpPut]
        public HttpResponseMessage SaveAllCustomFieldTabs(CustomFieldTabsViewModel viewModel)
        {
            SaveAllCustomFieldTabsRequest request = new SaveAllCustomFieldTabsRequest() { CustomFieldsViewModel = viewModel};
            SaveAllCustomFieldTabsResponse response = customFieldService.SaveAllCustomFieldTabs(request);
            return Request.BuildResponse(response);
        }

        /// <summary>
        /// Get accounts custom field value options as a list
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns>All CustomFieldsValueOptions </returns>
        [Route("customfieldsvalueoptions")][HttpGet]
        public dynamic GetCustomFieldsValueOptions(int accountId)
        {
            GetCustomFieldsValueOptionsRequest request = new GetCustomFieldsValueOptionsRequest() {AccountId = accountId};
            GetCustomFieldsValueOptionsResponse response = customFieldService.GetCustomFieldValueOptions(request);
            return response.CustomFieldValueOptions;
        }

        /// <summary>
        /// Gets the saved searchs count for custom field.
        /// </summary>
        /// <param name="fieldId">The field identifier.</param>
        /// <param name="valueOptionId">The value option identifier.</param>
        /// <returns></returns>
        [Route("savedsearchcustomfields")]
        [HttpGet]
        public HttpResponseMessage GetSavedSearchsCountForCustomField(int fieldId,int? valueOptionId)
        {
            GetSavedSearchsCountForCustomFieldRequest request = new GetSavedSearchsCountForCustomFieldRequest() { fieldId = fieldId, valueOptionId = valueOptionId };
            GetSavedSearchsCountForCustomFieldResponse response = customFieldService.GetSavedSearchsCountForCustomFieldById(request);
            return Request.BuildResponse(response);
        }

	}
}
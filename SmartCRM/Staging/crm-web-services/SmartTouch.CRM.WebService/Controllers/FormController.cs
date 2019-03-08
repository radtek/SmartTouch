using LandmarkIT.Enterprise.Utilities.ExceptionHandling;
using LandmarkIT.Enterprise.Utilities.Logging;
using SmartTouch.CRM.ApplicationServices.Messaging;
using SmartTouch.CRM.ApplicationServices.Messaging.Accounts;
using SmartTouch.CRM.ApplicationServices.Messaging.Forms;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.WebService.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.Http.Hosting;

namespace SmartTouch.CRM.WebService.Controllers
{
    /// <summary>
    /// Creating forms controller for forms module
    /// </summary>    
    public class FormController : SmartTouchApiController
    {
        private readonly IFormService formService;
        private readonly IDropdownValuesService dropDownService;
        private readonly IContactService contactService;
        private readonly IAccountService accountService;

        /// <summary>
        ///  Creating constructor for forms controller for accessing
        /// </summary>
        /// <param name="formService">formService</param>
        /// <param name="dropDownService">dropDownService</param>
        /// <param name="contactService">contactService</param>
        /// <param name="accountService">accountService</param>
        public FormController(IFormService formService, IDropdownValuesService dropDownService, IContactService contactService, IAccountService accountService)
        {
            this.formService = formService;
            this.dropDownService = dropDownService;
            this.contactService = contactService;
            this.accountService = accountService;
        }

        /// <summary>
        /// Insert a new form
        /// </summary>
        /// <param name="viewModel">Properties of a form.</param>
        /// <returns>Form Insertion Deatails response</returns>
        [Route("Forms")]
        [HttpPost]
        public HttpResponseMessage PostForm(FormViewModel viewModel)
        {
            InsertFormRequest request = new InsertFormRequest() { FormViewModel = viewModel };
            InsertFormResponse response = formService.InsertForm(request);
            return Request.BuildResponse(response);
        }

        /// <summary>
        /// Update and existing form
        /// </summary>
        /// <param name="viewModel">Properties of a form.</param>
        /// <returns>Form Updation Deatails response</returns>
        [Route("Forms")]
        [HttpPut]
        public HttpResponseMessage PutForm(FormViewModel viewModel)
        {
            UpdateFormRequest request = new UpdateFormRequest() { FormViewModel = viewModel, AccountId = this.AccountId, RequestedBy = this.UserId, RoleId = this.RoleId };
            UpdateFormResponse response = formService.UpdateForm(request);
            return Request.BuildResponse(response);
        }

        /// <summary>
        /// Delete form(s)
        /// </summary>
        /// <param name="formIds">Ids of a form.</param>
        /// <returns>Form Deletion Deatails response</returns>
        [Route("Form/DeleteForm")]
        [HttpDelete]
        public FormViewModel DeleteForm(DeleteFormRequest formIds)
        {
            DeleteFormRequest request = new DeleteFormRequest() { FormIDs = formIds.FormIDs };
            DeleteFormResponse response = formService.DeleteForm(request);
            return response.FormViewModel;
        }

        /// <summary>
        /// Submit a form
        /// </summary>
        /// <param name="viewModel">Properties of a submitted form form.</param>
        /// <returns>Form Submitted Deatails response</returns>
        [System.Web.Mvc.AllowAnonymous]
        [Route("submitformapi")]
        [HttpPost]
        [HttpGet]
        public HttpResponseMessage SubmitFormAPI2(SubmittedFormAPIViewModel viewModel)
        {
            Logger.Current.Verbose("In SubmitFormAPI2 method. Request received to submit form.");

            SubmittedFormViewModel submittedFormViewModel = new SubmittedFormViewModel();
            submittedFormViewModel.SubmittedFormFields = new List<SubmittedFormFieldViewModel>();
            var getFormData = viewModel.SubmittedFormFields.ToList();

            var frm = getFormData.GroupBy(x => x.Key).Select(x => new
            {
                Key = x.Select(g => g.Key).FirstOrDefault(),
                Value = string.Join("|", x.Select(g => g.Value).ToList())
            });

            foreach (var field in frm)
            {
                submittedFormViewModel.SubmittedFormFields.Add(new SubmittedFormFieldViewModel()
                {
                    Key = field.Key,
                    Value = !string.IsNullOrEmpty(field.Value)? field.Value.Trim(): field.Value
                });
            }
            SubmitFormRequest request = new SubmitFormRequest() { SubmittedFormViewModel = submittedFormViewModel, RequestedBy = this.UserId, AccountId = viewModel.AccountId };
            request.SubmittedFormViewModel.SubmittedOn = DateTime.Now.ToUniversalTime();
            request.SubmittedFormViewModel.FormId = viewModel.FormId;
            request.SubmittedFormViewModel.AccountId = viewModel.AccountId;
            SubmitFormResponse response = formService.SubmitForm(request);

            if (response.Exception == null && response.FormSubmissionEntryViewModel != null)
                return Request.BuildResponse(new FormSubmissionAPIResponse()
                {
                    Acknowledgement = "Form submitted successfully",
                    FormSubmissionId = response.FormSubmissionEntryViewModel.FormSubmissionId
                });
            else
                return Request.BuildResponse(new FormSubmissionAPIResponse()
                {
                    Acknowledgement = "Form submission failed",
                    Exception = response.Exception
                });
        }

        /// <summary>
        /// Submit a form - AJAX
        /// </summary>
        /// <param name="formData">Form data.</param>
        /// <returns>Form Submitted Deatails response</returns>
        [System.Web.Mvc.AllowAnonymous]
        [Route("submitform")]
        [HttpPost]
        [HttpGet]
        public HttpResponseMessage SubmitForm(FormDataCollection formData)
        {
            Logger.Current.Verbose("In SubmitForm method. Request received to submit form.");
            HttpResponseMessage formSubmissionResponse;
            SubmittedFormViewModel submittedFormViewModel = new SubmittedFormViewModel();
            var context = System.Web.HttpContext.Current.Request;

            submittedFormViewModel.SubmittedFormFields = new List<SubmittedFormFieldViewModel>();
            try
            {
                Logger.Current.Informational("Submitted form from Context: " + context.Form);
                if (formData != null)
                {
                    var getFormData = formData.ToList();
                    var submittedToForm = formData.Where(c => c.Key == "formid").FirstOrDefault();
                    var formAccountId = formData.Where(c => c.Key == "accountid").FirstOrDefault();
                    var requestDomain = formData.Where(c => c.Key == "domainname").FirstOrDefault();
                    var submittedBy = formData.Where(c => c.Key == "userid").FirstOrDefault();
                    var stiTrackingId = formData.Where(c => c.Key == "STITrackingID").FirstOrDefault();
                    var link = formData.Where(c => c.Key == "redirect-override").FirstOrDefault();
                    getFormData.Remove(submittedToForm);
                    getFormData.Remove(formAccountId);
                    getFormData.Remove(requestDomain);
                    getFormData.Remove(submittedBy);
                    getFormData.Remove(stiTrackingId);
                    getFormData.Remove(link);

                    int parsedFormId = 0;
                    int parsedAccountId = 0;
                    int.TryParse(submittedToForm.Value, out parsedFormId);
                    int.TryParse(formAccountId.Value, out parsedAccountId);

                    if (parsedFormId > 0 && parsedAccountId > 0)
                    {
                        Logger.Current.Verbose("Request received to submit form with ID: " + parsedFormId);


                        try
                        {
                            var frm = getFormData.GroupBy(x => x.Key).Select(x => new
                            {
                                Key = x.Select(g => g.Key).FirstOrDefault(),
                                value = string.Join("|", x.Select(g => g.Value).ToList())
                            });

                            foreach (var field in frm)
                            {
                                submittedFormViewModel.SubmittedFormFields.Add(new SubmittedFormFieldViewModel()
                                {
                                    Key = field.Key,
                                    Value = !string.IsNullOrEmpty(field.value) ? field.value.Trim() : field.value
                                });
                            }

                            SubmitFormRequest request = new SubmitFormRequest()
                            {
                                SubmittedFormViewModel = submittedFormViewModel,
                                RequestedBy = this.UserId,
                                AccountId = parsedAccountId
                            };

                            request.SubmittedFormViewModel.SubmittedOn = DateTime.Now.ToUniversalTime();
                            request.SubmittedFormViewModel.FormId = parsedFormId;
                            request.SubmittedFormViewModel.AccountId = parsedAccountId;
                            request.SubmittedFormViewModel.IPAddress = requestDomain.Value;
                            request.SubmittedFormViewModel.STITrackingID = stiTrackingId.Value;
                            var domain = accountService.GetAccountDomainUrl(new GetAccountDomainUrlRequest() { AccountId = parsedAccountId }).DomainUrl;

                            var userId = 0;
                            int.TryParse(submittedBy.Value, out userId);

                            request.SubmittedFormViewModel.OwnerId =
                                domain == requestDomain.Value ? userId : 0; // If this is a test form submission domain and requestDomain would be same. Hence, the submitting user will be the owner.

                            formService.SubmitForm(request);

                        }
                        catch (Exception ex)
                        {
                            Logger.Current.Error("Error in form submission: ", ex);
                        }

                        finally
                        {
                            SubmitFormResponse getFormResponse = formService.GetFormAcknowdegement(parsedFormId);
                            Logger.Current.Informational("Response: " + getFormResponse.Acknowledgement.Acknowledgement);
                            formSubmissionResponse = Request.BuildResponse(new FormResponse()
                            {
                                Success = true,
                                Acknowledgement = !string.IsNullOrEmpty(link.Value) ? link.Value : getFormResponse.Acknowledgement.Acknowledgement,
                                AcknowledgementType = !string.IsNullOrEmpty(link.Value) ? AcknowledgementType.Url : getFormResponse.Acknowledgement.AcknowledgementType
                            });
                        }
                    }
                    else
                    {
                        formSubmissionResponse = new HttpRequestMessage().CreateResponse(HttpStatusCode.NotAcceptable);
                    }
                }
                else
                    throw new Exception("Received empty form data");
                
            }
            catch (Exception ex)
            {
                ex.Data.Add("submitteddata", formData);
                Logger.Current.Error("Error in form submission: ", ex);
                formSubmissionResponse = new HttpRequestMessage().CreateResponse(HttpStatusCode.NotAcceptable);
            }
            return formSubmissionResponse;
        }

        /// <summary>
        /// Submit a form - Plain post
        /// </summary>
        /// <param name="formData"></param>
        /// <returns>Form Submitted Deatails response</returns>
        [System.Web.Mvc.AllowAnonymous]
        [Route("submitformpost")]
        [HttpPost]
        [HttpGet]
        public HttpResponseMessage SubmitFormPost(FormDataCollection formData)
        {
            Logger.Current.Verbose("In SubmitFormPost method. Request received to submit form.");
            SubmitFormResponse response = formService.ProcessFormSubmissionRequest(new SubmitFormRequest() { FormData = formData, RequestedBy = this.UserId });
            var httpRequestMessage = new HttpRequestMessage();
            httpRequestMessage.SetConfiguration(new HttpConfiguration());
            string message = response.Status == HttpStatusCode.OK ? "Form submitted successfully" : (response.Exception.Message ?? "An error occured. Please contact administrator.");
            return httpRequestMessage.CreateResponse(response.Status, message);
        }

        /// <summary>
        /// Handling Form Acknowledgement Massage
        /// </summary>
        /// <param name="response">Giving Form Acknowledgement message data </param>
        /// <returns>Getting Form Acknowledgement message data from DB  </returns>

        private HttpResponseMessage HandleFormAcknowledgementMessage(SubmitFormResponse response)
        {
            if (response.Acknowledgement.AcknowledgementType == AcknowledgementType.Message)
            {
                Logger.Current.Verbose("Reloading clients page");
                var redirectUrl = System.Web.HttpContext.Current.Request.UrlReferrer != null
                    ? System.Web.HttpContext.Current.Request.UrlReferrer.AbsoluteUri : "";
                Logger.Current.Verbose("Acknowledgment type is Message: " + redirectUrl);
                response.Acknowledgement.Acknowledgement = redirectUrl;
            }
            Logger.Current.Verbose("Redirecting to " + response.Acknowledgement);

            var redirectResponse = Request.CreateResponse(HttpStatusCode.Moved);
            redirectResponse.Headers.Location = new Uri(response.Acknowledgement.Acknowledgement);
            return redirectResponse;
        }

        /// <summary>
        /// Reindex all forms
        /// </summary>
        /// <returns>reIndexed Forms</returns>
        [Route("ReIndexForms")]
        public HttpResponseMessage ReIndexForms()
        {
            ReIndexDocumentResponse response = formService.ReIndexForms(new ReIndexDocumentRequest());
            return Request.BuildResponse(response);
        }

        /// <summary>
        /// Gets the filed data by identifier.
        /// </summary>
        /// <param name="fieldid">The fieldid.</param>
        /// <returns></returns>
        [Route("Form/GetField")]
        [HttpGet]
        public HttpResponseMessage GetFiledDataById(int fieldid)
        {
            var response = formService.GetFiledDataById(new GetFiledDataByIdRequest { FieldId = fieldid });
            return Request.BuildResponse(response);
        }

        /// <summary>
        /// Create a API Form.
        /// </summary>
        /// <param name="fieldid">The form Name.</param>
        /// <param name="fieldid">The accountId.</param>
        /// <param name="fieldid">The userId.</param>
        /// <returns></returns>
        [Route("createform")]
        [HttpPost]
        public HttpResponseMessage CreateForm(FormEntryViewModel model)
        {
            var response = new CreateAPIFormsResponse();
            try
            {
                if (model.AccountId != this.AccountId)
                    throw new Exception("Invalid AccountID");
                FormViewModel formModel = new FormViewModel();
                formModel.AccountId = model.AccountId;
                formModel.Acknowledgement = "Thank you !!";
                formModel.AcknowledgementType = AcknowledgementType.Message;
                formModel.CreatedBy = model.CreatedBy;
                formModel.LastModifiedOn = DateTime.UtcNow;
                formModel.LastModifiedBy = model.CreatedBy;
                formModel.HTMLContent = "<div></div>";
                formModel.Status = FormStatus.Active;
                formModel.Name = model.Name;
                formModel.IsAPIForm = true;
                response = formService.CreateAPIForm(new CreateAPIFormsRequest() { AccountId = model.AccountId, FormViewModel = formModel, RequestedBy = model.CreatedBy });
            }
            catch (Exception ex)
            {
                response.Exception = ex;
            }
            if (response.Exception != null)
            {
                var message = response.Exception.Message.Replace("[|", "").Replace("|]", "");
                response.Exception = new UnsupportedOperationException(message);
            }
            return Request.BuildResponse(response);
        }

        /// <summary>
        /// Update a API Form name.
        /// </summary>
        /// <param name="fieldid">The form Name.</param>
        /// <param name="fieldid">The accountId.</param>
        /// <param name="fieldid">The userId.</param>
        /// <returns></returns>
        [Route("updateformname")]
        [HttpPost]
        public HttpResponseMessage UpdateFormName(FormEntryViewModel model)
        {
            var response = new UpdateFormNameResponse();
            try
            {
                if (model.AccountId != this.AccountId)
                    throw new Exception("Invalid AccountID");
                response = formService.UpdateFormName(new UpdateFormNameRequest() { FormId = model.Id, FormName = model.Name, AccountId = model.AccountId, RequestedBy = model.LastModifiedBy });
            }
            catch (Exception ex)
            {
                response.Exception = ex;
            }
            if (response.Exception != null)
            {
                var message = response.Exception.Message.Replace("[|", "").Replace("|]", "");
                response.Exception = new UnsupportedOperationException(message);
            }
            return Request.BuildResponse(response);
        }
    }

    /// <summary>
    /// For form response 
    /// </summary>
    public class FormResponse : ApplicationServices.Messaging.ServiceResponseBase
    {
        /// <summary>
        /// Success is a property for FormResponse class
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Acknowledgement is a property for FormResponse class
        /// </summary>
        public string Acknowledgement { get; set; }

        /// <summary>
        /// Creating reference variable to AcknowledgementType
        /// </summary>
        public AcknowledgementType AcknowledgementType { get; set; }
    }
}

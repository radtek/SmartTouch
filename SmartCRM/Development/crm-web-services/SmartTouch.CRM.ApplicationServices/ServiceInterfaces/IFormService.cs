using SmartTouch.CRM.ApplicationServices.Messaging;
using SmartTouch.CRM.ApplicationServices.Messaging.Common;
using SmartTouch.CRM.ApplicationServices.Messaging.Contacts;
using SmartTouch.CRM.ApplicationServices.Messaging.Forms;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.Forms;

namespace SmartTouch.CRM.ApplicationServices.ServiceInterfaces
{
    public interface IFormService
    {
        GetFormResponse GetForm(GetFormRequest request);
        InsertFormResponse InsertForm(InsertFormRequest request);
        UpdateFormResponse UpdateForm(UpdateFormRequest request);
        DeleteFormResponse DeleteForm(DeleteFormRequest request);
        SearchFormsResponse GetAllForms(SearchFormsRequest request);
        GetAllContactFieldsResponse GetAllContactFields(GetAllContactFieldsRequest request);
        SubmitFormResponse SubmitForm(SubmitFormRequest request);
        GetFormContactsResponse GetFormViewSubmissions(GetFormContactsRequest request);
        ReIndexDocumentResponse ReIndexForms(ReIndexDocumentRequest request);
        ReIndexDocumentResponse ReIndexFormSubmissions(ReIndexDocumentRequest request);
        GetFormSubmissionResponse GetFormSubmission(GetFormSubmissionRequest request);
        GetAllFieldsResponse GetAllFields(GetAllFieldsRequest request);
        FormData GetFormData(int formSubmissionId);
        FormIndexingResponce FormIndexing(FormIndexingRequest request);
        GetFiledDataByIdResponce GetFiledDataById(GetFiledDataByIdRequest request);
        GetFormSubmissionDataResponse GetFormSubmittedData();
        string GetFormName(int formSubmissionId);
        void InsertFormSubmittedData(SubmittedFormViewModel submittedFormViewModel);
        GetFormFieldIDsResponse GetFormFieldIDs(GetFormFieldIDsRequest request);
        GetFormSubmissionsResponse GetFormSubmissions(GetFormSubmissionsRequest request);
        GetFormNameByIdResponse GetFormNameById(GetFormNameByIdRequest request);
        SubmitFormResponse ProcessFormSubmissionRequest(SubmitFormRequest formSubmissionRequest);
        SubmitFormResponse GetFormAcknowdegement(int formId);
        FormSubmissionEntryViewModel InsertFormSubmissionEntry(int contactId, SubmittedFormViewModel viewModel, short leadSourceId);
        CreateAPIFormsResponse CreateAPIForm(CreateAPIFormsRequest request);
        UpdateFormNameResponse UpdateFormName(UpdateFormNameRequest request);
        GetApproveLeadsResponse GetLeadsApproveQueue(GetApproveLeadsRequest request);
        UpdateFailedFromLeadsResponse UpdateFailedForm(UpdateFailedFormLeadsRequest request);
        void FailedFormsSummaryEmail();
        GetFailedFormsResultsResponse GetFailedFormsResults(GetFailedFormsResultsRequest request);
    }
}

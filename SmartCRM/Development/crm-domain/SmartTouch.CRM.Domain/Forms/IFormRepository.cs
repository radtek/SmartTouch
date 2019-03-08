using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Domain.Fields;
using SmartTouch.CRM.Infrastructure.Domain;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Domain.Actions;

namespace SmartTouch.CRM.Domain.Forms
{
    public interface IFormRepository : IRepository<Form, int>
    {
        bool IsFormNameUnique(Form form);

        void DeactivateForm(int[] formIds);

        IList<Field> GetAllContactFields();

        IEnumerable<Form> FindAll(string name, int limit, int pageNumber, byte status, int AccountID);

        IEnumerable<Form> FindAll(string name, byte status, int AccountID);

        Form GetFormById(int formId);

        IList<FieldValueOption> GetAllFieldValueOptions(int fieldId);

        FieldValueOption GetFieldValueOption(int fieldValueOptionId);

        IEnumerable<int> GetContactsByFormID(int FormID);

        bool isFormSubmissionAllowed(int formId);

        FormSubmission GetFormSubmissionByID(int FormSubmissionID);

        bool isAssociatedWithWorkflows(int[] FormID);

        bool isAssociatedWithLeadScoreRules(int[] FormID);

        bool isLinkedToWorkflows(int FormID);

        IEnumerable<Field> GetAllFields(int AccountID);

        Field GetFiledDataById(int fieldId);

        int InsertSubmittedFormData(SubmittedFormData submittedData, IEnumerable<SubmittedFormFieldData> submittedFormFieldData);

        SubmittedFormData GetFormSubmittedData();

        /// <summary>
        /// Get Form Submission Data by formSubmissionID NEXG- 3014
        /// </summary>
        /// <param name="formSubmissionID"></param>
        /// <returns></returns>
        SubmittedFormData GetFormSubmittedData(int formSubmissionID);

        IEnumerable<SubmittedFormFieldData> GetFormSubmittedFieldData(int formDataId);

        void UpdateFormSubmissionStatus(int submittedFormDataID, SubmittedFormStatus status, string spamRemarks, int? formSubmissionID);

        IEnumerable<string> GetAllSpamKeyWords();

        IEnumerable<SpamValidator> GetAllSpamValidators(int accountId);

        bool CheckForSpamIP(string ipAddress, int accountId, bool isSpam);

        void InsertSpamIPAddress(string IPAddress,bool Spam,int AccountId);

        int GettingFormIPSubmissionCount(int accountId, string ipAddress,int timeLmit,bool isFormSubmission);

        string GetFormNameById(int formId);

        bool CheckForIPExclusion(string ipAddress, int accountId);

        IEnumerable<Phone> GetPhoneFields(int contactId);

        IEnumerable<int> GetFormFieldIDs(int formId);

        IEnumerable<FormSubmission> GetFormSubmissions(int formId, DateTime? startDate, DateTime? endDate, int pageLimt, int pageNumber);

        FormAcknowledgement GetFormAcknowledgement(int formId);

        int CreateAPIForm(string formName, int accountId, int userId);

        bool UpdateFormName(int formId, string formName, int modifiedBy);

        bool IsValidAPISubmission(int formId, int accountId);
        List<short> GetDropdownValueTypeIdsByPhoneTypes(List<short> ids,int accountId);
        void ScheduleIndexing(int entityId, IndexType indexType, bool isPerculationNeeded);

        List<ApproveLeadsQueue> GetLeadsQueue(int accountId, int pageNumber, int limit, short dateRange);

        void UpdateFormData(ApproveLeadsQueue queue, int userId);
        
        string GetAPIJson(int apiLeadSubmissionID);

        void UpdateAPIData(string json, int userId, int apiLeadSubmissionId);

        List<int> GetActiveAcountIds();

        List<UserSummary> GetUserDetails(int accountId);

        IEnumerable<ActionContactsSummary> GetFailedFormsLeads(int accountId);
    }
    public interface IFormSubmissionRepository : IRepository<FormSubmission, int>
    {
        FormData GetFormSubmission(int formSubmissionId);
        string GetFormName(int formSubmissionId);
    }
}

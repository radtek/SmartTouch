using AutoMapper;
using LandmarkIT.Enterprise.Extensions;
using LandmarkIT.Enterprise.Utilities.ExceptionHandling;
using LandmarkIT.Enterprise.Utilities.Logging;
using RestSharp;
using SmartTouch.CRM.ApplicationServices.Exceptions;
using SmartTouch.CRM.ApplicationServices.Messaging;
using SmartTouch.CRM.ApplicationServices.Messaging.Campaigns;
using SmartTouch.CRM.ApplicationServices.Messaging.Common;
using SmartTouch.CRM.ApplicationServices.Messaging.Contacts;
using SmartTouch.CRM.ApplicationServices.Messaging.CustomFields;
using SmartTouch.CRM.ApplicationServices.Messaging.Forms;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.CustomFields;
using SmartTouch.CRM.Domain.Dropdowns;
using SmartTouch.CRM.Domain.Fields;
using SmartTouch.CRM.Domain.Forms;
using SmartTouch.CRM.Domain.LeadScoreRules;
using SmartTouch.CRM.Domain.Tags;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Domain.Workflows;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.Domain;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using SmartTouch.CRM.MessageQueues;
using SmartTouch.CRM.SearchEngine.Indexing;
using SmartTouch.CRM.SearchEngine.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;
using System.Text.RegularExpressions;
using System.Net.Http.Formatting;
using SmartTouch.CRM.ApplicationServices.Messaging.Accounts;
using SmartTouch.CRM.Domain.Communication;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ServiceImplementations
{
    public class FormService : IFormService
    {
        readonly IFormRepository formRepository;
        readonly IFormSubmissionRepository formSubmissionRepository;
        readonly IContactRepository contactRepository;
        readonly IUnitOfWork unitOfWork;
        readonly ITagRepository tagRepository;
        readonly IMessageService messageService;
        readonly IIndexingService indexingService;
        readonly ISearchService<Form> searchService;
        readonly ICachingService cachingService;
        readonly IContactService contactService;
        readonly ICustomFieldService customFieldService;
        readonly IDropdownRepository dropdownRepository;
        readonly IGeoService geoService;
        readonly IAccountService accountService;
        readonly IFindSpamService findSpamService;

        public FormService(IFormRepository formRepository
            , IFormSubmissionRepository formSubmissionRepository
            , IUnitOfWork unitOfWork
            , ITagRepository tagRepository
            , IContactRepository contactRepository
            , ICachingService cachingService
            , IDropdownRepository dropdownRepository
            , IContactService contactService
            , ICustomFieldService customFieldService
            , IIndexingService indexingService
            , ISearchService<Form> searchService
            , IMessageService messageService
            , IGeoService geoService
            , IAccountService accountService
            , IFindSpamService findSpamService)
        {
            this.formRepository = formRepository;
            this.formSubmissionRepository = formSubmissionRepository;
            this.unitOfWork = unitOfWork;
            this.tagRepository = tagRepository;
            this.contactRepository = contactRepository;
            this.customFieldService = customFieldService;
            this.contactService = contactService;
            this.cachingService = cachingService;
            this.dropdownRepository = dropdownRepository;
            this.messageService = messageService;
            this.searchService = searchService;
            this.indexingService = indexingService;
            this.geoService = geoService;
            this.accountService = accountService;
            this.findSpamService = findSpamService;
        }

        public GetFormResponse GetForm(GetFormRequest request)
        {
            GetFormResponse response = new GetFormResponse();
            Logger.Current.Verbose("Request received to fetch the form with FormID: " + request.Id);
            hasAccess(request.Id, request.RequestedBy, request.AccountId, request.RoleId);
            Form form = formRepository.GetFormById(request.Id);
            FormViewModel formViewModel = Mapper.Map<Form, FormViewModel>(form);
            response.FormViewModel = formViewModel;

            return response;
        }

        void hasAccess(int documentId, int? userId, int accountId, short roleId)
        {
            Logger.Current.Verbose("Request received to check access permission for DocumentId: " + documentId);

            bool isAccountAdmin = cachingService.IsAccountAdmin(roleId, accountId);
            if (!isAccountAdmin)
            {
                bool isPrivate = cachingService.IsModulePrivate(AppModules.Forms, accountId);
                if (isPrivate && !searchService.IsCreatedBy(documentId, userId, accountId))
                    throw new PrivateDataAccessException("Requested user is not authorized to get this Form.");
            }
        }

        public SearchFormsResponse GetAllForms(SearchFormsRequest request)
        {
            Logger.Current.Verbose("Request received to get all forms ");
            IEnumerable<Type> types = new List<Type>() { typeof(Form) };
            SearchFormsResponse response = search(request, types, null, false, false);

            return response;
        }

        private ResourceNotFoundException GetFormNotFoundException()
        {
            return new ResourceNotFoundException("[|The requested form was not found.|]");
        }

        void updateForm(FormViewModel viewModel)
        {
            Logger.Current.Verbose("Private method. Request received to update form. Id: " + viewModel.FormId);

            if (viewModel.Name.Length > 75)
            {
                throw new UnsupportedOperationException("[|Form Name Is Maximum 75 characters.|]");
            }

            Form form = Mapper.Map<FormViewModel, Form>(viewModel);
            bool isFormNameUnique = formRepository.IsFormNameUnique(form);
            if (!isFormNameUnique)
            {
                var message = "[|Form with name|] \"" + form.Name + "\" [|already exists.|]";
                throw new UnsupportedOperationException(message);
            }
            if (form.Status == FormStatus.Inactive)
            {
                bool isLinkedtoWorkflow = formRepository.isLinkedToWorkflows(form.Id);
                if (isLinkedtoWorkflow)
                    throw new UnsupportedOperationException("[|The selected Form is linked to Workflow|].[|You can not update the Status to Inactive|].");
            }


            foreach (FormField field in form.FormFields)
            {
                field.FormId = form.Id;
            }

            isFormValid(form);
            formRepository.Update(form);
            Form updatedForm = unitOfWork.Commit() as Form;

            foreach (Tag tag in form.Tags.Where(t => t.Id == 0))
            {
                Tag savedTag = tagRepository.FindBy(tag.TagName, viewModel.AccountId);
                indexingService.IndexTag(savedTag);
                accountService.ScheduleAnalyticsRefresh(savedTag.Id, (byte)IndexType.Tags);
            }

            updatedForm.Tags = null;
            updatedForm.HTMLContent = null;
            if (indexingService.Update<Form>(updatedForm) > 0)
                Logger.Current.Verbose("Form updated to elasticsearch successfully");

            Logger.Current.Informational("Form updated successfully. Id:" + updatedForm.Id);
        }

        public UpdateFormResponse UpdateForm(UpdateFormRequest request)
        {
            Logger.Current.Informational("Form updated successfully. Id:" + request.FormViewModel.FormId);

            hasAccess(request.FormViewModel.FormId, request.RequestedBy, request.AccountId, request.RoleId);
            request.FormViewModel.LastModifiedBy = (int)request.RequestedBy;
            request.FormViewModel.LastModifiedOn = DateTime.UtcNow;
            updateForm(request.FormViewModel);
            return new UpdateFormResponse();

        }

        public DeleteFormResponse DeleteForm(DeleteFormRequest request)
        {
            Logger.Current.Informational("Request received to delete form(s). Id(s):" + request.FormIDs);
            bool isAssociatedWithWorkflow = formRepository.isAssociatedWithWorkflows(request.FormIDs);
            bool isAssociatedWithLeadScoreRules = formRepository.isAssociatedWithLeadScoreRules(request.FormIDs);
            if (isAssociatedWithLeadScoreRules && isAssociatedWithWorkflow)
                throw new UnsupportedOperationException("[|The selected Form(s) is associated with Workflows and Lead Score Rules|]. [|Delete operation cancelled|]");
            else if (isAssociatedWithLeadScoreRules)
                throw new UnsupportedOperationException("[|The selected Form(s) is associated with lead score|]. [|Delete operation cancelled|]");
            else if (isAssociatedWithWorkflow)
                throw new UnsupportedOperationException("[|The selected Form(s) is associated with Workflow|]. [|Delete operation cancelled|]");
            formRepository.DeactivateForm(request.FormIDs);
            foreach (int formId in request.FormIDs)
                indexingService.Remove<Form>(formId);
            return new DeleteFormResponse();
        }

        public InsertFormResponse InsertForm(InsertFormRequest request)
        {
            Logger.Current.Verbose("Request received to insert a new Form.");

            FormViewModel newForm = insertForm(request.FormViewModel);
            return new InsertFormResponse() { FormViewModel = newForm };

        }

        FormViewModel insertForm(FormViewModel viewModel)
        {
            if (viewModel.Name.Length > 75)
            {
                throw new UnsupportedOperationException("[|Form Name Is Maximum 75 characters.|]");
            }

            viewModel.HTMLContent = viewModel.HTMLContent != null ?
                               viewModel.HTMLContent.Replace("\n", "") : null;

            Form form = Mapper.Map<FormViewModel, Form>(viewModel);

            bool isFormNameUnique = formRepository.IsFormNameUnique(form);
            if (!isFormNameUnique)
            {
                Logger.Current.Verbose("Duplicate form identified");
                var message = "[|Form with name|] \"" + form.Name + "\" [|already exists. Please choose a different name.|]";
                throw new UnsupportedOperationException(message);
            }
            isFormValid(form);
            formRepository.Insert(form);
            Form newForm = unitOfWork.Commit() as Form;

            foreach (Tag tag in form.Tags.Where(t => t.Id == 0))
            {
                Tag savedTag = tagRepository.FindBy(tag.TagName, viewModel.AccountId);
                indexingService.IndexTag(savedTag);
                accountService.ScheduleAnalyticsRefresh(savedTag.Id, (byte)IndexType.Tags);
            }

            newForm.Tags = null;
            newForm.HTMLContent = null;
            if (indexingService.Index<Form>(newForm) > 0)
                Logger.Current.Verbose("Indexed the form successfully");

            Logger.Current.Informational("Form inserted successfully.");
            return Mapper.Map<Form, FormViewModel>(newForm);
            //return null;
        }

        void isFormValid(Form form)
        {
            Logger.Current.Verbose("Request received to validate form with FormID " + form.Id);
            IEnumerable<BusinessRule> brokenRules = form.GetBrokenRules();

            if (brokenRules.Any())
            {
                StringBuilder brokenRulesBuilder = new StringBuilder();
                foreach (BusinessRule rule in brokenRules)
                {
                    brokenRulesBuilder.AppendLine(rule.RuleDescription);
                }

                throw new Exception(brokenRulesBuilder.ToString());
            }
        }

        /// <summary>
        /// Get all contact fields
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public GetAllContactFieldsResponse GetAllContactFields(GetAllContactFieldsRequest request)
        {
            var response = new GetAllContactFieldsResponse();
            var fields = formRepository.GetAllContactFields();
            response.ContactFields = Mapper.Map<IList<Field>, IList<FieldViewModel>>(fields);
            return response;
        }


        public GetFormSubmissionDataResponse GetFormSubmittedData()
        {
            SubmittedFormData data = formRepository.GetFormSubmittedData();
            IEnumerable<SubmittedFormFieldData> fieldData = new List<SubmittedFormFieldData>();
            SubmittedFormViewModel submitteddata = new SubmittedFormViewModel();
            GetFormSubmissionDataResponse responce = new GetFormSubmissionDataResponse();

            if (data != null)
            {
                submitteddata = Mapper.Map<SubmittedFormData, SubmittedFormViewModel>(data);
                fieldData = formRepository.GetFormSubmittedFieldData(data.SubmittedFormDataID);
                submitteddata.SubmittedFormFields = Mapper.Map<IEnumerable<SubmittedFormFieldData>, IEnumerable<SubmittedFormFieldViewModel>>(fieldData).ToList();
                responce.SubmittedFormViewModel = submitteddata;
            };
            return responce;

        }
       
        /// <summary>
        /// Get Submitted FormData By by formSubmissionID NEXG-3014
        /// </summary>
        /// <param name="formSubmissionID"></param>
        /// <returns></returns>
        public GetFormSubmissionDataResponse GetFormSubmittedData(int formSubmissionID)
        {
            SubmittedFormData data = formRepository.GetFormSubmittedData(formSubmissionID);
            IEnumerable<SubmittedFormFieldData> fieldData = new List<SubmittedFormFieldData>();
            SubmittedFormViewModel submitteddata = new SubmittedFormViewModel();
            GetFormSubmissionDataResponse responce = new GetFormSubmissionDataResponse();

            if (data != null)
            {
                submitteddata = Mapper.Map<SubmittedFormData, SubmittedFormViewModel>(data);
                fieldData = formRepository.GetFormSubmittedFieldData(data.SubmittedFormDataID);
                submitteddata.SubmittedFormFields = Mapper.Map<IEnumerable<SubmittedFormFieldData>, IEnumerable<SubmittedFormFieldViewModel>>(fieldData).ToList();
                responce.SubmittedFormViewModel = submitteddata;
            };
            return responce;

        }

        public void InsertFormSubmittedData(SubmittedFormViewModel submittedFormViewModel)
        {
            try
            {
                GetFormResponse formResponse = GetForm(new GetFormRequest(submittedFormViewModel.FormId));
                Logger.Current.Verbose("Fetching dropdownvalues from repository");
                var dropdownValues = dropdownRepository.FindAll("", 10, 1, submittedFormViewModel.AccountId);
                bool isAllowSubmission = false;
                IEnumerable<DropdownViewModel> dropdownViewModel = Mapper.Map<IEnumerable<Dropdown>, IEnumerable<DropdownViewModel>>(dropdownValues);
                Logger.Current.Informational("Fetched dropdownvalues");

                submittedFormViewModel.SubmittedFormFields.Each(sf =>
                {
                    sf.Value = !string.IsNullOrEmpty(sf.Value) ? sf.Value.Trim() : sf.Value;
                });

                var emailfield = submittedFormViewModel.SubmittedFormFields.SingleOrDefault(f => f.Key == ((byte)Entities.ContactFields.PrimaryEmail).ToString());
                var firstNamefield = submittedFormViewModel.SubmittedFormFields.SingleOrDefault(f => f.Key == ((byte)Entities.ContactFields.FirstNameField).ToString());
                var lastNamefield = submittedFormViewModel.SubmittedFormFields.SingleOrDefault(f => f.Key == ((byte)Entities.ContactFields.LastNameField).ToString());

                if (firstNamefield != null && string.IsNullOrEmpty(firstNamefield.Value) && lastNamefield != null && string.IsNullOrEmpty(lastNamefield.Value) && emailfield != null && string.IsNullOrEmpty(emailfield.Value))
                    isAllowSubmission = true;
                else if (firstNamefield != null && !string.IsNullOrEmpty(firstNamefield.Value) && lastNamefield != null && string.IsNullOrEmpty(lastNamefield.Value) && emailfield != null && string.IsNullOrEmpty(emailfield.Value))
                    isAllowSubmission = true;
                else if (firstNamefield != null && string.IsNullOrEmpty(firstNamefield.Value) && lastNamefield != null && !string.IsNullOrEmpty(lastNamefield.Value) && emailfield != null && string.IsNullOrEmpty(emailfield.Value))
                    isAllowSubmission = true;
                else
                    isAllowSubmission = false;

                Logger.Current.Informational("Fetched FirstName, LastName and Email from Form fields");
                if (formResponse.FormViewModel != null && formResponse.FormViewModel.Status == FormStatus.Active && !isAllowSubmission)
                {
                    #region Valid Submission
                    Logger.Current.Informational("Valid submission");
                    int formLastUpdatedBy = formResponse.FormViewModel.LastModifiedBy != 0 ? formResponse.FormViewModel.LastModifiedBy : formResponse.FormViewModel.CreatedBy;                    
                    submittedFormViewModel.CreatedBy = formLastUpdatedBy;
                    //submittedFormViewModel.CreatedBy = userService.GetDefaultAccountAdmin();
                    Logger.Current.Verbose("Form submission converting SubmittedViewModel to PersonViewModel");
                    PersonViewModel submittedPersonViewModel = convertToPersonViewModel(submittedFormViewModel, dropdownViewModel);
                    Logger.Current.Verbose("Form submission converted to PersonViewModel");
                    Contact contact = Mapper.Map<PersonViewModel, Person>(submittedPersonViewModel);

                    Person person = contact as Person;
                    IEnumerable<Address> addresses = person.Addresses;
                    List<BusinessRule> BusinessRules = new List<BusinessRule>();
                    foreach (Address address in addresses)
                        BusinessRules.AddRange(address.GetBrokenRules());

                    if (BusinessRules.Count > 0)
                    {
                        StringBuilder AddressValidationMessage = new StringBuilder();
                        BusinessRules = BusinessRules.Distinct().ToList();
                        foreach (BusinessRule rule in BusinessRules)
                            AddressValidationMessage.Append(rule.RuleDescription + "</br>");
                        AddressValidationMessage.Remove(AddressValidationMessage.Length - 5, 5);
                        throw new UnsupportedOperationException(AddressValidationMessage.ToString());
                    }
                    // validate: contactService.isContactValid(contact);
                    if (string.IsNullOrEmpty(person.FirstName) && string.IsNullOrEmpty(person.LastName) && emailfield != null && !string.IsNullOrEmpty(emailfield.Value) && !person.IsValidEmail(emailfield.Value))
                    {
                        formRepository.UpdateFormSubmissionStatus(submittedFormViewModel.SubmittedFormDataID, SubmittedFormStatus.Fail, "Bad Email", null);
                        return;
                    }


                    SearchParameters parameters = new SearchParameters() { AccountId = formResponse.FormViewModel.AccountId };
                    SearchResult<Contact> duplicateResult = new SearchResult<Contact>();
                    var duplicateContacts = contactService.CheckIfDuplicate(new CheckContactDuplicateRequest() { Person = person }).Contacts;
                    duplicateResult = new SearchResult<Contact>() { Results = duplicateContacts, TotalHits = duplicateContacts != null ? duplicateContacts.Count() : 0 };
                    PersonViewModel personViewModel = new PersonViewModel();
                    Exception ex = new Exception();

                    PersonViewModel previousdata = null;
                    var leadSourceField = submittedFormViewModel.SubmittedFormFields.SingleOrDefault(f => f.Key == ((byte)Entities.ContactFields.LeadSource).ToString());
                    var communityField = submittedFormViewModel.SubmittedFormFields.SingleOrDefault(f => f.Key == ((byte)Entities.ContactFields.Community).ToString());
                    var leadSourceDropDown = dropdownViewModel.Where(c => c.DropdownID == (byte)DropdownFieldTypes.LeadSources).FirstOrDefault().DropdownValuesList;
                    var leadSourceDropDownValue = new DropdownValueViewModel();
                    if (leadSourceField != null && !string.IsNullOrEmpty(leadSourceField.Value))
                    {
                        Logger.Current.Verbose("Attempting to fetch Lead Source drop down value submitted");
                        leadSourceDropDownValue = leadSourceDropDown.Where(e => e.DropdownValueID == short.Parse(leadSourceField.Value)).FirstOrDefault();
                    }

                    if(leadSourceDropDownValue == null || leadSourceField == null)
                    {
                        Logger.Current.Verbose("Attempting to fetch Classic Lead Source drop down value");
                        leadSourceDropDownValue = leadSourceDropDown.Where(e => e.DropdownValue == "Classic Form Submission").FirstOrDefault();
                        if (leadSourceDropDownValue == null)
                        {
                            Logger.Current.Verbose("Attempting to fetch Next Gen account default Lead Source drop down value");
                            leadSourceDropDownValue = leadSourceDropDown.Where(e => e.IsDefault).FirstOrDefault();
                            if (leadSourceDropDownValue == null)
                            {
                                Logger.Current.Verbose("Attempting to fetch First Lead Source drop down value");
                                leadSourceDropDownValue = leadSourceDropDown.FirstOrDefault();
                                if (leadSourceDropDownValue == null)
                                    throw new UnsupportedOperationException("[|The accound do not have the specified or any lead source configured. Please contact administrator|]");
                            }
                        }
                    }

                    DropdownValueViewModel leadSourceDropdownViewModel = new DropdownValueViewModel()
                    {
                        AccountID = formResponse.FormViewModel.AccountId,
                        DropdownID = (byte)ContactFields.LeadSource,
                        DropdownValue = leadSourceDropDownValue.DropdownValue,
                        DropdownValueID = leadSourceDropDownValue.DropdownValueID
                    };

                    var communityDropdownViewModel = new DropdownValueViewModel();
                    if (communityField != null)
                    {
                        IEnumerable<DropdownValueViewModel> communities = dropdownViewModel.Where(c => c.DropdownID == (byte)DropdownFieldTypes.Community).FirstOrDefault().DropdownValuesList;
                        DropdownValueViewModel selectedcommunity = communities.Where(x => x.DropdownValueID == short.Parse(communityField.Value)).FirstOrDefault();
                        if (selectedcommunity == null)
                            throw new UnsupportedOperationException("[|Community field is deleted, Please contact adminstrator|]");

                        communityDropdownViewModel = new DropdownValueViewModel()
                        {
                            AccountID = formResponse.FormViewModel.AccountId,
                            DropdownID = (byte)ContactFields.Community,
                            DropdownValue = selectedcommunity != null? selectedcommunity.DropdownValue:string.Empty,
                            DropdownValueID = selectedcommunity != null? selectedcommunity.DropdownValueID:(short)0
                        };
                    }

                    GetPersonResponse contactresposne = default(GetPersonResponse);
                    if (duplicateResult.TotalHits > 0)
                    {
                        GetPersonRequest contactrequest = new GetPersonRequest(duplicateResult.Results.Select(x => x.Id).FirstOrDefault())
                        {
                            AccountId = formResponse.FormViewModel.AccountId,
                            IncludeLastTouched = false

                        };
                        contactresposne = contactService.GetPerson(contactrequest);
                        previousdata = contactresposne.PersonViewModel;
                        Logger.Current.Informational("Company name for the submission : " + contactresposne.PersonViewModel.CompanyName);
                    }

                    string jsoncontent = this.GenerateJson(formResponse.FormViewModel.FormFields, formResponse.FormViewModel.AccountId,
                                                                                       submittedFormViewModel.SubmittedFormFields, previousdata);
                    submittedFormViewModel.SubmittedData = jsoncontent;
                    if (duplicateResult.TotalHits == 0)
                    {
                        submittedPersonViewModel.SelectedLeadSource = new List<DropdownValueViewModel>();
                        submittedPersonViewModel.SelectedLeadSource = submittedPersonViewModel.SelectedLeadSource.Append(leadSourceDropdownViewModel);
                        if (communityField != null)
                        {
                            submittedPersonViewModel.Communities = new List<DropdownValueViewModel>();
                            submittedPersonViewModel.Communities = submittedPersonViewModel.Communities.Append(communityDropdownViewModel);
                        }
                        Logger.Current.Verbose("Contact is ready to Insert into DB: " + personViewModel.ContactID);
                        //if (submittedPersonViewModel.LifecycleStage > 0)
                        //{
                            IEnumerable<DropdownValueViewModel> lifecycles = dropdownViewModel.Where(c => c.DropdownID == (byte)DropdownFieldTypes.LifeCycle).FirstOrDefault().DropdownValuesList;
                            if (!lifecycles.Where(l => l.DropdownValueID == submittedPersonViewModel.LifecycleStage).IsAny())
                                submittedPersonViewModel.LifecycleStage = lifecycles.Where(l => l.IsDefault).Select(s => s.DropdownValueID).FirstOrDefault();
                        //}

                        if (submittedPersonViewModel.Phones.IsAny())
                        {
                            IEnumerable<DropdownValueViewModel> phones = dropdownViewModel.Where(c => c.DropdownID == (byte)DropdownFieldTypes.PhoneNumberType).FirstOrDefault().DropdownValuesList;
                            submittedPersonViewModel.Phones.Each(p =>
                            {
                                if (!phones.Where(t => t.DropdownValueID == p.PhoneType).IsAny())
                                    p.PhoneType = phones.Where(t => t.IsDefault).Select(s => s.DropdownValueID).FirstOrDefault();
                            });
                        }
                        InsertPersonResponse insertPersonResponse = addPerson(submittedPersonViewModel, submittedPersonViewModel.CreatedBy, formResponse.FormViewModel.AccountId);
                        personViewModel = insertPersonResponse.PersonViewModel;
                        if (personViewModel != null)
                        {
                            Logger.Current.Verbose("Contact inserted successfully after submitting form. ContactId: " + personViewModel.ContactID);
                            contactRepository.TrackContactIPAddress(personViewModel.ContactID, submittedFormViewModel.IPAddress, submittedFormViewModel.STITrackingID);
                        }
                        ex = insertPersonResponse.Exception;
                    }
                    else
                    {
                        var existingContact = duplicateResult.Results.FirstOrDefault() as Person;
                        if (existingContact.OwnerId != null)
                            submittedFormViewModel.OwnerId = (int)existingContact.OwnerId;
                        if (string.IsNullOrEmpty(existingContact.CompanyName))
                            existingContact.CompanyName = previousdata != null ? previousdata.CompanyName : string.Empty;

                        var leadSource = existingContact.LeadSources != null ? existingContact.LeadSources.Select(e => e.Id).ToList() : new List<short>();
                        PersonViewModel existingContactViewModel = Mapper.Map<Person, PersonViewModel>(existingContact);
                        if (leadSource.IndexOf(leadSourceDropdownViewModel.DropdownValueID) == -1)
                        {
                            existingContactViewModel.SelectedLeadSource = existingContactViewModel.SelectedLeadSource ?? new List<DropdownValueViewModel>();
                            existingContactViewModel.SelectedLeadSource = existingContactViewModel.SelectedLeadSource.Append(leadSourceDropdownViewModel);
                        }

                        var community = existingContact.Communities != null ? existingContact.Communities.Select(e => e.Id).ToList() : new List<short>();
                        if (communityDropdownViewModel != null && community.IndexOf(communityDropdownViewModel.DropdownValueID) == -1)
                        {
                            existingContactViewModel.Communities = existingContactViewModel.Communities ?? new List<DropdownValueViewModel>();
                            existingContactViewModel.Communities = existingContactViewModel.Communities.Append(communityDropdownViewModel);
                        }
                        foreach (var customField in existingContactViewModel.CustomFields)
                        {
                            customField.ContactId = existingContactViewModel.ContactID;
                            Logger.Current.Verbose("contactField.ContactID: " + customField.ContactId);
                            Logger.Current.Verbose("existingContactViewModel.ContactID: " + existingContactViewModel.ContactID);
                        }

                        Logger.Current.Verbose("Contact is ready to Update into DB: " + personViewModel.ContactID);
                        UpdatePersonResponse updatePersonResponse = updatePerson(existingContactViewModel, submittedFormViewModel, submittedPersonViewModel.SocialMediaUrls);

                        personViewModel = updatePersonResponse.PersonViewModel;
                        ex = updatePersonResponse.Exception;
                        if (personViewModel != null)
                        {
                            Logger.Current.Verbose("Contact updated successfully after submitting form. ContactId: " + personViewModel.ContactID);
                            contactRepository.TrackContactIPAddress(personViewModel.ContactID, submittedFormViewModel.IPAddress, submittedFormViewModel.STITrackingID);
                        }
                    }

                    if (ex != null)
                        throw new UnsupportedOperationException("An exception occured while inserting contact through form. FormId: " + formResponse.FormViewModel.FormId, ex);
                    else
                        Logger.Current.Verbose("Person inserted/updated successfully. ContactId: " + personViewModel.ContactID);

                    FormSubmissionEntryViewModel formSubmission = InsertFormSubmissionEntry(personViewModel.ContactID, submittedFormViewModel, leadSourceDropDownValue.DropdownValueID);
                    formRepository.UpdateFormSubmissionStatus(submittedFormViewModel.SubmittedFormDataID, SubmittedFormStatus.Completed, "Completed", formSubmission.FormSubmissionId);
                    //for indexing Contact.
                    formRepository.ScheduleIndexing(personViewModel.ContactID, IndexType.Contacts, true);
                    #endregion
                }
                else
                {
                    Logger.Current.Informational("Invalid Form Details: " + submittedFormViewModel.SubmittedFormDataID);
                    formRepository.UpdateFormSubmissionStatus(submittedFormViewModel.SubmittedFormDataID, SubmittedFormStatus.Fail, "Failed", null);
                }
            }
            catch (Exception ex)
            {
                ex.Data.Clear();
                ex.Data.Add("SubmittedFormDataID", submittedFormViewModel.SubmittedFormDataID);
                Logger.Current.Error("Error While Processing Form Submission", ex);
                formRepository.UpdateFormSubmissionStatus(submittedFormViewModel.SubmittedFormDataID, SubmittedFormStatus.Fail, ex.Message, null);
            }
        }

        private string GetIPFromContext()
        {
            Logger.Current.Verbose("Request received to find the IP from the context");
            System.Web.HttpRequest currentRequest = System.Web.HttpContext.Current.Request;
            String clientIP = currentRequest.ServerVariables["REMOTE_ADDR"] ?? "";
            Logger.Current.Informational("Returning IP: " + clientIP);
            return clientIP;
        }

        public SubmitFormResponse ProcessFormSubmissionRequest(SubmitFormRequest formSubmissionRequest)
        {
            Logger.Current.Verbose("In ProcessFormSubmissionRequest");
            var response = new SubmitFormResponse();
            var formData = formSubmissionRequest.FormData;
            if (formData != null)
            {
                try
                {
                    SubmittedFormViewModel submittedFormViewModel = new SubmittedFormViewModel();
                    submittedFormViewModel.SubmittedFormFields = new List<SubmittedFormFieldViewModel>();
                    var getFormData = formData.ToList();
                    var formId = formData.FirstOrDefault(c => c.Key == "formid");
                    var accountId = formData.FirstOrDefault(c => c.Key == "accountid");
                    var requestDomain = formData.FirstOrDefault(c => c.Key == "domainname");
                    var submittedBy = formData.FirstOrDefault(c => c.Key == "userid");
                    var stiTrackingId = formData.FirstOrDefault(c => c.Key == "STITrackingID");

                    getFormData.Remove(formId);
                    getFormData.Remove(accountId);
                    getFormData.Remove(requestDomain);
                    getFormData.Remove(submittedBy);
                    getFormData.Remove(stiTrackingId);
                    int parsedFormId = 0;
                    int parsedAccountId = 0;
                    int.TryParse(formId.Value, out parsedFormId);
                    int.TryParse(accountId.Value, out parsedAccountId);
                    var contactEmail = getFormData.Where(c => c.Key == "7").FirstOrDefault();
                    var check = getFormData.Where(c => c.Key == "8").FirstOrDefault();

                    if (parsedFormId > 0 && parsedAccountId > 0 && !string.IsNullOrEmpty(contactEmail.Value))
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
                                Value = field.value
                            });
                        }

                        SubmitFormRequest request = new SubmitFormRequest()
                        {
                            SubmittedFormViewModel = submittedFormViewModel,
                            RequestedBy = formSubmissionRequest.RequestedBy,
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

                        response = this.SubmitForm(request);
                        Logger.Current.Informational("Form fields recorded successfully for processing");
                        response.Status = System.Net.HttpStatusCode.OK;
                    }
                    else
                    {
                        response.Exception = new Exception("AccountID or FormID or Email passed are empty or invalid");
                        response.Status = System.Net.HttpStatusCode.NotAcceptable;
                    }

                }
                catch (Exception ex)
                {
                    Logger.Current.Error("Error Occured while submitting form.", ex);
                    response.Exception = new Exception("Error occured. Form could not be processed");
                    response.Status = System.Net.HttpStatusCode.InternalServerError;
                }
            }
            else
            {
                response.Status = System.Net.HttpStatusCode.NotAcceptable;
                response.Exception = new Exception("Received empty form data.");
            }
            return response;
        }

        /// <summary>
        /// Add a new person from the form submission
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public SubmitFormResponse SubmitForm(SubmitFormRequest request)
        {
            Logger.Current.Verbose("Request received to submit a new Form. FormId: " + request.SubmittedFormViewModel.FormId);
            SubmitFormResponse response = new SubmitFormResponse();
            request.SubmittedFormViewModel.CreatedBy = request.RequestedBy;
            request.SubmittedFormViewModel.Status = SubmittedFormStatus.ReadyToProcess;
            request.SubmittedFormViewModel.IPAddress = GetIPFromContext();

            SubmittedFormData submittedData = Mapper.Map<SubmittedFormViewModel, SubmittedFormData>(request.SubmittedFormViewModel);
            IEnumerable<SubmittedFormFieldData> submittedFormFieldData = Mapper.Map<IEnumerable<SubmittedFormFieldViewModel>, IEnumerable<SubmittedFormFieldData>>(request.SubmittedFormViewModel.SubmittedFormFields.ToArray());
            var submissionId = formRepository.InsertSubmittedFormData(submittedData, submittedFormFieldData);
            response.FormSubmissionEntryViewModel = new FormSubmissionEntryViewModel() { FormSubmissionId = submissionId };
            //Make Form Submitted Contacts into CRM Contcats by kiran on 23/05/2018 - NEXG-3014 
            var task = Task.Run(() => ProcessTheFormSubmittedContacts(submissionId));
            task.Wait();
            var result = task.Status;
           // ProcessTheFormSubmittedContacts();
            //End
            return response;
        }

        InsertPersonResponse addPerson(PersonViewModel viewModel, int? ownerId, int accountId)
        {
            PersonViewModel personViewModel = viewModel;
            personViewModel.CreatedOn = DateTime.Now.ToUniversalTime();
            personViewModel.CreatedBy = ownerId;
            personViewModel.ReferenceId = Guid.NewGuid();
            personViewModel.FirstContactSource = ContactSource.Forms;
            personViewModel.ContactSource = ContactSource.Forms;
            personViewModel.IncludeInReports = true;
            InsertPersonRequest request = new InsertPersonRequest() { PersonViewModel = personViewModel, RequestedBy = null, AccountId = accountId, RequestedFrom = RequestOrigin.Forms };
            InsertPersonResponse response = contactService.InsertPerson(request);

            return response;
        }

        UpdatePersonResponse updatePerson(PersonViewModel personViewModel, SubmittedFormViewModel submittedFormViewModel, IEnumerable<Url> socialUrls)
        {
            UpdatePersonRequest request = new UpdatePersonRequest();

            request.PersonViewModel = updatePersonViewModel(personViewModel, submittedFormViewModel, socialUrls);
            request.PersonViewModel.ContactSource = ContactSource.Forms;
            Logger.Current.Informational("Company Id : " + request.PersonViewModel.CompanyID);            
            Logger.Current.Informational("Company name : " + request.PersonViewModel.CompanyName);
            request.AccountId = personViewModel.AccountID;
            request.RequestedFrom = RequestOrigin.Forms;
            request.PersonViewModel.IncludeInReports = true;
            UpdatePersonResponse response = contactService.UpdatePerson(request);

            return response;
        }

        public string GenerateJson(IEnumerable<FormFieldViewModel> formFields, int AccountID
            , IEnumerable<SubmittedFormFieldViewModel> SubmittedFormFields, PersonViewModel previousData)
        {
            IEnumerable<CustomFieldValueOptionViewModel> valueOptions = customFieldService
                .GetCustomFieldValueOptions(new GetCustomFieldsValueOptionsRequest() { AccountId = AccountID }).CustomFieldValueOptions;

            var dropdownValues = cachingService.GetDropdownValues(AccountID);
            IEnumerable<DropdownValueViewModel> phoneFields
                = dropdownValues.Where(i => i.DropdownID == (byte)DropdownFieldTypes.PhoneNumberType).FirstOrDefault().DropdownValuesList;

            var oldNewValues = new Dictionary<string, object> { };
            Action<string,int,object> AddOldNewValues = (key,fieldId ,value) =>
            {
                if (!oldNewValues.ContainsKey(key))
                    oldNewValues.Add(key, value);
                else
                    oldNewValues.Add(key+fieldId, value);
            };

            foreach (FormFieldViewModel field in formFields.OrderBy(f => f.SortId))
            {
                string previousValue = string.Empty;
                var submittedValue = SubmittedFormFields.Where(s => s.Key == field.FieldId.ToString()).Select(s => s.Value).FirstOrDefault();

                if (field.FieldId == (int)ContactFields.Community)
                {
                    if (previousData != null && previousData.Communities != null && previousData.Communities.Any())
                    {
                        var previousCommunities = previousData.Communities.Select(c => c.DropdownValue).ToArray();
                        previousValue = string.Join(",", previousCommunities);
                    }
                    var dropdown = dropdownValues.Where(c => c.DropdownID == (byte)DropdownFieldTypes.Community).FirstOrDefault();
                    var dropdownValue = "";
                    if (!string.IsNullOrEmpty(submittedValue))
                        dropdownValue = dropdown.DropdownValuesList.Where(c => c.DropdownValueID == short.Parse(submittedValue)).Select(c => c.DropdownValue).FirstOrDefault();

                    Logger.Current.Informational("Title : " + field.Title + " old value : " + previousValue + " New value : " + dropdownValue == null ? "" : dropdownValue);
                    AddOldNewValues(field.DisplayName, field.FieldId ,new { OldValue = previousValue, NewValue = dropdownValue == null ? "" : dropdownValue });
                }
                else if (field.FieldId == (int)ContactFields.LeadSource)
                {
                    if (previousData != null && previousData.SelectedLeadSource != null && previousData.SelectedLeadSource.Any())
                    {
                        var previousLeadSources = previousData.SelectedLeadSource.Select(c => c.DropdownValue).ToArray();
                        previousValue = string.Join(",", previousLeadSources);
                    }
                    var dropdown = dropdownValues.Where(c => c.DropdownID == (byte)DropdownFieldTypes.LeadSources).FirstOrDefault();
                    var dropdownValue = "";
                    if (!string.IsNullOrEmpty(submittedValue))
                        dropdownValue = dropdown.DropdownValuesList.Where(c => c.DropdownValueID == short.Parse(submittedValue)).Select(c => c.DropdownValue).FirstOrDefault();

                    Logger.Current.Informational("Title : " + field.Title + " old value : " + previousValue + " New value : " + dropdownValue == null ? "" : dropdownValue);
                    AddOldNewValues(field.DisplayName, field.FieldId, new { OldValue = previousValue, NewValue = dropdownValue == null ? "" : dropdownValue });
                }
                else if ((field.FieldInputTypeId == FieldType.checkbox || field.FieldInputTypeId == FieldType.dropdown
                        || field.FieldInputTypeId == FieldType.multiselectdropdown || field.FieldInputTypeId == FieldType.radio)
                        && submittedValue != null && !string.IsNullOrEmpty(submittedValue) && field.FieldId != (int)ContactFields.StateField
                        && field.FieldId != (int)ContactFields.CountryField)
                {
                    try
                    {
                        Logger.Current.Informational("FieldID : " + field.FieldId + " Submitted Value : " + submittedValue);
                        IList<int> splitSubmittedValue = Array.ConvertAll(submittedValue.Split('|'), int.Parse);
                        string optionValue = string.Empty;
                        if (valueOptions != null && valueOptions.Any())
                        {
                            var actualValues = valueOptions.Where(c => splitSubmittedValue.Contains(c.CustomFieldValueOptionId)).Select(c => c.Value);
                            optionValue = string.Join(",", actualValues.ToArray());
                        }
                        var selectedValues = previousData != null ? previousData.CustomFields.Where(i => i.CustomFieldId.ToString() == field.FieldId.ToString())
                            .Select(x => x.Value).FirstOrDefault() : null;
                        Logger.Current.Informational("FieldID : " + field.FieldId + " Selected Value : " + selectedValues);

                        var previouscustomfielddata = Array.ConvertAll(string.IsNullOrEmpty(selectedValues) ? new string[0] : selectedValues.Split('|').Where(w => !string.IsNullOrEmpty(w)).ToArray(), int.Parse);
                        string[] previousCustomfieldsValues = previousData == null ? new string[0] : valueOptions.Where(c => previouscustomfielddata.Contains(c.CustomFieldValueOptionId)).Select(c => c.Value).ToArray();
                        previousValue = string.Join(",", previousCustomfieldsValues);

                        Logger.Current.Informational("Title : " + field.Title + " old value : " + previousValue + " New value : " + optionValue);
                        AddOldNewValues(field.DisplayName, field.FieldId, new { OldValue = previousValue, NewValue = optionValue });
                    }
                    catch(Exception ex)
                    {
                        ex.Data.Clear();
                        ex.Data.Add("SubmittedCustomfieldID", field.FieldId);
                        ex.Data.Add("SubmittedCustomfieldValue", submittedValue);
                        Logger.Current.Error("Error while Processing CustomFields from form submission:" + ex.Message);
                    }

                }
                else if (previousData == null)
                {
                    Logger.Current.Informational("Title : " + field.Title + " old value : " + string.Empty + " New value : " + submittedValue);
                    AddOldNewValues(field.DisplayName, field.FieldId, new { OldValue = "", NewValue = submittedValue });
                }

                else
                {
                    ContactFields contactField = (ContactFields)field.FieldId;
                    if (ContactFields.FirstNameField == contactField)
                        previousValue = previousData.FirstName;
                    else if (ContactFields.LastNameField == contactField)
                        previousValue = previousData.LastName;
                    else if (ContactFields.PrimaryEmail == contactField)
                        previousValue = (previousData.Emails != null && previousData.Emails.Any()) ?
                                        previousData.Emails.Where(x => x.IsPrimary).FirstOrDefault().EmailId : "";
                    else if (ContactFields.TitleField == contactField)
                        previousValue = previousData.Title;
                    else if (ContactFields.MobilePhoneField == contactField)
                    {
                        short dropdownValueId = phoneFields.Where(i => i.DropdownValueTypeID == (short)DropdownValueTypes.MobilePhone)
                            .Select(x => x.DropdownValueID).FirstOrDefault();
                        previousValue = previousData.Phones.Where(i => i.PhoneType == dropdownValueId).Any() ?
                                        previousData.Phones.Where(i => i.PhoneType == dropdownValueId).FirstOrDefault().Number : "";
                    }
                    else if (ContactFields.CompanyNameField == contactField)
                        previousValue = previousData.CompanyName;
                    else if (ContactFields.HomePhoneField == contactField)
                    {
                        short dropdownValueId = phoneFields.Where(i => i.DropdownValueTypeID == (short)DropdownValueTypes.Homephone)
                            .Select(x => x.DropdownValueID).FirstOrDefault();
                        previousValue = previousData.Phones.Where(i => i.PhoneType == dropdownValueId).Any() ?
                                           previousData.Phones.Where(i => i.PhoneType == dropdownValueId).FirstOrDefault().Number : "";
                    }
                    else if (ContactFields.WorkPhoneField == contactField)
                    {
                        short dropdownValueId = phoneFields.Where(i => i.DropdownValueTypeID == (short)DropdownValueTypes.WorkPhone)
                            .Select(x => x.DropdownValueID).FirstOrDefault();
                        previousValue = previousData.Phones.Where(i => i.PhoneType == dropdownValueId).Any() ?
                                       previousData.Phones.Where(i => i.PhoneType == dropdownValueId).FirstOrDefault().Number : "";
                    }
                    else if (ContactFields.FacebookUrl == contactField)
                    {
                        previousValue = previousData.SocialMediaUrls.Where(x => x.MediaType == "Facebook").FirstOrDefault() == null ? ""
                                            : previousData.SocialMediaUrls.Where(x => x.MediaType == "Facebook").FirstOrDefault().URL;
                    }
                    else if (ContactFields.TwitterUrl == contactField)
                    {
                        previousValue = previousData.SocialMediaUrls.Where(x => x.MediaType == "Twitter").FirstOrDefault() == null ? ""
                                            : previousData.SocialMediaUrls.Where(x => x.MediaType == "Twitter").FirstOrDefault().URL;
                    }
                    else if (ContactFields.LinkedInUrl == contactField)
                        previousValue = previousData.SocialMediaUrls.Where(x => x.MediaType == "LinkedIn").FirstOrDefault() == null ? ""
                                           : previousData.SocialMediaUrls.Where(x => x.MediaType == "LinkedIn").FirstOrDefault().URL;
                    else if (ContactFields.GooglePlusUrl == contactField)
                        previousValue = previousData.SocialMediaUrls.Where(x => x.MediaType == "Google+").FirstOrDefault() == null ? ""
                                          : previousData.SocialMediaUrls.Where(x => x.MediaType == "Google+").FirstOrDefault().URL;
                    else if (ContactFields.WebsiteUrl == contactField)
                        previousValue = previousData.SocialMediaUrls.Where(x => x.MediaType == "Website").FirstOrDefault() == null ? ""
                                        : previousData.SocialMediaUrls.Where(x => x.MediaType == "Website").FirstOrDefault().URL;
                    else if (ContactFields.BlogUrl == contactField)
                        previousValue = previousData.SocialMediaUrls.Where(x => x.MediaType == "Blog").FirstOrDefault() == null ? ""
                                        : previousData.SocialMediaUrls.Where(x => x.MediaType == "Blog").FirstOrDefault().URL;
                    else if (ContactFields.AddressLine1Field == contactField)
                        previousValue = previousData.Addresses.Where(i => i.IsDefault == true).Any() ?
                                        previousData.Addresses.Where(i => i.IsDefault == true).FirstOrDefault().AddressLine1 : "";
                    else if (ContactFields.AddressLine2Field == contactField)
                        previousValue = previousData.Addresses.Where(i => i.IsDefault == true).Any() ?
                                       previousData.Addresses.Where(i => i.IsDefault == true).FirstOrDefault().AddressLine2 : "";
                    else if (ContactFields.CityField == contactField)
                        previousValue = previousData.Addresses.Where(i => i.IsDefault == true).Any() ?
                                       previousData.Addresses.Where(i => i.IsDefault == true).FirstOrDefault().City : "";
                    else if (ContactFields.StateField == contactField)
                        previousValue = previousData.Addresses.Where(i => i.IsDefault == true).Any() ?
                                       previousData.Addresses.Where(i => i.IsDefault == true).FirstOrDefault().State.Name : "";
                    else if (ContactFields.ZipCodeField == contactField)
                        previousValue = previousData.Addresses.Where(i => i.IsDefault == true).Any() ?
                                       previousData.Addresses.Where(i => i.IsDefault == true).FirstOrDefault().ZipCode : "";
                    else if (ContactFields.CountryField == contactField)
                        previousValue = previousData.Addresses.Where(i => i.IsDefault == true).Any() ?
                                       previousData.Addresses.Where(i => i.IsDefault == true).FirstOrDefault().Country.Name : "";
                    else
                    {
                        previousValue = previousData.CustomFields.Where(i => i.CustomFieldId == field.FieldId).Select(x => x.Value).FirstOrDefault();
                        previousValue = (previousValue == null ? "" : previousValue);
                    }
                    Logger.Current.Informational("Title : " + field.Title + " old value : " + previousValue + " New value : " + submittedValue);
                    AddOldNewValues(field.DisplayName, field.FieldId, new { OldValue = previousValue, NewValue = submittedValue == null ? "" : submittedValue });
                }

            }
            JavaScriptSerializer js = new JavaScriptSerializer();
            return js.Serialize(oldNewValues);
        }

        /// <summary>
        /// Insert a form submission record into 'FormSubmissions' table that links to the new contact inserted
        /// </summary>
        /// <param name="contactId"></param>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        public FormSubmissionEntryViewModel InsertFormSubmissionEntry(int contactId, SubmittedFormViewModel viewModel, short leadSourceId)
        {
            FormSubmission formSubmission = new FormSubmission();
            formSubmission.ContactId = contactId;
            formSubmission.FormId = viewModel.FormId;
            formSubmission.IPAddress = viewModel.IPAddress;
            formSubmission.SubmittedOn = viewModel.SubmittedOn;
            formSubmission.StatusID = Entities.FormSubmissionStatus.New;
            formSubmission.SubmittedData = viewModel.SubmittedData;
            formSubmission.LeadSourceID = leadSourceId;
            FormSubmissionEntryResponse response = insertFormSubmission(contactId, viewModel.AccountId, formSubmission);
            return response.FormSubmissionEntry;
        }

        FormSubmissionEntryResponse insertFormSubmission(int contactId, int accountId, FormSubmission formSubmission)
        {
            Logger.Current.Informational("Inserting Form Submission: ContactId - " + contactId + ", AccountId - " + accountId);
            if (contactId == 0)
            {
                throw new Exception("[|Error occured while inserting form submission|]");
            };

            //isFormSubmissionValid(formSubmission);
            FormSubmission newFormSubmission = new FormSubmission();
            try
            {
                formSubmissionRepository.Insert(formSubmission);
                newFormSubmission = unitOfWork.Commit() as FormSubmission;

                this.addToTopic(formSubmission.FormId, contactId, accountId, newFormSubmission.Id);
                Form form = formRepository.GetFormById(formSubmission.FormId);
                form.HTMLContent = null;
                if (form.IsDeleted == false)
                    indexingService.Index<Form>(form);
                Logger.Current.Informational("Indexing Form : FormId - " + form.Id);
            }
            catch (Exception ex)
            {
                Logger.Current.Error("An error occured while inserting Form Submission entry", ex);
            }

            return new FormSubmissionEntryResponse() { FormSubmissionEntry = Mapper.Map<FormSubmission, FormSubmissionEntryViewModel>(newFormSubmission) };
        }

        void addToTopic(int formId, int contactId, int accountId, int formSubmissionId)
        {
            var message = new TrackMessage()
            {
                EntityId = formId,
                AccountId = accountId,
                ContactId = contactId,
                LeadScoreConditionType = (int)LeadScoreConditionType.ContactSubmitsForm,
                LinkedEntityId = formSubmissionId
            };
            messageService.SendMessages(new Messaging.Messages.SendMessagesRequest()
                {
                    Message = message
                });
        }

        PersonViewModel convertToPersonViewModel(SubmittedFormViewModel viewModel, IEnumerable<DropdownViewModel> dropdownValues)
        {
            Logger.Current.Verbose("In convertToPersonViewModel");

            PersonViewModel personViewModel = new PersonViewModel();
            Person person = new Person();
            var firstNameField = viewModel.SubmittedFormFields.SingleOrDefault(f => f.Key == ((byte)Entities.ContactFields.FirstNameField).ToString());
            personViewModel.FirstName = firstNameField != null ? firstNameField.Value : null;
            var lastNameField = viewModel.SubmittedFormFields.SingleOrDefault(f => f.Key == ((byte)Entities.ContactFields.LastNameField).ToString());
            personViewModel.LastName = lastNameField != null ? lastNameField.Value : null;
            Logger.Current.Verbose("First Name, Last Name ");

            var companyField = viewModel.SubmittedFormFields.SingleOrDefault(f => f.Key == ((byte)Entities.ContactFields.CompanyNameField).ToString());
            personViewModel.CompanyName = companyField != null ? companyField.Value : null;

            var title = viewModel.SubmittedFormFields.SingleOrDefault(f => f.Key == ((byte)Entities.ContactFields.TitleField).ToString());
            personViewModel.Title = title != null ? title.Value : null;

            var emailField = viewModel.SubmittedFormFields.SingleOrDefault(f => f.Key == ((byte)Entities.ContactFields.PrimaryEmail).ToString());
            personViewModel.Emails = new List<Email>();
            if (emailField != null && !string.IsNullOrEmpty(emailField.Value) && person.IsValidEmail(emailField.Value))
            {
                personViewModel.Emails.Add(new Email()
                   {
                       AccountID = viewModel.AccountId,
                       EmailId = emailField.Value,
                       IsPrimary = true,
                       EmailStatusValue = EmailStatus.NotVerified,
                   });

            }
            var mobilePhoneField = viewModel.SubmittedFormFields.SingleOrDefault(f => f.Key == ((byte)Entities.ContactFields.MobilePhoneField).ToString());
            var homePhoneField = viewModel.SubmittedFormFields.SingleOrDefault(f => f.Key == ((byte)Entities.ContactFields.HomePhoneField).ToString());
            var workPhoneField = viewModel.SubmittedFormFields.SingleOrDefault(f => f.Key == ((byte)Entities.ContactFields.WorkPhoneField).ToString());
            Logger.Current.Verbose("Extracting phonetypes");

            var phoneTypes = dropdownValues.Where(s => s.DropdownID == (byte)DropdownFieldTypes.PhoneNumberType)
                 .Select(s => s.DropdownValuesList).FirstOrDefault().Where(d => d.IsActive == true);
            Logger.Current.Verbose("Extracted phonetypes");

            personViewModel.Phones = new List<Phone>();

            if (mobilePhoneField != null && !string.IsNullOrEmpty(mobilePhoneField.Value) && !(mobilePhoneField.Value.Length < 10 || mobilePhoneField.Value.Length > 15))
            {
                Phone phone = new Phone();
                phone.Number = mobilePhoneField.Value;
                phone.AccountID = viewModel.AccountId;
                phone.IsPrimary = phoneTypes.Where(c => c.DropdownValueTypeID == (short)DropdownValueTypes.MobilePhone).Select(c => c.IsDefault).FirstOrDefault();
                phone.PhoneType = phoneTypes.Where(c => c.DropdownValueTypeID == (short)DropdownValueTypes.MobilePhone).Select(c => c.DropdownValueID).FirstOrDefault();
                phone.PhoneTypeName = phoneTypes.Where(c => c.DropdownValueTypeID == (short)DropdownValueTypes.MobilePhone).Select(c => c.DropdownValue).FirstOrDefault();
                if (person.IsValidPhoneNumberLength(phone.Number.TrimStart(new char[] { '0', '1' })))
                    personViewModel.Phones.Add(phone);
                Logger.Current.Verbose("Mobile phone extracted");

            }

            if (homePhoneField != null && !string.IsNullOrEmpty(homePhoneField.Value) && !(homePhoneField.Value.Length < 10 || homePhoneField.Value.Length > 15))
            {
                Phone phone = new Phone();
                phone.Number = homePhoneField.Value;
                phone.AccountID = viewModel.AccountId;
                phone.IsPrimary = phoneTypes.Where(c => c.DropdownValueTypeID == (short)DropdownValueTypes.Homephone).Select(c => c.IsDefault).FirstOrDefault();
                phone.PhoneType = phoneTypes.Where(c => c.DropdownValueTypeID == (short)DropdownValueTypes.Homephone).Select(c => c.DropdownValueID).FirstOrDefault();
                phone.PhoneTypeName = phoneTypes.Where(c => c.DropdownValueTypeID == (short)DropdownValueTypes.Homephone).Select(c => c.DropdownValue).FirstOrDefault();
                if (person.IsValidPhoneNumberLength(phone.Number))

                    personViewModel.Phones.Add(phone);
                Logger.Current.Verbose("Home phone extracted");

            }

            if (workPhoneField != null && !string.IsNullOrEmpty(workPhoneField.Value) && !(workPhoneField.Value.Length < 10 || workPhoneField.Value.Length > 15))
            {
                Phone phone = new Phone();
                phone.Number = workPhoneField.Value;
                phone.AccountID = viewModel.AccountId;
                phone.IsPrimary = phoneTypes.Where(c => c.DropdownValueTypeID == (short)DropdownValueTypes.WorkPhone).Select(c => c.IsDefault).FirstOrDefault();
                phone.PhoneType = phoneTypes.Where(c => c.DropdownValueTypeID == (short)DropdownValueTypes.WorkPhone).Select(c => c.DropdownValueID).FirstOrDefault();
                phone.PhoneTypeName = phoneTypes.Where(c => c.DropdownValueTypeID == (short)DropdownValueTypes.WorkPhone).Select(c => c.DropdownValue).FirstOrDefault();
                if (person.IsValidPhoneNumberLength(phone.Number))
                    personViewModel.Phones.Add(phone);
                Logger.Current.Verbose("Work phone extracted");

            }

            if (personViewModel.Phones != null && personViewModel.Phones.Any())
            {
                var primaryPhone = personViewModel.Phones.Where(p => p.IsPrimary == true).FirstOrDefault();
                if (primaryPhone == null)
                {
                    personViewModel.Phones.Where(p => p.IsPrimary = true).FirstOrDefault();
                }
            }

            var facebookUrl = viewModel.SubmittedFormFields.SingleOrDefault(f => f.Key == ((byte)Entities.ContactFields.FacebookUrl).ToString());
            var twitterUrl = viewModel.SubmittedFormFields.SingleOrDefault(f => f.Key == ((byte)Entities.ContactFields.TwitterUrl).ToString());
            var linkedInUrl = viewModel.SubmittedFormFields.SingleOrDefault(f => f.Key == ((byte)Entities.ContactFields.LinkedInUrl).ToString());
            var googlePlusUrl = viewModel.SubmittedFormFields.SingleOrDefault(f => f.Key == ((byte)Entities.ContactFields.GooglePlusUrl).ToString());
            var websiteUrl = viewModel.SubmittedFormFields.SingleOrDefault(f => f.Key == ((byte)Entities.ContactFields.WebsiteUrl).ToString());
            var blogUrl = viewModel.SubmittedFormFields.SingleOrDefault(f => f.Key == ((byte)Entities.ContactFields.BlogUrl).ToString());


            IList<Url> socialMediaUrls = new List<Url>();
            if (facebookUrl != null)
            {
                socialMediaUrls.Add(new Url { MediaType = "Facebook", URL = facebookUrl.Value });
            }
            if (twitterUrl != null)
            {
                socialMediaUrls.Add(new Url { MediaType = "Twitter", URL = twitterUrl.Value });
            }
            if (linkedInUrl != null)
            {
                socialMediaUrls.Add(new Url { MediaType = "LinkedIn", URL = linkedInUrl.Value });
            }
            if (googlePlusUrl != null)
            {
                socialMediaUrls.Add(new Url { MediaType = "Google+", URL = googlePlusUrl.Value });
            }
            if (websiteUrl != null)
            {
                socialMediaUrls.Add(new Url { MediaType = "Website", URL = websiteUrl.Value });
            }
            if (blogUrl != null)
            {
                socialMediaUrls.Add(new Url { MediaType = "Blog", URL = blogUrl.Value });
            }
            personViewModel.SocialMediaUrls = socialMediaUrls;

            var accountPermissions = cachingService.GetAccountPermissions(viewModel.AccountId);
            Logger.Current.Verbose("Fetched Account Permissions");

            if (accountPermissions.Contains((byte)AppModules.FullContact))
            {
                var socialProfileVM = ManagePersonSocialProfiles(personViewModel);
                personViewModel = socialProfileVM;
            }
            Logger.Current.Verbose("Fetched social profiles");

            personViewModel.Addresses = new List<AddressViewModel>();
            var addressLine1 = viewModel.SubmittedFormFields.SingleOrDefault(f => f.Key == ((byte)Entities.ContactFields.AddressLine1Field).ToString());
            var addressLine2 = viewModel.SubmittedFormFields.SingleOrDefault(f => f.Key == ((byte)Entities.ContactFields.AddressLine2Field).ToString());
            var city = viewModel.SubmittedFormFields.SingleOrDefault(f => f.Key == ((byte)Entities.ContactFields.CityField).ToString());
            var state = viewModel.SubmittedFormFields.SingleOrDefault(f => f.Key == ((byte)Entities.ContactFields.StateField).ToString());
            var country = viewModel.SubmittedFormFields.SingleOrDefault(f => f.Key == ((byte)Entities.ContactFields.CountryField).ToString());
            //FIX: If user submits country as Canada or United States or India, appropriate country code will be assigned.
            if (country != null)
            {
                if (country.Value.ToLower().Contains("Canada"))
                    country.Value = "CN";
                else if (country.Value.ToLower().Contains("India"))
                    country.Value = "IN";
                else
                    country.Value = "US";
            }

            var zip = viewModel.SubmittedFormFields.SingleOrDefault(f => f.Key == ((byte)Entities.ContactFields.ZipCodeField).ToString());

            if (addressLine1 != null || addressLine2 != null || city != null || zip != null || country != null || state != null)
            {
                Logger.Current.Verbose("Processing Address1");
                geoService.GetCountriesAndStates(new ApplicationServices.Messaging.Geo.GetCountriesAndStatesRequest());
                Logger.Current.Verbose("Processing Address2");

                var addressDropdown = dropdownValues.Where(s => s.DropdownID == (byte)DropdownFieldTypes.AddressType)
                                                .Select(s => s.DropdownValuesList).FirstOrDefault();
                Logger.Current.Verbose("Processing Address2.1");

                var defaultAddressType = new DropdownValueViewModel();
                if (addressDropdown != null && addressDropdown.Any())
                {
                    Logger.Current.Verbose("Processing Address2.2");
                    defaultAddressType = addressDropdown.Where(c => c.IsActive == true && c.IsDefault == true).FirstOrDefault();
                    Logger.Current.Verbose("Processing Address2.3");

                    if (defaultAddressType == null)
                    {
                        Logger.Current.Verbose("Processing Address2.4");
                        defaultAddressType = addressDropdown.Where(c => c.IsActive == true).FirstOrDefault();
                    }
                    if (defaultAddressType != null)
                    {
                        Logger.Current.Verbose("Processing Address3");

                        AddressViewModel newAddress = new AddressViewModel();
                        Logger.Current.Verbose("Processing Address4");
                        newAddress.AddressTypeID = defaultAddressType.DropdownValueID;
                        Logger.Current.Verbose("Processing Address5");
                        newAddress.AddressLine1 = addressLine1 != null && !string.IsNullOrEmpty(addressLine1.Value) ? addressLine1.Value : "";
                        Logger.Current.Verbose("Processing Address6");
                        newAddress.AddressLine2 = addressLine2 != null && !string.IsNullOrEmpty(addressLine2.Value) ? addressLine2.Value : "";
                        Logger.Current.Verbose("Processing Address7");
                        newAddress.City = city != null && !string.IsNullOrEmpty(city.Value) ? city.Value : "";
                        Logger.Current.Verbose("Processing Address8");
                        if (state != null)
                        {
                            Logger.Current.Verbose("Processing Address9");
                            newAddress.State = new State()
                            {
                                Code = state.Value
                            };
                            Logger.Current.Verbose("Processing Address10");
                        }
                        else
                        {
                            Logger.Current.Verbose("Processing Address11");
                            newAddress.State = new State();
                        }
                        if (country != null)
                        {
                            Logger.Current.Verbose("Processing Address12");
                            newAddress.Country = new Country()
                            {
                                Code = country.Value
                            };
                            Logger.Current.Verbose("Processing Address13");
                        }
                        else
                        {
                            Logger.Current.Verbose("Processing Address14");
                            newAddress.Country = new Country();
                        }
                        Logger.Current.Verbose("Processing Address15");

                        var zipCode = zip != null && !string.IsNullOrEmpty(zip.Value) ? zip.Value : "";
                        Logger.Current.Verbose("Processing Address16");
                        newAddress.ZipCode = zipCode;
                        Logger.Current.Verbose("Processing Address17");
                        newAddress.IsDefault = true;
                        Logger.Current.Verbose("Processing Address18");

                        Logger.Current.Verbose("Processing Address19");

                        if ((newAddress.State != null && !string.IsNullOrEmpty(newAddress.State.Code)) &&
                            (newAddress.Country == null || string.IsNullOrEmpty(newAddress.Country.Code)))
                        {
                            Logger.Current.Verbose("Processing Address20");
                            newAddress.Country = new Country();
                            Logger.Current.Verbose("Processing Address21");
                            newAddress.Country.Code = newAddress.State.Code.Substring(0, 2);
                            Logger.Current.Verbose("Processing Address22");
                        }
                        personViewModel.Addresses.Add(newAddress);
                    }
                }
            }
            Logger.Current.Verbose("Processed Addresses23");

            personViewModel.ContactType = Entities.ContactType.Person.ToString();
            personViewModel.SecondaryEmails = new List<dynamic>();
            Logger.Current.Verbose("Extracting lifecycle stages");

            personViewModel.LifecycleStages = dropdownValues.Where(s => s.DropdownID == (byte)DropdownFieldTypes.LifeCycle)
             .Select(s => s.DropdownValuesList).ToList().FirstOrDefault().Where(d => d.IsActive == true);

            Logger.Current.Verbose("Lifecycle stages extracted");

            var defaultLifeCycleType = personViewModel.LifecycleStages.SingleOrDefault(a => a.IsDefault);
            if (personViewModel.LifecycleStages != null && personViewModel.LifecycleStages.Any())
                personViewModel.LifecycleStage = defaultLifeCycleType != null ? defaultLifeCycleType.DropdownValueID
                    : personViewModel.LifecycleStages.FirstOrDefault().DropdownValueID;

            Logger.Current.Verbose("Identified default lcstage");

            //personViewModel.LifecycleStage = (int)Entities.LifecycleStage.Lead;
            personViewModel.AccountID = viewModel.AccountId;
            personViewModel.LastUpdatedOn = DateTime.Now.ToUniversalTime();
            personViewModel.LastUpdatedBy = viewModel.CreatedBy;
            personViewModel.CustomFields = new List<ContactCustomFieldMapViewModel>();
            personViewModel.OwnerId = viewModel.OwnerId == 0 ? (int?)null : viewModel.OwnerId;
            GetAllCustomFieldsResponse accountCustomFields = new GetAllCustomFieldsResponse();
            GetAllCustomFieldsRequest request = new GetAllCustomFieldsRequest(viewModel.AccountId);
            Logger.Current.Verbose("Fetching customfields");

            accountCustomFields.CustomFields = customFieldService.GetAllCustomFields(request).CustomFields;
            Logger.Current.Verbose("Processing Customfields");

            foreach (SubmittedFormFieldViewModel submittedField in viewModel.SubmittedFormFields)
            {
                try
                {
                    int result = 0;
                    if (int.TryParse(submittedField.Key, out result))
                    {
                        var isCustomField = accountCustomFields.CustomFields.Where(c => c.FieldId == int.Parse(submittedField.Key)).FirstOrDefault();
                        if (isCustomField != null)
                        {
                            ContactCustomFieldMapViewModel contactCustomField = new ContactCustomFieldMapViewModel();
                            contactCustomField.CustomFieldId = int.Parse(submittedField.Key);
                            contactCustomField.Value = submittedField.Value;
                            contactCustomField.FieldInputTypeId = (int)isCustomField.FieldInputTypeId;
                            personViewModel.CustomFields.Add(contactCustomField);
                        }
                    }
                }
                catch
                {
                    Logger.Current.Informational("While insert: Submitted value of Key: " + submittedField.Key + " cannot be parsed. Value: " + submittedField.Value);
                }
            }
            Logger.Current.Verbose("Returning personviewmodel");

            return personViewModel;
        }

        FullContact GetContactData(string emailId, ContactType contactType)
        {
            var client = new RestClient("https://api.fullcontact.com");
            var apiKey = System.Configuration.ConfigurationManager.AppSettings["FullContactKey"];

            var request = new RestRequest("v2/person.json?", Method.GET);
            request.RequestFormat = DataFormat.Json;
            request.AddParameter("email", emailId, ParameterType.GetOrPost);
            request.AddParameter("apiKey", apiKey, ParameterType.GetOrPost);

            request.AddHeader("Content-Type", "application/json;charset=UTF-8");

            FullContact fullContact = new FullContact();
            try
            {
                fullContact = client.Execute<FullContact>(request).Data;
            }
            catch (Exception ex)
            {
                Logger.Current.Error("An error occured while fetching social profiles from Full Contact Api :" + ex);
            }
            return fullContact;
        }

        PersonViewModel ManagePersonSocialProfiles(PersonViewModel viewModel)
        {
            PersonViewModel updatedModel = viewModel;
            FullContact fullContact = GetContactData(viewModel.Emails.Where(w => w.IsPrimary == true).Select(s => s.EmailId).FirstOrDefault(), ContactType.Person);

            if (fullContact != null && fullContact.socialProfiles != null && fullContact.socialProfiles.Count > 0)
            {
                var facebookUrl = fullContact.socialProfiles.SingleOrDefault(p => p.typeId == "facebook");
                var twitterUrl = fullContact.socialProfiles.SingleOrDefault(p => p.typeId == "twitter");
                var linkedinUrl = fullContact.socialProfiles.SingleOrDefault(p => p.typeId == "linkedin");
                var googleUrl = fullContact.socialProfiles.SingleOrDefault(p => p.typeId == "googleplus");

                if (facebookUrl != null && !string.IsNullOrEmpty(facebookUrl.url))
                    if (updatedModel.SocialMediaUrls.SingleOrDefault(p => p.MediaType == "Facebook") != null)
                        updatedModel.SocialMediaUrls.SingleOrDefault(p => p.MediaType == "Facebook").URL = facebookUrl.url;
                    else
                        updatedModel.SocialMediaUrls.Add(new Url { MediaType = "Facebook", URL = facebookUrl.url });

                if (twitterUrl != null && !string.IsNullOrEmpty(twitterUrl.url))
                    if (updatedModel.SocialMediaUrls.SingleOrDefault(p => p.MediaType == "Twitter") != null)
                        updatedModel.SocialMediaUrls.SingleOrDefault(p => p.MediaType == "Twitter").URL = twitterUrl.url;
                    else
                        updatedModel.SocialMediaUrls.Add(new Url { MediaType = "Twitter", URL = twitterUrl.url });

                if (linkedinUrl != null && !string.IsNullOrEmpty(linkedinUrl.url))
                    if (updatedModel.SocialMediaUrls.SingleOrDefault(p => p.MediaType == "LinkedIn") != null)
                        updatedModel.SocialMediaUrls.SingleOrDefault(p => p.MediaType == "LinkedIn").URL = linkedinUrl.url;
                    else
                        updatedModel.SocialMediaUrls.Add(new Url { MediaType = "LinkedIn", URL = linkedinUrl.url });

                if (googleUrl != null && !string.IsNullOrEmpty(googleUrl.url))
                    if (updatedModel.SocialMediaUrls.SingleOrDefault(p => p.MediaType == "Google+") != null)
                        updatedModel.SocialMediaUrls.SingleOrDefault(p => p.MediaType == "Google+").URL = googleUrl.url;
                    else
                        updatedModel.SocialMediaUrls.Add(new Url { MediaType = "Google+", URL = googleUrl.url });
            }
            if (fullContact != null && fullContact.photos != null && fullContact.photos.Count > 0)
                updatedModel.ContactImageUrl = fullContact.photos.FirstOrDefault().url;
            return updatedModel;
        }

        PersonViewModel updatePersonViewModel(PersonViewModel personViewModel, SubmittedFormViewModel viewModel, IEnumerable<Url> socialUrls)
        {
            Person person = new Person();
            var dropdownValues = cachingService.GetDropdownValues(viewModel.AccountId);

            var firstNameField = viewModel.SubmittedFormFields.SingleOrDefault(f => f.Key == ((byte)Entities.ContactFields.FirstNameField).ToString());
            personViewModel.FirstName = firstNameField != null ? firstNameField.Value : personViewModel.FirstName;
            var lastNameField = viewModel.SubmittedFormFields.SingleOrDefault(f => f.Key == ((byte)Entities.ContactFields.LastNameField).ToString());
            personViewModel.LastName = lastNameField != null ? lastNameField.Value : personViewModel.LastName;
            var companyField = viewModel.SubmittedFormFields.SingleOrDefault(f => f.Key == ((byte)Entities.ContactFields.CompanyNameField).ToString());
            Logger.Current.Informational("personviewmodel.companyname : " + personViewModel.CompanyName);

            //if (companyField != null)
            //    Logger.Current.Informational("Company name is not null" + companyField.Value);
            //else
            //    Logger.Current.Informational("Company name is null");

            personViewModel.CompanyName = companyField != null ? companyField.Value : personViewModel.CompanyName;
            

            //var emailField = viewModel.SubmittedFormFields.SingleOrDefault(f => f.Key == ((byte)Entities.ContactFields.PrimaryEmail).ToString());

            var titleField = viewModel.SubmittedFormFields.SingleOrDefault(f => f.Key == ((byte)Entities.ContactFields.TitleField).ToString());
            personViewModel.Title = titleField != null ? titleField.Value : personViewModel.Title;

            var mobilePhoneField = viewModel.SubmittedFormFields.SingleOrDefault(f => f.Key == ((byte)Entities.ContactFields.MobilePhoneField).ToString());
            var homePhoneField = viewModel.SubmittedFormFields.SingleOrDefault(f => f.Key == ((byte)Entities.ContactFields.HomePhoneField).ToString());
            var workPhoneField = viewModel.SubmittedFormFields.SingleOrDefault(f => f.Key == ((byte)Entities.ContactFields.WorkPhoneField).ToString());

            var phoneTypes = dropdownValues.Where(s => s.DropdownID == (byte)DropdownFieldTypes.PhoneNumberType)
                 .Select(s => s.DropdownValuesList).FirstOrDefault().Where(d => d.IsActive == true);


            personViewModel.Phones = new List<Phone>();

            IEnumerable<Phone> existingPhones = formRepository.GetPhoneFields(personViewModel.ContactID);

            Phone mobilePhone = existingPhones.Where(w => w.DropdownValueTypeID == (short)DropdownValueTypes.MobilePhone).FirstOrDefault();
            if (mobilePhoneField != null && !string.IsNullOrEmpty(mobilePhoneField.Value) && !(mobilePhoneField.Value.Length < 10 || mobilePhoneField.Value.Length > 15))
            {
                string number = mobilePhoneField.Value.TrimStart(new char[] { '0', '1' });
                Phone phone = new Phone();
                phone.Number = number;
                phone.AccountID = viewModel.AccountId;
                phone.IsPrimary = mobilePhone != null ? mobilePhone.IsPrimary : (homePhoneField == null && workPhoneField == null && !existingPhones.IsAny()) ||
                    phoneTypes.Where(c => c.DropdownValueTypeID == (short)DropdownValueTypes.MobilePhone).Select(c => c.IsDefault).FirstOrDefault();
                phone.PhoneType = phoneTypes.Where(c => c.DropdownValueTypeID == (short)DropdownValueTypes.MobilePhone).Select(c => c.DropdownValueID).FirstOrDefault();
                phone.PhoneTypeName = phoneTypes.Where(c => c.DropdownValueTypeID == (short)DropdownValueTypes.MobilePhone).Select(c => c.DropdownValue).FirstOrDefault();
                if (person.IsValidPhoneNumberLength(number))
                    personViewModel.Phones.Add(phone);
            }
            else if (mobilePhone != null)
                personViewModel.Phones.Add(mobilePhone);

            Phone homePhone = existingPhones.Where(w => w.DropdownValueTypeID == (short)DropdownValueTypes.Homephone).FirstOrDefault();
            if (homePhoneField != null && !string.IsNullOrEmpty(homePhoneField.Value) && !(homePhoneField.Value.Length < 10 || homePhoneField.Value.Length > 15))
            {
                string number = homePhoneField.Value.TrimStart(new char[] { '0', '1' });
                Phone phone = new Phone();
                phone.Number = number;
                phone.AccountID = viewModel.AccountId;
                phone.IsPrimary = homePhone != null ? homePhone.IsPrimary : (mobilePhoneField == null && workPhoneField == null && !existingPhones.IsAny()) ||
                    phoneTypes.Where(c => c.DropdownValueTypeID == (short)DropdownValueTypes.Homephone).Select(c => c.IsDefault).FirstOrDefault();
                phone.PhoneType = phoneTypes.Where(c => c.DropdownValueTypeID == (short)DropdownValueTypes.Homephone).Select(c => c.DropdownValueID).FirstOrDefault();
                phone.PhoneTypeName = phoneTypes.Where(c => c.DropdownValueTypeID == (short)DropdownValueTypes.Homephone).Select(c => c.DropdownValue).FirstOrDefault();
                if (person.IsValidPhoneNumberLength(number))
                    personViewModel.Phones.Add(phone);
            }
            else if (homePhone != null)
                personViewModel.Phones.Add(homePhone);

            Phone workPhone = existingPhones.Where(w => w.DropdownValueTypeID == (short)DropdownValueTypes.WorkPhone).FirstOrDefault();
            if (workPhoneField != null && !string.IsNullOrEmpty(workPhoneField.Value) && !(workPhoneField.Value.Length < 10 || workPhoneField.Value.Length > 15))
            {
                string number = workPhoneField.Value.TrimStart(new char[] { '0', '1' });
                Phone phone = new Phone();
                phone.Number = number;
                phone.AccountID = viewModel.AccountId;
                phone.IsPrimary = workPhone != null ? workPhone.IsPrimary : (mobilePhoneField == null && homePhoneField == null && !existingPhones.IsAny()) ||
                    phoneTypes.Where(c => c.DropdownValueTypeID == (short)DropdownValueTypes.WorkPhone).Select(c => c.IsDefault).FirstOrDefault();
                phone.PhoneType = phoneTypes.Where(c => c.DropdownValueTypeID == (short)DropdownValueTypes.WorkPhone).Select(c => c.DropdownValueID).FirstOrDefault();
                phone.PhoneTypeName = phoneTypes.Where(c => c.DropdownValueTypeID == (short)DropdownValueTypes.WorkPhone).Select(c => c.DropdownValue).FirstOrDefault();
                if (person.IsValidPhoneNumberLength(number))
                    personViewModel.Phones.Add(phone);
            }
            else if (workPhone != null)
                personViewModel.Phones.Add(workPhone);

            IEnumerable<Phone> existingNonDefaultPhones = existingPhones.Where(w => w.DropdownValueTypeID != 9 && w.DropdownValueTypeID != 10 && w.DropdownValueTypeID != 11 && !w.IsDeleted);
            if (existingNonDefaultPhones.IsAny())
                existingNonDefaultPhones.Each(e =>
                {
                    personViewModel.Phones.Add(e);
                });

            var facebookUrl = socialUrls.Where(w => w.MediaType == "Facebook");
            var twitterUrl = socialUrls.Where(w => w.MediaType == "Twitter");
            var linkedInUrl = socialUrls.Where(w => w.MediaType == "LinkedIn");
            var googlePlusUrl = socialUrls.Where(w => w.MediaType == "Google+");
            var websiteUrl = viewModel.SubmittedFormFields.SingleOrDefault(f => f.Key == ((byte)Entities.ContactFields.WebsiteUrl).ToString());
            var blogUrl = viewModel.SubmittedFormFields.SingleOrDefault(f => f.Key == ((byte)Entities.ContactFields.BlogUrl).ToString());


            IList<Url> socialMediaUrls = new List<Url>();
            if (facebookUrl != null)
            {
                socialMediaUrls.Add(new Url { MediaType = "Facebook", URL = facebookUrl.Select(s => s.URL).FirstOrDefault() });
            }
            if (twitterUrl != null)
            {
                socialMediaUrls.Add(new Url { MediaType = "Twitter", URL = twitterUrl.Select(s => s.URL).FirstOrDefault() });
            }
            if (linkedInUrl != null)
            {
                socialMediaUrls.Add(new Url { MediaType = "LinkedIn", URL = linkedInUrl.Select(s => s.URL).FirstOrDefault() });
            }
            if (googlePlusUrl != null)
            {
                socialMediaUrls.Add(new Url { MediaType = "Google+", URL = googlePlusUrl.Select(s => s.URL).FirstOrDefault() });
            }
            if (websiteUrl != null)
            {
                socialMediaUrls.Add(new Url { MediaType = "Website", URL = websiteUrl.Value });
            }
            if (blogUrl != null)
            {
                socialMediaUrls.Add(new Url { MediaType = "Blog", URL = blogUrl.Value });
            }


            personViewModel.SocialMediaUrls = socialMediaUrls;
            personViewModel.Addresses = new List<AddressViewModel>();
            var addressLine1 = viewModel.SubmittedFormFields.SingleOrDefault(f => f.Key == ((byte)Entities.ContactFields.AddressLine1Field).ToString());
            var addressLine2 = viewModel.SubmittedFormFields.SingleOrDefault(f => f.Key == ((byte)Entities.ContactFields.AddressLine2Field).ToString());
            var city = viewModel.SubmittedFormFields.SingleOrDefault(f => f.Key == ((byte)Entities.ContactFields.CityField).ToString());
            var state = viewModel.SubmittedFormFields.SingleOrDefault(f => f.Key == ((byte)Entities.ContactFields.StateField).ToString());
            var zip = viewModel.SubmittedFormFields.SingleOrDefault(f => f.Key == ((byte)Entities.ContactFields.ZipCodeField).ToString());
            var country = viewModel.SubmittedFormFields.SingleOrDefault(f => f.Key == ((byte)Entities.ContactFields.CountryField).ToString());

            if (addressLine1 != null || addressLine2 != null || city != null || zip != null || country != null || state != null)
            {
                personViewModel.AddressTypes = dropdownValues.Where(s => s.DropdownID == (byte)DropdownFieldTypes.AddressType)
                                                .Select(s => s.DropdownValuesList).ToList().FirstOrDefault().Where(d => d.IsActive == true);

                AddressViewModel newAddress = new AddressViewModel();
                newAddress.AddressTypeID = personViewModel.AddressTypes.SingleOrDefault(a => a.IsDefault).DropdownValueID;
                newAddress.AddressLine1 = addressLine1 != null && !string.IsNullOrEmpty(addressLine1.Value) ? addressLine1.Value : "";
                newAddress.AddressLine2 = addressLine2 != null && !string.IsNullOrEmpty(addressLine2.Value) ? addressLine2.Value : "";
                newAddress.City = city != null && !string.IsNullOrEmpty(city.Value) ? city.Value : "";
                if (state != null)
                    newAddress.State = new State() { Code = state.Value };
                else
                    newAddress.State = new State();
                if (country != null)
                    newAddress.Country = new Country() { Code = country.Value };
                else
                    newAddress.Country = new Country();

                var zipCode = zip != null && !string.IsNullOrEmpty(zip.Value) ? zip.Value : "";
                newAddress.ZipCode = zipCode;
                newAddress.IsDefault = true;

                if ((newAddress.State != null && !string.IsNullOrEmpty(newAddress.State.Code)) &&
                    (newAddress.Country == null || string.IsNullOrEmpty(newAddress.Country.Code)))
                {
                    newAddress.Country = new Country();
                    newAddress.Country.Code = newAddress.State.Code.Substring(0, 2);
                }
                personViewModel.Addresses.Add(newAddress);
            }

            personViewModel.ContactType = Entities.ContactType.Person.ToString();
            personViewModel.SecondaryEmails = new List<dynamic>();

            personViewModel.LifecycleStages = dropdownValues.Where(s => s.DropdownID == (byte)DropdownFieldTypes.LifeCycle)
             .Select(s => s.DropdownValuesList).ToList().FirstOrDefault().Where(d => d.IsActive == true);
            var defaultLifeCycleType = personViewModel.LifecycleStages.SingleOrDefault(a => a.IsDefault);
            personViewModel.LifecycleStage = defaultLifeCycleType.DropdownValueID;

            personViewModel.OwnerId = viewModel.OwnerId;
            personViewModel.AccountID = viewModel.AccountId;
            personViewModel.LastUpdatedOn = DateTime.Now.ToUniversalTime();
            personViewModel.LastUpdatedBy = viewModel.CreatedBy;
            GetAllCustomFieldsResponse accountCustomFields = new GetAllCustomFieldsResponse();
            GetAllCustomFieldsRequest request = new GetAllCustomFieldsRequest(viewModel.AccountId);
            accountCustomFields.CustomFields = customFieldService.GetAllCustomFields(request).CustomFields;
            foreach (SubmittedFormFieldViewModel submittedField in viewModel.SubmittedFormFields)
            {
                try
                {
                    var isCustomField = accountCustomFields.CustomFields.Where(c => c.FieldId == int.Parse(submittedField.Key)).FirstOrDefault();
                    if (isCustomField != null)
                    {
                        ContactCustomFieldMapViewModel contactCustomField = new ContactCustomFieldMapViewModel();
                        contactCustomField.CustomFieldId = int.Parse(submittedField.Key);
                        contactCustomField.Value = submittedField.Value;
                        contactCustomField.FieldInputTypeId = (int)isCustomField.FieldInputTypeId;
                        contactCustomField.ContactId = personViewModel.ContactID;
                        var existingCustomField = personViewModel.CustomFields.Where(c => c.CustomFieldId == isCustomField.FieldId).FirstOrDefault();
                        if (existingCustomField == null)
                            personViewModel.CustomFields.Add(contactCustomField);
                        else
                            existingCustomField.Value = submittedField.Value;
                    }
                }
                catch
                {
                    Logger.Current.Informational("While Update: Submitted value of Key: " + submittedField.Key + " cannot be parsed. Value: " + submittedField.Value);
                }
            }
            return personViewModel;
        }
        FormSubmissionEntryViewModel generateFormSubmissionEntry(PersonViewModel personViewModel, int formId)
        {
            FormSubmissionEntryViewModel formSubmissionEntry = new FormSubmissionEntryViewModel();
            formSubmissionEntry.Form.FormId = formId;
            formSubmissionEntry.Person = personViewModel;
            formSubmissionEntry.Status = Entities.FormSubmissionStatus.New;
            formSubmissionEntry.SubmittedOn = new DateTime();
            return formSubmissionEntry;
        }

        SearchFormsResponse search(SearchFormsRequest request, IEnumerable<Type> types, IList<string> fields, bool matchAll, bool autoComplete)
        {
            SearchFormsResponse response = new SearchFormsResponse();

            SearchParameters parameters = new SearchParameters();
            parameters.Limit = request.Limit;
            parameters.PageNumber = request.PageNumber == 0 ? 1 : request.PageNumber;
            parameters.Types = types;
            parameters.MatchAll = matchAll;
            parameters.AccountId = request.AccountId;
            parameters.StartDate = request.StartDate;
            parameters.EndDate = request.EndDate;

            if (request.SortField != null)
            {
                List<string> sortFields = new List<string>();
                var maps = SmartTouch.CRM.ApplicationServices.ObjectMappers.MapperConfigurationProvider.Instance.FindTypeMapFor<FormViewModel, Form>();

                foreach (var propertyMap in maps.GetPropertyMaps())
                {
                    if (propertyMap.SourceMember != null && request.SortField.Equals(propertyMap.SourceMember.Name))
                    {
                        sortFields.Add(propertyMap.DestinationProperty.MemberInfo.Name);
                        break;
                    }
                }
                parameters.SortDirection = request.SortDirection;
                parameters.SortFields = sortFields;
            }
            Logger.Current.Informational("Search string:" + request.Query);
            Logger.Current.Informational("Parameters:" + parameters.ToString());
            SearchResult<Form> searchResult;
            bool isAccountAdmin = cachingService.IsAccountAdmin(request.RoleId, request.AccountId);
            bool isPrivate = cachingService.IsModulePrivate(AppModules.Forms, request.AccountId);
            if (isPrivate && !isAccountAdmin)
            {
                int userId = (int)request.RequestedBy;
                searchResult = searchService.Search(request.Query, c => request.UserIds.Contains(c.CreatedBy), parameters);
            }
            else if (request.UserIds.IsAny() && request.StartDate != null && request.EndDate != null)
            {
                int userId = (int)request.UserID;
                searchResult = searchService.Search(request.Query, c => request.UserIds.Contains(c.CreatedBy), parameters);
            }
            else
            {
                searchResult = searchService.Search(request.Query, parameters);
            }
            IEnumerable<Form> forms = searchResult.Results;

            Logger.Current.Informational("Search complete, total results:" + searchResult.Results.Count());

            if (forms == null)
                response.Exception = GetFormNotFoundException();
            else
            {
                IEnumerable<FormViewModel> list = Mapper.Map<IEnumerable<Form>, IEnumerable<FormViewModel>>(forms);

                response.Forms = list;
                response.TotalHits = searchResult.TotalHits;
            }

            return response;
        }

        public ReIndexDocumentResponse ReIndexForms(ReIndexDocumentRequest request)
        {
            Logger.Current.Verbose("Request for ReIndexing forms.");

            var forms = formRepository.FindAll().ToList();
            forms.ForEach(f => { f.HTMLContent = null; });
            int count = indexingService.ReIndexAll<Form>(forms);
            return new ReIndexDocumentResponse() { Documents = count };
        }

        public ReIndexDocumentResponse ReIndexFormSubmissions(ReIndexDocumentRequest request)
        {
            Logger.Current.Verbose("Request for ReIndexing form submissions.");

            var formSubmissions = formSubmissionRepository.FindAll();
            int count = indexingService.ReIndexAll<FormSubmission>(formSubmissions);
            return new ReIndexDocumentResponse() { Documents = count };
        }


        public GetFormContactsResponse GetFormViewSubmissions(GetFormContactsRequest request)
        {
            Logger.Current.Verbose("Request for fetching all the contacts that are mapped to the form");
            GetFormContactsResponse response = new GetFormContactsResponse();

            IEnumerable<int> contactsIdList = formRepository.GetContactsByFormID(request.FormID);
            if (contactsIdList != null)
            {
                response.ContactIdList = contactsIdList.Distinct();
                return response;
            }
            return null;

        }


        public GetFormSubmissionResponse GetFormSubmission(GetFormSubmissionRequest request)
        {

            GetFormSubmissionResponse response = new GetFormSubmissionResponse();
            FormSubmission submission = formRepository.GetFormSubmissionByID(request.FormSubmissionID);
            response.FormSubmission = Mapper.Map<FormSubmission, FormSubmissionEntryViewModel>(submission);
            return response;

        }


        public GetAllFieldsResponse GetAllFields(GetAllFieldsRequest request)
        {
            GetAllFieldsResponse response = new GetAllFieldsResponse();
            response.Fields = Mapper.Map<IEnumerable<Field>, IEnumerable<FieldViewModel>>(formRepository.GetAllFields(request.AccountId));
            return response;
        }

        public FormData GetFormData(int formSubmissionId)
        {
            var formSubmission = formSubmissionRepository.GetFormSubmission(formSubmissionId);
            return formSubmission;
        }

        public FormIndexingResponce FormIndexing(FormIndexingRequest request)
        {
            FormIndexingResponce responce = new FormIndexingResponce();
            foreach (var id in request.FormIds)
            {
                Form form = formRepository.GetFormById(id);
                form.HTMLContent = null;
                if (form.IsDeleted == false)
                    indexingService.Index<Form>(form);
                Logger.Current.Informational("Indexing Form : FormId - " + form.Id);
            }

            return responce;
        }

        public GetFiledDataByIdResponce GetFiledDataById(GetFiledDataByIdRequest request)
        {
            return new GetFiledDataByIdResponce { Field = formRepository.GetFiledDataById(request.FieldId) };
        }

        /// <summary>
        /// Getting Form Name
        /// </summary>
        /// <param name="formSubmissionId"></param>
        /// <returns></returns>
        public string GetFormName(int formSubmissionId)
        {
            string formName = formSubmissionRepository.GetFormName(formSubmissionId);
            return formName;
        }

        public GetFormFieldIDsResponse GetFormFieldIDs(GetFormFieldIDsRequest request)
        {
            Logger.Current.Verbose("In FormService/GetFormFieldIDs");
            return new GetFormFieldIDsResponse { FieldIDs = formRepository.GetFormFieldIDs(request.FormID) };
        }

        public GetFormSubmissionsResponse GetFormSubmissions(GetFormSubmissionsRequest request)
        {
            Logger.Current.Informational("Request received to fetch Form submissions with Id: " + request.FormId);
            GetFormSubmissionsResponse response = new GetFormSubmissionsResponse();
            var submissions = formRepository.GetFormSubmissions(request.FormId, request.StartDate, request.EndDate, request.PageLimit, request.PageNumber);
            IList<FormSubmissionEntryViewModel> formSubmissions = new List<FormSubmissionEntryViewModel>();
            foreach (FormSubmission fs in submissions)
            {
                formSubmissions.Add(new FormSubmissionEntryViewModel()
                {
                    FormSubmissionId = fs.Id,
                    IPAddress = fs.IPAddress,
                    Status = fs.StatusID,
                    SubmittedData = fs.SubmittedData,
                    SubmittedOn = fs.SubmittedOn,
                });
            }
            response.FormSubmissions = formSubmissions;
            response.TotalHits = submissions.IsAny() ? submissions.FirstOrDefault().TotalCount : 0;
            Logger.Current.Informational("Fetch Submissions count: " + response.FormSubmissions.Count());
            return response;
        }

        public GetFormNameByIdResponse GetFormNameById(GetFormNameByIdRequest request)
        {
            Logger.Current.Informational("Request received to fetch Form name with Id: " + request.FormId);
            GetFormNameByIdResponse response = new GetFormNameByIdResponse();
            response.FormName = formRepository.GetFormNameById(request.FormId);
            Logger.Current.Informational("Form name identified as: " + response.FormName);
            return response;
        }

        public SubmitFormResponse GetFormAcknowdegement(int formId)
        {
            SubmitFormResponse response = new SubmitFormResponse();
            response.Acknowledgement = formRepository.GetFormAcknowledgement(formId);
            Logger.Current.Informational("Type: " + response.Acknowledgement.AcknowledgementType + " . Value: " + response.Acknowledgement.Acknowledgement);
            return response;
        }

        public CreateAPIFormsResponse CreateAPIForm(CreateAPIFormsRequest request)
        {
            CreateAPIFormsResponse response = new CreateAPIFormsResponse();
            if (request.AccountId != 0 && request.RequestedBy.HasValue && request.FormViewModel != null)
            {
                InsertFormResponse formResponse = this.InsertForm(new InsertFormRequest() { AccountId = request.AccountId, FormViewModel = request.FormViewModel, RequestedBy = request.RequestedBy.Value });
                response.ViewModel = new FormEntryViewModel() { Id = formResponse.FormViewModel.FormId, Name = formResponse.FormViewModel.Name };
            }
            return response;
        }

        public UpdateFormNameResponse UpdateFormName(UpdateFormNameRequest request)
        {
            UpdateFormNameResponse response = new UpdateFormNameResponse();
            if (request.FormId != 0)
            {
                if (!string.IsNullOrEmpty(request.FormName))
                {
                    if (request.FormName.Length > 75)
                        throw new UnsupportedOperationException("[|Form Name Is Maximum 75 characters.|]");
                    Form form = new Form() { Id = request.FormId, Name = request.FormName, AccountID = request.AccountId };
                    bool isFormNameUnique = formRepository.IsFormNameUnique(form);
                    if (!isFormNameUnique)
                    {
                        Logger.Current.Verbose("Duplicate form identified");
                        var message = "[|Form with name|] \"" + form.Name + "\" [|already exists. Please choose a different name.|]";
                        throw new UnsupportedOperationException(message);
                    }

                    bool success = formRepository.UpdateFormName(request.FormId, request.FormName, request.RequestedBy.Value);
                    if (!success)
                        throw new UnsupportedOperationException("[|Unable to update Form Name|]");
                    var updatedForm = formRepository.GetFormById(request.FormId);
                    updatedForm.Tags = null;
                    updatedForm.HTMLContent = null;
                    if (indexingService.Update<Form>(updatedForm) > 0)
                        Logger.Current.Verbose("Form updated to elasticsearch successfully");

                    Logger.Current.Informational("Form updated successfully. Id:" + updatedForm.Id);
                }
                else
                    throw new UnsupportedOperationException("[|Please provide Form Name for updating.|]");
            }
            else
                throw new UnsupportedOperationException("[|Please provide Form ID for updating|]");

            return response;
        }

        /// <summary>
        /// Process Form Lead into CRM
        /// NEXG-3014
        /// </summary>
        public void ProcessTheFormSubmittedContacts(int formSubmissionID)
        {
            string spamRemarks = string.Empty;
            GetFormSubmissionDataResponse responce = new GetFormSubmissionDataResponse();
            responce = GetFormSubmittedData(formSubmissionID);

            if (responce != null && responce.SubmittedFormViewModel != null)
            {
                Logger.Current.Verbose("Processing SubmittedFormDataID: " + responce.SubmittedFormViewModel.SubmittedFormDataID);
                IDictionary<string, string> fields = responce.SubmittedFormViewModel.SubmittedFormFields.ToDictionary(x => x.Key, x => x.Value);
                bool IsSpam = findSpamService.SpamCheck(fields, responce.SubmittedFormViewModel.AccountId, responce.SubmittedFormViewModel.IPAddress, responce.SubmittedFormViewModel.FormId, true, out spamRemarks);
                if (IsSpam)
                {
                    Logger.Current.Informational("Spam Submission. SubmittedFormDataID:" + responce.SubmittedFormViewModel.SubmittedFormDataID);
                    formRepository.UpdateFormSubmissionStatus(responce.SubmittedFormViewModel.SubmittedFormDataID, SubmittedFormStatus.Spam, spamRemarks, null);
                }
                else
                {
                    Logger.Current.Informational("Not a spam Submission. SubmittedFormDataID:" + responce.SubmittedFormViewModel.SubmittedFormDataID);
                    InsertFormSubmittedData(responce.SubmittedFormViewModel);

                }
                Logger.Current.Informational("Completed processing submission, " + responce.SubmittedFormViewModel.SubmittedFormDataID);

            }
        }
    }
}

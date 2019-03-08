using AutoMapper;
using LandmarkIT.Enterprise.Utilities.ExceptionHandling;
using LandmarkIT.Enterprise.Utilities.Logging;
using SmartTouch.CRM.ApplicationServices.Messaging.CustomFields;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.CustomFields;
using SmartTouch.CRM.Domain.Fields;
using SmartTouch.CRM.Domain.Tags;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Infrastructure.Domain;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SmartTouch.CRM.ApplicationServices.ServiceImplementations
{
    public class CustomFieldService : ICustomFieldService
    {
        readonly ICustomFieldRepository customFieldRepository;
        readonly IUnitOfWork unitOfWork;
        readonly ITagRepository tagRepository;

        public CustomFieldService(ICustomFieldRepository customFieldRepository, IUnitOfWork unitOfWork, ITagRepository tagRepository)
        {
            this.customFieldRepository = customFieldRepository;
            this.unitOfWork = unitOfWork;
            this.tagRepository = tagRepository;
        }

        public GetCustomFieldTabResponse GetCustomField(GetCustomFieldTabRequest request)
        {
            return new GetCustomFieldTabResponse();
        }

        public InsertCustomFieldResponse InsertCustomField(InsertCustomFieldRequest request)
        {
            return new InsertCustomFieldResponse();
        }

        public UpdateCustomFieldResponse UpdateCustomField(UpdateCustomFieldRequest request)
        {
            return new UpdateCustomFieldResponse();
        }

        public DeleteCustomFieldSectionResponse DeleteCustomField(DeleteCustomFieldRequest request)
        {
            return new DeleteCustomFieldSectionResponse();
        }

        public GetAllCustomFieldsResponse GetAllCustomFields(GetAllCustomFieldsRequest request)
        {
            Logger.Current.Verbose("Request received to fetch all custom fields for account : " + request.AccountId);
            GetAllCustomFieldsResponse response = new GetAllCustomFieldsResponse();
            Logger.Current.Verbose("Request received to get CustomFieldTabs.");
            IEnumerable<Field> customFields = customFieldRepository.GetAllCustomFieldsByAccountId(request.AccountId);
            IEnumerable<FieldViewModel> customfields = Mapper.Map<IEnumerable<Field>, IEnumerable<FieldViewModel>>(customFields);
            response.CustomFields = new List<FieldViewModel>(customfields);
            return response;
        }

        public GetAllCustomFieldsResponse GetAllCustomFieldsForForms(GetAllCustomFieldsRequest request)
        {
            Logger.Current.Verbose("Request received to fetch all custom fields for forms for account : " + request.AccountId);
            GetAllCustomFieldsResponse response = new GetAllCustomFieldsResponse();
            Logger.Current.Verbose("Request received to get CustomFields for forms.");
             IEnumerable<Field> customFields = customFieldRepository.GetAllCustomFieldsForForms(request.AccountId);
            IEnumerable<FieldViewModel> customfields = Mapper.Map<IEnumerable<Field>, IEnumerable<FieldViewModel>>(customFields);
            response.CustomFields = new List<FieldViewModel>(customfields);
            return response;
        }

        public GetAllCustomFieldTabsResponse GetCustomField(GetAllCustomFieldTabsRequest request)
        {
            return new GetAllCustomFieldTabsResponse();
        }

        public GetAllCustomFieldTabsResponse GetAllCustomFieldTabs(GetAllCustomFieldTabsRequest request)
        {
            Logger.Current.Verbose("Request received to fetch all custom field tabs for account : " + request.AccountId);
            GetAllCustomFieldTabsResponse response = new GetAllCustomFieldTabsResponse();
            Logger.Current.Verbose("Request received to get CustomFieldTabs.");
             IEnumerable<CustomFieldTab> customFieldTabs = customFieldRepository.GetAllCustomFieldTabs(request.AccountId);
            IEnumerable<CustomFieldTabViewModel> customfieldTabs = Mapper.Map<IEnumerable<CustomFieldTab>, IEnumerable<CustomFieldTabViewModel>>(customFieldTabs);
            response.CustomFieldsViewModel = new CustomFieldTabsViewModel();
            response.CustomFieldsViewModel.CustomFieldTabs = new List<CustomFieldTabViewModel>(customfieldTabs);
            return response;
        }

        public InsertCustomFieldTabResponse InsertCustomFieldTab(InsertCustomFieldTabRequest request)
        {
            Logger.Current.Verbose("Request received to insert a new CustomFieldTab with name : " + request.CustomFieldTabViewModel.Name);
            CustomFieldTabViewModel newCustomFieldTab = insertCustomFieldTab(request.CustomFieldTabViewModel);
            return new InsertCustomFieldTabResponse() { CustomFieldTabViewModel = newCustomFieldTab };
        }

        CustomFieldTabViewModel insertCustomFieldTab(CustomFieldTabViewModel viewModel)
        {
            CustomFieldTab customFieldTab = Mapper.Map<CustomFieldTabViewModel, CustomFieldTab>(viewModel);
            bool isCustomFieldTabNameUnique = customFieldRepository.IsCustomFieldTabNameUnique(customFieldTab);
            if (!isCustomFieldTabNameUnique)
            {
                var message = "[|Custom Field Tab with name|] \"" + customFieldTab.Name + "\" [|already exists. Please choose a different name.|]";
                throw new UnsupportedOperationException(message);
            }
            isCustomFieldTabValid(customFieldTab);
            customFieldRepository.Insert(customFieldTab);
            CustomFieldTab newCustomFieldTab = unitOfWork.Commit() as CustomFieldTab;
            Logger.Current.Verbose("[|Custom Field Tab inserted successfully.|]");
            return Mapper.Map<CustomFieldTab, CustomFieldTabViewModel>(newCustomFieldTab);
        }

        public SaveAllCustomFieldTabsResponse SaveAllCustomFieldTabs(SaveAllCustomFieldTabsRequest request)
        {
            Logger.Current.Verbose("Request received to insert a new CustomFieldTab with name : " + request.AccountId);
            IEnumerable<CustomFieldTab> cusmFieldTabs
                = Mapper.Map<IEnumerable<CustomFieldTabViewModel>, IEnumerable<CustomFieldTab>>(request.CustomFieldsViewModel.CustomFieldTabs);
            var customFields = cusmFieldTabs.SelectMany(i => i.Sections).SelectMany(g => g.CustomFields).Where(i=>!i.IsLeadAdapterField).Select(i => i.Title).ToList();
            var duplicates = customFields.GroupBy(s => s).SelectMany(grp => grp.Skip(1));
            var duplicatefields = string.Join("',' ", duplicates);
            if (duplicates.Any())
            {
                var message = "Fields with name(s) '" + duplicatefields + "' already exist. Please choose different name(s).";
                throw new UnsupportedOperationException(message);
            }
            foreach (CustomFieldTab customFieldTab in cusmFieldTabs)
            {
                bool isCustomFieldTabNameUnique = customFieldRepository.IsCustomFieldTabNameUnique(customFieldTab);
                isCustomFieldTabValid(customFieldTab);
                if (!isCustomFieldTabNameUnique)
                {
                    var message = "[|Custom Field Tab with name|] \"" + customFieldTab.Name + "\" [|already exists. Please choose a different name.|]";
                    throw new UnsupportedOperationException(message);
                }
                if (customFieldTab.Id > 0)
                    customFieldRepository.Update(customFieldTab);
                else
                    customFieldRepository.Insert(customFieldTab);
                unitOfWork.Commit();
            }
            return new SaveAllCustomFieldTabsResponse();
        }

        public UpdateCustomFieldTabResponse UpdateCustomFieldTab(UpdateCustomFieldTabRequest request)
        {
            Logger.Current.Verbose("Request received to update the CustomFieldTab with id= " + request.CustomFieldTabViewModel.CustomFieldTabId);
            updateCustomFieldTab(request.CustomFieldTabViewModel);
            return new UpdateCustomFieldTabResponse();
        }

        void updateCustomFieldTab(CustomFieldTabViewModel customFieldTabViewModel)
        {
            CustomFieldTab customFieldTab = Mapper.Map<CustomFieldTabViewModel, CustomFieldTab>(customFieldTabViewModel);
            bool isCustomFieldTabNameUnique = customFieldRepository.IsCustomFieldTabNameUnique(customFieldTab);
            if (!isCustomFieldTabNameUnique)
            {
                var message = "[|CustomFieldTab with name|] \"" + customFieldTab.Name + "\" [|already exists.|]";
                throw new UnsupportedOperationException(message);
            }
            isCustomFieldTabValid(customFieldTab);
            customFieldRepository.Update(customFieldTab);
            unitOfWork.Commit();
            Logger.Current.Informational("CustomFieldTab updated successfully.");
        }

        public DeleteCustomFieldTabResponse DeleteCustomFieldTab(DeleteCustomFieldTabRequest request)
        {
            Logger.Current.Verbose("Request received to delete the Custom Field Tab with id= " + request.Id);
            customFieldRepository.DeactivateCustomFieldTab(request.Id);
            return new DeleteCustomFieldTabResponse();
        }

        void isCustomFieldTabValid(CustomFieldTab customFieldTab)
        {
            Logger.Current.Verbose("Request received to validate CustomFieldTab with id " + customFieldTab.Id);
            IEnumerable<BusinessRule> brokenRules = customFieldTab.GetBrokenRules();
            if (brokenRules != null)
                brokenRules = brokenRules.Distinct();
            if (brokenRules.Any())
            {
                StringBuilder brokenRulesBuilder = new StringBuilder();
                foreach (BusinessRule rule in brokenRules)
                {
                    brokenRulesBuilder.AppendLine(rule.RuleDescription);
                }

                throw new UnsupportedOperationException(brokenRulesBuilder.ToString());
            }
        }

        public GetContactCustomFieldsResponse GetContactCustomFields(GetContactCustomFieldsRequest request)
        {
            Logger.Current.Verbose("Request received to get custom fields for contact with id: " + request.Id);
            GetContactCustomFieldsResponse response = new GetContactCustomFieldsResponse();
            Logger.Current.Verbose("Request received to get contact custom fields");
            IEnumerable<ContactCustomField> contactCustomFields = customFieldRepository.ContactCustomFields(request.Id);
            response.ContactCustomFields = Mapper.Map<IEnumerable<ContactCustomField>, IEnumerable<ContactCustomFieldMapViewModel>>(contactCustomFields);
            return response;
        }

        public GetCustomFieldsValueOptionsResponse GetCustomFieldValueOptions(GetCustomFieldsValueOptionsRequest request)
        {
            Logger.Current.Verbose("Request received to get custom field value options for account with id: " + request.AccountId);
            GetCustomFieldsValueOptionsResponse response = new GetCustomFieldsValueOptionsResponse();
            Logger.Current.Verbose("Request received to get custom field value options");
            IEnumerable<FieldValueOption> customFieldsValueOptions = customFieldRepository.GetCustomFieldsValueOptions(request.AccountId);
            response.CustomFieldValueOptions =
                Mapper.Map<IEnumerable<FieldValueOption>, IEnumerable<CustomFieldValueOptionViewModel>>(customFieldsValueOptions); ;
            return response;
        }


        public GetLeadAdapterCustomFieldResponse GetLeadAdapterCustomFieldsByType(GetLeadAdapterCustomFieldRequest request)
        {
            GetLeadAdapterCustomFieldResponse response = new GetLeadAdapterCustomFieldResponse();
            IEnumerable<CustomField> customFields = customFieldRepository.GetLeadAdapterCustomFields(request.LeadAdapterType, request.AccountId);
            var customfieldviewmodel = customFields.Select(x => new CustomFieldViewModel()
            {
                AccountID = request.AccountId,
                DisplayName = x.Title,
                FieldInputTypeId = x.FieldInputTypeId,
                IsCustomField = true,
                IsLeadAdapterField = true,
                IsDropdownField = false,
                LeadAdapterType = (byte)request.LeadAdapterType,
                Title = x.Title,
                StatusId = Entities.FieldStatus.Active,
                SortId = x.SortId
            });
            response.CustomFields = customfieldviewmodel;
            return response;
        }

        public GetSavedSearchsCountForCustomFieldResponse GetSavedSearchsCountForCustomFieldById(GetSavedSearchsCountForCustomFieldRequest request)
        {
            GetSavedSearchsCountForCustomFieldResponse response = new GetSavedSearchsCountForCustomFieldResponse();
            var count = customFieldRepository.GetSavedSearchsCountForCustomFieldById(request.fieldId,request.valueOptionId);
            response.Count = count;
            return response;

        }

    }
}

using AutoMapper;
using LandmarkIT.Enterprise.Extensions;
using LandmarkIT.Enterprise.Utilities.ExceptionHandling;
using SmartTouch.CRM.ApplicationServices.Messaging.DropdownValues;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.Dropdowns;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SmartTouch.CRM.ApplicationServices.ServiceImplementations
{
    public class DropdownValuesService : IDropdownValuesService
    {
        readonly IDropdownRepository dropdownRepository;

        readonly IUnitOfWork unitOfWork;

        public DropdownValuesService(IDropdownRepository dropdownsRepository,
             IUnitOfWork unitOfWork)
        {
            if (dropdownsRepository == null) throw new ArgumentNullException("dropdownFieldsRepository");
            if (unitOfWork == null) throw new ArgumentNullException("unitOfWork");
            this.dropdownRepository = dropdownsRepository;
            this.unitOfWork = unitOfWork;
        }

        public GetDropdownListResponse GetAll(GetDropdownListRequest request)
        {
            GetDropdownListResponse response = new GetDropdownListResponse();
            IEnumerable<Dropdown> dropdownValues = dropdownRepository.FindAll(request.Query, request.Limit, request.PageNumber, request.AccountID);
            if (dropdownValues == null)
                response.Exception = GetDropdownvaluesNotFoundException();
            else
            {
                IEnumerable<DropdownViewModel> list = Mapper.Map<IEnumerable<Dropdown>, IEnumerable<DropdownViewModel>>(dropdownValues);
                response.DropdownValuesViewModel = list;
                response.TotalHits = dropdownRepository.FindAll(request.Query, request.AccountID).Count();
            }

            return response;
        }

        public GetDropdownListResponse GetAllByAccountID(string name, int? accountID)
        {
            GetDropdownListResponse response = new GetDropdownListResponse();
            IEnumerable<Dropdown> dropdownValues = dropdownRepository.FindAll(name, accountID);
            if (dropdownValues == null)
                response.Exception = GetDropdownvaluesNotFoundException();
            else
            {
                IEnumerable<DropdownViewModel> list = Mapper.Map<IEnumerable<Dropdown>, IEnumerable<DropdownViewModel>>(dropdownValues);
                response.DropdownValuesViewModel = list;
                response.TotalHits = dropdownValues.IsAny() ? dropdownValues.Select(s => s.TotalDropdownCount).FirstOrDefault() : 0;
            }
            return response;
        }
        private UnsupportedOperationException GetDropdownvaluesNotFoundException()
        {
            return new UnsupportedOperationException("[|The requested DropdownValues was not found.|]");
        }

        public GetDropdownValueResponse GetDropdownValue(GetDropdownValueRequest request)
        {
            GetDropdownValueResponse response = new GetDropdownValueResponse();
            Dropdown dropdown = dropdownRepository.FindBy(request.DropdownID, request.AccountId);
            
            if (dropdown == null)
                response.Exception = GetDropdownvaluesNotFoundException();
            else
            {
                DropdownViewModel dropdownViewModel = Mapper.Map<Dropdown, DropdownViewModel>(dropdown);
                response.DropdownValues = dropdownViewModel;
            }
            return response;
        }


        public GetLeadSourcesResponse GetLeadSources(GetLeadSourcesRequest request)
        {
            GetLeadSourcesResponse response = new GetLeadSourcesResponse();
            IEnumerable<DropdownValue> leadSources = dropdownRepository.GetLeadSources(request.DropdownId, request.AccountId).ToList();
            if (leadSources == null)
                throw new UnsupportedOperationException("[|The requested lead sources list was not found.|]");
            else
            {
                IEnumerable<DropdownValueViewModel> list = Mapper.Map<IEnumerable<DropdownValue>, IEnumerable<DropdownValueViewModel>>(leadSources);
                response.LeadSources = list;
            }
            return response;
        }

        public InsertDropdownResponse InsertDropdownValue(InsertDropdownRequest request)
        {
            InsertDropdownResponse response = new InsertDropdownResponse();

            Dropdown dropdown = Mapper.Map<DropdownViewModel, Dropdown>(request.DropdownViewModel);
            dropdownRepository.Update(dropdown);
            Dropdown newDropdown = unitOfWork.Commit() as Dropdown;
            response.DropdownViewModel = Mapper.Map<Dropdown, DropdownViewModel>(newDropdown);
            return response;
        }

        public GetOpportunityStageGroupResponse GetOppoertunityStageGroups(GetOpportunityStageGroupRequest request)
        {
            GetOpportunityStageGroupResponse response = new GetOpportunityStageGroupResponse();
            IEnumerable<dynamic> opportunityGroup = dropdownRepository.GetOppoertunityStageGroups();
            response.OpportunityGroups = opportunityGroup;

            return response;
        }

        public GetDropdownValueResponse GetDropdownValue(short dropDownValueId)
        {
            GetDropdownValueResponse response = new GetDropdownValueResponse();
            DropdownValue dropdown = dropdownRepository.GetDropdownValue(dropDownValueId);

            if (dropdown == null)
            {
                response.Exception = GetDropdownvaluesNotFoundException();
            }
            else
            {
                DropdownValueViewModel dropdownViewModel = Mapper.Map<DropdownValue, DropdownValueViewModel>(dropdown);
                response.DropdownValue = dropdownViewModel;
            }
            return response;
        }

        public GetTemplateDataResponse GetTemplateData(GetTemplateDataRequest request)
        {
            GetTemplateDataResponse response = new GetTemplateDataResponse();
            string savedFileName = Path.Combine(System.Configuration.ConfigurationManager.AppSettings["API_TEMPLATES_PHYSICAL_PATH"].ToString(), request.FileName);
            using (StreamReader reader = new StreamReader(savedFileName))
            {
                do
                {
                    response.FileCode = reader.ReadToEnd();
                } while (!reader.EndOfStream);
            }
            return response;
        }

    }
}

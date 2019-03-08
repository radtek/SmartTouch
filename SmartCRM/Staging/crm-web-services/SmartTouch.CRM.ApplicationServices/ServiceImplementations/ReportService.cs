using AutoMapper;
using LandmarkIT.Enterprise.Extensions;
using LandmarkIT.Enterprise.Utilities.Excel;
using LandmarkIT.Enterprise.Utilities.ExceptionHandling;
using LandmarkIT.Enterprise.Utilities.Logging;
using SmartTouch.CRM.ApplicationServices.Exceptions;
using SmartTouch.CRM.ApplicationServices.Messaging.Campaigns;
using SmartTouch.CRM.ApplicationServices.Messaging.Contacts;
using SmartTouch.CRM.ApplicationServices.Messaging.Reports;
using SmartTouch.CRM.ApplicationServices.Messaging.User;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.Campaigns;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.Dropdowns;
using SmartTouch.CRM.Domain.Opportunities;
using SmartTouch.CRM.Domain.Reports;
using SmartTouch.CRM.Domain.Search;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.SearchEngine.Search;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ServiceImplementations
{
    public class ReportService : IReportService
    {
        readonly IReportRepository reportRepository;
        readonly ICachingService cachingService;
        readonly ISearchService<Contact> searchService;
        readonly IContactRepository contactRepository;
        readonly ICampaignRepository campaignRepository;
        readonly IOpportunityRepository opportunityRepository;
        readonly ITourService tourService;
        readonly IUserService userService;

        public ReportService(IReportRepository reportRepository, ICachingService cachingService, ISearchService<Contact> searchService
            , IContactRepository contactRepository, ICampaignRepository campaignRepository, IOpportunityRepository opportunityRepository,
           ITourService tourService, IUserService userService)
        {
            this.reportRepository = reportRepository;
            this.cachingService = cachingService;
            this.searchService = searchService;
            this.contactRepository = contactRepository;
            this.campaignRepository = campaignRepository;
            this.opportunityRepository = opportunityRepository;
            this.tourService = tourService;
            this.userService = userService;
        }


        #region OpportunityPipeline

        public ReportDataResponse GetOpportunityPipeline(ReportDataRequest request)
        {
            ReportDataResponse response = new ReportDataResponse();

            response.ReportData = reportRepository.GetOpportunityPipeline(new ReportFilters
            {
                StartDate = request.ReportViewModel.CustomStartDate,
                EndDate = request.ReportViewModel.CustomEndDate,
                SelectedOwners = request.ReportViewModel.OwnerIds,
                TrafficLifeCycle = request.ReportViewModel.OpportunityStageIds,
                AccountId = request.AccountId,
                SelectedCommunities = request.ReportViewModel.CommunityIds,
                Type = request.ReportViewModel.GroupId
            });

            IEnumerable<DashboardPieChartDetails> pieChartDetails =  opportunityRepository.OppertunityPipelinefunnelChartDetails(request.AccountId, request.ReportViewModel.OwnerIds, false, request.ReportViewModel.CustomStartDate, request.ReportViewModel.CustomEndDate);
            response.ReportData.DashboardPieCharData = pieChartDetails.Where(d => request.ReportViewModel.OpportunityStageIds.Any(o => o == d.DropdownValueID)).ToList();
            return response;
        }

        public ReportDataResponse GetOpportunityPipelineContacts(ReportDataRequest request)
        {
            ReportDataResponse response = new ReportDataResponse();

            response.ContactsData = reportRepository.GetOpportunityPipelineContacts(new ReportFilters
            {
                StartDate = request.ReportViewModel.CustomStartDate,
                EndDate = request.ReportViewModel.CustomEndDate,
                SelectedOwners = request.ReportViewModel.OwnerIds,
                TrafficLifeCycle = request.ReportViewModel.OpportunityStageIds,
                AccountId = request.AccountId,
                SelectedCommunities = request.ReportViewModel.CommunityIds,
                Type = request.ReportViewModel.GroupId,
                RowId = request.ReportViewModel.RowId
            });

            response.ContactIds = response.ContactsData.Select(p => p.contactID).ToList();

            return response;
        }


        #endregion

        #region TrafficbyType

        public ReportDataResponse GetTrafficByTypeData(ReportDataRequest request)
        {
            ReportDataResponse response = new ReportDataResponse();

            response.ReportData = reportRepository.GetTrafficByType(new ReportFilters
            {
                AccountId = request.AccountId,
                Type = request.ReportViewModel.GroupId,
                Top5Only = 0, // for bar chart
                DateRange = request.ReportViewModel.DateRange,
                StartDate = request.ReportViewModel.CustomStartDate,
                EndDate = request.ReportViewModel.CustomEndDate,
                SelectedOwners = request.ReportViewModel.OwnerIds,
                TrafficType = request.ReportViewModel.TourTypeIds,
                StartDatePrev = request.ReportViewModel.IsCompared == true ? request.ReportViewModel.CustomStartDatePrev : DateTime.Parse("1900-01-01"),
                EndDatePrev = request.ReportViewModel.IsCompared == true ? request.ReportViewModel.CustomEndDatePrev : DateTime.Parse("1900-01-01"),
                SelectedCommunities = request.ReportViewModel.CommunityIds,
                IsComparedTo = request.ReportViewModel.IsCompared
            });

            return response;
        }

        public ReportDataResponse GetTrafficByTypeContacts(ReportDataRequest request)
        {
            ReportDataResponse response = new ReportDataResponse();

            response.ContactsData = reportRepository.GetTrafficByTypeContacts(new ReportFilters
            {
                StartDate = request.ReportViewModel.CustomStartDate,
                EndDate = request.ReportViewModel.CustomEndDate,
                SelectedOwners = request.ReportViewModel.OwnerIds,
                TrafficType = request.ReportViewModel.TourTypeIds,
                AccountId = request.AccountId,
                SelectedCommunities = request.ReportViewModel.CommunityIds,
                Type = request.ReportViewModel.GroupId,
                RowId = request.ReportViewModel.RowId
            });

            response.ContactIds = response.ContactsData.Select(p => p.contactID).ToList();

            return response;
        }


        #endregion

        #region TrafficbySource
        public ReportDataResponse GetTrafficBySourceData(ReportDataRequest request)
        {
            ReportDataResponse response = new ReportDataResponse();

            ReportFilters reportFilters = new ReportFilters
            {
                AccountId = request.AccountId,
                Type = request.ReportViewModel.GroupId,
                Top5Only = 1, // for pie chart
                DateRange = request.ReportViewModel.DateRange,
                StartDate = request.ReportViewModel.CustomStartDate,
                EndDate = request.ReportViewModel.CustomEndDate,
                SelectedOwners = request.ReportViewModel.OwnerIds,
                TrafficSource = request.ReportViewModel.LeadSourceIds,
                StartDatePrev = request.ReportViewModel.IsCompared == true ? request.ReportViewModel.CustomStartDatePrev : DateTime.Parse("1900-01-01"),
                EndDatePrev = request.ReportViewModel.IsCompared == true ? request.ReportViewModel.CustomEndDatePrev : DateTime.Parse("1900-01-01"),
                SelectedCommunities = request.ReportViewModel.CommunityIds,
                IsComparedTo = request.ReportViewModel.IsCompared
            };

            response.ReportData = reportRepository.GetTrafficBySource(reportFilters);

            //Logger.Current.Verbose("Type : " + reportFilters.Type + " DateRange : "+reportFilters.DateRange + " StartDate : "+reportFilters.StartDate +
            //    " EndDate : "+ reportFilters.EndDate + " SelectedOwners : " + string.Join(", ", reportFilters.SelectedOwners)+ " TrafficSource : " + string.Join(", ", reportFilters.TrafficSource) +
            //    " StartDatePrev : "+ reportFilters.StartDatePrev + " EndDatePrev : " + reportFilters.EndDatePrev + " Selected Communities : "+ string.Join(", ", reportFilters.SelectedCommunities) + " IsComparedTo : " + reportFilters.IsComparedTo);
            return response;
        }

        public ReportDataResponse GetNewContactsVisualizationData(ReportDataRequest request)
        {
            ReportDataResponse response = new ReportDataResponse();
            var isAccountAdmin = CheckingDataSharing(request.RoleId, request.AccountId, request.IsSTAdmin);
            response.ReportData = reportRepository.GetNewLeadsVisuvalization(new ReportFilters
            {
                AccountId = request.AccountId,
                Type = request.ReportViewModel.GroupId,
                Top5Only = 1, // for pie chart
                DateRange = request.ReportViewModel.DateRange,
                StartDate = request.ReportViewModel.CustomStartDate,
                EndDate = request.ReportViewModel.CustomEndDate,
                SelectedOwners = request.ReportViewModel.OwnerIds,
                IsAdmin = isAccountAdmin,
                //  TrafficSource = request.ReportViewModel.LeadSourceIds,
                StartDatePrev = request.ReportViewModel.IsCompared == true ? request.ReportViewModel.CustomStartDatePrev : DateTime.Parse("1900-01-01"),
                EndDatePrev = request.ReportViewModel.IsCompared == true ? request.ReportViewModel.CustomEndDatePrev : DateTime.Parse("1900-01-01"),
                //  SelectedCommunities = request.ReportViewModel.CommunityIds,
                IsComparedTo = request.ReportViewModel.IsCompared
            });
            return response;
        }

        public ReportDataResponse GetTrafficBySourceContacts(ReportDataRequest request)
        {
            ReportDataResponse response = new ReportDataResponse();

            response.ContactsData = reportRepository.GetTrafficBySourceContacts(new ReportFilters
            {
                StartDate = request.ReportViewModel.CustomStartDate,
                EndDate = request.ReportViewModel.CustomEndDate,
                SelectedOwners = request.ReportViewModel.OwnerIds,
                TrafficSource = request.ReportViewModel.LeadSourceIds,
                AccountId = request.AccountId,
                SelectedCommunities = request.ReportViewModel.CommunityIds,
                Type = request.ReportViewModel.GroupId,
                RowId = request.ReportViewModel.RowId
            });

            response.ContactIds = response.ContactsData.Select(p => p.contactID).ToList();

            return response;
        }
        #endregion

        #region TrafficbyLifeCycle

        public ReportDataResponse GetTrafficByLifeCycleData(ReportDataRequest request)
        {
            ReportDataResponse response = new ReportDataResponse();

            response.ReportData = reportRepository.GetTrafficByLifecycle(new ReportFilters
            {
                AccountId = request.AccountId,
                Type = request.ReportViewModel.GroupId,
                Top5Only = 1, // for pie chart
                DateRange = request.ReportViewModel.DateRange,
                StartDate = request.ReportViewModel.CustomStartDate,
                EndDate = request.ReportViewModel.CustomEndDate,
                SelectedOwners = request.ReportViewModel.OwnerIds,
                TrafficLifeCycle = request.ReportViewModel.LifeStageIds,
                StartDatePrev = request.ReportViewModel.IsCompared == true ? request.ReportViewModel.CustomStartDatePrev : DateTime.Parse("1900-01-01"),
                EndDatePrev = request.ReportViewModel.IsCompared == true ? request.ReportViewModel.CustomEndDatePrev : DateTime.Parse("1900-01-01"),
                SelectedCommunities = request.ReportViewModel.CommunityIds,
                IsComparedTo = request.ReportViewModel.IsCompared
            });

            return response;
        }

        public ReportDataResponse GetTrafficByLifeCycleContacts(ReportDataRequest request)
        {
            ReportDataResponse response = new ReportDataResponse();

            response.ContactsData = reportRepository.GetTrafficByLifeCycleContacts(new ReportFilters
            {
                StartDate = request.ReportViewModel.CustomStartDate,
                EndDate = request.ReportViewModel.CustomEndDate,
                SelectedOwners = request.ReportViewModel.OwnerIds,
                TrafficLifeCycle = request.ReportViewModel.LifeStageIds,
                AccountId = request.AccountId,
                SelectedCommunities = request.ReportViewModel.CommunityIds,
                Type = request.ReportViewModel.GroupId,
                RowId = request.ReportViewModel.RowId
            });

            response.ContactIds = response.ContactsData.Select(p => p.contactID).ToList();

            return response;
        }


        #endregion

        #region Activity

        public ReportDataResponse GetActivityData(ReportDataRequest request)
        {
            ReportDataResponse response = new ReportDataResponse();

            response.ReportData = reportRepository.GetActivitiesByModule(new ReportFilters
            {
                StartDate = request.ReportViewModel.CustomStartDate,
                EndDate = request.ReportViewModel.CustomEndDate,
                SelectedOwners = request.ReportViewModel.OwnerIds,
                ModuleIDs = request.ReportViewModel.ModuleIds,
                AccountId = request.AccountId
            });

            return response;
        }


        public ReportDataResponse GetActivityReportContacts(ReportDataRequest request)
        {
            ReportDataResponse response = new ReportDataResponse();

            response.ContactIds = reportRepository.GetActivityReportDrildownModuleContactIds(new ReportFilters
            {
                StartDate = request.ReportViewModel.CustomStartDate,
                EndDate = request.ReportViewModel.CustomEndDate,
                SelectedOwners = request.ReportViewModel.RowId != 0 ? new int[] { request.ReportViewModel.RowId } : request.ReportViewModel.OwnerIds,
                AccountId = request.AccountId,
                ModuleIDs = request.ReportViewModel.ModuleIds
            });

            return response;
        }

        public ReportDataResponse GetActivityReportTourContacts(ReportDataRequest request)
        {
            ReportDataResponse response = new ReportDataResponse();
            response.ContactIds = reportRepository.GetActivityReportTourContactIds(new ReportFilters
            {
                StartDate = request.ReportViewModel.CustomStartDate,
                EndDate = request.ReportViewModel.CustomEndDate,
                SelectedOwners = request.ReportViewModel.OwnerIds,
                ModuleIDs = request.ReportViewModel.ModuleIds,
                AccountId = request.AccountId,
                RowId = request.ReportViewModel.RowId
            });

            return response;

        }

        public ReportDataResponse GetActivityReportNoteContacts(ReportDataRequest request)
        {
            ReportDataResponse response = new ReportDataResponse();
            response.ContactIds = reportRepository.GetActivityReportNoteContactsIds(new ReportFilters
            {
                StartDate = request.ReportViewModel.CustomStartDate,
                EndDate = request.ReportViewModel.CustomEndDate,
                SelectedOwners = request.ReportViewModel.OwnerIds,
                ModuleIDs = request.ReportViewModel.ModuleIds,
                AccountId = request.AccountId,
                RowId = request.ReportViewModel.RowId
            });

            return response;
        }
        #endregion

        #region TrafficByTypeAndLifecycle

        public ReportDataResponse GetTrafficByTypeAndLifeCycleData(ReportDataRequest request)
        {
            ReportDataResponse response = new ReportDataResponse();

            response.ReportData = reportRepository.GetTrafficByTypeAndLifeCycle(new ReportFilters
            {
                AccountId = request.AccountId,
                Type = request.ReportViewModel.GroupId,
                Top5Only = 1, // for pie chart
                DateRange = request.ReportViewModel.DateRange,
                StartDate = request.ReportViewModel.CustomStartDate,
                EndDate = request.ReportViewModel.CustomEndDate,
                SelectedOwners = request.ReportViewModel.OwnerIds,
                TrafficLifeCycle = request.ReportViewModel.LifeStageIds,
                TrafficType = request.ReportViewModel.TourTypeIds,
                StartDatePrev = request.ReportViewModel.IsCompared == true ? request.ReportViewModel.CustomStartDatePrev : DateTime.Parse("1900-01-01"),
                EndDatePrev = request.ReportViewModel.IsCompared == true ? request.ReportViewModel.CustomEndDatePrev : DateTime.Parse("1900-01-01"),
                SelectedCommunities = request.ReportViewModel.CommunityIds,
                IsComparedTo = request.ReportViewModel.IsCompared
            });

            return response;
        }


        public ReportDataResponse GetTrafficByTypeAndLifeCycleContacts(ReportDataRequest request)
        {
            ReportDataResponse response = new ReportDataResponse();
            response.ContactsData = reportRepository.GetTrafficByTypeAndLifeCycleContacts(new ReportFilters
            {
                StartDate = request.ReportViewModel.CustomStartDate,
                EndDate = request.ReportViewModel.CustomEndDate,
                SelectedOwners = request.ReportViewModel.OwnerIds,
                TrafficLifeCycle = request.ReportViewModel.LifeStageIds,
                TrafficType = request.ReportViewModel.TourTypeIds,
                AccountId = request.AccountId,
                SelectedCommunities = request.ReportViewModel.CommunityIds,
                Type = request.ReportViewModel.GroupId,
                RowId = request.ReportViewModel.RowId,
                DropdownType = request.ReportViewModel.DropdownType
            });

            response.ContactIds = response.ContactsData.Select(p => p.contactID).ToList();
            return response;
        }


        #endregion

        private bool CheckingDataSharing(short roleID, int accountId, bool isStAdmin)
        {
            var isAccountAdmin = cachingService.IsAccountAdmin(roleID, accountId);
            if (isStAdmin == true || isAccountAdmin == true)
                return isAccountAdmin = true;

            if (isAccountAdmin == false)
            {
                bool isPrivate = cachingService.IsModulePrivate(AppModules.Contacts, accountId);
                if (isPrivate == false)
                    isAccountAdmin = true;
            }
            return isAccountAdmin;
        }

        # region FirstLeadSourceReport
        public ReportDataResponse RunFirstLeadSourceReport(ReportDataRequest request)
        {
            ReportDataResponse response = new ReportDataResponse();

            response.ReportData = reportRepository.GetFirstLeadSourceReport(new ReportFilters
            {
                AccountId = request.AccountId,
                Type = request.ReportViewModel.GroupId,
                Top5Only = 1, // for pie chart
                DateRange = request.ReportViewModel.DateRange,
                StartDate = request.ReportViewModel.CustomStartDate,
                EndDate = request.ReportViewModel.CustomEndDate,
                SelectedOwners = request.ReportViewModel.OwnerIds,
                TrafficLifeCycle = request.ReportViewModel.LifeStageIds,
                SelectedCommunities = request.ReportViewModel.CommunityIds
            });

            if (response.ReportData != null)
            {
                
                var resultsList = response.ReportData.GridData.ToList();
                var dropdownValues = resultsList.Select(s => s.DropdownValues);
                var dropdownValuesList = cachingService.GetDropdownValues(request.AccountId);
                if (dropdownValues != null && dropdownValues.Any())
                {
                    var sw = new System.Diagnostics.Stopwatch();
                    sw.Start();

                    IEnumerable<int?> ids = new List<int?>();
                    var idsString = dropdownValues.FirstOrDefault().Select(s => s.DropdownValueName);
                    if (idsString != null)
                        ids = idsString.Select(s => { int i; int.TryParse(s, out i); return (int?)i; });
                    IEnumerable<FirstLeadSourceReportResponse> gridNames = new List<FirstLeadSourceReportResponse>();
                    if (request.ReportViewModel.GroupId == 1)
                    {
                        var usersResponse = userService.GetUsersByUserIDs(new GetUsersByUserIDsRequest() { UserIDs = ids });
                        if (usersResponse.Users != null)
                            gridNames = usersResponse.Users.Select(se => new FirstLeadSourceReportResponse { ID = se.UserID, Name = se.FirstName + " " + se.LastName });
                    }
                    else if (request.ReportViewModel.GroupId == 2)
                    {
                        var lifecycleStages = dropdownValuesList.Where(se => se.DropdownID == (byte)DropdownFieldTypes.LifeCycle).
                          Select(d => d.DropdownValuesList).ToList().FirstOrDefault();

                        gridNames = lifecycleStages.Where(w => ids.Contains(w.DropdownValueID) && w.DropdownID == (byte)DropdownFieldTypes.LifeCycle).ToList()
                       .Select(f => new FirstLeadSourceReportResponse { ID = f.DropdownValueID, Name = f.DropdownValue }).ToList();
                    }
                    else if (request.ReportViewModel.GroupId == 3)
                    {
                        var communities = dropdownValuesList.Where(se => se.DropdownID == (byte)DropdownFieldTypes.Community).
                          Select(d => d.DropdownValuesList).ToList().FirstOrDefault();

                        gridNames = communities.Where(w => ids.Contains(w.DropdownValueID) && w.DropdownID == (byte)DropdownFieldTypes.Community).ToList()
                       .Select(f => new FirstLeadSourceReportResponse { ID = f.DropdownValueID, Name = f.DropdownValue }).ToList();
                    }
                    if (gridNames != null)
                    {
                        foreach (var reportData in resultsList)
                        {
                            foreach (var data in reportData.DropdownValues)
                            {
                                int id = Convert.ToInt32(data.DropdownValueName);
                                data.DropdownValueId = id;
                                data.DropdownValueName = gridNames.Where(w => w.ID == id).Select(n => n.Name).FirstOrDefault();
                            }
                        }
                    }

                    sw.Stop();
                    var timeelapsed = sw.Elapsed;
                    Logger.Current.Informational("Time elapsed to process AE/LeadSources/Communities:" + timeelapsed);
                }

                IEnumerable<int> leadSourceIds = resultsList.Select(s => s.ID);
                var LeadSources = dropdownValuesList.Where(w => w.DropdownID == (byte)DropdownFieldTypes.LeadSources).Select(s => s.DropdownValuesList).ToList().FirstOrDefault();
                var leadSourceValues = LeadSources.Where(w => leadSourceIds.Contains(w.DropdownValueID)).ToList().Select(s => new FirstLeadSourceReportResponse { ID = s.DropdownValueID, Name = s.DropdownValue }).ToList();

                foreach (var data in resultsList)
                    data.Name = leadSourceValues.Where(w => w.ID == data.ID).Select(s => s.Name).FirstOrDefault();
                response.ReportData.GridData = resultsList;
            }

            return response;
        }

        public ReportDataResponse FirstLeadSourceReportContacts(ReportDataRequest request)
        {
            ReportDataResponse response = new ReportDataResponse();

            response.ContactsData = reportRepository.GetFirstLEadSourceReportContacts(new ReportFilters
            {
                AccountId = request.AccountId,
                Type = request.ReportViewModel.GroupId,
                Top5Only = 1, // for pie chart
                DateRange = request.ReportViewModel.DateRange,
                StartDate = request.ReportViewModel.CustomStartDate,
                EndDate = request.ReportViewModel.CustomEndDate,
                SelectedOwners = request.ReportViewModel.OwnerIds,
                TrafficLifeCycle = request.ReportViewModel.LifeStageIds,
                TrafficSource = request.ReportViewModel.LeadSourceIds,
                SelectedCommunities = request.ReportViewModel.CommunityIds
            });
            response.ContactIds = response.ContactsData.Select(p => p.contactID).ToList();
            return response;
        }
        #endregion

        public StandardReportResponse RunNightlyStatusReport(ReportDataRequest request)
        {
            StandardReportResponse response = new StandardReportResponse();

            response.ReportList = reportRepository.GetNightlyStatusReportData(new ReportFilters() 
            {
                AccountIDs = request.ReportViewModel.AccountIds,
                StartDate = request.ReportViewModel.CustomStartDate,
                EndDate  = request.ReportViewModel.CustomEndDate,
                SubscriptionID = request.ReportViewModel.SubscriptionID,
                Type = (int)request.ReportViewModel.ReportType,
                PageNumber = request.ReportViewModel.PageNumber,
                PageLimit = (int)request.ReportViewModel.ShowTop
            });

            return response;
        }

        public StandardReportResponse RunLoginFrequencyReport(ReportDataRequest request)
        {
            StandardReportResponse response = new StandardReportResponse();

            response.ReportList = reportRepository.GetLoginFrequencyReportData(new ReportFilters()
            {
                AccountIDs = request.ReportViewModel.AccountIds,
                AccountId = request.ReportViewModel.AccountId,
                StartDate = request.ReportViewModel.CustomStartDate,
                EndDate = request.ReportViewModel.CustomEndDate,
                Type = request.ReportViewModel.LoginFrequencyReportID
            });

            return response;
        }

        #region AllLeadSourceReport
        public ReportDataResponse RunAllLeadSourceReport(ReportDataRequest request)
        {
            ReportDataResponse response = new ReportDataResponse();

            response.ReportData = reportRepository.GetAllLeadSourceReport(new ReportFilters
            {
                AccountId = request.AccountId,
                Type = request.ReportViewModel.GroupId,
                DateRange = request.ReportViewModel.DateRange,
                StartDate = request.ReportViewModel.CustomStartDate,
                EndDate = request.ReportViewModel.CustomEndDate,
                TrafficSource = request.ReportViewModel.LeadSourceIds,
                TrafficLifeCycle = request.ReportViewModel.LifeStageIds,
                SelectedCommunities = request.ReportViewModel.CommunityIds
            });

            return response;
        }

        public ReportDataResponse AllLeadSourceReportContacts(ReportDataRequest request)
        {
            ReportDataResponse response = new ReportDataResponse();

            response.ContactsData = reportRepository.GetAllLeadSourceReportContacts(new ReportFilters
            {
                AccountId = request.AccountId,
                Type = request.ReportViewModel.GroupId,
                Top5Only = 1, // for pie chart
                DateRange = request.ReportViewModel.DateRange,
                StartDate = request.ReportViewModel.CustomStartDate,
                EndDate = request.ReportViewModel.CustomEndDate,
                SelectedOwners = request.ReportViewModel.OwnerIds,
                TrafficLifeCycle = request.ReportViewModel.LifeStageIds,
                TrafficSource = request.ReportViewModel.LeadSourceIds,
                SelectedCommunities = request.ReportViewModel.CommunityIds,
                ColumnIndex = request.ReportViewModel.ColumnIndex
            });
            response.ContactIds = response.ContactsData.Select(p => p.contactID).ToList();
            return response;
        }
        #endregion

        #region HotList

        public async Task<HotListResponse> HotListDataAsync(HotListRequest request)
        {
            SearchDefinition searchDefinition = Mapper.Map<ReportViewModel, SearchDefinition>(request.HotlistViewModel);
            List<SearchFilter> filters = searchDefinition.Filters.ToList();
            SearchFilter enddate = new SearchFilter();
            enddate.Field = ContactFields.LastUpdateOn;
            enddate.Qualifier = SearchQualifier.IsLessThanEqualTo;
            enddate.SearchText = Convert.ToString(request.HotlistViewModel.CustomEndDate);
            enddate.IsDateTime = true;
            filters.Add(enddate);
            searchDefinition.CustomPredicateScript += "  AND (" + filters.Count() + ") ";


            SearchFilter startdate = new SearchFilter();
            startdate.Field = ContactFields.LastUpdateOn;
            startdate.Qualifier = SearchQualifier.IsGreaterThanEqualTo;
            startdate.SearchText = Convert.ToString(request.HotlistViewModel.CustomStartDate);
            startdate.IsDateTime = true;
            filters.Add(startdate);
            searchDefinition.CustomPredicateScript += "  AND (" + filters.Count() + ") ";

            searchDefinition.Filters = filters;

            if (searchDefinition.Id == 0 && string.IsNullOrEmpty(searchDefinition.Name))
                searchDefinition.Name = "Adhoc Search";
            searchDefinition.AccountID = request.AccountId;
            SearchParameters parameters = new SearchParameters();

            parameters.Limit = (int)request.HotlistViewModel.ShowTop;
            parameters.PageNumber = request.HotlistViewModel.PageNumber;
            parameters.SortField = ContactSortFieldType.LeadScore;
            if (request.Fields != null)
                parameters.Fields = request.Fields;

            bool isAccountAdmin = cachingService.IsAccountAdmin(request.RoleId, request.AccountId);
            bool isPrivate = cachingService.IsModulePrivate(AppModules.Contacts, request.AccountId);
            if (isPrivate && !isAccountAdmin)
            {
                int userId = (int)request.RequestedBy;
                parameters.IsPrivateSearch = true;
                parameters.DocumentOwnerId = userId;
            }
            else
                parameters.IsPrivateSearch = false;

            HotListResponse response = new HotListResponse();
            var dropdowns = cachingService.GetDropdownValues(searchDefinition.AccountID);

            var lifecycleStages = dropdowns.Where(s => s.DropdownID == (byte)DropdownFieldTypes.LifeCycle).
                      Select(s => s.DropdownValuesList).ToList().FirstOrDefault();

            var leadSources = dropdowns.Where(s => s.DropdownID == (byte)DropdownFieldTypes.LeadSources).Select(s => s.DropdownValuesList).ToList().FirstOrDefault();

            response.RunListResult = await searchResultsAsync(searchDefinition, parameters);
            IEnumerable<int?> OwnerIds = response.RunListResult.Results.Select(s => s.OwnerId);
            IEnumerable<Owner> Owners = contactRepository.GetUserNames(OwnerIds);
            foreach (var contact in response.RunListResult.Results)
            {
                contact.LifecycleName = lifecycleStages.Where(e => e.DropdownValueID == contact.LifecycleStage).Select(s => s.DropdownValue).FirstOrDefault();
                contact.OwnerName = Owners.Where(o => o.OwnerId == contact.OwnerId).Select(s => s.OwnerName).FirstOrDefault();
                if (contact.AllLeadSources != null)
                    contact.LeadSources = string.Join(", ", leadSources.Where(e => contact.AllLeadSources.Contains(e.DropdownValueID)).Select(s => s.DropdownValue));
                else
                    contact.LeadSources = "";
            }

            return response;
        }

        public ReportDataResponse GetWebVisitsReport(ReportDataRequest request)
        {
            ReportDataResponse response = new ReportDataResponse();
            int TotalHits = default(int);

            var pageNumber = request.ReportViewModel.PageNumber;

            var recordsLimit = (int)request.ReportViewModel.ShowTop;
            var UserId = 0;
            var IsAdmin = CheckingDataSharing(request.RoleId, request.AccountId, request.IsSTAdmin);
            if (request.ReportViewModel.OwnerIds == null)
            {
                request.ReportViewModel.OwnerIds = IsAdmin == true ? contactRepository.GetUsers(request.AccountId, UserId, request.IsSTAdmin).Select(u => u.OwnerId).ToArray() : new int[] { request.UserId };
            }
            else
            {
                request.ReportViewModel.OwnerIds = IsAdmin == true ? request.ReportViewModel.OwnerIds : new int[] { request.UserId };
            }
            response.WebVisits = reportRepository.GetWebVisitsReport(new ReportFilters
            {
                StartDate = request.ReportViewModel.CustomStartDate,
                EndDate = request.ReportViewModel.CustomEndDate,
                SelectedOwners = request.ReportViewModel.OwnerIds,
                AccountId = request.AccountId,
                TrafficSource = request.ReportViewModel.LeadSourceIds,
                PageLimit = recordsLimit,
                PageNumber = pageNumber,
                SortDirection = request.ReportViewModel.SortDirection,
                SortField = request.ReportViewModel.SortField
            }, out TotalHits);

            if (response.WebVisits.IsAny())
            {
                response.WebVisits.Each(wv =>
                {
                    wv.VisitedOn = wv.VisitedOn.ToUtc().ToUtcBrowserDatetime();
                });
            }
            
            response.WebVisits = response.WebVisits.ToList(); 
            response.TotalHits = TotalHits;
            return response;
        }

        public ReportDataResponse GetCustomReportData(ReportDataRequest request)
        {
            ReportDataResponse response = new ReportDataResponse();

            response.ReportData = reportRepository.GetCustomReportData(new ReportFilters
            {
                AccountId = request.AccountId,
                ReportId = request.ReportViewModel.ReportId,
                StartDate = request.ReportViewModel.CustomStartDate,
                EndDate = request.ReportViewModel.CustomEndDate,

            });

            return response;
        }

        public ReportDataResponse GetHotListData(ReportDataRequest request)
        {
            ReportDataResponse response = new ReportDataResponse();

            var UserId = 0;
            // bool isPrivate = cachingService.IsModulePrivate(AppModules.Contacts, request.AccountId);
            var IsAdmin = CheckingDataSharing(request.RoleId, request.AccountId, request.IsSTAdmin);

            var dropdownValues = cachingService.GetDropdownValues(request.AccountId);

            request.ReportViewModel.LeadSourceIds = dropdownValues.Where(s => s.DropdownID == (byte)DropdownFieldTypes.LeadSources).
                                  Select(s => s.DropdownValuesList).ToList().FirstOrDefault().Where(d => d.IsActive == true).Select(p => p.DropdownValueID).ToArray();

            if (request.ReportViewModel.OwnerIds == null)
            {
                request.ReportViewModel.OwnerIds = IsAdmin == true ? contactRepository.GetUsers(request.AccountId, UserId, request.IsSTAdmin).Select(u => u.OwnerId).ToArray() : new int[] { request.UserId };
            }
            else
            {
                request.ReportViewModel.OwnerIds = IsAdmin == true ? request.ReportViewModel.OwnerIds : new int[] { request.UserId };
            }
            response.HotlistGridData = reportRepository.GetHotlistReportData(new ReportFilters
            {
                StartDate = request.ReportViewModel.CustomStartDate,
                EndDate = request.ReportViewModel.CustomEndDate,
                SelectedOwners = request.ReportViewModel.OwnerIds,
                TrafficLifeCycle = request.ReportViewModel.LifeStageIds,
                AccountId = request.AccountId,
                TrafficSource = request.ReportViewModel.LeadSourceIds,
                PageLimit = (int)request.ReportViewModel.ShowTop,
                PageNumber = request.ReportViewModel.PageNumber,
                IsDasboardView = request.IsDasboardView
            });
            response.ContactIds = response.HotlistGridData.Contacts.ToList();
            var ddv = cachingService.GetDropdownValues(request.AccountId);

            LandmarkIT.Enterprise.Extensions.EnumerableExtentions.Each(response.HotlistGridData.HotlistData, f =>
            {
                f.Lifecycle = GetDDValue(ddv, (int)(f.LifeCycleStageId), (byte)DropdownFieldTypes.LifeCycle);
                f.LeadSource = GetDDValue(ddv, (int)(f.LeadSourceId), (byte)DropdownFieldTypes.LeadSources);
            });

            response.TotalHits = response.HotlistGridData.TotalHits;
            return response;
        }

        private string GetDDValue(IEnumerable<DropdownViewModel> ddv, int id, byte stage)
        {
            if (id == 0)
                return string.Empty;

            var stageName = ddv.Where(s => s.DropdownID == stage).FirstOrDefault().DropdownValuesList.Where(d => d.DropdownValueID == id).FirstOrDefault().DropdownValue;
            return stageName;
        }

        private async Task<SearchResult<ContactReportEntry>> AdvancedSearchForStandardReportAsync(StandardReportRequest request)
        {
            SearchParameters parameters = new SearchParameters();
            SearchDefinition searchDefinition = GetSearchDefination(request, out parameters, 0);
            return await searchResultsAsync(searchDefinition, parameters);
        }

        private SearchDefinition GetSearchDefination(StandardReportRequest request, out SearchParameters searchParameters, int Leadtype)
        {
            //search paramters
            SearchParameters parameters = new SearchParameters();

            parameters.Limit = (int)request.ReportViewModel.ShowTop;
            parameters.PageNumber = request.ReportViewModel.PageNumber == 0 ? 1 : request.ReportViewModel.PageNumber;
            parameters.Types = request.Types != null && request.Types.Any() ? request.Types : new List<Type>() { typeof(Person), typeof(Company) };

            if (request.ReportViewModel.Fields != null)
                parameters.Fields = request.Fields;

            bool isAccountAdmin = cachingService.IsAccountAdmin(request.RoleId, request.AccountId);
            bool isPrivate = cachingService.IsModulePrivate(AppModules.Contacts, request.AccountId);
            if (isPrivate && !isAccountAdmin)
            {
                int userId = (int)request.RequestedBy;
                parameters.IsPrivateSearch = true;
                parameters.DocumentOwnerId = userId;
            }
            else
                parameters.IsPrivateSearch = false;
            searchParameters = parameters;
            //search definition
            SearchDefinition searchDefinition = Mapper.Map<ReportViewModel, SearchDefinition>(request.ReportViewModel);
            searchDefinition.PageNumber = request.ReportViewModel.PageNumber == 0 ? (short)1 : request.ReportViewModel.PageNumber;
            List<SearchFilter> filters = searchDefinition.Filters.ToList();

            if (searchParameters.IsPrivateSearch)
            {
                SearchFilter owner = new SearchFilter();
                owner.Field = ContactFields.Owner;
                owner.Qualifier = SearchQualifier.Is;
                owner.SearchText = searchParameters.DocumentOwnerId.ToString();
                filters.Add(owner);
                if (searchDefinition.CustomPredicateScript != null)
                    searchDefinition.CustomPredicateScript += "  AND (" + filters.Count() + ") ";
                else
                    searchDefinition.CustomPredicateScript += "(" + filters.Count() + ") ";

                if (searchDefinition.PredicateType == 0)
                    searchDefinition.PredicateType = SearchPredicateType.And;
            }

            //SearchFilter to exclude contacts through Imports
            //  int importDropdownId = reportRepository.GetImportDropdownValue(request.AccountId);
            SearchFilter importFilter = new SearchFilter();
            importFilter.Field = ContactFields.IncludeInReports;
            importFilter.Qualifier = SearchQualifier.IsNot;
            importFilter.SearchText ="0";  //ContactSource.Imports
            filters.Add(importFilter);
            if (searchDefinition.CustomPredicateScript != null)
                searchDefinition.CustomPredicateScript += "  AND (" + filters.Count() + ") ";
            else
                searchDefinition.CustomPredicateScript += "(" + filters.Count() + ") ";

            if (searchDefinition.PredicateType == 0)
                searchDefinition.PredicateType = SearchPredicateType.And;


            SearchFilter enddate = new SearchFilter();
            enddate.Field = ContactFields.CreatedOn;
            enddate.Qualifier = SearchQualifier.IsLessThanEqualTo;
            enddate.SearchText = Convert.ToString(request.ReportViewModel.CustomEndDate);
            enddate.IsDateTime = true;
            filters.Add(enddate);
            if (searchDefinition.CustomPredicateScript != null)
                searchDefinition.CustomPredicateScript += "  AND (" + filters.Count() + ") ";
            else
                searchDefinition.CustomPredicateScript += "(" + filters.Count() + ") ";

            if (searchDefinition.PredicateType == 0)
                searchDefinition.PredicateType = SearchPredicateType.And;


            SearchFilter startdate = new SearchFilter();
            startdate.Field = ContactFields.CreatedOn;
            startdate.Qualifier = SearchQualifier.IsGreaterThanEqualTo;
            startdate.SearchText = Convert.ToString(request.ReportViewModel.CustomStartDate);
            startdate.IsDateTime = true;
            filters.Add(startdate);

            searchDefinition.CustomPredicateScript += "  AND (" + filters.Count() + ") ";
            if (request.ReportViewModel.LeadSourceIds != null && request.ReportViewModel.LeadSourceIds.Length != 0)
            {
                var searchtext = string.Join("|", request.ReportViewModel.LeadSourceIds);

                SearchFilter leadSource = new SearchFilter();
                leadSource.Field = ContactFields.AllLeadSources;
                leadSource.Qualifier = SearchQualifier.Is;
                leadSource.IsDropdownField = true;
                leadSource.SearchText = searchtext;
                filters.Add(leadSource);
                searchDefinition.CustomPredicateScript += " AND  (" + filters.Count() + ")";
                searchDefinition.PredicateType = searchDefinition.PredicateType == 0 ? SearchPredicateType.And : searchDefinition.PredicateType;
            }
            if (Leadtype == 1)
            {
                SearchFilter leadSource = new SearchFilter();
                leadSource.Field = ContactFields.LastTouchedThrough;
                leadSource.Qualifier = SearchQualifier.IsEmpty;
                leadSource.IsDropdownField = true;
                // leadSource.SearchText = searchtext;
                filters.Add(leadSource);
                searchDefinition.CustomPredicateScript += " AND  (" + filters.Count() + ")";
                searchDefinition.PredicateType = searchDefinition.PredicateType == 0 ? SearchPredicateType.And : searchDefinition.PredicateType;
            }

            searchDefinition.Filters = filters;

            if (searchDefinition.Id == 0 && string.IsNullOrEmpty(searchDefinition.Name))
                searchDefinition.Name = "Adhoc Search";
            searchDefinition.AccountID = request.AccountId;
            //  Session["AdvancedSearchVM"] = searchDefinition;
            return searchDefinition;

        }

        async Task<SearchResult<ContactReportEntry>> searchResultsAsync(SearchDefinition searchDefinition, SearchParameters parameters)
        {
            Logger.Current.Verbose("Request for Run advanced search");
            parameters.PageNumber = searchDefinition.PageNumber;
            var result = await searchService.AdvancedSearchAsync("", searchDefinition, parameters);
            SearchResult<ContactReportEntry> searchResult = new SearchResult<ContactReportEntry>();
            searchResult.TotalHits = result.TotalHits;
            Logger.Current.Verbose("Advanced search results total :" + result.TotalHits);
            searchResult.Results = Mapper.Map<IEnumerable<Contact>, IEnumerable<ContactReportEntry>>(result.Results);
            return searchResult;
        }

        #endregion

        #region CampaignList

        public StandardReportResponse RunCampaignReport(StandardReportRequest request)
        {
            Logger.Current.Verbose("Request to Campaigns List by clickrate");
            StandardReportResponse response = new StandardReportResponse();
            Logger.Current.Informational("userID : " + request.RequestedBy);
            int TotalHits = default(int);
            //  bool isAccountAdmin = cachingService.IsAccountAdmin(request.RoleId, request.AccountId);
            var isAccountAdmin = false;
            if (request.IsSTadmin == true)
                isAccountAdmin = true;
            else
                isAccountAdmin = cachingService.IsAccountAdmin(request.RoleId, request.AccountId);

            if (isAccountAdmin == false)
            {
                bool isPrivate = cachingService.IsModulePrivate(AppModules.Campaigns, request.AccountId);
                if (isPrivate == false)
                    isAccountAdmin = true;
            }

            IEnumerable<CampaignReportData> campaigns = campaignRepository.GetCampaignListByClicks(request.ReportViewModel.CustomStartDate, request.ReportViewModel.
                CustomEndDate, request.AccountId, request.ReportViewModel.PageNumber, request.ReportViewModel.ShowTop, isAccountAdmin, request.UserID, out TotalHits, request.isReputationReport);

            if (campaigns == null)
                response.Exception = GetCampaignNotFoundException();

            IEnumerable<CampaignEntryViewModel> campaignlist = Mapper.Map<IEnumerable<CampaignReportData>, IEnumerable<CampaignEntryViewModel>>(campaigns);
            response.ReportList = campaignlist;
            response.TotalHits = TotalHits;
            return response;
        }

        public GetWorkflowsForCampaignReportResponse GetWorkflowsForCampaignReport(GetWorkflowsForCampaignReportRequest request)
        {
            GetWorkflowsForCampaignReportResponse response = new GetWorkflowsForCampaignReportResponse();
            response.WorkflowNames = campaignRepository.GetWorkflwosForCampaignReport(request.CampaignID);
            return response;
        }

        public ReportExportResponce RunCampaignReportExport(ReportExportRequest request)
        {
            ReportExportResponce responce = new ReportExportResponce();
            var isAccountAdmin = false;
            if (request.IsSTadmin == true)
                isAccountAdmin = true;
            else
                isAccountAdmin = cachingService.IsAccountAdmin(request.RoleId, request.AccountId);

            if (isAccountAdmin == false)
            {
                bool isPrivate = cachingService.IsModulePrivate(AppModules.Campaigns, request.AccountId);
                if (isPrivate == false)
                    isAccountAdmin = true;
            }

            DataTable datatable = campaignRepository.GetCampaignReportExport(request.CustomStartDate, request.CustomEndDate, isAccountAdmin, request.AccountId, request.RequestedBy);

            ReadExcel exl = new ReadExcel();
            responce.fileContent = exl.ConvertDataSetToExcel(datatable, string.Empty);
            responce.fileName = "Campaign List Report";
            return responce;
        }


        public StandardReportResponse GetDashboardcampaignList(StandardReportRequest request)
        {
            Logger.Current.Verbose("Request to Campaigns List by clickrate for Dashboard List");

            StandardReportResponse response = new StandardReportResponse();
            Logger.Current.Informational("userID : " + request.RequestedBy);
            Logger.Current.Informational("custom StartDate : " + request.ReportViewModel.CustomStartDate);
            Logger.Current.Informational("custom Enddate : " + request.ReportViewModel.CustomEndDate);
            var isAccountAdmin = false;
            if (request.IsSTadmin == true)
                isAccountAdmin = true;
            else
                isAccountAdmin = cachingService.IsAccountAdmin(request.RoleId, request.AccountId);

            if (isAccountAdmin == false)
            {
                bool isPrivate = cachingService.IsModulePrivate(AppModules.Campaigns, request.AccountId);
                if (isPrivate == false)
                    isAccountAdmin = true;
            }

            IEnumerable<CampaignReportData> campaigns = campaignRepository.GetCampaignListByClicks(request.ReportViewModel.CustomStartDate, request.ReportViewModel.
                CustomEndDate, request.AccountId, isAccountAdmin, request.UserID);

            if (campaigns == null)
                response.Exception = GetCampaignNotFoundException();

            IEnumerable<CampaignEntryViewModel> campaignlist = Mapper.Map<IEnumerable<CampaignReportData>, IEnumerable<CampaignEntryViewModel>>(campaigns);
            response.ReportList = campaignlist;

            return response;
        }
        #endregion

        #region NewLeads

        public StandardReportResponse GetNewLeadsVisualizationAsync(StandardReportRequest request)
        {
            StandardReportResponse response = new StandardReportResponse();

            SearchParameters parameters = new SearchParameters();

            request.Types = new List<Type>() { typeof(Person), typeof(Contact) };
            SearchDefinition searchdefinition = GetSearchDefination(request, out parameters, 0);

            var areaChartData = searchService.GetContactsAggregationByDate(searchdefinition, parameters);

            var pieChartData = searchService.GetTopLeadSources(searchdefinition, parameters);

            response.TopLeads = GetAreaChartData(areaChartData, request.ReportViewModel.CustomStartDate, request.ReportViewModel.CustomEndDate);

            List<int> dropdownValueKeys = new List<int>(pieChartData.Keys);
            IList<ChartData> pieChartdetails = new List<ChartData>();

            var dropdowns = cachingService.GetDropdownValues(request.AccountId);
            var leadSources = dropdowns.Where(s => s.DropdownID == (byte)DropdownFieldTypes.LeadSources).Select(s => s.DropdownValuesList).FirstOrDefault();
            foreach (int dropdownValue in dropdownValueKeys)
            {
                var leadSource = leadSources.Where(dv => dv.DropdownValueID == (short)dropdownValue).FirstOrDefault();
                if (leadSource != null)
                {
                    ChartData chartData = new ChartData();
                    chartData.Name = leadSource.DropdownValue;
                    chartData.DropdownValueID = (short)dropdownValue;
                    chartData.Value = pieChartData[dropdownValue];
                    pieChartdetails.Add(chartData);
                }
            }

            response.TopFive = pieChartdetails.OrderByDescending(c => c.Value).Take(5).ToList();

            request.ReportViewModel.CustomStartDate = request.ReportViewModel.CustomStartDatePrev;
            request.ReportViewModel.CustomEndDate = request.ReportViewModel.CustomEndDatePrev;

            searchdefinition = GetSearchDefination(request, out parameters, 0);

            var prevAreaChartData = searchService.GetContactsAggregationByDate(searchdefinition, parameters);

            response.TopPreviousLeads = GetAreaChartData(prevAreaChartData, request.ReportViewModel.CustomStartDate, request.ReportViewModel.CustomEndDate);
            return response;

        }

        public StandardReportResponse GetUntouchedLeadsVisualizationAsync(StandardReportRequest request)
        {
            StandardReportResponse response = new StandardReportResponse();

            SearchParameters parameters = new SearchParameters();

            request.Types = new List<Type>() { typeof(Person), typeof(Contact) };
            SearchDefinition searchdefinition = GetSearchDefination(request, out parameters, 1);

            var fielterViewModel = Mapper.Map<SearchDefinition, AdvancedSearchViewModel>(searchdefinition);

            response.AdvancedSearchViewModel = fielterViewModel;

            var areaChartData = searchService.GetContactsAggregationByDate(searchdefinition, parameters);

            response.TopLeads = GetAreaChartData(areaChartData, request.ReportViewModel.CustomStartDate, request.ReportViewModel.CustomEndDate);

            //  List<int> dropdownValueKeys = new List<int>(pieChartData.Keys);
            // IList<ChartData> pieChartdetails = new List<ChartData>();

            // var dropdowns = cachingService.GetDropdownValues(request.AccountId);

            //foreach (int dropdownValue in dropdownValueKeys)
            //{
            //    ChartData chartData = new ChartData();
            //    chartData.Name = dropdowns.Where(s => s.DropdownID == (byte)DropdownFieldTypes.LeadSources).Select(s => s.DropdownValuesList).ToList().
            //       SingleOrDefault().Where(dv => dv.DropdownValueID == (short)dropdownValue).SingleOrDefault().DropdownValue;
            //    chartData.DropdownValueID = (short)dropdownValue;
            //    chartData.Value = pieChartData[dropdownValue];
            //    pieChartdetails.Add(chartData);
            //}

            //response.TopFive = pieChartdetails.OrderByDescending(c => c.Value).Take(5).ToList();

            request.ReportViewModel.CustomStartDate = request.ReportViewModel.CustomStartDatePrev;
            request.ReportViewModel.CustomEndDate = request.ReportViewModel.CustomEndDatePrev;

            searchdefinition = GetSearchDefination(request, out parameters, 1);

            var prevAreaChartData = searchService.GetContactsAggregationByDate(searchdefinition, parameters);
            response.TopPreviousLeads = GetAreaChartData(prevAreaChartData, request.ReportViewModel.CustomStartDate, request.ReportViewModel.CustomEndDate);
            return response;

        }

        private List<ChartData> GetUnTouchedAreaChartData(IDictionary<DateTime, long> chartdata, DateTime customStartDate, DateTime customEndDate)
        {
            List<ChartData> areaChartDetails = new List<ChartData>();
            if (chartdata != null && chartdata.Any())
            {
                IDictionary<DateTime, long> areaChartData = new Dictionary<DateTime, long>();
                long untouchedInCurrentMonth = 0;
                foreach (var data in chartdata)
                {
                    if (data.Key.Date.Ticks > customStartDate.Date.Ticks && data.Key.Date.Ticks < customEndDate.Date.Ticks)
                        areaChartData.Add(data.Key.Date, data.Value);
                    else
                        untouchedInCurrentMonth += data.Value;
                }
                areaChartData.Add(customStartDate.Date, untouchedInCurrentMonth);
              

                IEnumerable<DateTime> dateTimes = areaChartData.Keys.OrderByDescending(o => o.Date);
                var startDate = dateTimes.FirstOrDefault();
                var endDate = dateTimes.LastOrDefault();
                int days = (startDate - endDate).Days + 1;        //((startDate.Year - endDate.Year) * 12) + startDate.Month - endDate.Month;
                var dateRange = Enumerable.Range(0, days)
               .Select(i => endDate.AddDays(i))
                .ToList();

                List<DateTime> dateKeys = new List<DateTime>(areaChartData.Keys.Select(s => s.Date));
                foreach (DateTime dateValue in dateRange)
                {
                    ChartData chartData = new ChartData();
                    chartData.Name = dateValue.Date.ToString();
                    if (dateKeys.Contains(dateValue.Date))
                        chartData.Value = areaChartData[dateValue.Date];
                    else
                        chartData.Value = 0;
                    chartData.PreviousValue = 0;
                    areaChartDetails.Add(chartData);
                }
            }
            return areaChartDetails;
        }

        public async Task<StandardReportResponse> GetNewLeadsListAsync(StandardReportRequest request)
        {
            StandardReportResponse response = new StandardReportResponse();
            request.Types = new List<Type>() { typeof(Person), typeof(Contact) };
            var dropdowns = cachingService.GetDropdownValues(request.AccountId);

            var lifecycleStages = dropdowns.Where(s => s.DropdownID == (byte)DropdownFieldTypes.LifeCycle).
                      Select(s => s.DropdownValuesList).ToList().SingleOrDefault();

            var leadSources = dropdowns.Where(s => s.DropdownID == (byte)DropdownFieldTypes.LeadSources).Select(s => s.DropdownValuesList).ToList().FirstOrDefault();

            SearchParameters parameters = new SearchParameters();
            SearchDefinition searchDefinition = GetSearchDefination(request, out parameters, 0);
            searchDefinition.IsAggregationNeeded = true;    //Aggregation is needed to apply or filter between selected users 
            var searchResults = await searchResultsAsync(searchDefinition, parameters);

            var fielterViewModel = Mapper.Map<SearchDefinition, AdvancedSearchViewModel>(searchDefinition);

            response.AdvancedSearchViewModel = fielterViewModel;
            response.TotalHits = searchResults.TotalHits;

            IEnumerable<int?> OwnerIds = searchResults.Results.Select(s => s.OwnerId);
            IEnumerable<Owner> Owners = contactRepository.GetUserNames(OwnerIds);
            foreach (var contact in searchResults.Results)
            {
                if (contact.LifecycleStage != null)
                    contact.LifecycleName = lifecycleStages.Where(e => e.DropdownValueID == contact.LifecycleStage).Select(s => s.DropdownValue).FirstOrDefault();

                if (contact.OwnerId != null)
                    contact.OwnerName = Owners.Where(o => o.OwnerId == contact.OwnerId).Select(s => s.OwnerName).FirstOrDefault();
                else
                    contact.OwnerName = "";

                if (contact.AllLeadSources != null)
                    contact.LeadSources = string.Join(", ", leadSources.Where(e => contact.AllLeadSources.Contains(e.DropdownValueID)).Select(s => s.DropdownValue));
                else
                    contact.LeadSources = "";
            }

            response.ReportList = searchResults.Results;

            if (response.ContactSearchResult == null)
                GetContactsNotFoundException();
            return response;
        }

        private List<ChartData> GetAreaChartData(IDictionary<DateTime, long> chartdata, DateTime customStartDate, DateTime customEndDate)
        {

            var startDate = customStartDate.Date;
            var endDate = customEndDate.Date;
            int days = (endDate - startDate).Days + 1; // incl. endDate 
            var dateRange = Enumerable.Range(0, days)
               .Select(i => startDate.AddDays(i))
             .ToList();
            List<DateTime> dateKeys = new List<DateTime>(chartdata.Keys);

            List<ChartData> areaChartDetails = new List<ChartData>();

            foreach (DateTime dateValue in dateRange)
            {
                ChartData chartData = new ChartData();
                chartData.Name = dateValue.Date.ToString();
                if (dateKeys.Contains(dateValue))
                    chartData.Value = chartdata[dateValue];
                else
                    chartData.Value = 0;
                chartData.PreviousValue = 0;
                areaChartDetails.Add(chartData);
            }
            return areaChartDetails;
        }
        #endregion

        #region FormsCountSummary
        public StandardReportResponse GetFormsCountSummaryReport(StandardReportRequest request)
        {
            Logger.Current.Verbose("Request to get data for FormsCountSummary Report");
            StandardReportResponse response = new StandardReportResponse();
            Logger.Current.Informational("userID : " + request.RequestedBy);

            var isAccountAdmin = this.IsAccountAdmin(request.IsSTadmin, request.RoleId, request.AccountId);
            var contactsList = reportRepository.GetFormsCountSummaryData(request.ReportViewModel.CustomStartDate, request.ReportViewModel.CustomEndDate,
              request.ReportViewModel.FormIds, request.ReportViewModel.LeadAdapterIds, request.ReportViewModel.GroupId, request.AccountId, request.UserID, isAccountAdmin);
            if (contactsList == null)
                response.Exception = GetCampaignNotFoundException();
            response.ReportList = contactsList;
            response.TotalHits = contactsList.Count();
            return response;
        }

        public GetReEngagementInfoResponse GetReEngagedContacts(GetReEngagementInfoRequest request)
        {
            GetReEngagementInfoResponse response = new GetReEngagementInfoResponse();
            Logger.Current.Verbose("In ReportService/GetReEngagedContacts");
            response.ContactIds = campaignRepository.GetReEngagedContacts(
                request.AccountId,
                request.CampaignId,
                request.StartDate,
                request.EndDate,
                request.IsDefaultDateRange,
                request.HasSelectedLinks,
                request.LinkIds,
                request.DrillDownPeriod
                ).ToList();
            return response;
        }

        public ReportDataResponse GetOnlineRegisteredContacts(ReportDataRequest request)
        {
            ReportDataResponse response = new ReportDataResponse();
            Logger.Current.Verbose("Request to get data for FormsCountSummary Contacts Data ");

            var isAccountAdmin = this.IsAccountAdmin(request.IsSTAdmin, request.RoleId, request.AccountId);
            response.ContactsData = reportRepository.GetOnlineRegisteredContacts(new ReportFilters
            {
                StartDate = request.ReportViewModel.CustomStartDate,
                EndDate = request.ReportViewModel.CustomEndDate,
                FormIDs = request.ReportViewModel.FormIds,
                LeadAdapterIDs = request.ReportViewModel.LeadAdapterIds,
                AccountId = request.AccountId,
                Type = request.ReportViewModel.GroupId,
                RowId = request.ReportViewModel.RowId,
                UserId = request.UserId,
                IsAdmin = isAccountAdmin
            });
            response.ContactIds = response.ContactsData.Select(c => c.contactID).ToList();
            return response;
        }
        #endregion

        #region BDXFreemiumCustomLeadReport
        public StandardReportResponse GetBDXFreemiumCustomLeadReportDetails(StandardReportRequest request)
        {
            Logger.Current.Verbose("Request to get data for FormsCountSummary Report");
            StandardReportResponse response = new StandardReportResponse();
            var pageNumber = request.ReportViewModel.PageNumber;
            if (request.ReportViewModel.PageNumber == 0)
                pageNumber = 1;
            var recordsLimit = (int)request.ReportViewModel.ShowTop;
            var records = (pageNumber - 1) * recordsLimit;
            var isAccountAdmin = this.IsAccountAdmin(request.IsSTadmin, request.RoleId, request.AccountId);
            IEnumerable<ReportContactInfo> contactsList = reportRepository.GetBDXFreemiumCustomLeadReportData(request.ReportViewModel.CustomStartDate,
                request.ReportViewModel.CustomEndDate, request.AccountId, isAccountAdmin, request.UserID);
            response.ContactIds = contactsList.Select(c => c.ContactID).ToList();
            response.TotalHits = contactsList.Count();
            contactsList = contactsList.Skip(records).Take(recordsLimit);
            response.ReportContacts = Mapper.Map<IEnumerable<ReportContactInfo>, IEnumerable<ContactReportEntry>>(contactsList);
            Logger.Current.Error("getting FormsCountSummary data :");
            return response;
        }
        #endregion

        private UnsupportedOperationException GetCampaignNotFoundException()
        {
            return new UnsupportedOperationException("The requested campaign was not found.");
        }

        private UnsupportedOperationException GetContactsNotFoundException()
        {
            return new UnsupportedOperationException("The requested Contacts are not found.");
        }

        public GetReportsResponse GetReportList(GetReportsRequest request)
        {
            GetReportsResponse response = new GetReportsResponse();
            ReportListViewModel viewmodel = new ReportListViewModel();
            Logger.Current.Informational(" fetching the ReportList.");
            if (request.SortField != null)
            {
                var maps = SmartTouch.CRM.ApplicationServices.ObjectMappers.MapperConfigurationProvider.Instance.FindTypeMapFor<ReportListEntry, Report>();

                foreach (var propertyMap in maps.GetPropertyMaps())
                {
                    if (request.SortField.Equals(propertyMap.SourceMember.Name))
                    {
                        request.SortField = propertyMap.DestinationProperty.MemberInfo.Name;
                        break;
                    }
                }
            }
            int totals = default(int);
            if (request.RequestedBy.HasValue)
            {
                var moduleIds = cachingService.AddAccountPermissions(request.AccountId);
                var reportsList = reportRepository.FindAll(request.Name, request.AccountId, request.RequestedBy.Value, request.PageNumber
                    , request.pageSize, out totals, request.SortField, request.Filter, moduleIds, request.SortDirection);
                viewmodel.Reports = Mapper.Map<IEnumerable<Report>, IEnumerable<ReportListEntry>>(reportsList);
            }
            response.TotalRecordsCount = totals;
            response.ReportListViewModel = viewmodel;

            return response;
        }

        public GetReportsResponse GetCustomReportList(GetReportsRequest request)
        {
            GetReportsResponse response = new GetReportsResponse();
            ReportListViewModel viewmodel = new ReportListViewModel();
            Logger.Current.Informational(" fetching the ReportList.");
            if (request.SortField != null)
            {
                var maps = SmartTouch.CRM.ApplicationServices.ObjectMappers.MapperConfigurationProvider.Instance.FindTypeMapFor<ReportListEntry, Report>();

                foreach (var propertyMap in maps.GetPropertyMaps())
                {
                    if (request.SortField.Equals(propertyMap.SourceMember.Name))
                    {
                        request.SortField = propertyMap.DestinationProperty.MemberInfo.Name;
                        break;
                    }
                }
            }
            int totals = default(int);
            if (request.RequestedBy.HasValue)
            {
                var reportsList = reportRepository.FindAllCustomReports(request.Name, request.AccountId, request.RequestedBy.Value, request.PageNumber, request.pageSize, out totals, request.SortField, request.SortDirection);
                viewmodel.Reports = Mapper.Map<IEnumerable<Report>, IEnumerable<ReportListEntry>>(reportsList);
            }
            response.TotalRecordsCount = totals;
            response.ReportListViewModel = viewmodel;

            foreach (var report in viewmodel.Reports)
            {
                report.ReportType = (byte)Reports.Custom;
            }
            return response;
        }

        public GetReportsResponse GetReportByType(GetReportsRequest request)
        {
            GetReportsResponse response = new GetReportsResponse();
            Logger.Current.Informational("Fetching the Report By Report Type. " + request.ReportType);
            response.Report = Mapper.Map<Report, ReportListEntry>(reportRepository.FindReportByType(request.ReportType, request.AccountId));
            return response;
        }

        public InsertDefaultReportsResponse InsertDefaultReports(InsertDefaultReportsRequest request)
        {
            InsertDefaultReportsResponse response = new InsertDefaultReportsResponse();
            Logger.Current.Informational(" inserting default reports for the new account ");
            reportRepository.InsertDefaultReports(request.AccountId);
            return response;
        }

        public InsertLastRunActivityResponse UpdateLastRunActivity(InsertLastRunActivityRequest request)
        {
            InsertLastRunActivityResponse response = new InsertLastRunActivityResponse();
            Logger.Current.Informational(" updating last run activity of report");
            if (request.RequestedBy.HasValue)
                reportRepository.UpdateLastRunActivity(request.ReportId, request.RequestedBy.Value, request.AccountId, request.ReportName);

            return response;
        }

        public GetUsersResponse GetUsers(GetUsersRequest request)
        {
            GetUsersResponse response = new GetUsersResponse();
            IEnumerable<Owner> owner;
            owner = contactRepository.GetUsers(request.AccountID, request.UserId, request.IsSTadmin).ToList();
            if (owner == null)
                throw new UnsupportedOperationException("[|The requested users list was not found.|]");
            response.Owner = owner;
            return response;
        }

        public InsertUserDashboardSettingsResponse InsertUserDashboardSettings(InsertUserDashboardSettingsRequest request)
        {
            InsertUserDashboardSettingsResponse response = new InsertUserDashboardSettingsResponse();
            var dashboardViewModel = request.DashboardViewModel;
            if (dashboardViewModel != null)
            {
                List<byte> dashboardIds = request.DashboardViewModel.Where(s => s.Value == true).Select(ds => ds.Id).ToList();
                reportRepository.InsertUserDashboardSettings(request.UserId, dashboardIds);
                return response;
            }
            return response;
        }

        public GetDashboardItemsResponse GetDashboardItems(int userId)
        {
            GetDashboardItemsResponse response = new GetDashboardItemsResponse();
            IEnumerable<dynamic> dashboardSettingViewModel;
            dashboardSettingViewModel = reportRepository.GetDashboardItems(userId);
            if (dashboardSettingViewModel == null)
                throw new ResourceNotFoundException("[|The requested condition list was not found.|]");
            response.DashboardSettingViewModel = dashboardSettingViewModel;
            return response;
        }

        public CustomReportDataResponse GetCustomReports(CustomReportDataRequest request)
        {
            CustomReportDataResponse response = new CustomReportDataResponse();
            response.hasCustomReports = reportRepository.GetCustomReports(request.AccountId);
            return response;
        }

        private bool IsAccountAdmin(bool isSTadmin, short roleId, int accountId)
        {
            var isAccountAdmin = false;

            isAccountAdmin = isSTadmin ? true : cachingService.IsAccountAdmin(roleId, accountId);

            if (isAccountAdmin == false)
            {
                isAccountAdmin = (cachingService.IsModulePrivate(AppModules.Contacts, accountId) == false) ? true : false;
            }
            return isAccountAdmin;
        }

        #region BDXCustomLeadReport
        public BDXCustomLeadReportResponse GetBDXCustomLeadReportDetails(BDXCustomLeadReportRequest request)
        {
            Logger.Current.Verbose("Request to get data for BDX Custom Lead Report");
            BDXCustomLeadReportResponse response = new BDXCustomLeadReportResponse();
            
            var isAccountAdmin = this.IsAccountAdmin(request.IsSTadmin, request.RoleId, request.AccountId);

            IEnumerable<BDXCustomLeadContactInfo> bdxCustomLeadData = reportRepository.GetBDXCustomLeadReportContactInfo(request.ReportViewModel.CustomStartDate,
                                                                                        request.ReportViewModel.CustomEndDate, request.AccountId, isAccountAdmin, request.UserID);
            response.ContactIds = bdxCustomLeadData.Select(c => c.ContactID);
            response.TotalHits = bdxCustomLeadData.Count();
            response.Contacts = Mapper.Map<IEnumerable<BDXCustomLeadContactInfo>, IEnumerable<BDXLeadReportEntry>>(bdxCustomLeadData);
            return response;
        }

        public BDXCustomLeadReportResponse GetBDXCustomLeadReportDetailsExport(BDXCustomLeadReportRequest request)
        {


            //     public DataTable GetBDXCustomLeadReportContactExcelInfo(DateTime startDate, DateTime endDate, int accountId, bool isAdmin, int userID)
            //{
            //    var procedureName = "[dbo].[Get_Account_BDX_Custom_Lead_Report]";
            //    var parms = new List<SqlParameter>
            //        {   
            //            new SqlParameter{ParameterName ="@AccountID", Value= accountId},
            //            new SqlParameter{ParameterName ="@FromDate", Value= startDate.Date},
            //            new SqlParameter{ParameterName="@ToDate ", Value = endDate.Date},
            //            new SqlParameter{ParameterName="@IsAdmin ", Value = isAdmin},
            //            new SqlParameter{ParameterName="@OwnerID ", Value = userID}
            //        };
            //    var db = ObjectContextFactory.Create();
            //    //IEnumerable<BDXCustomLeadContactInfo> data = db.ExecuteStoredProcedure<BDXCustomLeadContactInfo>(procedureName, parms);

            //    db.Database.Connection.Open();
            //    var cmd = db.Database.Connection.CreateCommand();
            //    //   cmd.CommandTimeout = 360;
            //    cmd.CommandText = procedureName;
            //    cmd.CommandType = CommandType.StoredProcedure;
            //    List<SqlParameter> parameters = parms;
            //    parameters.ForEach(p =>
            //    {
            //        cmd.Parameters.Add(p);
            //    });

            //    DbDataReader reader = cmd.ExecuteReader();

            //    DataTable table = new DataTable();
            //    table.Load(reader);



            //    return table;
            //}


            //    var pageNumber = request.ReportViewModel.PageNumber;
            //    if (request.ReportViewModel.PageNumber == 0)
            //        pageNumber = 1;
            //    var recordsLimit = (int)request.ReportViewModel.ShowTop;
            //    var records = (pageNumber - 1) * recordsLimit;
            //    var isAccountAdmin = false;
            //    if (request.IsSTadmin == true)
            //        isAccountAdmin = true;
            //    else
            //        isAccountAdmin = cachingService.IsAccountAdmin(request.RoleId, request.AccountId);
            //    if (isAccountAdmin == false)
            //    {
            //        bool isPrivate = cachingService.IsModulePrivate(AppModules.Contacts, request.AccountId);
            //        if (isPrivate == false)
            //            isAccountAdmin = true;
            //    }

            //    DataTable table = reportRepository.GetBDXCustomLeadReportContactExcelInfo(request.ReportViewModel.CustomStartDate,
            //                                                                                request.ReportViewModel.CustomEndDate, request.AccountId, isAccountAdmin, request.UserID);

            //    ReadExcel exl = new ReadExcel();
            //    byte[] array = null;
            //    array = exl.ConvertDataSetToExcel(table);
            BDXCustomLeadReportResponse response = new BDXCustomLeadReportResponse();

            return response;

        }

        #endregion

        public GetReportNameByTypeResponse GetReportNameByType(GetReportNameByTypeRequest request)
        {
            GetReportNameByTypeResponse response = new GetReportNameByTypeResponse();
            response.ReportName = reportRepository.GetReportNameByType(request.AccountId, request.ReportType);
            return response;
        }

        #region For Database Life Cycle Report
        public ReportDataResponse GetDatabaseLifeCycleData(ReportDataRequest request)
        {
            ReportDataResponse response = new ReportDataResponse();

            response.ReportData = reportRepository.GetAllDatabaseLifeCycleData(new ReportFilters
            {
                AccountId = request.AccountId,
                DateRange = request.ReportViewModel.DateRange,
                StartDate = request.ReportViewModel.CustomStartDate,
                EndDate = request.ReportViewModel.CustomEndDate,
                TrafficLifeCycle = request.ReportViewModel.LifeStageIds,
                SelectedOwners = request.ReportViewModel.OwnerIds,
                IsAdmin = request.IsSTAdmin
            });

            return response;

        }

        public ReportDataResponse AllDatabaseReportContacts(ReportDataRequest request)
        {
            ReportDataResponse response = new ReportDataResponse();

            response.ContactsData = reportRepository.GetAllDatabaseLifeCycleReportContacts (new ReportFilters
            {
                AccountId = request.AccountId,
                StartDate = request.ReportViewModel.CustomStartDate,
                EndDate = request.ReportViewModel.CustomEndDate,
                SelectedOwners = request.ReportViewModel.OwnerIds,
                TrafficLifeCycle = request.ReportViewModel.LifeStageIds,
                IsAdmin = request.IsSTAdmin
            });
            response.ContactIds = response.ContactsData.Select(p => p.contactID).ToList();
            return response;
        }

        public TourByContactsReponse GetTourByContactsReportData(TourByContactsRequest request)
        {
            TourByContactsReponse response = new TourByContactsReponse();
            IEnumerable<TourByContactReportInfo> tourByContactsInfo = reportRepository.GetTourByContactsReportData(request.AccountId, request.FromDate, request.ToDate, request.TourStatus, request.TourType, request.TourCommunity, request.pageSize, request.pageNumber,request.SortField,request.SortDirection);
            IEnumerable<TourByContactsViewModel> tourByContactsViewModel = Mapper.Map<IEnumerable<TourByContactReportInfo>, IEnumerable<TourByContactsViewModel>>(tourByContactsInfo);
            response.TourByContactsReportData = tourByContactsViewModel;
            return response;
        }

        #endregion
    }
}

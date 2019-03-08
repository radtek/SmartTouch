using AutoMapper;
using LandmarkIT.Enterprise.Utilities.Logging;
using SmartTouch.CRM.ApplicationServices.Exceptions;
using SmartTouch.CRM.ApplicationServices.Messaging.LeadScore;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.Campaigns;
using SmartTouch.CRM.Domain.LeadScoreRules;
using SmartTouch.CRM.Domain.Tags;
using SmartTouch.CRM.Infrastructure.Domain;
using SmartTouch.CRM.Domain.Forms;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using SmartTouch.CRM.SearchEngine.Indexing;
using System;
using SmartTouch.CRM.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SmartTouch.CRM.Domain.ValueObjects;
using LandmarkIT.Enterprise.Utilities.ExceptionHandling;
using SmartTouch.CRM.ApplicationServices.Messaging.Tags;
using System.Threading;
using LandmarkIT.Enterprise.Extensions;

namespace SmartTouch.CRM.ApplicationServices.ServiceImplementations
{
    public class LeadScoreRuleService : ILeadScoreRuleService
    {
        readonly ILeadScoreRuleRepository leadScoreRuleRepository;
        readonly ITagRepository tagRepository;
        readonly ICampaignRepository campaignRepository;
        readonly IFormRepository formRepository;
        readonly ICachingService cachingService;
        readonly IUnitOfWork unitOfWork;
        readonly ITagService tagService;
        IIndexingService indexingService;

        public LeadScoreRuleService(ILeadScoreRuleRepository leadScoreRuleRepository,
            ITagRepository tagRepository, ICampaignRepository campaignRepository, ICachingService cachingService,
            IFormRepository formRepository, IUnitOfWork unitOfWork, ITagService tagService, IIndexingService indexingService)
        {
            if (leadScoreRuleRepository == null) throw new ArgumentNullException("leadScoreRoleRepository");
            if (unitOfWork == null) throw new ArgumentNullException("unitOfWork");

            this.leadScoreRuleRepository = leadScoreRuleRepository;
            this.tagRepository = tagRepository;
            this.campaignRepository = campaignRepository;
            this.formRepository = formRepository;
            this.cachingService = cachingService;
            this.indexingService = indexingService;
            this.tagService = tagService;
            this.unitOfWork = unitOfWork;
        }

        public InsertLeadScoreRuleResponse CreateRule(InsertLeadScoreRuleRequest request)
        {
            Logger.Current.Verbose("Request for inserting LeadScore");
            LeadScoreRuleViewModel ruleVM = request.LeadScoreRuleViewModel;
            LeadScoreRule leadScoreRule = Mapper.Map<LeadScoreRuleViewModel, LeadScoreRule>(ruleVM);
            isLeadScoreValid(leadScoreRule);
            bool isDuplicate = leadScoreRuleRepository.IsDuplicate(leadScoreRule);
            if (isDuplicate)
                throw new UnsupportedOperationException("[|Rule already exists.|]");

            if ((leadScoreRule.ConditionID == LeadScoreConditionType.ContactNoteTagAdded || leadScoreRule.ConditionID == LeadScoreConditionType.ContactActionTagAdded) && leadScoreRule.Tags != null)
            {
                List<int> TagIDs = new List<int>();
                var newTags = ruleVM.TagsList.Where(i => i.TagID == 0);
                var alreadycreatedtags = ruleVM.TagsList.Where(i => i.TagID != 0);
                TagIDs.AddRange(alreadycreatedtags.Select(x => x.TagID));
                foreach (TagViewModel tag in newTags)
                {
                    var insertedtag = tagService.SaveTag(new SaveTagRequest()
                    {
                        TagViewModel = tag
                    }).TagViewModel;
                    TagIDs.Add(insertedtag.TagID);
                }
                leadScoreRule.SelectedTagID = TagIDs.ToArray();
            }

            int MaxLeadScoreID = leadScoreRuleRepository.GetMaxLeadScoreRuleID();
            MaxLeadScoreID++;
            leadScoreRuleRepository.InsertLeadScoreRules(leadScoreRule, MaxLeadScoreID);

            if ((leadScoreRule.ConditionID == LeadScoreConditionType.ContactActionTagAdded
                || leadScoreRule.ConditionID == LeadScoreConditionType.ContactNoteTagAdded)
                && leadScoreRule.SelectedTagID != null && leadScoreRule.SelectedTagID.Any())
            {
                IEnumerable<Tag> tags = tagRepository.FindByIDs(leadScoreRule.SelectedTagID);
                ReIndexLeadScoreTags(tags);
            }
            return new InsertLeadScoreRuleResponse();
        }

        private void isLeadScoreValid(LeadScoreRule leadScore)
        {
            IEnumerable<BusinessRule> brokenRules = leadScore.GetBrokenRules();
            Logger.Current.Informational("Broken rules count : " + brokenRules.Count());
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

        private ResourceNotFoundException GetContactNotFoundException()
        {
            return new ResourceNotFoundException("[|The requested leadscore was not found.|]");
        }

        public GetLeadScoreListResponse GetLeadScoresList(GetLeadScoreListRequest request)
        {
            GetLeadScoreListResponse response = new GetLeadScoreListResponse();
            List<byte> Conditions = leadScoreRuleRepository.GetConditionsFromModules(request.Modules);

            Conditions.Add((byte)LeadScoreConditionType.ContactVisitsWebsite);
            Conditions.Add((byte)LeadScoreConditionType.ContactVisitsWebPage);
            Conditions.Add((byte)LeadScoreConditionType.PageDuration);
            IEnumerable<LeadScoreRule> leadScoreRules = leadScoreRuleRepository.FindAll(request.Query, request.Limit, request.PageNumber, request.AccountID, request.SortField, request.SortDirection, Conditions);

            if (leadScoreRules == null)
            {
                response.Exception = GetLeadScoreNotFoundException();
            }
            else
            {
                IEnumerable<LeadScoreRuleViewModel> leadScoreList = Mapper.Map<IEnumerable<LeadScoreRule>, IEnumerable<LeadScoreRuleViewModel>>(leadScoreRules);
                response.LeadScoreViewModel = leadScoreList;
                response.TotalHits = leadScoreRuleRepository.LeadScoreRulesCount(request.Query, request.AccountID, Conditions);
            }
            return response;
        }

        public GetLeadScoreRuleByConditionResponse GetLeadScoreRule(GetLeadScoreRuleByConditionRequest request)
        {
            var leadScoreRule = leadScoreRuleRepository.GetLeadScoreRuleByCondition(
                (int)request.Condition, request.ConditionValue, request.AccountId);
            return new GetLeadScoreRuleByConditionResponse() { Rule = leadScoreRule };
        }

        public GetLeadScoreResponse GetLeadScoreRule(GetLeadScoreRequest request)
        {
            GetLeadScoreResponse response = new GetLeadScoreResponse();

            LeadScoreRule leadScoreRule = leadScoreRuleRepository.FindBy(request.Id);
            if (leadScoreRule == null)
            {
                response.Exception = GetLeadScoreNotFoundException();
            }
            else
            {
                LeadScoreRuleViewModel leadScoreRuleViewModel = Mapper.Map<LeadScoreRule, LeadScoreRuleViewModel>(leadScoreRule);
                if ((leadScoreRuleViewModel.ConditionID == LeadScoreConditionType.ContactActionTagAdded || leadScoreRuleViewModel.ConditionID == LeadScoreConditionType.ContactNoteTagAdded
                    || leadScoreRuleViewModel.ConditionID == LeadScoreConditionType.ContactTagAdded) && leadScoreRuleViewModel.SelectedTagID != null)
                {
                    IEnumerable<Tag> Tags = leadScoreRuleRepository.GetTags(leadScoreRuleViewModel.SelectedTagID);
                    IEnumerable<TagViewModel> TagData = Mapper.Map<IEnumerable<Tag>, IEnumerable<TagViewModel>>(Tags);
                    foreach (var tag in TagData)
                    {
                        tag.TagName += " *";
                    }
                    leadScoreRuleViewModel.TagsList = TagData;
                }
                response.LeadScoreViewModel = leadScoreRuleViewModel;
            }
            return response;
        }

        private Exception GetLeadScoreNotFoundException()
        {
            return new ResourceNotFoundException("[|The requested note was not found.|]");
        }

        public UpdateLeadScoreRuleResponse UpdateRule(UpdateLeadScoreRuleRequest request)
        {
            LeadScoreRuleViewModel ruleVM = request.LeadScoreRuleViewModel;
            LeadScoreRule leadScoreRule = Mapper.Map<LeadScoreRuleViewModel, LeadScoreRule>(ruleVM);
            isLeadScoreValid(leadScoreRule);
            bool isDuplicate = leadScoreRuleRepository.IsDuplicate(leadScoreRule);
            if (isDuplicate)
                throw new UnsupportedOperationException("[|Rule already exists.|]");


            if ((leadScoreRule.ConditionID == LeadScoreConditionType.ContactNoteTagAdded
                || leadScoreRule.ConditionID == LeadScoreConditionType.ContactActionTagAdded)
                && leadScoreRule.Tags != null)
            {
                List<int> TagIDs = new List<int>();
                var newTags = ruleVM.TagsList.Where(i => i.TagID == 0);
                var alreadycreatedtags = ruleVM.TagsList.Where(i => i.TagID != 0);
                TagIDs.AddRange(alreadycreatedtags.Select(x => x.TagID));
                foreach (TagViewModel tag in newTags)
                {
                    var insertedtag = tagService.SaveTag(new SaveTagRequest()
                    {
                        TagViewModel = tag
                    }).TagViewModel;
                    TagIDs.Add(insertedtag.TagID);
                }
                leadScoreRule.SelectedTagID = TagIDs.ToArray();
            }

            leadScoreRuleRepository.UpdateLeadScoreRules(leadScoreRule);

            if ((leadScoreRule.ConditionID == LeadScoreConditionType.ContactActionTagAdded
              || leadScoreRule.ConditionID == LeadScoreConditionType.ContactNoteTagAdded))
            {
                Thread t2 = new Thread(delegate()
                {
                    UpdateLeadScoreTags(request.AccountId);
                });
                t2.Start();
            }
            return new UpdateLeadScoreRuleResponse();
        }

        private void UpdateLeadScoreTags(int AccountID)
        {
            int[] LeadScoreTagIDs = leadScoreRuleRepository.GetLeadScoreTagIDs(AccountID);
            IEnumerable<Tag> tags = tagRepository.FindByIDs(LeadScoreTagIDs);
            ReIndexLeadScoreTags(tags);
        }

        public GetCategoriesResponse GetCategories(GetCategoriesRequest request)
        {
            GetCategoriesResponse response = new GetCategoriesResponse();
            IEnumerable<ScoreCategories> categories;
            IList<ScoreCategories> permissionCategories = new List<ScoreCategories>();


            categories = leadScoreRuleRepository.GetScoreCategories().ToList();
            // permissions = cachingService.GetUserPermissions(request.accountId);
            var usersPermissions = cachingService.GetUserPermissions(request.accountId);
            IEnumerable<byte> userModules = usersPermissions.Where(s => s.RoleId == (short)request.RoleId).Select(s => s.ModuleId).ToList();
            foreach (ScoreCategories categorie in categories)
            {
                if (!categorie.ModuleID.HasValue)
                {
                    permissionCategories.Add(categorie);
                }
                else
                {
                    if (userModules.Contains(categorie.ModuleID.Value))
                        permissionCategories.Add(categorie);
                }

            }
            if (permissionCategories == null)
                throw new ResourceNotFoundException("[|The requested categories list was not found.|]");
            response.Categories = permissionCategories;
            return response;
        }

        public GetConditionsResponse GetConditions(GetConditionsRequest request)
        {
            GetConditionsResponse response = new GetConditionsResponse();
            IEnumerable<dynamic> conditions;

            conditions = leadScoreRuleRepository.GetConditions(request.Id);
            if (conditions == null)
                throw new ResourceNotFoundException("[|The requested condition list was not found.|]");
            response.Conditions = conditions;
            return response;
        }

        public LeadScoreDuplicareRuleCountResponse LeadScoreDuplicateRulesCount(LeadScoreDuplicareRuleCountRequest request)
        {
            throw new NotImplementedException();
        }

        public DeleteLeadScoreResponse UpdateLeadScoreStatus(DeleteLeadScoreRequest request)
        {
            Logger.Current.Verbose("Request for user change the status");
            leadScoreRuleRepository.DeactivateRules(request.RuleID);
            if (request.conditionId == (int)LeadScoreConditionType.ContactActionTagAdded
                || request.conditionId == (int)LeadScoreConditionType.ContactNoteTagAdded)
            {
                Thread t2 = new Thread(delegate()
                {
                    UpdateLeadScoreTags(request.accountID);
                });
                t2.Start();
            }
            return new DeleteLeadScoreResponse();
        }

        private void ReIndexLeadScoreTags(IEnumerable<Tag> Tags)
        {
            foreach (Tag tag in Tags)
            {
                //tag.TagNameAutoComplete.Output += " *";
                indexingService.IndexTag(tag);
            }
        }

        public DeleteLeadScoreResponse DeleteRule(DeleteLeadScoreRequest request)
        {
            throw new NotImplementedException();
        }

        private Exception GetCampaignNotFoundException()
        {
            return new ResourceNotFoundException("[|The requested note was not found.|]");
        }

        public GetCampaignsResponse GetCampaigns(Messaging.LeadScore.GetCampaignsRequest request)
        {
            GetCampaignsResponse response = new Messaging.LeadScore.GetCampaignsResponse();
            IEnumerable<Campaign> campaigns;

            campaigns = leadScoreRuleRepository.GetCampaigns(request.accountId).ToList();
            if (campaigns == null)
            {
                throw new ResourceNotFoundException("[|The requested campaigns list was not found.|]");
            }
            else
            {
                IEnumerable<CampaignEntryViewModel> campaignslist = Mapper.Map<IEnumerable<Campaign>, IEnumerable<CampaignEntryViewModel>>(campaigns);
                response.Campaigns = campaignslist;
            }
            return response;
        }

        public Messaging.LeadScore.GetFormResponse GetForms(Messaging.LeadScore.GetFormsRequest request)
        {
            Messaging.LeadScore.GetFormResponse response = new Messaging.LeadScore.GetFormResponse();
            IEnumerable<Form> forms;
            var isAccountAdmin = false;
            //checking the Forms data permissions
            if (request.IsSTAdmin == true)
                isAccountAdmin = true;
            else
                isAccountAdmin = cachingService.IsAccountAdmin(request.RoleId, request.AccountId);
            if (isAccountAdmin == false)
            {
                bool isPrivate = cachingService.IsModulePrivate(AppModules.Forms, request.AccountId);
                if (isPrivate == false)
                    isAccountAdmin = true;
            }

            if (isAccountAdmin == true)
                forms = leadScoreRuleRepository.GetForms(request.AccountId).ToList();
            else
                forms = leadScoreRuleRepository.GetForms(request.AccountId, request.RequestedBy).ToList();
            if (forms == null)
            {
                throw new ResourceNotFoundException("[|The requested forms list was not found.|]");
            }
            else
            {
                IEnumerable<FormEntryViewModel> formslist = Mapper.Map<IEnumerable<Form>, IEnumerable<FormEntryViewModel>>(forms);
                response.Forms = formslist;
            }
            return response;
        }


        public GetLeadScoreRuleByConditionResponse GetLeadScoreRules(GetLeadScoreRuleByConditionRequest request)
        {
            var response = new GetLeadScoreRuleByConditionResponse();

            if (request.Condition == LeadScoreConditionType.PageDuration)
            {
                response.Rules = leadScoreRuleRepository.GetPageDurationRules((int)request.Condition, request.ConditionValue, request.AccountId, request.EntityID);
                return response;
            }

            response.Rules = leadScoreRuleRepository.GetLeadScoreRulesByCondition((int)request.Condition, request.ConditionValue, request.AccountId);
            return response;
        }


        public GetLeadScoreRuleByConditionResponse GetCampaignClickLeadScoreRule(GetLeadScoreRuleByConditionRequest request)
        {
            var leadScoreRule = leadScoreRuleRepository.GetLeadScoreRulesForLinkClicked(request.ConditionValue, request.AccountId, request.EntityID);
            return new GetLeadScoreRuleByConditionResponse() { Rules = leadScoreRule };
        }

        public GetLeadScoreCategoriesResponse GetLeadScoreCategories(GetLeadScoreCategoriesRequest request)
        {
            GetLeadScoreCategoriesResponse response = new GetLeadScoreCategoriesResponse();
            response.Categories = leadScoreRuleRepository.GetLeadScoreCategeories();
            return response;
        }

        public GetLeadScoreConditionsResponse GetLeadScoreConditions(GetLeadScoreConditionsRequest request)
        {
            GetLeadScoreConditionsResponse response = new GetLeadScoreConditionsResponse();
            response.Conditions = leadScoreRuleRepository.GetLeadScoreConditions();
            return response;
        }
    }
}

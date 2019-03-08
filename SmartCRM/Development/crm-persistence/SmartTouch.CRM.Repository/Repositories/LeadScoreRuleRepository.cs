using AutoMapper;
using LinqKit;
using SmartTouch.CRM.Domain.Campaigns;
using SmartTouch.CRM.Domain.Forms;
using SmartTouch.CRM.Domain.LeadScoreRules;
using SmartTouch.CRM.Domain.Tags;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using SmartTouch.CRM.Repository.Database;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;

namespace SmartTouch.CRM.Repository.Repositories
{
    public class LeadScoreRuleRepository : Repository<LeadScoreRule, int, LeadScoreRulesDb>, ILeadScoreRuleRepository
    {
        public LeadScoreRuleRepository(IUnitOfWork unitOfWork, IObjectContextFactory objectContextFactory)
            : base(unitOfWork, objectContextFactory) { }

        /// <summary>
        /// Finds all.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<LeadScoreRule> FindAll()
        {
            var db = ObjectContextFactory.Create();
            IEnumerable<LeadScoreRulesDb> leadScores = db.LeadScoreRules.Include(ls => ls.Condition).Where(ls => ls.IsActive == true).ToList();
            foreach (LeadScoreRulesDb ls in leadScores)
            {
                yield return Mapper.Map<LeadScoreRulesDb, LeadScoreRule>(ls);
            }
        }

        /// <summary>
        /// Finds all.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="limit">The limit.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="accountId">The account identifier.</param>
        /// <param name="sortField">The sort field.</param>
        /// <param name="direction">The direction.</param>
        /// <param name="Conditions">The conditions.</param>
        /// <returns></returns>
        public IEnumerable<LeadScoreRule> FindAll(string name, int limit, int pageNumber, int accountId, string sortField, ListSortDirection direction, IEnumerable<byte> Conditions)
        {
            var predicate = PredicateBuilder.True<LeadScoreRulesDb>();
            var records = (pageNumber - 1) * limit;
            if (!string.IsNullOrEmpty(name))
            {
                name = name.ToLower();
                predicate = predicate.And(a => a.ConditionDescription.Contains(name));
            }
            predicate = predicate.And(a => Conditions.Contains(a.ConditionID));
            predicate = predicate.And(a => a.IsActive == true);
            predicate = predicate.And(a => a.AccountID == accountId).And(a => a.ConditionID != 10);

            IEnumerable<LeadScoreRulesDb> leadScoreRules = findLeadScoresSummary(predicate).AsQueryable().OrderBy(sortField, direction).Skip(records).Take(limit);
            foreach (LeadScoreRulesDb dls in leadScoreRules)
            {
                yield return ConvertToDomain(dls);
            }
        }

        /// <summary>
        /// Finds the lead scores summary.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <returns></returns>
        IEnumerable<LeadScoreRulesDb> findLeadScoresSummary(System.Linq.Expressions.Expression<Func<LeadScoreRulesDb, bool>> predicate)
        {
            var db = ObjectContextFactory.Create();
            IEnumerable<LeadScoreRulesDb> leadScoreRules = db.LeadScoreRules.Include(x => x.Condition.ScoreCategory).AsExpandable()
                                                             .Where(predicate).AsEnumerable()
                                                             .GroupBy(z => z.LeadScoreRuleMapID)
                                                             .Select(x => new LeadScoreRulesDb
                                                             {
                                                                 LeadScoreRuleMapID = x.Key,
                                                                 ConditionDescription = x.FirstOrDefault().ConditionDescription,
                                                                 ConditionID = x.FirstOrDefault().ConditionID,
                                                                 Condition = x.FirstOrDefault().Condition,
                                                                 AccountID = x.FirstOrDefault().AccountID,
                                                                 AppliedToPreviousActions = x.FirstOrDefault().AppliedToPreviousActions,
                                                                 ConditionValue = x.FirstOrDefault().ConditionValue,
                                                                 CreatedBy = x.FirstOrDefault().CreatedBy,
                                                                 CreatedOn = x.FirstOrDefault().CreatedOn,
                                                                 LeadScoreRuleID = x.FirstOrDefault().LeadScoreRuleID,
                                                                 ModifiedBy = x.FirstOrDefault().ModifiedBy,
                                                                 ModifiedOn = x.FirstOrDefault().ModifiedOn,
                                                                 Score = x.FirstOrDefault().Score,
                                                                 SelectedCampaignLinks = x.FirstOrDefault().SelectedCampaignLinks
                                                             });
            return leadScoreRules;
        }

        /// <summary>
        /// Finds all.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        public IEnumerable<LeadScoreRule> FindAll(string name, int accountId)
        {
            var predicate = PredicateBuilder.True<LeadScoreRulesDb>();
            if (!string.IsNullOrEmpty(name))
            {
                name = name.ToLower();
                predicate = predicate.And(a => a.ConditionDescription.Contains(name));
            }
            predicate = predicate.And(a => a.IsActive == true);
            predicate = predicate.And(a => a.AccountID == accountId).And(a => a.ConditionID != 10);
            IEnumerable<LeadScoreRulesDb> leadScoreRules = findLeadScoresSummary(predicate);
            foreach (LeadScoreRulesDb dls in leadScoreRules)
            {
                yield return ConvertToDomain(dls);
            }
        }

        /// <summary>
        /// Finds the by.
        /// </summary>
        /// <param name="leadScoreId">The lead score identifier.</param>
        /// <returns></returns>
        public override LeadScoreRule FindBy(int leadScoreId)
        {
            using (var db = ObjectContextFactory.Create())
            {
                int temp;
                LeadScoreRulesDb leadScoreRuleDb = db.LeadScoreRules.Include(ls => ls.Condition.ScoreCategory).Where(x => x.LeadScoreRuleMapID == leadScoreId && x.IsActive == true)
                                                    .AsEnumerable()
                                                    .GroupBy(i => i.LeadScoreRuleMapID)
                                                    .Select(x => new LeadScoreRulesDb
                                                    {
                                                        AccountID = x.FirstOrDefault().AccountID,
                                                        AppliedToPreviousActions = x.FirstOrDefault().AppliedToPreviousActions,
                                                        Condition = x.FirstOrDefault().Condition,
                                                        ConditionDescription = x.FirstOrDefault().ConditionDescription,
                                                        ConditionID = x.FirstOrDefault().ConditionID,
                                                        ConditionValue = x.FirstOrDefault().ConditionValue,
                                                        CreatedBy = x.FirstOrDefault().CreatedBy,
                                                        CreatedOn = x.FirstOrDefault().CreatedOn,
                                                        LeadScoreRuleID = x.FirstOrDefault().LeadScoreRuleID,
                                                        LeadScoreRuleMapID = x.Key,
                                                        ModifiedBy = x.FirstOrDefault().ModifiedBy,
                                                        ModifiedOn = x.FirstOrDefault().ModifiedOn,
                                                        Score = x.FirstOrDefault().Score,
                                                        SelectedCampaignLinks = x.FirstOrDefault().SelectedCampaignLinks,
                                                        IsActive = x.FirstOrDefault().IsActive,
                                                        SelectedIDs = x.Select(g => int.TryParse(g.ConditionValue, out temp) ? temp : 0).ToArray()

                                                        //SelectedIDs = x.FirstOrDefault().ConditionID x.Select(g=> int.Parse(g.ConditionValue)).ToArray()
                                                    }).FirstOrDefault();
                var leadScoreConditionValues = db.LeadScoreConditionValues.Where(c => c.LeadScoreRuleID == leadScoreId).ToList();
                var leadScoreRule = Mapper.Map<LeadScoreRulesDb, LeadScoreRule>(leadScoreRuleDb);
                leadScoreRule.LeadScoreConditionValues = Mapper.Map<IEnumerable<LeadScoreConditionValuesDb>, IEnumerable<LeadScoreConditionValue>>(leadScoreConditionValues);
                return leadScoreRule;
            }
        }

        /// <summary>
        /// Gets the lead score rule by condition.
        /// </summary>
        /// <param name="conditionId">The condition identifier.</param>
        /// <param name="conditionValue">The condition value.</param>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        public LeadScoreRule GetLeadScoreRuleByCondition(int conditionId, string conditionValue, int accountId)
        {
            var db = ObjectContextFactory.Create();
            var leadScoreRuleDb = db.LeadScoreRules.FirstOrDefault(l => l.ConditionID == conditionId
                && l.AccountID == accountId && l.ConditionValue == conditionValue && l.IsActive == true);

            return Mapper.Map<LeadScoreRulesDb, LeadScoreRule>(leadScoreRuleDb);
        }

        /// <summary>
        /// Converts the type of to database.
        /// </summary>
        /// <param name="leadScoreRule">The lead score rule.</param>
        /// <param name="db">The database.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">Invalid leadscore id has been passed. Suspected Id forgery.</exception>
        public override LeadScoreRulesDb ConvertToDatabaseType(LeadScoreRule leadScoreRule, CRMDb db)
        {
            LeadScoreRulesDb leadScoreRuleDb;
            if (leadScoreRule.Id > 0)
            {
                leadScoreRuleDb = db.LeadScoreRules.SingleOrDefault(c => c.LeadScoreRuleID == leadScoreRule.Id);
                if (leadScoreRuleDb == null)
                    throw new ArgumentException("Invalid leadscore id has been passed. Suspected Id forgery.");
                leadScoreRuleDb = Mapper.Map<LeadScoreRule, LeadScoreRulesDb>(leadScoreRule, leadScoreRuleDb);
            }
            else
            {
                leadScoreRuleDb = Mapper.Map<LeadScoreRule, LeadScoreRulesDb>(leadScoreRule);
            }
            return leadScoreRuleDb;
        }

        /// <summary>
        /// Converts to domain.
        /// </summary>
        /// <param name="leadScoreDb">The lead score database.</param>
        /// <returns></returns>
        public override LeadScoreRule ConvertToDomain(LeadScoreRulesDb leadScoreDb)
        {
            return Mapper.Map<LeadScoreRulesDb, LeadScoreRule>(leadScoreDb);
        }

        /// <summary>
        /// Persists the value objects.
        /// </summary>
        /// <param name="leadScore">The lead score.</param>
        /// <param name="leadScoreDb">The lead score database.</param>
        /// <param name="db">The database.</param>
        public override void PersistValueObjects(LeadScoreRule leadScore, LeadScoreRulesDb leadScoreDb, CRMDb db)
        {
            //for future use
            //persist(domainType, dbType, db);
        }

        /// <summary>
        /// Deactivates the rule.
        /// </summary>
        /// <param name="contactId">The contact identifier.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public bool DeactivateRule(int contactId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Deletes the rule.
        /// </summary>
        /// <param name="leadScoreId">The lead score identifier.</param>
        public void DeleteRule(int leadScoreId)
        {
            var db = ObjectContextFactory.Create();
            var leadscoredb = db.LeadScoreRules.Where(ls => ls.LeadScoreRuleID == leadScoreId).FirstOrDefault();
            db.LeadScoreRules.Remove(leadscoredb);
            db.SaveChanges();
        }

        /// <summary>
        /// Finds all.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public IEnumerable<LeadScoreRule> FindAll(string name)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the score categories.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ScoreCategories> GetScoreCategories()
        {
            var db = ObjectContextFactory.Create();
            IEnumerable<ScoreCategoriesDb> scoreCategoriesDb = db.Categories.Where(c => c.ScoreCategoryID <= 4).ToList();
            if (scoreCategoriesDb != null)
                return convertToDomainCategories(scoreCategoriesDb);
            return null;
        }

        /// <summary>
        /// Converts to domain categories.
        /// </summary>
        /// <param name="categories">The categories.</param>
        /// <returns></returns>
        IEnumerable<ScoreCategories> convertToDomainCategories(IEnumerable<ScoreCategoriesDb> categories)
        {
            foreach (ScoreCategoriesDb scoreCategory in categories)
            {
                yield return new ScoreCategories() { Id = scoreCategory.ScoreCategoryID, Name = scoreCategory.Name, ModuleID = scoreCategory.ModuleID };
            }
        }

        /// <summary>
        /// Gets the conditions.
        /// </summary>
        /// <param name="categoryID">The category identifier.</param>
        /// <returns></returns>
        public IEnumerable<Condition> GetConditions(int categoryID)
        {
            var db = ObjectContextFactory.Create();
            IEnumerable<ConditionDb> conditionDb = db.Conditions.Where(c => c.ScoreCategoryID == categoryID).ToList();

            if (conditionDb != null)
                return convertToDomainConditions(conditionDb);
            return null;
        }

        /// <summary>
        /// Converts to domain conditions.
        /// </summary>
        /// <param name="conditions">The conditions.</param>
        /// <returns></returns>
        IEnumerable<Condition> convertToDomainConditions(IEnumerable<ConditionDb> conditions)
        {
            foreach (ConditionDb condition in conditions)
            {
                yield return new Condition() { Id = condition.ConditionID, Name = condition.Name };
            }
        }

        /// <summary>
        /// Determines whether [is duplicate rule] [the specified account identifier].
        /// </summary>
        /// <param name="accountID">The account identifier.</param>
        /// <param name="conditionId">The condition identifier.</param>
        /// <param name="conditionValue">The condition value.</param>
        /// <param name="leadScoreRuleMapID">The lead score rule map identifier.</param>
        /// <returns></returns>
        public bool IsDuplicateRule(int accountID, LeadScoreConditionType conditionId, string conditionValue, int leadScoreRuleMapID)
        {
            LeadScoreRulesDb leadScores = ObjectContextFactory.Create().LeadScoreRules.Where(c => c.ConditionID == (byte)conditionId
                                            && c.ConditionValue == conditionValue && c.AccountID == accountID
                                            && c.LeadScoreRuleMapID != leadScoreRuleMapID && c.IsActive == true).FirstOrDefault();
            if (leadScores != null)
                return true;
            return false;
        }

        /// <summary>
        /// Gets the tags.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <returns></returns>
        public IEnumerable<Tag> GetTags(int[] values)
        {
            var db = ObjectContextFactory.Create();
            IEnumerable<TagsDb> tagsDb = db.Tags.Where(x => values.Contains(x.TagID) && x.IsDeleted != true);
            if (tagsDb != null)
                return convertToDomainTags(tagsDb);
            return null;
        }

        /// <summary>
        /// Converts to domain tags.
        /// </summary>
        /// <param name="tags">The tags.</param>
        /// <returns></returns>
        IEnumerable<Tag> convertToDomainTags(IEnumerable<TagsDb> tags)
        {
            foreach (TagsDb tag in tags)
            {
                yield return new Tag() { Id = tag.TagID, TagName = tag.TagName, Description = tag.Description };
            }
        }

        /// <summary>
        /// Deactivates the rules.
        /// </summary>
        /// <param name="ruleID">The rule identifier.</param>
        public void DeactivateRules(int[] ruleID)
        {
            var db = ObjectContextFactory.Create();
            db.LeadScoreRules.Where(x => ruleID.Contains(x.LeadScoreRuleMapID)).ToList().ForEach(ls => ls.IsActive = false);
            db.SaveChanges();
        }

        /// <summary>
        /// Gets the lead sources.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<LeadSource> GetLeadSources()
        {
            var db = ObjectContextFactory.Create();
            IEnumerable<LeadSourceDb> leadSourceDb = db.LeadSource.ToList();
            if (leadSourceDb != null)
                return convertToDomainLeadSources(leadSourceDb);
            return null;
        }

        /// <summary>
        /// Converts to domain lead sources.
        /// </summary>
        /// <param name="leadSources">The lead sources.</param>
        /// <returns></returns>
        IEnumerable<LeadSource> convertToDomainLeadSources(IEnumerable<LeadSourceDb> leadSources)
        {
            foreach (LeadSourceDb leadSource in leadSources)
            {
                yield return new LeadSource() { Id = leadSource.LeadSourceID, Name = leadSource.Name };
            }
        }

        /// <summary>
        /// Gets the campaigns.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        public IEnumerable<Campaign> GetCampaigns(int accountId)
        {
            var db = ObjectContextFactory.Create();
            short[] CampaignStatuses = new short[]{
                (short)CampaignStatus.Active,
                (short)CampaignStatus.Draft,
                (short)CampaignStatus.Sent,
                (short)CampaignStatus.Scheduled                
            };
            IEnumerable<Campaign> campaignsDb = db.Campaigns.Where(c => CampaignStatuses.Contains(c.CampaignStatusID) && c.AccountID == accountId && c.IsDeleted == false)
                                                                .Select(c => new Campaign()
                                                                {
                                                                    Id = c.CampaignID,
                                                                    Name = c.Name
                                                                });
            return campaignsDb;
        }

        /// <summary>
        /// Gets the forms.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        IEnumerable<Form> ILeadScoreRuleRepository.GetForms(int accountId)
        {
            var db = ObjectContextFactory.Create();
            IEnumerable<dynamic> formsDb = db.Forms.Where(c => c.Status == (byte)FormStatus.Active && c.AccountID == accountId && c.IsDeleted == false).Select(f => new { FormID = f.FormID, Name = f.Name }).ToList();

            if (formsDb != null)
                return convertToDomainForm(formsDb);
            return null;
        }

        /// <summary>
        /// Gets the forms.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <param name="userID">The user identifier.</param>
        /// <returns></returns>
        IEnumerable<Form> ILeadScoreRuleRepository.GetForms(int accountId, int? userID)
        {
            var db = ObjectContextFactory.Create();
            IEnumerable<dynamic> formsDb = db.Forms.Where(c => c.Status == (byte)FormStatus.Active && c.AccountID == accountId && c.IsDeleted == false && c.CreatedBy == userID).Select(f => new { FormID = f.FormID, Name = f.Name }).ToList();

            if (formsDb != null)
                return convertToDomainForm(formsDb);
            return null;
        }

        /// <summary>
        /// Converts to domain form.
        /// </summary>
        /// <param name="forms">The forms.</param>
        /// <returns></returns>
        private IEnumerable<Form> convertToDomainForm(IEnumerable<dynamic> forms)
        {
            foreach (dynamic form in forms)
            {
                yield return new Form() { Id = form.FormID, Name = form.Name };
            }
        }

        /// <summary>
        /// Gets the form lead score.
        /// </summary>
        /// <param name="formId">The form identifier.</param>
        /// <param name="accountID">The account identifier.</param>
        /// <returns></returns>
        public bool GetFormLeadScore(int[] formId, int accountID)
        {
            var db = ObjectContextFactory.Create();
            IEnumerable<LeadScoreRulesDb> lsrdb = db.LeadScoreRules.Where(c => c.ConditionID == 3 && c.IsActive == true).ToList();

            foreach (int i in formId)
            {
                LeadScoreRulesDb dbs = lsrdb.Where(c => c.ConditionValue == i.ToString()).FirstOrDefault();
                return (dbs != null);
            }
            return true;
        }

        /// <summary>
        /// Leads the score rules count.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="accountID">The account identifier.</param>
        /// <param name="Conditions">The conditions.</param>
        /// <returns></returns>
        public int LeadScoreRulesCount(string name, int accountID, IEnumerable<byte> Conditions)
        {
            using (var db = ObjectContextFactory.Create())
            {
                var predicate = PredicateBuilder.True<LeadScoreRulesDb>();
                if (!string.IsNullOrEmpty(name))
                {
                    name = name.ToLower();
                    predicate = predicate.And(a => a.ConditionDescription.Contains(name));
                }

                predicate = predicate.And(a => Conditions.Contains(a.ConditionID));
                predicate = predicate.And(a => a.IsActive == true);
                predicate = predicate.And(a => a.AccountID == accountID).And(a => a.ConditionID != 10);
                return db.LeadScoreRules.AsExpandable().Where(predicate).GroupBy(x => x.LeadScoreRuleMapID).Count();
            }
        }

        /// <summary>
        /// Gets the maximum lead score rule identifier.
        /// </summary>
        /// <returns></returns>
        public int GetMaxLeadScoreRuleID()
        {
            using (var db = ObjectContextFactory.Create())
            {
                return db.LeadScoreRules.Max(i => i.LeadScoreRuleID);
            }
        }

        /// <summary>
        /// Inserts the lead score rules.
        /// </summary>
        /// <param name="leadScore">The lead score.</param>
        /// <param name="LeadScoreRuleMapID">The lead score rule map identifier.</param>
        /// <returns></returns>
        public bool InsertLeadScoreRules(LeadScoreRule leadScore, int LeadScoreRuleMapID)
        {
            using (var db = ObjectContextFactory.Create())
            {
                //int MaxLeadScoreRuleID = db.LeadScoreRules.Where(i => i.AccountID == leadScore.AccountID).Max(i => i.LeadScoreRuleID);
                IList<LeadScoreRulesDb> leadScoreRules = new List<LeadScoreRulesDb>();
                List<int> SelectedIDs = new List<int>();
                string SelectedCampaignLinks = null;
                if (leadScore.ConditionID == LeadScoreConditionType.ContactOpensEmail
                    && leadScore.SelectedCampaignID != null && leadScore.SelectedCampaignID.Any())
                {
                    SelectedIDs = leadScore.SelectedCampaignID.ToList();
                }
                else if (leadScore.ConditionID == LeadScoreConditionType.ContactClicksLink)
                {
                    SelectedIDs = leadScore.SelectedCampaignID == null ? SelectedIDs : leadScore.SelectedCampaignID.ToList();
                    SelectedCampaignLinks = leadScore.SelectedCampaignLinkID == null ? null :
                                                string.Join(",", leadScore.SelectedCampaignLinkID);
                }
                else if (leadScore.ConditionID == LeadScoreConditionType.ContactSubmitsForm
                         && leadScore.SelectedFormID != null && leadScore.SelectedFormID.Any())
                {
                    SelectedIDs = leadScore.SelectedFormID.ToList();
                }
                else if ((leadScore.ConditionID == LeadScoreConditionType.ContactActionTagAdded ||
                         leadScore.ConditionID == LeadScoreConditionType.ContactNoteTagAdded)
                         && leadScore.SelectedTagID != null && leadScore.SelectedTagID.Any())
                {
                    SelectedIDs = leadScore.SelectedTagID.ToList();
                }
                else if (leadScore.ConditionID == LeadScoreConditionType.ContactLeadSource
                          && leadScore.SelectedLeadSourceID != null && leadScore.SelectedLeadSourceID.Any())
                {
                    SelectedIDs = leadScore.SelectedLeadSourceID.ToList();
                }
                else if (leadScore.ConditionID == LeadScoreConditionType.ContactTourType
                         && leadScore.SelectedTourTypeID != null && leadScore.SelectedTourTypeID.Any())
                {
                    SelectedIDs = leadScore.SelectedTourTypeID.ToList();
                }
                else if (leadScore.ConditionID == LeadScoreConditionType.ContactNoteCategoryAdded
                         && leadScore.SelectedNoteCategoryID != null && leadScore.SelectedNoteCategoryID.Any())
                {
                    SelectedIDs = leadScore.SelectedNoteCategoryID.ToList();
                }
                else if (leadScore.ConditionID == LeadScoreConditionType.ContactActionCompleted
                    && leadScore.SelectedActionTypeID != null && leadScore.SelectedActionTypeID.Any())
                {
                    SelectedIDs = leadScore.SelectedActionTypeID.ToList();
                }


                if (leadScore.ConditionID == LeadScoreConditionType.ContactVisitsWebPage || leadScore.ConditionID == LeadScoreConditionType.ContactVisitsWebsite)
                {
                    LeadScoreRulesDb rule = new LeadScoreRulesDb()
                    {
                        AccountID = leadScore.AccountID,
                        AppliedToPreviousActions = leadScore.AppliedToPreviousActions,
                        ConditionDescription = leadScore.ConditionDescription,
                        ConditionValue = leadScore.ConditionValue,
                        CreatedBy = leadScore.CreatedBy,
                        CreatedOn = leadScore.CreatedOn,
                        LeadScoreRuleMapID = LeadScoreRuleMapID,
                        IsActive = true,
                        ModifiedBy = leadScore.ModifiedBy,
                        ModifiedOn = leadScore.ModifiedOn,
                        Score = leadScore.Score.Value,
                        ConditionID = (byte)leadScore.ConditionID,
                        SelectedCampaignLinks = SelectedCampaignLinks
                    };
                    leadScoreRules.Add(rule);
                }
                else if (leadScore.ConditionID == LeadScoreConditionType.PageDuration)
                {
                    LeadScoreRulesDb rule = new LeadScoreRulesDb()
                    {
                        AccountID = leadScore.AccountID,
                        AppliedToPreviousActions = leadScore.AppliedToPreviousActions,
                        ConditionDescription = leadScore.ConditionDescription,
                        ConditionValue = leadScore.ConditionValue,
                        CreatedBy = leadScore.CreatedBy,
                        CreatedOn = leadScore.CreatedOn,
                        LeadScoreRuleMapID = LeadScoreRuleMapID,
                        IsActive = true,
                        ModifiedBy = leadScore.ModifiedBy,
                        ModifiedOn = leadScore.ModifiedOn,
                        Score = leadScore.Score.Value,
                        ConditionID = (byte)leadScore.ConditionID,
                        SelectedCampaignLinks = SelectedCampaignLinks
                    };
                    leadScoreRules.Add(rule);
                    var existingConditionValue = db.LeadScoreConditionValues
                          .Where(c => c.LeadScoreRuleID == leadScore.LeadScoreRuleMapID && c.ValueType == (short)LeadScoreValueType.PageDuration).FirstOrDefault();
                    if (existingConditionValue != null)
                    {
                        existingConditionValue.Value = leadScore.LeadScoreConditionValues.Where(c => c.ValueType == LeadScoreValueType.PageDuration).Select(c => c.Value).FirstOrDefault() ?? "0";
                        db.Entry<LeadScoreConditionValuesDb>(existingConditionValue).State = EntityState.Modified;
                    }
                    else
                    {
                        LeadScoreConditionValuesDb conditionValue = new LeadScoreConditionValuesDb()
                        {
                            LeadScoreConditionValueID = 0,
                            LeadScoreRuleID = LeadScoreRuleMapID,
                            Value = leadScore.LeadScoreConditionValues.Where(c => c.ValueType == LeadScoreValueType.PageDuration).Select(c => c.Value).FirstOrDefault() ?? "0",
                            ValueType = (short)LeadScoreValueType.PageDuration
                        };
                        db.LeadScoreConditionValues.Add(conditionValue);
                    }
                }
                else
                {
                    if (SelectedIDs == null || !SelectedIDs.Any())
                    {
                        LeadScoreRulesDb rule = new LeadScoreRulesDb()
                        {
                            AccountID = leadScore.AccountID,
                            AppliedToPreviousActions = leadScore.AppliedToPreviousActions,
                            ConditionDescription = leadScore.ConditionDescription,
                            ConditionValue = null,
                            CreatedBy = leadScore.CreatedBy,
                            CreatedOn = leadScore.CreatedOn,
                            LeadScoreRuleMapID = LeadScoreRuleMapID,
                            IsActive = true,
                            ModifiedBy = leadScore.ModifiedBy,
                            ModifiedOn = leadScore.ModifiedOn,
                            Score = leadScore.Score.Value,
                            ConditionID = (byte)leadScore.ConditionID,
                            SelectedCampaignLinks = SelectedCampaignLinks
                        };
                        leadScoreRules.Add(rule);
                    }
                    else
                    {
                        foreach (int ID in SelectedIDs)
                        {
                            LeadScoreRulesDb rule = new LeadScoreRulesDb()
                            {
                                AccountID = leadScore.AccountID,
                                AppliedToPreviousActions = leadScore.AppliedToPreviousActions,
                                ConditionDescription = leadScore.ConditionDescription,
                                ConditionValue = ID == 0 ? null : ID.ToString(),
                                CreatedBy = leadScore.CreatedBy,
                                CreatedOn = leadScore.CreatedOn,
                                LeadScoreRuleMapID = LeadScoreRuleMapID,
                                IsActive = true,
                                ModifiedBy = leadScore.ModifiedBy,
                                ModifiedOn = leadScore.ModifiedOn,
                                Score = leadScore.Score.Value,
                                ConditionID = (byte)leadScore.ConditionID,
                                SelectedCampaignLinks = SelectedCampaignLinks
                            };
                            leadScoreRules.Add(rule);
                        }
                    }
                }

                db.LeadScoreRules.AddRange(leadScoreRules);
                db.SaveChanges();

                return true;

            }
        }

        /// <summary>
        /// Updates the lead score rules.
        /// </summary>
        /// <param name="leadScoreRule">The lead score rule.</param>
        public void UpdateLeadScoreRules(LeadScoreRule leadScoreRule)
        {
            var db = ObjectContextFactory.Create();
            db.LeadScoreRules.Where(i => i.LeadScoreRuleMapID == leadScoreRule.LeadScoreRuleMapID).ForEach(i => i.IsActive = false);
            InsertLeadScoreRules(leadScoreRule, leadScoreRule.LeadScoreRuleMapID);
            db.SaveChanges();
        }

        /// <summary>
        /// Gets the lead score rules by condition.
        /// </summary>
        /// <param name="conditionId">The condition identifier.</param>
        /// <param name="conditionValue">The condition value.</param>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        public IEnumerable<LeadScoreRule> GetLeadScoreRulesByCondition(int conditionId, string conditionValue, int accountId)
        {
            var db = ObjectContextFactory.Create();
            var leadScoreRuleDb = db.LeadScoreRules.Where(l => l.ConditionID == conditionId
                                                            && l.AccountID == accountId
                                                            && (l.ConditionValue == conditionValue || l.ConditionValue == null)
                                                            && l.IsActive == true);

            return Mapper.Map<IEnumerable<LeadScoreRulesDb>, IEnumerable<LeadScoreRule>>(leadScoreRuleDb);
        }


        public IEnumerable<LeadScoreRule> GetPageDurationRules(int conditionId, string conditionValue, int accountId, int duration)
        {
            using (var db = ObjectContextFactory.Create())
            {
                var pageDurationRulesDb = db.LeadScoreRules.Where(l => l.ConditionID == conditionId
                                                                && l.AccountID == accountId
                                                                && (l.ConditionValue == conditionValue || l.ConditionValue == "*")
                                                                && l.IsActive == true).ToList();
                var ruleIds = pageDurationRulesDb.Select(c => c.LeadScoreRuleMapID).ToList();
                var pageDurationRuleConditionValuesDb = db.LeadScoreConditionValues
                    .Where(c => ruleIds.Contains(c.LeadScoreRuleID)).ToList();
                var pageDurationRules = Mapper.Map<IEnumerable<LeadScoreRulesDb>, IEnumerable<LeadScoreRule>>(pageDurationRulesDb);
                var qualifiedRules = new List<LeadScoreRule>();
                foreach (var rule in pageDurationRules)
                {
                    var conditionValues = pageDurationRuleConditionValuesDb.Where(c => c.LeadScoreRuleID == rule.LeadScoreRuleMapID).ToList();
                    rule.LeadScoreConditionValues = Mapper.Map<IEnumerable<LeadScoreConditionValuesDb>, IEnumerable<LeadScoreConditionValue>>(conditionValues);
                }

                var valuePairs = pageDurationRuleConditionValuesDb.Select(c => new KeyValuePair<int, int>(c.LeadScoreRuleID, int.Parse(c.Value))).ToList();

                var anyPageRuleIds = pageDurationRules.Where(c => c.ConditionValue == "*").Select(c => c.LeadScoreRuleMapID).ToList();
                var bestAnyPageRule = valuePairs.Where(c => anyPageRuleIds.Contains(c.Key) && c.Value < duration)
                    .OrderByDescending(c => c.Value).FirstOrDefault();
                if (bestAnyPageRule.Key > 0)
                    qualifiedRules.Add(pageDurationRules.Where(c => c.LeadScoreRuleMapID == bestAnyPageRule.Key).FirstOrDefault());

                var specificPageRuleIds = pageDurationRules.Where(c => c.ConditionValue != "*").Select(c => c.LeadScoreRuleMapID).ToList();
                var bestSpecificPageRule = valuePairs.Where(c => specificPageRuleIds.Contains(c.Key) && c.Value < duration)
                    .OrderByDescending(c => c.Value).FirstOrDefault();
                if (bestSpecificPageRule.Key > 0)
                    qualifiedRules.Add(pageDurationRules.Where(c => c.LeadScoreRuleMapID == bestSpecificPageRule.Key).FirstOrDefault());

                return qualifiedRules;
            }
        }
        /// <summary>
        /// Determines whether the specified rule is duplicate.
        /// </summary>
        /// <param name="rule">The rule.</param>
        /// <returns></returns>
        public bool IsDuplicate(LeadScoreRule rule)
        {
            using (var db = ObjectContextFactory.Create())
            {
                if (rule.ConditionID == LeadScoreConditionType.ContactVisitsWebsite || rule.ConditionID == LeadScoreConditionType.ContactVisitsWebPage)
                {
                    return db.LeadScoreRules.Where(c => c.ConditionID == (byte)rule.ConditionID
                                               && c.ConditionValue == rule.ConditionValue && c.AccountID == rule.AccountID
                                               && c.LeadScoreRuleMapID != rule.LeadScoreRuleMapID && c.IsActive == true).Count() > 0;

                }
                else if (rule.ConditionID == LeadScoreConditionType.PageDuration)
                {
                    var currentConditionValue = rule.LeadScoreConditionValues.Where(c => c.ValueType == LeadScoreValueType.PageDuration)
                           .Select(c => c.Value).FirstOrDefault();
                    var duplicateRuleIDs = db.LeadScoreRules.Where(c => c.ConditionID == (byte)rule.ConditionID
                                               && c.ConditionValue == rule.ConditionValue && c.AccountID == rule.AccountID
                                               && c.LeadScoreRuleMapID != rule.LeadScoreRuleMapID && c.IsActive == true)
                                               .Select(c => c.LeadScoreRuleMapID).ToList();
                    var duplicateConditionValue = db.LeadScoreConditionValues
                        .Where(d => d.ValueType == (byte)LeadScoreValueType.PageDuration
                            && duplicateRuleIDs.Contains(d.LeadScoreRuleID) && d.Value == currentConditionValue)
                        .Select(c => c.Value).FirstOrDefault();

                    if (duplicateConditionValue != null && duplicateConditionValue == currentConditionValue)
                        return true;
                    else
                        return false;
                }
                else if (rule.ConditionID == LeadScoreConditionType.AnEmailSent || rule.ConditionID == LeadScoreConditionType.ContactSendText || rule.ConditionID == LeadScoreConditionType.ContactEmailReceived)
                {
                    return db.LeadScoreRules.Where(c => c.ConditionID == (byte)rule.ConditionID
                                                     && c.AccountID == rule.AccountID
                                                     && c.LeadScoreRuleMapID != rule.LeadScoreRuleMapID
                                                     && c.IsActive == true).Count() > 0;
                }
                else if (rule.ConditionID == LeadScoreConditionType.ContactClicksLink)
                {
                    if (rule.SelectedCampaignID == null)
                    {
                        return CheckDuplicateForNull(rule.ConditionID, rule.AccountID, rule.LeadScoreRuleMapID);
                    }
                    else if (rule.SelectedCampaignID != null && rule.SelectedCampaignID.Any())
                    {
                        string[] ConditionValues = rule.SelectedCampaignID.Select(x => x.ToString()).ToArray();
                        string[] LinkIDs = rule.SelectedCampaignLinkID.Select(x => x.ToString()).ToArray();
                        return db.LeadScoreRules.Where(c => c.ConditionID == (byte)rule.ConditionID
                                                        && c.AccountID == rule.AccountID
                                                        && c.LeadScoreRuleMapID != rule.LeadScoreRuleMapID
                                                        && c.IsActive == true
                                                        && ConditionValues.Contains(c.ConditionValue)
                                                        && LinkIDs.Any(i => c.SelectedCampaignLinks.Contains(i))).Any();
                    }
                    else
                    {
                        return false;
                    }
                }
                else if (rule.ConditionID == LeadScoreConditionType.ContactActionTagAdded || rule.ConditionID == LeadScoreConditionType.ContactNoteTagAdded)
                {
                    var selectedTagID = rule.Tags != null ? rule.Tags.Where(i => i.Id != 0).Select(x => x.Id) : null;
                    return rule.Tags == null || !rule.Tags.Any() ?
                           CheckDuplicateForNull(rule.ConditionID, rule.AccountID, rule.LeadScoreRuleMapID) :
                           CheckDuplicate(rule.ConditionID, selectedTagID.ToArray(), rule.AccountID, rule.LeadScoreRuleMapID);
                }
                else if (rule.ConditionID == LeadScoreConditionType.ContactActionCompleted)
                {
                    return rule.SelectedActionTypeID == null || !rule.SelectedActionTypeID.Any() ?
                          CheckDuplicateForNull(rule.ConditionID, rule.AccountID, rule.LeadScoreRuleMapID) :
                          CheckDuplicate(rule.ConditionID, rule.SelectedActionTypeID, rule.AccountID, rule.LeadScoreRuleMapID);
                }
                else
                {
                    if (rule.ConditionID == LeadScoreConditionType.ContactOpensEmail)
                    {
                        return rule.SelectedCampaignID == null || !rule.SelectedCampaignID.Any() ?
                            CheckDuplicateForNull(rule.ConditionID, rule.AccountID, rule.LeadScoreRuleMapID) :
                            CheckDuplicate(rule.ConditionID, rule.SelectedCampaignID, rule.AccountID, rule.LeadScoreRuleMapID);
                    }
                    else if (rule.ConditionID == LeadScoreConditionType.ContactSubmitsForm)
                    {
                        return rule.SelectedFormID == null || !rule.SelectedFormID.Any() ?
                                CheckDuplicateForNull(rule.ConditionID, rule.AccountID, rule.LeadScoreRuleMapID) :
                                CheckDuplicate(rule.ConditionID, rule.SelectedFormID, rule.AccountID, rule.LeadScoreRuleMapID);
                    }
                    else if (rule.ConditionID == LeadScoreConditionType.ContactLeadSource)
                    {
                        return rule.SelectedLeadSourceID == null || !rule.SelectedLeadSourceID.Any() ?
                              CheckDuplicateForNull(rule.ConditionID, rule.AccountID, rule.LeadScoreRuleMapID) :
                              CheckDuplicate(rule.ConditionID, rule.SelectedLeadSourceID, rule.AccountID, rule.LeadScoreRuleMapID);
                    }
                    else if (rule.ConditionID == LeadScoreConditionType.ContactTourType)
                    {
                        return rule.SelectedTourTypeID == null || !rule.SelectedTourTypeID.Any() ?
                              CheckDuplicateForNull(rule.ConditionID, rule.AccountID, rule.LeadScoreRuleMapID) :
                              CheckDuplicate(rule.ConditionID, rule.SelectedTourTypeID, rule.AccountID, rule.LeadScoreRuleMapID);
                    }
                    else if (rule.ConditionID == LeadScoreConditionType.ContactNoteCategoryAdded)
                    {
                        return rule.SelectedNoteCategoryID == null || !rule.SelectedNoteCategoryID.Any() ?
                              CheckDuplicateForNull(rule.ConditionID, rule.AccountID, rule.LeadScoreRuleMapID) :
                              CheckDuplicate(rule.ConditionID, rule.SelectedNoteCategoryID, rule.AccountID, rule.LeadScoreRuleMapID);
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }

        /// <summary>
        /// Checks the duplicate.
        /// </summary>
        /// <param name="Condition">The condition.</param>
        /// <param name="TypeIDs">The type i ds.</param>
        /// <param name="AccountID">The account identifier.</param>
        /// <param name="LeadScoreRuleMapID">The lead score rule map identifier.</param>
        /// <returns></returns>
        private bool CheckDuplicate(LeadScoreConditionType Condition, int[] TypeIDs, int AccountID, int LeadScoreRuleMapID)
        {
            using (var db = ObjectContextFactory.Create())
            {
                string[] ConditionValues = TypeIDs.Select(x => x.ToString()).ToArray();
                return db.LeadScoreRules.Where(c => c.ConditionID == (byte)Condition && c.AccountID == AccountID && ConditionValues.Contains(c.ConditionValue)
                                                && c.LeadScoreRuleMapID != LeadScoreRuleMapID && c.IsActive == true).Count() > 0;
            }
        }

        /// <summary>
        /// Checks the duplicate for null.
        /// </summary>
        /// <param name="Condition">The condition.</param>
        /// <param name="AccountID">The account identifier.</param>
        /// <param name="LeadScoreRuleMapID">The lead score rule map identifier.</param>
        /// <returns></returns>
        private bool CheckDuplicateForNull(LeadScoreConditionType Condition, int AccountID, int LeadScoreRuleMapID)
        {
            using (var db = ObjectContextFactory.Create())
            {
                return db.LeadScoreRules.Where(c => c.ConditionID == (byte)Condition && c.AccountID == AccountID && c.ConditionValue == null
                                                 && c.LeadScoreRuleMapID != LeadScoreRuleMapID && c.IsActive == true).Count() > 0;
            }
        }

        /// <summary>
        /// Gets the conditions from modules.
        /// </summary>
        /// <param name="UserModules">The user modules.</param>
        /// <returns></returns>
        public List<byte> GetConditionsFromModules(IEnumerable<byte> UserModules)
        {
            using (var db = ObjectContextFactory.Create())
            {
                return db.Conditions.Where(x => UserModules.Contains((byte)x.ModuleID)).Select(x => x.ConditionID).ToList();
            }
        }

        /// <summary>
        /// Gets the lead score rules for link clicked.
        /// </summary>
        /// <param name="conditionValue">The condition value.</param>
        /// <param name="accountId">The account identifier.</param>
        /// <param name="EntityID">The entity identifier.</param>
        /// <returns></returns>
        public IEnumerable<LeadScoreRule> GetLeadScoreRulesForLinkClicked(string conditionValue, int accountId, int EntityID)
        {
            var db = ObjectContextFactory.Create();
            var leadScoreRuleDb = db.LeadScoreRules.Where(l => l.ConditionID == (byte)LeadScoreConditionType.ContactClicksLink
                                                            && l.AccountID == accountId
                                                            && (l.ConditionValue == conditionValue || l.ConditionValue == null)
                                                            && (l.SelectedCampaignLinks.Contains(EntityID.ToString()) || string.IsNullOrEmpty(l.SelectedCampaignLinks))
                                                            && l.IsActive == true);
            return Mapper.Map<IEnumerable<LeadScoreRulesDb>, IEnumerable<LeadScoreRule>>(leadScoreRuleDb);
        }

        /// <summary>
        /// Gets the lead score tag i ds.
        /// </summary>
        /// <param name="AccountID">The account identifier.</param>
        /// <returns></returns>
        public int[] GetLeadScoreTagIDs(int AccountID)
        {
            var db = ObjectContextFactory.Create();
            var TagIDs = db.LeadScoreRules.Where(i => (i.ConditionID == (byte)LeadScoreConditionType.ContactNoteTagAdded
                                                    || i.ConditionID == (byte)LeadScoreConditionType.ContactActionTagAdded)
                                                    && i.ConditionValue != null
                                                    && i.AccountID == AccountID
                                                    && i.IsActive == true)
                                                    .Select(x => x.ConditionValue);

            List<int> TagID = new List<int>();
            foreach (string Tagid in TagIDs)
            {
                int Value;
                if (int.TryParse(Tagid, out Value))
                {
                    TagID.Add(Value);
                }
            };

            return TagID.ToArray();
        }

        /// <summary>
        /// Getting All Lead Score Categories.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ScoreCategories> GetLeadScoreCategeories()
        {
            using (var db = ObjectContextFactory.Create())
            {
                var sql = @"SELECT ScoreCategoryID AS Id,Name,ModuleID FROM ScoreCategories (NOLOCK) WHERE ScoreCategoryID<>5"; //5 Automation
                return db.Get<ScoreCategories>(sql, new { }).ToList();
            }
        }

        /// <summary>
        /// Getting All Lead Score Conditions.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Condition> GetLeadScoreConditions()
        {
            using (var db = ObjectContextFactory.Create())
            {
                var sql = @"SELECT ConditionID AS Id,Name,ScoreCategoryID,ModuleID FROM Conditions (NOLOCK) WHERE ScoreCategoryID <> 5 AND ConditionID NOT IN (4,11,24,25)"; //5 Automation
                return db.Get<Condition>(sql, new { }).ToList();
            }
        }
    }
}

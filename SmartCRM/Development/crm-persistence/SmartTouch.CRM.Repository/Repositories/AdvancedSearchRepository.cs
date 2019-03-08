using AutoMapper;
using LinqKit;
using SmartTouch.CRM.Domain.Fields;
using SmartTouch.CRM.Domain.Search;
using SmartTouch.CRM.Domain.Tags;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using SmartTouch.CRM.Repository.Database;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;

namespace SmartTouch.CRM.Repository.Repositories
{
    public class AdvancedSearchRepository : Repository<SearchDefinition, int, SearchDefinitionsDb>, IAdvancedSearchRepository
    {
        public AdvancedSearchRepository(IUnitOfWork unitOfWork, IObjectContextFactory objectContextFactory)
            : base(unitOfWork, objectContextFactory) { }

        /// <summary>
        /// Finds all default.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<SearchDefinition> FindAllDefault()
        {
            var predicate = PredicateBuilder.True<SearchDefinitionsDb>();
            predicate = predicate.And(a => a.AccountID == null && a.SelectAllSearch == false);

            var db = ObjectContextFactory.Create();
            var searchResults = db.SearchDefinitions.Include(i => i.SearchFilters) //Include("SearchFilters").Include("SearchDefinitionTagMap")
                .AsExpandable()
                .Where(predicate).ToList();
            foreach (var da in searchResults)
            {
                yield return ConvertToDomain(da);
            }
        }

        /// <summary>
        /// Finds all.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="IsPredefinedSearch">if set to <c>true</c> [is predefined search].</param>
        /// <param name="IsFavoriteSearch">if set to <c>true</c> [is favorite search].</param>
        /// <returns></returns>
        public IEnumerable<SearchDefinition> FindAll(int accountId, int userId, Boolean IsPredefinedSearch, Boolean IsFavoriteSearch)
        {
            string predicate =string.Empty ;

            if (IsPredefinedSearch)
                predicate = string.Format(" S.IsPreConfiguredSearch = 1");
            if (IsFavoriteSearch)
                predicate = string.Format(" S.IsFavoriteSearch = 1");

            return findAdvancedSearchResultsSummary(predicate, userId, accountId,0,10);
            //IEnumerable<SearchDefinitionsDb> advancedSearchResults = findAdvancedSearchResultsSummary(predicate);
            //foreach (SearchDefinitionsDb da in advancedSearchResults)
            //{
            //    yield return ConvertToDomain(da);
            //}
        }

        /// <summary>
        /// Finds the advanced search results summary.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns></returns>
        IEnumerable<SearchDefinition> findAdvancedSearchResultsSummary(string predicate, int userId,int accountId, int skip, int take)
        {
            var db = ObjectContextFactory.Create();
            string sql = string.Format(@";WITH UALogs AS
                        (
                         SELECT EntityID,LogDate, ROW_NUMBER() OVER (PARTITION BY EntityID ORDER BY LogDate DESC) RowNumber FROM UserActivityLogs UA (NOLOCK) 
                         WHERE UA.UserID=@userId AND UA.ModuleID=31 AND UA.UserActivityID=6 AND AccountID = @accountid
                        ), STags AS
                        (
                         SELECT SD.SearchDefinitionID, TagName =  STUFF((SELECT ', ' + T.TagName
                                                FROM vTags (NOLOCK) T 
                              INNER JOIN SearchDefinitionTagMap SDTI (NOLOCK) ON SDTI.SearchDefinitionTagID = T.TagID
                              WHERE SDTI.SearchDefinitionID = SD.SearchDefinitionID 
                              FOR XML PATH('')),1,2,'') 
                         FROM SearchDefinitions (NOLOCK) SD 
                         WHERE SD.AccountID = @accountid  AND SD.IsDeleted=0
                        )

                        SELECT S.AccountID,S.CreatedBy,S.CreatedOn,S.CustomPredicateScript,UA.LogDate AS LastRunDate,S.SearchDefinitionID AS Id,S.IsFavoriteSearch,
                        S.IsPreConfiguredSearch,S.SearchPredicateTypeID AS PredicateType,S.ElasticQuery,S.SearchDefinitionName AS Name,S.SelectAllSearch, COUNT(1) OVER() as TotalSearchsCount , ST.TagName
                        FROM SearchDefinitions (NOLOCK) S
                        LEFT JOIN UALogs UA ON S.SearchDefinitionID = UA.EntityID AND UA.RowNumber = 1
                        LEFT JOIN STags ST ON ST.SearchDefinitionID = S.SearchDefinitionID
                        WHERE   S.AccountID= @accountid AND S.SelectAllSearch = 0  AND S.IsDeleted=0 AND {0}
                        ORDER BY COALESCE(UA.LogDate,S.LastRunDate) DESC,  S.CreatedOn ASC
                        OFFSET @skip ROWS
                        FETCH NEXT @take ROWS ONLY", predicate);
            IEnumerable<SearchDefinition> searchResults = db.Get<SearchDefinition>(sql, new { accountid = accountId, userId = userId, skip= skip, take= take }).ToList();
           

            return searchResults;
        }

        /// <summary>
        /// Finds all.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="limit">The limit.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="accountId">The account identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns></returns>
        public IEnumerable<SearchDefinition> FindAll(string name, int limit, int pageNumber, int accountId, int userId)
        {
            string predicate = string.Empty;
            var skip = (pageNumber - 1) * limit;
            if (!string.IsNullOrEmpty(name))
            {
                name = name.ToLower();
                predicate = string.Format(" S.SearchDefinitionName LIKE '%{0}%' AND S.IsPreConfiguredSearch = 0", name);
            }
            else
            {
                predicate = string.Format(" S.IsPreConfiguredSearch = 0");
            }

            return findAdvancedSearchResultsSummary(predicate, userId,accountId,skip, limit);
        }

        /// <summary>
        /// Finds all favorite searches.
        /// </summary>
        /// <param name="limit">The limit.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="accountId">The account identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns></returns>
        public IEnumerable<SearchDefinition> FindAllFavoriteSearches(int limit, int pageNumber, int accountId, int userId)
        {
            string predicate = string.Empty;
            var skip = (pageNumber - 1) * limit;

            predicate = predicate = string.Format(" S.IsFavoriteSearch = 1");
            return findAdvancedSearchResultsSummary(predicate, userId,accountId,skip, limit);
        }

        /// <summary>
        /// Finds the by.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public SearchDefinition FindBy(short id)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Finds all.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public IEnumerable<SearchDefinition> FindAll()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Finds the by.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public override SearchDefinition FindBy(int id)
        {
            SearchDefinitionsDb searchDatabase = getSearchDefinitionDb(id);
            if (searchDatabase != null)
            {
                SearchDefinition searchDatabaseConvertedToDomain = ConvertToDomain(searchDatabase);
                return searchDatabaseConvertedToDomain;                
            }
            return null;
        }

        /// <summary>
        /// Converts the type of to database.
        /// </summary>
        /// <param name="domainType">Type of the domain.</param>
        /// <param name="db">The database.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">Invalid SearchDefinitionID has been passed. Suspected Id forgery.</exception>
        public override SearchDefinitionsDb ConvertToDatabaseType(SearchDefinition domainType, CRMDb db)
        {
            SearchDefinitionsDb searchDefinitionsDb;
            int SearchDefinitionID = domainType.Id;
            if (SearchDefinitionID > 0)
            {
                searchDefinitionsDb = db.SearchDefinitions.SingleOrDefault(c => c.SearchDefinitionID == SearchDefinitionID);
                if (searchDefinitionsDb == null)
                    throw new ArgumentException("Invalid SearchDefinitionID has been passed. Suspected Id forgery.");
                searchDefinitionsDb = Mapper.Map<SearchDefinition, SearchDefinitionsDb>(domainType, searchDefinitionsDb);
            }
            else
            {
                searchDefinitionsDb = Mapper.Map<SearchDefinition, SearchDefinitionsDb>(domainType);
            }
            return searchDefinitionsDb;
        }

        /// <summary>
        /// Converts to domain.
        /// </summary>
        /// <param name="dbOject">The database oject.</param>
        /// <returns></returns>
        public override SearchDefinition ConvertToDomain(SearchDefinitionsDb dbOject)
        {
            return Mapper.Map<SearchDefinition>(dbOject);
        }

        /// <summary>
        /// Finds the search definitions.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        public IEnumerable<SearchDefinition> FindSearchDefinitions(int accountId)
        {
            List<SearchDefinition> searchDefinitions = new List<SearchDefinition>();
            IEnumerable<SearchDefinitionsDb> searchDefinitionsDb = getSearchDefinitionsDb(accountId);
            foreach (var Db in searchDefinitionsDb)
            {
                var searchDefinition = Mapper.Map<SearchDefinition>(Db);
                searchDefinitions.Add(searchDefinition);
            }
            return searchDefinitions;
        }

        /// <summary>
        /// Gets the search definitions database.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        IEnumerable<SearchDefinitionsDb> getSearchDefinitionsDb(int accountId)
        {
            var db = ObjectContextFactory.Create();
            List<SearchDefinitionsDb> searchDefinitionsDb = db.SearchDefinitions.Include(i => i.SearchFilters).Include("SearchFilters.DropdownValue").Include("SearchFilters.Fields").Where(w => w.AccountID == accountId && w.SelectAllSearch == false).ToList();
            return searchDefinitionsDb;
        }

        /// <summary>
        /// Gets the search definition database.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        SearchDefinitionsDb getSearchDefinitionDb(int id)
        {
            var db = ObjectContextFactory.Create();

            var sdb = new SearchDefinitionsDb();
            var sql = @"select * from SearchDefinitions (nolock) s where s.SearchDefinitionID = @id
                        select * from SearchFilters (nolock) where SearchDefinitionID = @id
                        select * from SearchDefinitionTagMap (nolock) where SearchDefinitionID = @id 
                        select  t.* from SearchDefinitionTagMap (nolock) sdt
                        inner join vtags t on sdt.SearchDefinitionTagID = t.TagID
                        where sdt.SearchDefinitionID = @id";
            db.GetMultiple(sql, (r) =>
                {
                    sdb = r.Read<SearchDefinitionsDb>().ToList().FirstOrDefault();
                    sdb.SearchFilters = r.Read<SearchFiltersDb>().ToList();
                    sdb.SearchTags = r.Read<SearchDefinitionTagMapDb>().ToList();
                    var tags = r.Read<TagsDb>().ToList();
                    sdb.SearchTags.ForEach(st =>
                        {
                            st.Tags = tags.Where(t => t.TagID == st.SearchDefinitionTagID).FirstOrDefault();
                        });
                }, new { Id = id });
            return sdb;
        }

        /// <summary>
        /// Persists the value objects.
        /// </summary>
        /// <param name="domainType">Type of the domain.</param>
        /// <param name="dbType">Type of the database.</param>
        /// <param name="db">The database.</param>
        public override void PersistValueObjects(SearchDefinition domainType, SearchDefinitionsDb dbType, CRMDb db)
        {
            PersistAdvancedSearchFilterMap(domainType, dbType, db);
            PersistAdvancedSearchTagsMap(domainType, dbType, db);
        }

        /// <summary>
        /// Persists the advanced search tags map.
        /// </summary>
        /// <param name="searchDefinition">The search definition.</param>
        /// <param name="searchDefinitionDb">The search definition database.</param>
        /// <param name="db">The database.</param>
        void PersistAdvancedSearchTagsMap(SearchDefinition searchDefinition, SearchDefinitionsDb searchDefinitionDb, CRMDb db)
        {
            if (searchDefinition.TagsList.Any())
            {
                var searchDefinitionTags = db.SearchDefinitionTagMap.Where(a => a.SearchDefinitionID == searchDefinition.Id).ToList();
                foreach (Tag tag in searchDefinition.TagsList)
                {
                    if (tag.Id == 0)
                    {
                        var tagDb = db.Tags.SingleOrDefault(t => t.TagName.Equals(tag.TagName) && t.AccountID.Equals(tag.AccountID) && t.IsDeleted != true);                        
                        if(tagDb == null)
                        {
                            tagDb = Mapper.Map<Tag, TagsDb>(tag);
                            tagDb.IsDeleted = false;
                            tagDb = db.Tags.Add(tagDb);
                        }
                        var advancedSearchTag = new SearchDefinitionTagMapDb()
                        {
                            SearchDefinition = searchDefinitionDb,
                            Tags = tagDb
                        };

                        db.SearchDefinitionTagMap.Add(advancedSearchTag);
                    }
                    else if (!searchDefinitionTags.Any(a => a.SearchDefinitionTagID == tag.Id))
                    {
                        db.SearchDefinitionTagMap.Add(new SearchDefinitionTagMapDb() { SearchDefinitionID = searchDefinition.Id, SearchDefinitionTagID = tag.Id });
                        db.RefreshAnalytics.Add(new RefreshAnalyticsDb() { EntityID = tag.Id, EntityType = 5, Status = 1, LastModifiedOn = DateTime.Now.ToUniversalTime() });
                    }
                }

                IList<int> tagIds = searchDefinition.TagsList.Where(a => a.Id > 0).Select(a => a.Id).ToList();
                var unMapActionTags = searchDefinitionTags.Where(a => !tagIds.Contains(a.SearchDefinitionTagID));
                foreach (SearchDefinitionTagMapDb searchDefinitionTagMapDb in unMapActionTags)
                {
                    db.SearchDefinitionTagMap.Remove(searchDefinitionTagMapDb);
                    db.RefreshAnalytics.Add(new RefreshAnalyticsDb() { EntityID = searchDefinitionTagMapDb.SearchDefinitionTagID, EntityType = 5, Status = 1, LastModifiedOn = DateTime.Now.ToUniversalTime() });
                }
            }

        }

        /// <summary>
        /// Persists the advanced search filter map.
        /// </summary>
        /// <param name="searchDefinition">The search definition.</param>
        /// <param name="SearchDefinitionsDb">The search definitions database.</param>
        /// <param name="db">The database.</param>
        void PersistAdvancedSearchFilterMap(SearchDefinition searchDefinition, SearchDefinitionsDb SearchDefinitionsDb, CRMDb db)
        {
            var searchFiltersMap = db.SearchFilters.Where(c => c.SearchDefinitionID == searchDefinition.Id).ToList();
            if (searchDefinition.Filters != null && searchDefinition.IsPreConfiguredSearch == false)
            {
                foreach (var filter in searchDefinition.Filters)
                {
                    if (filter.SearchFilterId != 0)
                    {
                        var searchFilterMap = searchFiltersMap.SingleOrDefault(r => r.SearchFilterID == filter.SearchFilterId);
                        searchFilterMap.SearchDefinitionID = filter.SearchDefinitionID;
                        searchFilterMap.SearchQualifierTypeID = (short)filter.Qualifier;
                        searchFilterMap.FieldID = filter.IsDropdownField ? (int?)null : (int)filter.Field;
                        searchFilterMap.SearchText = filter.SearchText;
                        searchFilterMap.IsCustomField = filter.IsCustomField;
                        searchFilterMap.IsDropdownField = filter.IsDropdownField;
                        searchFilterMap.IsDateTime = filter.IsDropdownField;
                        searchFilterMap.DropdownId = filter.DropdownId;
                        searchFilterMap.DropdownValueID = filter.IsDropdownField ? filter.DropdownValueId : null;

                    }
                    else
                    {
                        short? dropdownvalue = null;
                        int? fieldId = null;
                        if (!filter.IsDropdownField)
                            fieldId = (int)filter.Field;
                        else
                            dropdownvalue = filter.DropdownValueId;
                        db.SearchFilters.Add(new SearchFiltersDb
                        {
                            FieldID = fieldId,
                            SearchDefinitionID = SearchDefinitionsDb.SearchDefinitionID,
                            SearchQualifierTypeID = (short)filter.Qualifier,
                            SearchText = filter.SearchText,
                            IsCustomField = filter.IsCustomField,
                            IsDropdownField = filter.IsDropdownField,
                            IsDateTime = filter.IsDateTime,
                            DropdownId = filter.DropdownId,
                            DropdownValueID = dropdownvalue
                        });
                    }
                }
                if (searchDefinition.Id > 0)
                {
                    IList<short> searchFilterIds = searchDefinition.Filters.Where(n => n.SearchFilterId > 0).Select(n => n.SearchFilterId).ToList();
                    var unMapSearchFilters = searchFiltersMap.Where(n => !searchFilterIds.Contains(n.SearchFilterID));
                    db.SearchFilters.RemoveRange(unMapSearchFilters);
                }
            }
            if (searchDefinition.IsPreConfiguredSearch == true)
            {
                int filterCount = 0;
                var phoneDropdownValues = new DropdownValueDb[] { };
                if (searchDefinition.Name == "Call List")
                    phoneDropdownValues = db.DropdownValues.Where(d => d.AccountID == searchDefinition.AccountID && d.DropdownID == (short)DropdownFieldTypes.PhoneNumberType).ToArray();
                foreach (var filter in searchDefinition.Filters)
                {
                    filterCount = filterCount + 1;
                    short? dropdownvalue = null;
                    int? fieldId = null;
                    if (!filter.IsDropdownField)
                        fieldId = (int)filter.Field;
                    else
                        dropdownvalue = filter.DropdownValueId;

                    if (filter.Field == ContactFields.LifecycleStageField)
                    {
                        var dropdownValueId = db.DropdownValues.Where(d => d.AccountID == searchDefinition.AccountID && d.DropdownValueTypeID == (short)DropdownValueTypes.Customer).SingleOrDefault().DropdownValueID;
                        filter.SearchText = dropdownValueId.ToString();
                    }

                    if (searchDefinition.Filters.Count() == 3)
                    {
                        filter.DropdownValueId = (short)phoneDropdownValues[filterCount - 1].DropdownValueID;
                        dropdownvalue = filter.DropdownValueId;
                    }


                    db.SearchFilters.Add(new SearchFiltersDb
                    {

                        FieldID = fieldId,
                        SearchDefinitionID = SearchDefinitionsDb.SearchDefinitionID,
                        SearchQualifierTypeID = (short)filter.Qualifier,
                        SearchText = filter.SearchText,
                        IsCustomField = filter.IsCustomField,
                        IsDropdownField = filter.IsDropdownField,
                        IsDateTime = filter.IsDateTime,
                        DropdownId = filter.DropdownId,
                        DropdownValueID = dropdownvalue
                    });
                }

            }
        }

        /// <summary>
        /// Deletes the searches.
        /// </summary>
        /// <param name="SearchDefinitionIDs">The search definition ids.</param>
        /// <returns></returns>
        public string DeleteSearches(List<int> SearchDefinitionIDs)
        {
            var db = ObjectContextFactory.Create();
            var DeleteSearcheDefinitions = db.SearchDefinitions.Where(i => SearchDefinitionIDs.Contains(i.SearchDefinitionID)).ToList();
            var DeleteSearchFilters = db.SearchFilters.Where(i => SearchDefinitionIDs.Contains(i.SearchDefinitionID)).ToList();
            var DeleteSearchTags = db.SearchDefinitionTagMap.Where(i => SearchDefinitionIDs.Contains(i.SearchDefinitionID)).ToList();

            string message = "";
            var workflow = db.WorkflowTriggers.Where(t => SearchDefinitionIDs.Contains((short)t.SearchDefinitionID) && t.Workflows.IsDeleted == false)
                .Include(i => i.SearchDefinitions).Include(i => i.Workflows).FirstOrDefault();
            if (workflow != null)
            {
                message = workflow.SearchDefinitions.SearchDefinitionName + " is associated with an " + workflow.Workflows.WorkflowName + " workflow"
                    + ". DELETE operation cancelled.";
                return message;
            }
            var campaign = db.CampaignSearchDefinitions.Where(cs => SearchDefinitionIDs.Contains(cs.SearchDefinitionID) && cs.Campaign.IsDeleted== false)
                .Include(i => i.SearchDefinition).Include(i => i.Campaign).FirstOrDefault();
            if (campaign != null)
            {
                message = campaign.SearchDefinition.SearchDefinitionName + " is associated with " + campaign.Campaign.Name + " campaign"
                    + ". DELETE operation cancelled.";
                return message;
            }

            DeleteSearcheDefinitions.ForEach(sd => sd.IsDeleted = true);
             db.SaveChanges();
            return message;
        }

        /// <summary>
        /// Gets the last run date.
        /// </summary>
        /// <param name="SearchDefinitionID">The search definition identifier.</param>
        /// <returns></returns>
        public DateTime? GetLastRunDate(short SearchDefinitionID)
        {
            var db = ObjectContextFactory.Create();
            var SearchDefinition = db.SearchDefinitions.Where(i => i.SearchDefinitionID == SearchDefinitionID).FirstOrDefault();
            if (SearchDefinition != null)
                return SearchDefinition.LastRunDate;
            else
                return null;
        }

        /// <summary>
        /// Updates the search definition.
        /// </summary>
        /// <param name="SearchDefinitionID">The search definition identifier.</param>
        /// <returns></returns>
        public bool UpdateSearchDefinition(short SearchDefinitionID)
        {
            var db = ObjectContextFactory.Create();
            bool IsUpdated = false;
            var SearchDefinition = db.SearchDefinitions.Where(i => i.SearchDefinitionID == SearchDefinitionID).FirstOrDefault();
            if (SearchDefinition != null)
            {
                SearchDefinition.IsFavoriteSearch = SearchDefinition.IsFavoriteSearch == true ? false : true;
                IsUpdated = true;
            }
            db.SaveChanges();
            return IsUpdated;

        }

        /// <summary>
        /// Determines whether [is search name unique] [the specified definition].
        /// </summary>
        /// <param name="definition">The definition.</param>
        /// <returns></returns>
        public bool IsSearchNameUnique(SearchDefinition definition)
        {
            var db = ObjectContextFactory.Create();
            var searchFound = db.SearchDefinitions.Where(c => c.SearchDefinitionName == definition.Name && c.AccountID == definition.AccountID && c.SelectAllSearch == false).Select(c => c).FirstOrDefault();
            if (searchFound != null && definition.Id != searchFound.SearchDefinitionID)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Gets the search fields.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        public IEnumerable<Field> GetSearchFields(int accountId)
        {
            var db = ObjectContextFactory.Create();
            var sql = @"SELECT F.* FROM Fields (NOLOCK) F
                        WHERE (F.AccountID =@AccountId OR F.AccountID IS NULL) AND F.StatusID = 201 AND F.FieldID NOT IN(5,6,4,40)
                        ORDER BY F.Title ASC";

            IEnumerable<FieldsDb> fieldsDb = db.Get<FieldsDb>(sql, new { AccountId = accountId }).ToList(); //We are excluding phones here and getting them from caching.
            return (fieldsDb != null) ? Mapper.Map<IEnumerable<FieldsDb>, IEnumerable<Field>>(fieldsDb) : null;            
        }

        /// <summary>
        /// Gets the updatable fields.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        public IEnumerable<Field> GetUpdatableFields(int accountId)
        {
            List<int> nonUpdatebleFields = getNonUpdatableFields().ToList();
            nonUpdatebleFields.Add((int)ContactFields.Community);
            var db = ObjectContextFactory.Create();
            var sql = @"SELECT * FROM Fields (NOLOCK) 
                        WHERE (AccountID=@AccountId OR AccountID IS NULL) AND StatusID=201 AND FieldID NOT IN @NonUpdateFields AND IsLeadAdapterField = 0
                        ORDER BY Title ASC";
            IEnumerable<FieldsDb> fieldsDb = db.Get<FieldsDb>(sql, new { AccountId = accountId, NonUpdateFields = nonUpdatebleFields }).ToList();
            return (fieldsDb != null) ? Mapper.Map<IEnumerable<FieldsDb>, IEnumerable<Field>>(fieldsDb) : null;       
        }

        /// <summary>
        /// Gets the non updatable fields.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<int> getNonUpdatableFields()
        {
            var nonUpdatableFields = new List<int>();
            nonUpdatableFields.Add((int)ContactFields.FirstNameField);
            nonUpdatableFields.Add((int)ContactFields.LastNameField);
            nonUpdatableFields.Add((int)ContactFields.CompanyNameField);
            nonUpdatableFields.Add((int)ContactFields.MobilePhoneField);
            nonUpdatableFields.Add((int)ContactFields.HomePhoneField);
            nonUpdatableFields.Add((int)ContactFields.WorkPhoneField);
            nonUpdatableFields.Add((int)ContactFields.PrimaryEmail);
            nonUpdatableFields.Add((int)ContactFields.DonotEmail);
            nonUpdatableFields.Add((int)ContactFields.AddressLine1Field);
            nonUpdatableFields.Add((int)ContactFields.AddressLine2Field);
            nonUpdatableFields.Add((int)ContactFields.CityField);
            nonUpdatableFields.Add((int)ContactFields.StateField);
            nonUpdatableFields.Add((int)ContactFields.ZipCodeField);
            nonUpdatableFields.Add((int)ContactFields.CountryField);
            nonUpdatableFields.Add((int)ContactFields.PartnerTypeField);
            nonUpdatableFields.Add((int)ContactFields.CreatedBy);
            nonUpdatableFields.Add((int)ContactFields.CreatedOn);
            nonUpdatableFields.Add((int)ContactFields.LastTouched);
            nonUpdatableFields.Add((int)ContactFields.LastTouchedThrough);
            nonUpdatableFields.Add((int)ContactFields.LeadAdapter);
            nonUpdatableFields.Add((int)ContactFields.LeadScore);
            nonUpdatableFields.Add((int)ContactFields.FirstSourceType);
            nonUpdatableFields.Add((int)ContactFields.Owner);
            nonUpdatableFields.Add((int)ContactFields.ContactTag);
            nonUpdatableFields.Add((int)ContactFields.FormName);
            nonUpdatableFields.Add((int)ContactFields.FormsubmittedOn);
            nonUpdatableFields.Add((int)ContactFields.WebPage);
            nonUpdatableFields.Add((int)ContactFields.WebPageDuration);
            nonUpdatableFields.Add((int)ContactFields.LeadSourceDate);
            nonUpdatableFields.Add((int)ContactFields.FirstLeadSourceDate);
            nonUpdatableFields.Add((int)ContactFields.FirstLeadSource);
            nonUpdatableFields.Add((int)ContactFields.NoteSummary);
            nonUpdatableFields.Add((int)ContactFields.LastNoteDate);
            nonUpdatableFields.Add((int)ContactFields.LastNote);
            nonUpdatableFields.Add((int)ContactFields.TourType);
            nonUpdatableFields.Add((int)ContactFields.TourDate);
            nonUpdatableFields.Add((int)ContactFields.TourCreator);
            nonUpdatableFields.Add((int)ContactFields.EmailStatus);
            nonUpdatableFields.Add((int)ContactFields.Community);
            nonUpdatableFields.Add((int)ContactFields.TourAssignedUsers);
            nonUpdatableFields.Add((int)ContactFields.ActionCreatedDate);
            nonUpdatableFields.Add((int)ContactFields.ActionType);
            nonUpdatableFields.Add((int)ContactFields.ActionDate);
            nonUpdatableFields.Add((int)ContactFields.ActionStatus);
            nonUpdatableFields.Add((int)ContactFields.ActionAssignedTo);
            nonUpdatableFields.Add((int)ContactFields.NoteCategory);
            nonUpdatableFields.Add((int)ContactFields.LastNoteCategory);

            return nonUpdatableFields;
        }

        /// <summary>
        /// Gets the search value options.
        /// </summary>
        /// <param name="fieldId">The field identifier.</param>
        /// <returns></returns>
        public IEnumerable<FieldValueOption> GetSearchValueOptions(int fieldId)
        {
            var db = ObjectContextFactory.Create();
            if (fieldId == (int)ContactFields.FirstSourceType)
                return db.ContactFirstSource.Select(s => new FieldValueOption()
                {
                    FieldId = fieldId,
                    Id = s.SourceID,
                    Value = s.FirstSource
                });
            else
            {
                IEnumerable<CustomFieldValueOptionsDb> valueOptionsDb = db.CustomFieldValueOptions.Where(w => w.CustomFieldID == fieldId && w.IsDeleted == false).OrderBy(w=>w.Order);

                return (valueOptionsDb != null && valueOptionsDb.Any()) ? Mapper.Map<IEnumerable<CustomFieldValueOptionsDb>, IEnumerable<FieldValueOption>>(valueOptionsDb) : null;
            }
        }

        /// <summary>
        /// Gets the lead adapters.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        public IEnumerable<FieldValueOption> GetLeadAdapters(int accountId)
        {
            var db = ObjectContextFactory.Create();
            IEnumerable<LeadAdapterTypesDb> leadAdaptersDb = db.LeadAdapterTypes.Where(w => w.LeadAdapterTypeID != 11 &&
                db.LeadAdapters.Where(wh => wh.AccountID == accountId).Select(s => s.LeadAdapterTypeID).Distinct().Contains(w.LeadAdapterTypeID));

            return (leadAdaptersDb != null && leadAdaptersDb.Any()) ? Mapper.Map<IEnumerable<LeadAdapterTypesDb>, IEnumerable<FieldValueOption>>(leadAdaptersDb) : null;            
        }

        /// <summary>
        /// Gets the tags.
        /// </summary>
        /// <param name="accountID">The account identifier.</param>
        /// <returns></returns>
        public IEnumerable<Tag> GetTags(int accountID)
        {
            var tagsDb = new List<TagsDb>();
            if (accountID != 0)
            {
                var db = ObjectContextFactory.Create();
                var sql = @"select distinct t.TagID, t.TagName from ContactTagMap (nolock) ctm
                            inner join vtags t on t.TagID = ctm.TagID and ctm.AccountId = t.AccountId
                            where t.IsDeleted = 0 and t.AccountID = @accountId";
                tagsDb = db.Get<TagsDb>(sql, new { accountId = accountID }).ToList();
            }
            foreach (var tag in tagsDb)
                yield return Mapper.Map<TagsDb, Tag>(tag);
        }

        /// <summary>
        /// Updates the last run activity.
        /// </summary>
        /// <param name="searchDefinitionId">The search definition identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="accountId">The account identifier.</param>
        /// <param name="searchName">Name of the search.</param>
        public void UpdateLastRunActivity(int searchDefinitionId, int userId, int accountId, string searchName)
        {
            if (searchDefinitionId != 0 && userId != 0 && accountId != 0)
            {
                var db = ObjectContextFactory.Create();
                UserActivityLogsDb log = new UserActivityLogsDb();
                log.AccountID = accountId;
                log.UserID = userId;
                log.EntityID = searchDefinitionId;
                log.EntityName = searchName;
                log.LogDate = DateTime.UtcNow;
                log.ModuleID = (byte)AppModules.AdvancedSearch;
                log.UserActivityID = (byte)UserActivityType.LastRunOn;
                db.UserActivitiesLog.Add(log);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// Updates the view activity.
        /// </summary>
        /// <param name="searchDefinitionId">The search definition identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="accountId">The account identifier.</param>
        /// <param name="searchName">Name of the search.</param>
        public void UpdateViewActivity(int searchDefinitionId, int userId, int accountId, string searchName)
        {
            if (searchDefinitionId != 0 && userId != 0 && accountId != 0)
            {
                var db = ObjectContextFactory.Create();
                UserActivityLogsDb log = new UserActivityLogsDb();
                log.AccountID = accountId;
                log.UserID = userId;
                log.EntityID = searchDefinitionId;
                log.EntityName = searchName;
                log.LogDate = DateTime.UtcNow;
                log.ModuleID = (byte)AppModules.AdvancedSearch;
                log.UserActivityID = (byte)UserActivityType.Read;
                db.UserActivitiesLog.Add(log);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// Gets the search description.
        /// </summary>
        /// <param name="searchDefinitionId">The search definition identifier.</param>
        /// <returns></returns>
        public string GetSearchDescription(int searchDefinitionId)
        {
            string searchDesc = string.Empty;
            if (searchDefinitionId != 0)
            {
                var db = ObjectContextFactory.Create();
                var procedureName = "[dbo].[getSearchTitle]";
                db.QueryStoredProc(procedureName, (r) => searchDesc = r.Read<string>().FirstOrDefault(), new { searchDefinitionID = searchDefinitionId });
            }
            return searchDesc;
        }

        /// <summary>
        /// Gets the column preferences.
        /// </summary>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="entityType">Type of the entity.</param>
        /// <returns></returns>
        public IEnumerable<AVColumnPreferences> GetColumnPreferences(int entityId, byte entityType)
        {
            IEnumerable<AVColumnPreferences> preference = new List<AVColumnPreferences>();
            if (entityId != default(int) && entityType != default(byte))
            { 
                var db = ObjectContextFactory.Create();
                var preferencesDb = db.AVColumnPreferences.Where(w => w.EntityID == entityId && w.EntityType == entityType);
                if (preferencesDb != null && preferencesDb.Count() > 0)
                    preference = Mapper.Map<IEnumerable<AVColumnPreferencesDb>, IEnumerable<AVColumnPreferences>>(preferencesDb);
            }
            return preference;
        }

        public void SaveColumnPreferences(int entityId, byte entityType, IEnumerable<int> fields, byte showingType)
        {
            if (fields != null && fields.Any())
            { 
                var db = ObjectContextFactory.Create();
                var oldColumns = db.AVColumnPreferences.Where(w => w.EntityID == entityId && w.EntityType == entityType);
                db.AVColumnPreferences.RemoveRange(oldColumns);
                IList<AVColumnPreferencesDb> Columns = new List<AVColumnPreferencesDb>();
                foreach (var field in fields) 
                {
                    AVColumnPreferencesDb columnsDb = new AVColumnPreferencesDb();
                    columnsDb.EntityID = entityId;
                    columnsDb.EntityType = entityType;
                    columnsDb.FieldID = field;
                    columnsDb.FieldType = 1;
                    columnsDb.ShowingType = showingType;
                    Columns.Add(columnsDb);
                }
                db.AVColumnPreferences.AddRange(Columns);
                db.SaveChanges();
            }
        }

        public Dictionary<int, int> GetAllSearchDefinitionIsAndAccountIds()
        {
            var db = ObjectContextFactory.Create();
            var sql = @"SELECT DISTINCT SearchDefinitionID,AccountID FROM SmartSearchQueue(NOLOCK) WHERE IsProcessed=0 ";
            Dictionary<int,int> searchDefinitions = db.Get<SearchDefinitionsDb>(sql, new { }).ToDictionary(x => x.SearchDefinitionID, x => x.AccountID.Value);
            return searchDefinitions;
        }

        public void InsertSmartSearchQueue(int searchDefinitionId, int accountId)
        {
            var db = ObjectContextFactory.Create();
            var sql = @"INSERT INTO SmartSearchQueue VALUES(@searchDefinitionID,@isProcessed,@createdOn,@accountId)";
            db.Execute(sql, new { searchDefinitionID = searchDefinitionId, isProcessed = false, createdOn = DateTime.Now.ToUniversalTime(), accountId  = accountId });
        }

        public void UpdateSmartSearchQueue(int searchDefinitionId, int accountId,bool status)
        {
            var db = ObjectContextFactory.Create();
            var sql = @"UPDATE SmartSearchQueue SET IsProcessed=@isprocessed WHERE SearchDefinitionID=@searchDefinitionID AND AccountID=@accountId AND IsProcessed=0";
            db.Execute(sql, new { isprocessed = status,  searchDefinitionID = searchDefinitionId, accountId = accountId });

        }

        public IEnumerable<string> GetSearchDefinitionNamesByIds(List<int> searchdefinitionIds)
        {
            var db = ObjectContextFactory.Create();
            var sql = @"SELECT SearchDefinitionName FROM SearchDefinitions (NOLOCK) WHERE SearchDefinitionID IN @SearchDefinitionIds";
            return db.Get<string>(sql, new { SearchDefinitionIds = searchdefinitionIds }).ToList();
        }

    }
}

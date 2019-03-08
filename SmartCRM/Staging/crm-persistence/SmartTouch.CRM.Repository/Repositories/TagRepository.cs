using AutoMapper;
using LandmarkIT.Enterprise.Utilities.Logging;
using LinqKit;
using SmartTouch.CRM.Domain.Opportunities;
using SmartTouch.CRM.Domain.Tags;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using SmartTouch.CRM.Repository.Database;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SmartTouch.CRM.Repository.Repositories
{
    public class TagRepository : Repository<Tag, int, TagsDb>, ITagRepository
    {
        public TagRepository(IUnitOfWork unitOfWork, IObjectContextFactory objectContextFactory)
            : base(unitOfWork, objectContextFactory)
        {
        }

        /// <summary>
        /// Finds all.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Tag> FindAll()
        {
            var tagsDb = ObjectContextFactory.Create().VTags.Where(i => i.IsDeleted != true);
            if (tagsDb != null)
            {
                foreach (VTagsDb db in tagsDb)
                    yield return ConvertToDomain(db);
            }
        }

        /// <summary>
        /// Finds all.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        public IEnumerable<Tag> FindAll(int accountId)
        {
            var tagsDb = ObjectContextFactory.Create().VTags.Where(p => p.AccountID == accountId && p.IsDeleted != true).OrderBy(p => p.TagName);
            if (tagsDb != null)
            {
                foreach (VTagsDb db in tagsDb)
                {
                    yield return ConvertToDomain(db);
                }
            }
        }

        /// <summary>
        /// Finds the tags summary.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="sortField">The sort field.</param>
        /// <param name="direction">The direction.</param>
        /// <returns></returns>
        public IEnumerable<VTagsDb> FindTagsSummary(int accountId, string name, string sortField, ListSortDirection direction, int limit, int pageNumber)
        {
            var skip = (pageNumber - 1) * limit;
            var take = limit;
            var db = ObjectContextFactory.Create();

            var sortFieldMappings = new Dictionary<string, string>()
                {
                    {"Id","TagID"},
                    {"TagName", "TagName"},
                    {"Count","Count"},
                };
            Func<string, string> GetSortField = (s) =>
            {
                if (sortFieldMappings.ContainsKey(s))
                    return sortFieldMappings[s];
                else
                    return s;
            };
            sortField = sortField ?? "Id";
            string sortDirection = direction == ListSortDirection.Ascending ? "ASC" : "DESC";
            name = "%" + name + "%";

            var sql = string.Format(@"DECLARE @cnt INT = 0
                        SELECT @cnt = COUNT (t.tagid) FROM vTags T WHERE T.AccountID = @id and coalesce(t.isdeleted,0) = 0 and t.tagname like  @name
                        SELECT t.*, @cnt as TotalCount from dbo.vtags t 
                        where t.accountid = @id and coalesce(t.isdeleted,0) = 0 and t.tagname like @name
                        order by {0} {1}
                        OFFSET @skip ROWS
                        FETCH NEXT @take ROWS ONLY", GetSortField(sortField), sortDirection);

            var tags = db.Get<VTagsDb>(sql, new { Id = accountId, Name = name, skip = skip, take = take });
            return tags;
        }

        /// <summary>
        /// Finds the lead score rule tags.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <param name="tags">The tags.</param>
        /// <returns></returns>
        IEnumerable<Tag> FindLeadScoreRuleTags(int accountId, IEnumerable<Tag> tags)
        {
            var db = ObjectContextFactory.Create();
            var sql = @"select cast(ISNULL(conditionvalue, 0) as int) from leadscorerules (nolock) where conditionid in (6,7) and accountid = @id and isactive = 0";
            var lrules = db.Get<int>(sql, new { Id = accountId });
            lrules.ForEach(lr =>
                {
                    if (tags.Any(t => t.Id == lr))
                    {
                        tags.Where(t => t.Id == lr).First().LeadScoreTag = true;
                    }

                });
            return tags;
        }

        /// <summary>
        /// Finds the by.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public override Tag FindBy(int id)
        {
            var target = default(Tag);
            var tagDatabase = ObjectContextFactory.Create().VTags.FirstOrDefault(c => c.TagID == id && c.IsDeleted != true);
            if (tagDatabase != null) target = ConvertToDomain(tagDatabase);
            return target;
        }

        /// <summary>
        /// Finds the by.
        /// </summary>
        /// <param name="tagName">Name of the tag.</param>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        public Tag FindBy(string tagName, int accountId)
        {
            var target = default(Tag);
            var tagDatabase = ObjectContextFactory.Create().VTags
                                                  .FirstOrDefault(c => c.TagName.Equals(tagName)
                                                                        && c.IsDeleted != true
                                                                        && c.AccountID == accountId);
            if (tagDatabase != null) target = ConvertToDomain(tagDatabase);
            return target;
        }

        /// <summary>
        /// Deletes for contact.
        /// </summary>
        /// <param name="tagId">The tag identifier.</param>
        /// <param name="contactId">The contact identifier.</param>
        public void DeleteForContact(int tagId, int contactId, int accountID)
        {
            var db = ObjectContextFactory.Create();
            ContactTagMapDb contactTagDb = db.ContactTags.FirstOrDefault(c => c.TagID == tagId && c.ContactID == contactId && c.AccountID == accountID);
            db.RefreshAnalytics.Add(new RefreshAnalyticsDb() { EntityID = tagId, EntityType = 5, Status = 1, LastModifiedOn = DateTime.Now.ToUniversalTime() });
            if (contactTagDb != null)
            {
                db.ContactTags.Remove(contactTagDb);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// Deletes the opportunity tag.
        /// </summary>
        /// <param name="tagId">The tag identifier.</param>
        /// <param name="OpportunityID">The opportunity identifier.</param>
        public void DeleteOpportunityTag(int tagId, int OpportunityID)
        {
            var db = ObjectContextFactory.Create();
            OpportunityTagMap opportunityTagDb = db.OpportunityTagMap.FirstOrDefault(c => c.TagID == tagId && c.OpportunityID == OpportunityID);
            db.RefreshAnalytics.Add(new RefreshAnalyticsDb() { EntityID = tagId, EntityType = 5, Status = 1, LastModifiedOn = DateTime.Now.ToUniversalTime() });
            db.OpportunityTagMap.Remove(opportunityTagDb);
            db.SaveChanges();
        }

        /// <summary>
        /// Finds the by contact.
        /// </summary>
        /// <param name="contactId">The contact identifier.</param>
        /// <returns></returns>
        public IEnumerable<Tag> FindByContact(int contactId, int accountID)
        {
            var db = ObjectContextFactory.Create();
            var tagids = db.ContactTags.Where(c => c.ContactID == contactId && c.AccountID == accountID).Select(c => c.TagID).ToList();

            return db.VTags.Where(t => tagids.Contains(t.TagID)).Join(db.vTagsLeadScore, t => t.TagID, l => l.TagID, (t, l) => new Tag()
            {
                Id = t.TagID,
                TagName = t.TagName,
                Description = t.Description,
                CreatedBy = t.CreatedBy,
                AccountID = t.AccountID,
                LeadScoreTag = l.Count > 0
            });
        }

        /// <summary>
        /// Finds the by opportunity.
        /// </summary>
        /// <param name="opportuintyId">The opportuinty identifier.</param>
        /// <returns></returns>
        public IEnumerable<Tag> FindByOpportunity(int opportuintyId)
        {
            var tagdb = ObjectContextFactory.Create().OpportunityTagMap.Include(c => c.Tags).Where(c => c.OpportunityID == opportuintyId).Select(c => c.Tags).ToList();
            // return Mapper.Map<IEnumerable<TagsDb>, IEnumerable<Tag>>(tagdb);
            foreach (TagsDb da in tagdb)
            {
                yield return ConvertToDomain(da);
            }
        }

        /// <summary>
        /// Finds the by campaign.
        /// </summary>
        /// <param name="campaignId">The campaign identifier.</param>
        /// <returns></returns>
        public IEnumerable<Tag> FindByCampaign(int campaignId)
        {
            var db = ObjectContextFactory.Create();
            var sql = @"SELECT T.TagID Id, T.TagName, T.[Description], T.AccountID, T.CreatedBy, T.[Count], T.IsDeleted FROM CampaignTagMap (NOLOCK) CCT 
                        INNER JOIN vTags (NOLOCK) T ON T.TagID = CCT.TagID
                        WHERE CCT.CampaignID = @campaignID";
            var tags = db.Get<Tag>(sql, new { campaignID = campaignId });
            return tags;
        }

        /// <summary>
        /// Finds the contact tags by campaign.
        /// </summary>
        /// <param name="campaignId">The campaign identifier.</param>
        /// <returns></returns>
        public IEnumerable<Tag> FindContactTagsByCampaign(int campaignId)
        {
            var db = ObjectContextFactory.Create();
            var sql = @"SELECT T.TagID Id, T.TagName, T.[Description], T.AccountID, T.CreatedBy, T.[Count], T.IsDeleted FROM CampaignContactTagMap (NOLOCK) CCT 
                        INNER JOIN vTags (NOLOCK) T ON T.TagID = CCT.TagID
                        WHERE CCT.CampaignID = @campaignID";
            var tags = db.Get<Tag>(sql, new { campaignID = campaignId });
            return tags;
        }

        /// <summary>
        /// Saves the contact tag.
        /// </summary>
        /// <param name="contactId">The contact identifier.</param>
        /// <param name="tagName">Name of the tag.</param>
        /// <param name="tagId">The tag identifier.</param>
        /// <param name="accountId">The account identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns></returns>
        public Tag SaveContactTag(int contactId, string tagName, int tagId, int accountId, int userId)
        {
            var dbContext = ObjectContextFactory.Create();
            TagsDb tagDb;
            if (tagId == 0)
                tagDb = dbContext.Tags.Where(t => t.TagName == tagName && t.AccountID == accountId && t.IsDeleted != true).FirstOrDefault();
            else
            {
                tagDb = dbContext.Tags.Where(t => t.TagID == tagId && t.AccountID == accountId && t.IsDeleted != true).FirstOrDefault();
                dbContext.RefreshAnalytics.Add(new RefreshAnalyticsDb() { EntityID = tagId, EntityType = 5, Status = 1, LastModifiedOn = DateTime.Now.ToUniversalTime() });

            }
            var tagDbs = new TagsDb();
            TagsDb persistedTagDb = new TagsDb();

            if (tagDb == null)
            {
                tagDbs = new TagsDb
                {
                    TagName = tagName,
                    Description = tagName,
                    AccountID = accountId,
                    CreatedBy = userId,
                    IsDeleted = false
                };
                persistedTagDb = dbContext.Tags.Add(tagDbs);
            }
            else
                persistedTagDb = tagDb;

            if (contactId != 0)
            {

                var contactTagMap = dbContext.ContactTags.Where(t => t.TagID == persistedTagDb.TagID && t.ContactID == contactId && t.AccountID == accountId).FirstOrDefault();
                if (contactTagMap == null)
                {
                    var contactTagsDb = new ContactTagMapDb
                    {
                        //Contact = contactsDb,
                        ContactID = contactId,
                        Tag = persistedTagDb,
                        TaggedBy = userId,
                        TaggedOn = DateTime.Now.ToUniversalTime(),
                        AccountID = accountId
                    };
                    dbContext.ContactTags.Add(contactTagsDb);
                }

            }
            dbContext.SaveChanges();

            Tag tag = Mapper.Map<TagsDb, Tag>(persistedTagDb);
            if (tagId == 0)
                this.ScheduleAnalyticsRefreshForTags(tag.Id);

            tag.LeadScoreTag = GetLeadScoreTags(persistedTagDb.AccountID, persistedTagDb.TagID);
            return tag;
        }

        /// <summary>
        /// Saves the contacts for tag.
        /// </summary>
        /// <param name="contactIds">The contact ids.</param>
        /// <param name="tagName">Name of the tag.</param>
        /// <param name="tagId">The tag identifier.</param>
        /// <param name="accountId">The account identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns></returns>
        public Tag SaveContactsForTag(IEnumerable<int> contactIds, string tagName, int tagId, int accountId, int userId)
        {
            var dbContext = ObjectContextFactory.Create();
            TagsDb tagDb;
            if (tagId == 0)
                tagDb = dbContext.Tags.Where(t => t.TagName == tagName && t.AccountID == accountId && t.IsDeleted != true).FirstOrDefault();
            else
            {
                tagDb = dbContext.Tags.Where(t => t.TagID == tagId && t.AccountID == accountId && t.IsDeleted != true).FirstOrDefault();
                dbContext.RefreshAnalytics.Add(new RefreshAnalyticsDb() { EntityID = tagId, EntityType = 5, Status = 1, LastModifiedOn = DateTime.Now.ToUniversalTime() });
            }
            var tagDbs = new TagsDb();
            TagsDb persistedTagDb = new TagsDb();

            if (tagDb == null)
            {
                tagDbs = new TagsDb
                {
                    TagName = tagName,
                    Description = tagName,
                    AccountID = accountId,
                    CreatedBy = userId,
                    IsDeleted = false
                };
                persistedTagDb = dbContext.Tags.Add(tagDbs);
            }
            else
                persistedTagDb = tagDb;

            if (contactIds.Count() != 0)
            {
                var sql = @"select ctm.contactid from contacttagmap (nolock) ctm where tagid = @tagid and accountid = @accountId";
                //var sql = @"select  r.datavalue  from (select cam.contactid from ContactTagMap cam right outer join  dbo.Split(@ContactIds,',') split 
                //on split.DataValue = cam.contactid and cam.TagID = @TagId) r where r.TagID is null";
                var dbData = ObjectContextFactory.Create();
                var taggedContacts = dbData.Get<int>(sql, new { TagId = persistedTagDb.TagID, accountid = accountId });
                taggedContacts = contactIds.Except(taggedContacts);
                var contactTagList = new List<ContactTagMapDb>();
                foreach (int id in taggedContacts)
                {
                    contactTagList.Add(new ContactTagMapDb
                    {
                        //Contact = contactsDb,
                        ContactID = id,
                        Tag = persistedTagDb,
                        TaggedBy = userId,
                        TaggedOn = DateTime.Now.ToUniversalTime(),
                        AccountID = accountId
                    });
                }
                dbContext.ContactTags.AddRange(contactTagList);
            }

            dbContext.SaveChanges();

            Tag tag = Mapper.Map<TagsDb, Tag>(persistedTagDb);

            if (tagId == 0)
                this.ScheduleAnalyticsRefreshForTags(tag.Id);

            tag.LeadScoreTag = GetLeadScoreTags(persistedTagDb.AccountID, persistedTagDb.TagID);
            return tag;
        }

        private void ScheduleAnalyticsRefreshForTags(int tagId)
        {
            var db = ObjectContextFactory.Create();
            if (tagId != 0)
            {
                
                RefreshAnalyticsDb refreshAnalytics = new RefreshAnalyticsDb();
                refreshAnalytics.EntityID = tagId;
                refreshAnalytics.EntityType = 5;
                refreshAnalytics.Status = 1;
                refreshAnalytics.LastModifiedOn = DateTime.Now.ToUniversalTime();
                db.RefreshAnalytics.Add(refreshAnalytics);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// Saves the contact tag.
        /// </summary>
        /// <param name="contactId">The contact identifier.</param>
        /// <param name="tagId">The tag identifier.</param>
        /// <param name="createdBy">The created by.</param>
        public void SaveContactTag(int contactId, int tagId, int createdBy, int accountID)
        {
            var db = ObjectContextFactory.Create();
            if (contactId != 0 && tagId != 0)
            {
                var contactTagMap = db.ContactTags.Where(w => w.TagID == tagId && w.ContactID == contactId && w.AccountID == accountID).FirstOrDefault();
                db.RefreshAnalytics.Add(new RefreshAnalyticsDb() { EntityID = tagId, EntityType = 5, Status = 1, LastModifiedOn = DateTime.Now.ToUniversalTime() });
                if (contactTagMap == null)
                {
                    var contactTagDb = new ContactTagMapDb()
                    {
                        ContactID = contactId,
                        TagID = tagId,
                        TaggedOn = DateTime.Now.ToUniversalTime(),
                        TaggedBy = createdBy,
                        AccountID = accountID
                    };
                    db.ContactTags.Add(contactTagDb);
                }
                db.SaveChanges();
            }
        }

        /// <summary>
        /// Saves the opportunity tag.
        /// </summary>
        /// <param name="opportunityID">The opportunity identifier.</param>
        /// <param name="tagName">Name of the tag.</param>
        /// <param name="tagId">The tag identifier.</param>
        /// <param name="accountId">The account identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns></returns>
        public Tag SaveOpportunityTag(int opportunityID, string tagName, int tagId, int accountId, int userId)
        {
            var dbContext = ObjectContextFactory.Create();
            TagsDb tagDb;
            if (tagId == 0)
                tagDb = dbContext.Tags.Where(t => t.TagName == tagName && t.AccountID == accountId && t.IsDeleted != true).FirstOrDefault();
            else
            {
                tagDb = dbContext.Tags.Where(t => t.TagID == tagId && t.AccountID == accountId && t.IsDeleted != true).FirstOrDefault();
                dbContext.RefreshAnalytics.Add(new RefreshAnalyticsDb() { EntityID = tagId, EntityType = 5, Status = 1, LastModifiedOn = DateTime.Now.ToUniversalTime() });
            }
            var newTag = new TagsDb();
            TagsDb persistedTagDb = new TagsDb();

            if (tagDb == null)
            {
                newTag = new TagsDb
                {
                    TagName = tagName,
                    Description = tagName,
                    AccountID = accountId,
                    CreatedBy = userId,
                    IsDeleted = false
                };
                persistedTagDb = dbContext.Tags.Add(newTag);
            }
            else
                persistedTagDb = tagDb;

            if (opportunityID != 0)
            {
                var opportunityTagMap = dbContext.OpportunityTagMap.Where(t => t.TagID == persistedTagDb.TagID & t.OpportunityID == opportunityID).FirstOrDefault();
                if (opportunityTagMap == null)
                {
                    var opportunityTagsDb = new OpportunityTagMap
                    {
                        OpportunityID = opportunityID,
                        Tags = persistedTagDb,
                        TaggedBy = userId,
                        TaggedOn = DateTime.Now.ToUniversalTime()
                    };
                    dbContext.OpportunityTagMap.Add(opportunityTagsDb);
                }
            }
            dbContext.SaveChanges();
            Tag tag = Mapper.Map<TagsDb, Tag>(persistedTagDb);

            if (tagId == 0)
                this.ScheduleAnalyticsRefreshForTags(tag.Id);

            tag.LeadScoreTag = GetLeadScoreTags(persistedTagDb.AccountID, persistedTagDb.TagID);
            return tag;
        }

        /// <summary>
        /// Converts to domain.
        /// </summary>
        /// <param name="tagDatabase">The tag database.</param>
        /// <returns></returns>
        public Tag ConvertToDomain(VTagsDb tagDatabase)
        {
            tagDatabase.LeadScoreTag = GetLeadScoreTags(tagDatabase.AccountID, tagDatabase.TagID);
            Tag tag = Mapper.Map<VTagsDb, Tag>(tagDatabase);
            return tag;
        }

        /// <summary>
        /// Converts to domain
        /// </summary>
        /// <param name="tagDatabase"></param>
        /// <returns></returns>
        public override Tag ConvertToDomain(TagsDb tagDatabase)
        {
            tagDatabase.LeadScoreTag = GetLeadScoreTags(tagDatabase.AccountID, tagDatabase.TagID);
            Tag tag = Mapper.Map<TagsDb, Tag>(tagDatabase);
            return tag;
        }

        /// <summary>
        /// Gets the lead score tags.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <param name="TagID">The tag identifier.</param>
        /// <returns></returns>
        public bool GetLeadScoreTags(int accountId, int TagID)
        {
            var db = ObjectContextFactory.Create();
            bool LeadScoreTag = db.LeadScoreRules.Where(i => i.AccountID == accountId && (i.ConditionID == 6 || i.ConditionID == 7) && i.ConditionValue == TagID.ToString() && i.IsActive == true).Count() > 0;
            return LeadScoreTag;
        }

        /// <summary>
        /// Converts the type of to database.
        /// </summary>
        /// <param name="domainType">Type of the domain.</param>
        /// <param name="db">The database.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">Invalid tag id has been passed. Suspected Id forgery.</exception>
        public override TagsDb ConvertToDatabaseType(Tag domainType, CRMDb db)
        {
            TagsDb tagsDb;
            if (domainType.Id > 0)
            {
                tagsDb = db.Tags.SingleOrDefault(c => c.TagID == domainType.Id && c.IsDeleted != true);
                if (tagsDb == null)
                    throw new ArgumentException("Invalid tag id has been passed. Suspected Id forgery.");
                tagsDb = Mapper.Map<Tag, TagsDb>(domainType, tagsDb);
            }
            else //New tag
            {
                tagsDb = Mapper.Map<Tag, TagsDb>(domainType);
            }
            return tagsDb;
        }

        /// <summary>
        /// Persists the value objects.
        /// </summary>
        /// <param name="domainType">Type of the domain.</param>
        /// <param name="tagsDb">The tags database.</param>
        /// <param name="db">The database.</param>
        public override void PersistValueObjects(Tag domainType, TagsDb tagsDb, CRMDb db)
        {
            //will use this
            //persistTagTypes(domainType, tagsDb, db);
            //persistUsers(domainType, tagsDb, db);
        }

        /// <summary>
        /// Searches the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public IEnumerable<Tag> Search(string name)
        {
            throw new NotImplementedException();
        }

        ///// <summary>
        ///// Finds all.
        ///// </summary>
        ///// <param name="name">The name.</param>
        ///// <param name="accountId">The account identifier.</param>
        ///// <param name="sortField">The sort field.</param>
        ///// <param name="direction">The direction.</param>
        ///// <returns></returns>
        //public IEnumerable<Tag> FindAll(string name, int accountId, string sortField = "", ListSortDirection direction = ListSortDirection.Descending)
        //{
        //    IEnumerable<VTagsDb> tags = FindTagsSummary(accountId, name, sortField, direction);
        //    var tagsd = Mapper.Map<IEnumerable<Tag>>(tags);
        //    return FindLeadScoreRuleTags(accountId, tagsd);
        //}

        /// <summary>
        /// Finds all.
        /// </summary>
        /// <param name="limit">The limit.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="accountId">The account identifier.</param>
        /// <param name="sortField">The sort field.</param>
        /// <param name="direction">The direction.</param>
        /// <returns></returns>
        public IEnumerable<Tag> FindAll(int limit, int pageNumber, string tagName, int accountId, string sortField = "", ListSortDirection direction = ListSortDirection.Descending)
        {
            var records = (pageNumber - 1) * limit;
            IEnumerable<VTagsDb> tags = FindTagsSummary(accountId, tagName, sortField, direction, limit, pageNumber);
            foreach (VTagsDb da in tags)
            {
                yield return ConvertToDomain(da);
            }
        }

        ///// <summary>
        ///// Finds all.
        ///// </summary>
        ///// <param name="name">The name.</param>
        ///// <param name="accountId">The account identifier.</param>
        ///// <returns></returns>
        //public IEnumerable<Tag> FindAll(string name, int accountId)
        //{
        //    IEnumerable<VTagsDb> tags = FindTagsSummary(accountId, name, GetPropertyName<TagsDb, string>(t => t.TagName), ListSortDirection.Descending);
        //    foreach (VTagsDb da in tags)
        //    {
        //        yield return ConvertToDomain(da);
        //    }
        //}

        /// <summary>
        /// Determines whether [is duplicate tag] [the specified tag name].
        /// </summary>
        /// <param name="tagName">Name of the tag.</param>
        /// <param name="accountId">The account identifier.</param>
        /// <param name="tagId">The tag identifier.</param>
        /// <returns></returns>
        public bool IsDuplicateTag(string tagName, int accountId, int tagId)
        {
            IQueryable<VTagsDb> tags = ObjectContextFactory.Create().VTags.Where(c => c.TagName == tagName && c.AccountID == accountId && c.IsDeleted != true);

            return (tags.Where(c => c.TagID != tagId).Any());
        }

        /// <summary>
        /// Deletes the tags.
        /// </summary>
        /// <param name="tagIDs">The tag i ds.</param>
        public IEnumerable<int> DeleteTags(int[] tagIDs, int accountID)
        {
            var db = ObjectContextFactory.Create();
            var ActionTags = db.ActionTags.Where(p => tagIDs.Contains(p.TagID));
            var ContactTags = db.ContactTags.Where(p => tagIDs.Contains(p.TagID) && p.AccountID == accountID);
            var NoteTags = db.NoteTags.Where(p => tagIDs.Contains(p.TagID));
            var CampaignTags = db.CampaignTags.Where(p => tagIDs.Contains(p.TagID));
            var CampaignContactTags = db.CampaignContactTags.Where(p => tagIDs.Contains(p.TagID));
            var OpportunityTags = db.OpportunityTagMap.Where(p => tagIDs.Contains(p.TagID));
            var FormTags = db.FormTags.Where(p => tagIDs.Contains(p.TagID));
            var LeadAdapterTags = db.LeadAdapterTags.Where(p => tagIDs.Contains(p.TagID));
            var ImportTags = db.ImportTagMap.Where(p => tagIDs.Contains(p.TagID));
            var SearchDefinitionTags = db.SearchDefinitionTagMap.Where(p => tagIDs.Contains(p.SearchDefinitionTagID));
            IEnumerable<TagsDb> tagsDb = db.Tags.Where(p => tagIDs.Contains(p.TagID) && p.IsDeleted != true);
            IEnumerable<int> effectedContacts = ContactTags.Select(s => s.ContactID);
            foreach (TagsDb tag in tagsDb)
                tag.IsDeleted = true;

            db.ActionTags.RemoveRange(ActionTags);
            db.ContactTags.RemoveRange(ContactTags);
            db.NoteTags.RemoveRange(NoteTags);
            db.CampaignTags.RemoveRange(CampaignTags);
            db.CampaignContactTags.RemoveRange(CampaignContactTags);
            db.OpportunityTagMap.RemoveRange(OpportunityTags);
            db.FormTags.RemoveRange(FormTags);
            db.LeadAdapterTags.RemoveRange(LeadAdapterTags);
            db.ImportTagMap.RemoveRange(ImportTags);
            db.SearchDefinitionTagMap.RemoveRange(SearchDefinitionTags);
            db.SaveChanges();
            return effectedContacts;
        }

        /// <summary>
        /// Merges the tags.
        /// </summary>
        /// <param name="sourcetagId">The sourcetag identifier.</param>
        /// <param name="destinationtagId">The destinationtag identifier.</param>
        public void MergeTags(int sourcetagId, int destinationtagId, int accountID)
        {
            var db = ObjectContextFactory.Create();

            //var existedActionTags = db.ActionTags.Where(a => a.TagID == destinationtagId).FirstOrDefault();
            //if (existedActionTags != null)
            //    db.ActionTags.Remove(existedActionTags);

            var actionTags = db.ActionTags.Where(a => a.TagID == sourcetagId).ToList();
            foreach (var actionTag in actionTags)
            {
                var actionTagAction = db.ActionTags.Where(a => a.ActionID == actionTag.ActionID).ToList();
                foreach (var action in actionTagAction)
                {
                    if (action.TagID == destinationtagId)
                        db.ActionTags.Remove(action);
                }
            }

            actionTags.ForEach(p => p.TagID = destinationtagId);
            //foreach (var actionTag in actionTags)
            //{
            //    //var actionTagsperAction = db.ActionTags.Where(a => a.ActionID == actionTag.ActionID).ToList();
            //    //foreach (var action in actionTagsperAction)
            //    //{
            //    //    if (action.TagID == sourcetagId)
            //    //    {
            //    //        ActionTagsMapDb actionTagMap = db.ActionTags.FirstOrDefault(a => a.ActionTagMapID == action.ActionTagMapID);
            //    //        db.ActionTags.Remove(actionTagMap);
            //    //        //contact.TagID = destinationtagId;
            //    //    }
            //    //}
            //    actionTag.TagID = destinationtagId;
            //}

            //var existedContactTag = db.ContactTags.Where(a => a.TagID == destinationtagId).FirstOrDefault();
            //if (existedContactTag != null)
            //    db.ContactTags.Remove(existedContactTag);

            var contactTags = db.ContactTags.Where(a => a.TagID == sourcetagId && a.AccountID == accountID).ToList();
            foreach (var contactTag in contactTags)
            {
                var contactTagsContact = db.ContactTags.Where(a => a.ContactID == contactTag.ContactID && a.AccountID == accountID).ToList();
                foreach (var contact in contactTagsContact)
                {
                    if (contact.TagID == destinationtagId)
                        db.ContactTags.Remove(contact);
                }
            }
            contactTags.ForEach(p => p.TagID = destinationtagId);

            //var existedOpportunityTags = db.OpportunityTagMap.Where(a => a.TagID == destinationtagId).FirstOrDefault();
            //if (existedOpportunityTags != null)
            //    db.OpportunityTagMap.Remove(existedOpportunityTags);

            var opportunityTags = db.OpportunityTagMap.Where(a => a.TagID == sourcetagId).ToList();
            foreach (var opportunityTag in opportunityTags)
            {
                var opportunityTagopportunity = db.OpportunityTagMap.Where(a => a.OpportunityID == opportunityTag.OpportunityID).ToList();
                foreach (var opportunity in opportunityTagopportunity)
                {
                    if (opportunity.TagID == destinationtagId)
                        db.OpportunityTagMap.Remove(opportunity);
                }
            }
            opportunityTags.ForEach(p => p.TagID = destinationtagId);

            //foreach (var contactTag in contactTags)
            //{
            //    var contactTagsPerContact = db.ContactTags.Where(a => a.ContactID == contactTag.ContactID).ToList();
            //    foreach (var contact in contactTagsPerContact)
            //    {
            //        if (contact.TagID == sourcetagId)
            //        {
            //            ContactTagMapDb contactTagMap = db.ContactTags.FirstOrDefault(a => a.ContactTagMapID == contact.ContactTagMapID);
            //            db.ContactTags.Remove(contactTagMap);
            //            //contact.TagID = destinationtagId;

            //        }
            //    }
            //    contactTag.TagID = destinationtagId;

            //}

            //var existedNoteTags = db.NoteTags.Where(a => a.TagID == destinationtagId).FirstOrDefault();
            //if (existedNoteTags != null)
            //    db.NoteTags.Remove(existedNoteTags);

            var noteTags = db.NoteTags.Where(a => a.TagID == sourcetagId).ToList();
            foreach (var noteTag in noteTags)
            {
                var noteTagsNote = db.NoteTags.Where(a => a.NoteID == noteTag.NoteID).ToList();
                foreach (var note in noteTagsNote)
                {
                    if (note.TagID == destinationtagId)
                        db.NoteTags.Remove(note);
                }
            }
            noteTags.ForEach(p => p.TagID = destinationtagId);

            //foreach (var noteTag in noteTags)
            //{
            //    //var noteTagsPerNote = db.NoteTags.Where(n => n.NoteID == noteTag.NoteID).ToList();
            //    //foreach (var note in noteTagsPerNote)
            //    //{
            //    //    if (note.TagID == sourcetagId)
            //    //    {
            //    //        NoteTagsMapDb noteTagMap = db.NoteTags.FirstOrDefault(n => n.NoteTagMapID == note.NoteTagMapID);
            //    //        db.NoteTags.Remove(noteTagMap);
            //    //    }
            //    //}

            //    noteTag.TagID = destinationtagId;
            //}

            //var existedCampaignTag = db.CampaignTags.Where(a => a.TagID == destinationtagId).FirstOrDefault();
            //if (existedCampaignTag != null)
            //    db.CampaignTags.Remove(existedCampaignTag);

            var campaignTags = db.CampaignTags.Where(a => a.TagID == sourcetagId).ToList();
            foreach (var campaignTag in campaignTags)
            {
                var campaignTagCampaign = db.CampaignTags.Where(a => a.CampaignID == campaignTag.CampaignID).ToList();
                foreach (var campaign in campaignTagCampaign)
                {
                    if (campaign.TagID == destinationtagId)
                        db.CampaignTags.Remove(campaign);
                }
            }
            campaignTags.ForEach(p => p.TagID = destinationtagId);
            //foreach (var campaignTag in campaignTags)
            //{
            //    campaignTag.TagID = destinationtagId;
            //}

            //var existedCampaignContactTag = db.CampaignContactTags.Where(a => a.TagID == destinationtagId).FirstOrDefault();
            //if (existedCampaignContactTag != null)
            //    db.CampaignContactTags.Remove(existedCampaignContactTag);

            var campaignContactTags = db.CampaignContactTags.Where(a => a.TagID == sourcetagId).ToList();
            foreach (var campaignContactTag in campaignContactTags)
            {
                var campaignContactTagCampaign = db.CampaignContactTags.Where(a => a.CampaignID == campaignContactTag.CampaignID).ToList();
                foreach (var campaign in campaignContactTagCampaign)
                {
                    if (campaign.TagID == destinationtagId)
                        db.CampaignContactTags.Remove(campaign);
                }
            }
            campaignContactTags.ForEach(p => p.TagID = destinationtagId);


            //var existedFormTag = db.FormTags.Where(a => a.TagID == destinationtagId).FirstOrDefault();
            //if (existedFormTag != null)
            //    db.FormTags.Remove(existedFormTag);

            var formTags = db.FormTags.Where(a => a.TagID == sourcetagId).ToList();
            foreach (var formTag in formTags)
            {
                var formTagForm = db.FormTags.Where(a => a.FormID == formTag.FormID).ToList();
                foreach (var form in formTagForm)
                {
                    if (form.TagID == destinationtagId)
                        db.FormTags.Remove(form);
                }
            }
            formTags.ForEach(p => p.TagID = destinationtagId);


            var leadAdapterTags = db.LeadAdapterTags.Where(a => a.TagID == sourcetagId);
            foreach (var leadadapterTag in leadAdapterTags)
            {
                var leadAdapterTagLeadadapter = db.LeadAdapterTags.Where(a => a.LeadAdapterID == leadadapterTag.LeadAdapterID);
                foreach (var leadadapter in leadAdapterTagLeadadapter)
                {
                    if (leadadapter.TagID == destinationtagId)
                        db.LeadAdapterTags.Remove(leadadapter);
                }
            }
            leadAdapterTags.ForEach(p => p.TagID = destinationtagId);

            var importTags = db.ImportTagMap.Where(a => a.TagID == sourcetagId);
            foreach (var importtag in importTags)
            {
                var importTagImport = db.ImportTagMap.Where(a => a.ImportTagMapID == importtag.ImportTagMapID);
                foreach (var import in importTagImport)
                {
                    if (import.TagID == destinationtagId)
                        db.ImportTagMap.Remove(import);
                }
            }
            importTags.ForEach(p => p.TagID = destinationtagId);



            var searchdefinitionTags = db.SearchDefinitionTagMap.Where(a => a.SearchDefinitionTagID == sourcetagId);
            foreach (var searchtag in searchdefinitionTags)
            {
                var searchTagSearch = db.SearchDefinitionTagMap.Where(a => a.SearchDefinitionTagMapID == searchtag.SearchDefinitionTagMapID);
                foreach (var search in searchTagSearch)
                {
                    if (search.SearchDefinitionTagID == destinationtagId)
                        db.SearchDefinitionTagMap.Remove(search);
                }
            }
            searchdefinitionTags.ForEach(p => p.SearchDefinitionTagID = destinationtagId);

            var workflowTriggerTags = db.WorkflowTriggers.Where(a => a.TagID == sourcetagId);
            workflowTriggerTags.ForEach(p => p.TagID = destinationtagId);

            var workflowactiontags = db.WorkflowTagActions.Where(a => a.TagID == sourcetagId);
            workflowactiontags.ForEach(p => p.TagID = destinationtagId);


            TagsDb tagsDb = db.Tags.FirstOrDefault(c => c.TagID == sourcetagId && c.IsDeleted != true);
            if (tagsDb != null)
                tagsDb.IsDeleted = true;

            db.SaveChanges();
        }

        public IDictionary<int, int> TotalContactsByTagIds(IEnumerable<int> tagIds,int accountId)
        {
            IEnumerable<Tag> tagCounts = new List<Tag>() { };
            var emails = new List<byte>()
            {
                (byte)EmailStatus.NotVerified, (byte)EmailStatus.Verified, (byte)EmailStatus.SoftBounce
            };

            using (var db = ObjectContextFactory.Create())
            {
                db.QueryStoredProc("dbo.Get_Tags_Contact_Count", (reader) =>
                {
                    tagCounts = reader.Read<Tag>().ToList();
                }, new
                {
                    TagIds = tagIds.AsTableValuedParameter("dbo.Contact_List"),
                    AccountId = accountId 
                });

                return tagCounts.ToDictionary(k => k.Id, v => v.Count.HasValue ? v.Count.Value : 0);

            }
        }

        /// <summary>
        /// Returns the contact count when contacts are public
        /// </summary>
        /// <param name="tagId"></param>
        /// <param name="accountId"></param>
        /// <returns></returns>
        public int TotalContactsByTag(int tagId, int accountId)
        {
            CRMDb db = ObjectContextFactory.Create();
            var emails = new List<byte>()
            {
                (byte)EmailStatus.NotVerified, (byte)EmailStatus.Verified, (byte)EmailStatus.SoftBounce
            };
            var sql = @"select distinct ce.email from contacttagmap (nolock) ctm
                        inner join contacts  (nolock) c on c.contactid = ctm.contactid and c.accountid = ctm.accountid
                        inner join contactemails (nolock) ce on ce.contactid = c.contactid  and c.accountid = ce.accountid
                        where ce.isprimary = 1 and c.isdeleted = 0 and ctm.tagid = @tagid
                        and ce.EmailStatus in (select datavalue from dbo.Split(@emailstatus,','))";

            var contactCount = db.Get<string>(sql, new { TagId = tagId, emailstatus = string.Join(",", emails.ToArray()) }).Count();
            return contactCount;
        }

        /// <summary>
        /// Returns the contact count when contacts are private
        /// </summary>
        /// <param name="tagId"></param>
        /// <param name="accountId"></param>
        /// <param name="ownerId"></param>
        /// <returns></returns>
        public int TotalContactsByTag(int tagId, int accountId, int? ownerId)
        {
            CRMDb db = ObjectContextFactory.Create();

            var tagContacts = db.ContactTags.Where(ct => ct.TagID == tagId && ct.AccountID == accountId)
               .Join(db.Contacts.Where(c => c.IsDeleted == false && c.OwnerID == (int?)ownerId && c.AccountID == accountId), ct => ct.ContactID,
               c => c.ContactID, (ct, c) => new
               {
                   contactIds = db.ContactEmails.Where(ce => ce.AccountID == accountId
                       && ce.ContactID == ct.ContactID &&
                       (ce.EmailStatus == (byte)EmailStatus.NotVerified || ce.EmailStatus == (byte)EmailStatus.Verified
                       || ce.EmailStatus == (byte)EmailStatus.SoftBounce) && ce.IsPrimary == true).Select(ce => ce.ContactID)
               });

            int contactsCount = tagContacts.Count() > 0 ? tagContacts.Sum(c => c.contactIds.Count()) : 0;
            return contactsCount;
        }

        /// <summary>
        /// Returns the possible unique contacts with requested tags and search definitions
        /// </summary>
        /// <param name="tagIds"></param>
        /// <param name="accountId"></param>
        /// <returns></returns>
        public int TotalContactsByTags(IEnumerable<int> tagIds, int accountId)
        {
            CRMDb db = ObjectContextFactory.Create();
            //var tagContacts = db.ContactTags.Where(ct => tagIds.Contains(ct.TagID))
            //              .Join(db.Contacts.Where(c => c.AccountID == accountId && c.IsDeleted == false
            //                      && c.PrimaryEmail != null && c.DoNotEmail == false),
            //                  ct => ct.ContactID,
            //                  c => c.ContactID,
            //                  (ct, c) => new { ContactID = c.ContactID, EmailId = c.PrimaryEmail }).Distinct().Count();

            var tagContacts = db.ContactTags.Where(ct => tagIds.Contains(ct.TagID) && ct.AccountID == accountId)
             .Join(db.Contacts.Where(c => c.IsDeleted == false && c.AccountID == accountId), ct => ct.ContactID,
             c => c.ContactID, (ct, c) => new
             {
                 contactIds = db.ContactEmails.Where(ce => ce.AccountID == accountId
                     && ce.ContactID == ct.ContactID &&
                     (ce.EmailStatus == (byte)EmailStatus.NotVerified || ce.EmailStatus == (byte)EmailStatus.Verified
                     || ce.EmailStatus == (byte)EmailStatus.SoftBounce) && ce.IsPrimary == true).Select(ce => ce.ContactID)
             });

            int contactsCount = tagContacts.Count() > 0 ? tagContacts.Sum(c => c.contactIds.Count()) : 0;

            return contactsCount;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tagIds"></param>
        /// <param name="accountId"></param>
        /// <returns></returns>
        public int TotalContactsByTags(IEnumerable<int> tagIds, int accountId, int? ownerId)
        {
            CRMDb db = ObjectContextFactory.Create();
            //var tagContacts = db.ContactTags.Where(ct => tagIds.Contains(ct.TagID))
            //              .Join(db.Contacts.Where(c => c.AccountID == accountId && c.IsDeleted == false
            //                      && c.PrimaryEmail != null && c.DoNotEmail == false),
            //                  ct => ct.ContactID,
            //                  c => c.ContactID,
            //                  (ct, c) => new { ContactID = c.ContactID, EmailId = c.PrimaryEmail }).Distinct().Count();

            var tagContacts = db.ContactTags.Where(ct => tagIds.Contains(ct.TagID) && ct.AccountID == accountId)
             .Join(db.Contacts.Where(c => c.IsDeleted == false && c.OwnerID == ownerId && c.AccountID == accountId), ct => ct.ContactID,
             c => c.ContactID, (ct, c) => new
             {
                 contactIds = db.ContactEmails.Where(ce => ce.AccountID == accountId
                     && ce.ContactID == ct.ContactID &&
                     (ce.EmailStatus == (byte)EmailStatus.NotVerified || ce.EmailStatus == (byte)EmailStatus.Verified
                     || ce.EmailStatus == (byte)EmailStatus.SoftBounce) && ce.IsPrimary == true).Select(ce => ce.ContactID)
             });

            int contactsCount = tagContacts.Count() > 0 ? tagContacts.Sum(c => c.contactIds.Count()) : 0;

            return contactsCount;
        }

        /// <summary>
        /// Alls the tags by contacts.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="limit">The limit.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="accountId">The account identifier.</param>
        /// <param name="isAdmin">if set to <c>true</c> [is admin].</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="totalHits">The total hits.</param>
        /// <param name="sortField">The sort field.</param>
        /// <param name="direction">The direction.</param>
        /// <returns></returns>
        public IEnumerable<Tag> AllTagsByContacts(string name, int limit, int pageNumber, int accountId, bool isAdmin, int userId, out int totalHits, int accountID, string sortField = "",
            ListSortDirection direction = ListSortDirection.Descending)
       {   
            var db = ObjectContextFactory.Create();
            IEnumerable<Tag> tagslist = new List<Tag>();
            string sql = string.Empty;
            db.QueryStoredProc("[dbo].[GetTagsReport]", (reader) =>
            {
                tagslist = reader.Read<Tag>().ToList();
                sql = reader.Read<string>().FirstOrDefault();
            }, new { AccountId = accountId, UserId = (!isAdmin ? userId : 0), TagName = name, SortingOrder = direction, SortField = sortField, PageNumber = pageNumber, PageSize = limit });
            Logger.Current.Verbose(sql);

            totalHits = tagslist.Select(t=>t.TotalTagCount).FirstOrDefault();
            return tagslist;
        }

        /// <summary>
        /// Gets the popular tags.
        /// </summary>
        /// <param name="limit">The limit.</param>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        public IEnumerable<Tag> GetPopularTags(int limit, int accountId)
        {
            var db = ObjectContextFactory.Create();
            var sql = @"select vt.TagID Id,CASE	WHEN vtls.Count > 0 THEN vt.TagName + ' *'	ELSE vt.TagName END TagName, vt.Count, 
                        CASE	WHEN vtls.Count > 0 THEN 1	ELSE 0 END LeadScoreTag	from vtags vt 
                        inner join vTagsLeadScore vtls on vt.TagID = vtls.TagID
                        where vt.AccountID = @accountId and IsDeleted = 0
                        order by vt.Count desc offset 0 rows fetch next @limit rows only";


            IEnumerable<Tag> recentTagsOpt = db.Get<Tag>(sql, new { limit = limit, accountId = accountId });
            return recentTagsOpt;

        }

        /// <summary>
        /// Gets the recent tags.
        /// </summary>
        /// <param name="limit">The limit.</param>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        public IEnumerable<Tag> GetRecentTags(int limit, int accountId)
        {
            var db = ObjectContextFactory.Create();
            var sql = @"select vt.TagID Id,CASE	WHEN vtls.Count > 0 THEN vt.TagName + ' *'	ELSE vt.TagName END TagName, vt.Count, 
                        CASE	WHEN vtls.Count > 0 THEN 1	ELSE 0 END LeadScoreTag	from vtags vt 
                        inner join vTagsLeadScore vtls on vt.TagID = vtls.TagID
                        where vt.AccountID = @accountId and IsDeleted = 0
                        order by vt.TagID desc offset 0 rows fetch next @limit rows only";


            IEnumerable<Tag> recentTags = db.Get<Tag>(sql, new { limit = limit, accountId = accountId });
            return recentTags;

        }

        public IEnumerable<RecentPopularTag> GetRecentAndPopularTags(int accountId)
        {
            using (var db = ObjectContextFactory.Create())
            {
                var recentPopularTags = new List<RecentPopularTag>();
                var sql = @"exec dbo.GetRecentPopularTags @accountId";
                db.GetMultiple(sql, (r) =>
                    {
                        recentPopularTags = r.Read<RecentPopularTag>().ToList();
                        recentPopularTags.AddRange(r.Read<RecentPopularTag>().ToList());
                    }, new { accountId = accountId });

                return recentPopularTags;
            }
        }


        /// <summary>
        /// Saves the contact tags.
        /// </summary>
        /// <param name="contacts">The contacts.</param>
        /// <param name="opportunities">The opportunities.</param>
        /// <param name="tags">The tags.</param>
        /// <param name="accountId">The account identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns></returns>
        public List<Tag> SaveContactTags(IEnumerable<int> contacts, List<Opportunity> opportunities, IEnumerable<Tag> tags, int accountId, int userId)
        {
            if (tags != null && tags.Any())
            {
                List<Tag> persistedTagDb = new List<Tag>();
                foreach (Tag tag in tags)
                {
                    if (tag != null)
                    {
                        Tag tagDb = SaveContactsForTag(contacts, tag.TagName, tag.Id, accountId, userId);
                        persistedTagDb.Add(tagDb);
                        foreach (Opportunity opportunity in opportunities)
                        {
                            Tag tagDbs = SaveOpportunityTag(opportunity.Id, tag.TagName, tag.Id, accountId, userId);
                            persistedTagDb.Add(tagDbs);
                        }
                    }
                }
                return persistedTagDb;
            }
            return null;
        }

        /// <summary>
        /// Determines whether [is associated with workflows] [the specified tag identifier].
        /// </summary>
        /// <param name="TagID">The tag identifier.</param>
        /// <returns></returns>
        public bool isAssociatedWithWorkflows(int[] TagID)
        {
            var db = ObjectContextFactory.Create();
            int triggerTags = db.WorkflowTriggers.Include(c => c.Workflows).Where(c => c.TagID.HasValue && TagID.Contains(c.TagID.Value) && c.Workflows.IsDeleted != true).Count();
            int actionTags = db.WorkflowTagActions.Include(c => c.WorkflowAction.Workflow).Where(c => TagID.Contains(c.TagID) && c.WorkflowAction.Workflow.IsDeleted != true).Count();
            return triggerTags + actionTags > 0;
        }

        /// <summary>
        /// Determines whether [is associated with lead score rules] [the specified tag identifier].
        /// </summary>
        /// <param name="TagID">The tag identifier.</param>
        /// <returns></returns>
        public bool isAssociatedWithLeadScoreRules(int[] TagID)
        {
            var db = ObjectContextFactory.Create();
            IEnumerable<string> TagIDs = TagID.Select(x => x.ToString());
            return db.LeadScoreRules.Where(c => (c.ConditionID == 6 || c.ConditionID == 7)
                                            && c.IsActive == true
                                            && TagIDs.Contains(c.ConditionValue)).
                                            Join(db.Tags.Where(i => i.IsDeleted != true), i => i.ConditionValue, j => j.TagID.ToString(), (i, j) => i.ConditionID)
                                            .Count() > 0;
        }

        /// <summary>
        /// Gets the name of the property.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TR">The type of the r.</typeparam>
        /// <param name="property">The property.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">The lambda expression 'property' should point to a valid Property</exception>
        string GetPropertyName<T, TR>(Expression<Func<T, TR>> property)
        {
            var propertyInfo = (property.Body as MemberExpression).Member as PropertyInfo;
            if (propertyInfo == null)
            {
                throw new ArgumentException("The lambda expression 'property' should point to a valid Property");
            }
            return propertyInfo.Name;
        }

        /// <summary>
        /// Finds the by i ds.
        /// </summary>
        /// <param name="TagIDs">The tag i ds.</param>
        /// <returns></returns>
        public IEnumerable<Tag> FindByIDs(int[] TagIDs)
        {
            var db = ObjectContextFactory.Create();
            var tags = db.VTags.Where(c => TagIDs.Contains(c.TagID) && c.IsDeleted != true);
            foreach (VTagsDb tag in tags)
                yield return ConvertToDomain(tag);
        }

        public List<int> GetContactsByTag(int tagId, int accountID)
        {
            var db = ObjectContextFactory.Create();
            List<int> contactIds = db.ContactTags.Where(p => p.TagID == tagId && p.AccountID == accountID).Select(p => p.ContactID).ToList();
            return contactIds;
        }

        /// <summary>
        /// Search tags by name.
        /// </summary>
        /// <param name="AccountID">The account identifier.</param>
        /// <returns></returns>
        public IEnumerable<Tag> SearchTagsByTagName(int AccountID, string tagName, int limit)
        {
            var db = ObjectContextFactory.Create();
            var tagsDb = db.VTags.Where(p => p.AccountID == AccountID && p.IsDeleted == false && p.TagName.IndexOf(tagName) == 0).OrderBy(p => p.TagName).Take(limit).ToList();
            foreach (VTagsDb tag in tagsDb)
                yield return Mapper.Map<VTagsDb, Tag>(tag);
        }

        public int GetContactsCountByTagId(int tagId)
        {
            var db = ObjectContextFactory.Create();
            var sql = @"SELECT COUNT FROM vTags (NOLOCK) WHERE TagID=@TagId";
            return db.Get<int>(sql, new { TagId = tagId }).FirstOrDefault();
        }

        public IEnumerable<string> GetTagNamesByIds(List<int> tagIds)
        {
            var db = ObjectContextFactory.Create();
            var sql = @"SELECT TagName FROM Tags (NOLOCK) WHERE TagID IN @TagIds";
            return db.Get<string>(sql, new { TagIds = tagIds }).ToList();
        }

        public IEnumerable<Tag> GetTagsByName(string tagName, int accountId)
        {
            using(var db = ObjectContextFactory.Create())
            {
                var sql = @"SELECT TOP 15 TagID AS Id, TagName,Count FROM Tags(NOLOCK) WHERE TagName LIKE @Name and AccountID=@AccountId AND IsDeleted=0";
                return db.Get<Tag>(sql, new { Name = tagName, AccountId = accountId }).ToList(); 
            }
        }

    }
}

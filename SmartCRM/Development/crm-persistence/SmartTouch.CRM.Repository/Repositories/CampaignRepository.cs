using SmartTouch.CRM.Domain.ValueObjects;
using AutoMapper;
using LandmarkIT.Enterprise.Extensions;
using LandmarkIT.Enterprise.Utilities.ExceptionHandling;
using LandmarkIT.Enterprise.Utilities.Logging;
using LinqKit;
using SmartTouch.CRM.Domain.Campaigns;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.Images;
using SmartTouch.CRM.Domain.Reports;
using SmartTouch.CRM.Domain.Search;
using SmartTouch.CRM.Domain.Tags;
using SmartTouch.CRM.Domain.Users;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using SmartTouch.CRM.Repository.Database;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;

namespace SmartTouch.CRM.Repository.Repositories
{
    public class CampaignRepository : Repository<Campaign, int, CampaignsDb>, ICampaignRepository
    {
        public CampaignRepository(IUnitOfWork unitOfWork, IObjectContextFactory objectContextFactory)
            : base(unitOfWork, objectContextFactory)
        { }

        /// <summary>
        /// Finds all.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Campaign> FindAll()
        {
            return FindAll(0);
        }

        /// <summary>
        /// Finds all.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        public IEnumerable<Campaign> FindAll(int accountId = 0)
        {
            var predicate = PredicateBuilder.True<CampaignsDb>();
            if (accountId != 0)
                predicate = predicate.And(c => c.AccountID == accountId);
            predicate = predicate.And(c => c.IsDeleted == false);

            IEnumerable<CampaignsDb> campaigns = ObjectContextFactory.Create().Campaigns.Include(c => c.Tags).AsExpandable()
                .Where(predicate)
                .Select(c => new
                {
                    CampaignID = c.CampaignID,
                    AccountID = c.AccountID,
                    CampaignStatusID = c.CampaignStatusID,
                    CampaignTemplateID = c.CampaignTemplateID,
                    CreatedBy = c.CreatedBy,
                    CreatedDate = c.CreatedDate,
                    ServiceProviderID = c.ServiceProviderID,
                    From = c.From,
                    Name = c.Name,
                    ScheduleTime = c.ScheduleTime,
                    Subject = c.Subject,
                    IsDeleted = false,
                    ProcessedDate = c.ProcessedDate,
                    LastUpdatedOn = c.LastUpdatedOn
                }).ToList().OrderByDescending(c => c.CampaignID).Select(x => new CampaignsDb()
                {
                    CampaignID = x.CampaignID,
                    AccountID = x.AccountID,
                    CampaignStatusID = x.CampaignStatusID,
                    CampaignTemplateID = x.CampaignTemplateID,
                    CreatedBy = x.CreatedBy,
                    CreatedDate = x.CreatedDate,
                    ServiceProviderID = x.ServiceProviderID,
                    From = x.From,
                    Name = x.Name,
                    ScheduleTime = x.ScheduleTime,
                    Subject = x.Subject,
                    IsDeleted = false,
                    ProcessedDate = x.ProcessedDate,
                    LastUpdatedOn = x.LastUpdatedOn
                });

            return Mapper.Map<IEnumerable<CampaignsDb>, IEnumerable<Campaign>>(campaigns);
        }

        public IEnumerable<Campaign> FindAll(int accountId, int campaignStatusId, int pageNumber, int pageSize, int[] userIds, DateTime? startDate, DateTime? endDate, string name = "", string sortField = "", ListSortDirection direction = ListSortDirection.Descending)
        {
            var skip = (pageNumber - 1) * pageSize;
            var take = pageSize;
            var db = ObjectContextFactory.Create();
            var statuses = new List<CampaignStatus>();
            string sortCampaignStatus = string.Empty;
            string forCampaignReport = string.Empty;

            if (campaignStatusId == 0)
            {
                CampaignStatus[] allStatuses = {   CampaignStatus.Draft,
                                                   CampaignStatus.Queued,
                                                   CampaignStatus.Analyzing,
                                                   CampaignStatus.Sending,
                                                   CampaignStatus.Sent,
                                                   CampaignStatus.Active,
                                                   CampaignStatus.Cancelled,
                                                   CampaignStatus.Failure,
                                                   CampaignStatus.Scheduled,
                                                   CampaignStatus.Queued,
                                                   CampaignStatus.Delayed,
                                                   CampaignStatus.Retrying
                };
                statuses.AddRange(allStatuses);
            }
            else
                statuses.Add(((CampaignStatus)campaignStatusId));

            #region Sorting
            var sortFieldMappings = new Dictionary<string, string>()
                {
                    {"Name", "C.Name"},
                    {"RecipientCount","CA.Recipients"},
                    {"SentCount", "Sent"},
                    {"DeliveredCount","Delivered"},
                    {"OpenCount","Opened"},
                    {"ClickCount","Clicked"},
                    {"ComplaintCount","Complained"},
                    {"OptOutCount","OptedOut"},
                    {"CampaignStatus", "S.Name"}
                };

            Func<string, string> GetSortField = (s) =>
            {
                if (sortFieldMappings.ContainsKey(s))
                    return sortFieldMappings[s];
                else
                    return s;
            };
            sortField = sortField == null ? (((CampaignStatus)campaignStatusId) == CampaignStatus.Sent ? "ProcessedDate" : "LastUpdatedOn") : sortField;
            if (sortField.Contains("CampaignStatus"))
                sortCampaignStatus = ", CampaignStatusID ASC";

            string sortDirection = direction == ListSortDirection.Ascending ? "ASC" : "DESC";
            #endregion

            name = "%" + name + "%";

            #region Query
            if (startDate != null && endDate != null)
                forCampaignReport = string.Format("AND C.CreatedDate BETWEEN @startDate AND @endDate AND C.CreatedBy in ({0})", string.Join(",", userIds) );

            var sql = string.Format(@"SELECT C.CampaignID Id, C.Name, C.AccountId AccountID,C.CampaignStatusID CampaignStatus, C.ProcessedDate, C.IsLinkedToWorkflows,C.ScheduleTime,
			                        COALESCE(CA.Recipients,0) RecipientCount, 
			                        COALESCE(CA.Sent,0) [SentCount], 
			                        COALESCE(CA.Delivered,0) DeliveredCount, 
			                        COALESCE(CA.Opened,0) OpenCount, 
			                        COALESCE(CA.Clicked,0) ClickCount, 
			                        COALESCE(CA.Complained,0) ComplaintCount, 
			                        COALESCE(CA.OptedOut,0) OptOutCount,
			                        COALESCE(C.LastUpdatedOn,0) LastUpdatedOn,
                                    C.CampaignTypeID CampaignTypeId, 
			                        COUNT(1) OVER() as TotalCampaignCount FROM Campaigns (NOLOCK) C 
                        LEFT OUTER JOIN CampaignAnalytics CA (NOLOCK) ON C.CampaignID = CA.CampaignID
                        INNER JOIN Statuses (NOLOCK) S ON S.StatusID = C.CampaignStatusID
                        WHERE C.AccountID = @accountId AND C.IsDeleted = 0 AND C.CampaignStatusID IN @statuses
                        {3}
                        AND C.Name LIKE @name 
                        ORDER BY {0} {1} {2}
                        OFFSET @skip ROWS
                        FETCH NEXT @take ROWS ONLY", GetSortField(sortField), sortDirection, sortCampaignStatus, forCampaignReport);

            var campaigns = db.Get<Campaign>(sql, new
            {
                accountId = accountId,
                skip = skip,
                take = take,
                name = name,
                statuses = statuses,
                startDate = startDate,
                endDate = endDate
            });
            #endregion


            return campaigns;
        }
        /// <summary>
        /// Searches the campaign.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public IEnumerable<Campaign> SearchCampaign(string name)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Finds the by campaign date.
        /// </summary>
        /// <param name="campaignDate">The campaign date.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public IEnumerable<Campaign> FindByCampaignDate(DateTime campaignDate)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Finds the by email.
        /// </summary>
        /// <param name="email">The email.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public IEnumerable<Campaign> FindByEmail(string email)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the campaign by identifier.
        /// </summary>
        /// <param name="campaignId">The campaign identifier.</param>
        /// <returns></returns>
        public Campaign GetCampaignById(int campaignId)
        {
            CampaignsDb campaignDatabase = GetCampaignsDb(campaignId);

            if (campaignDatabase != null)
            {
                Campaign campaignDatabaseConvertedToDomain = ConvertToDomain(campaignDatabase);
                return campaignDatabaseConvertedToDomain;
            }
            return null;
        }

        /// <summary>
        /// Gets the campaigns database.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        CampaignsDb GetCampaignsDb(int id)
        {
            var db = ObjectContextFactory.Create();
            try
            {
                var sql = @"select c.*,isNull(cpcm.PlainTextContent,'') PlainTextContent from Campaigns (nolock) c
                                left join campaignplaintextcontentmap (nolock) cpcm on cpcm.CampaignID = c.CampaignID
                                where c.campaignid = @campaignId;
                            select * from UserSocialMediaPosts where campaignid = @campaignId;
                    
                            ";
                var campaignResult = new CampaignsDb();
                db.GetMultiple(sql, (r) =>
                {
                    var campaign = r.Read<CampaignsDb>().ToList();
                    var posts = r.Read<UserSocialMediaPostsDb>().ToList();

                    campaignResult = campaign.FirstOrDefault();
                    if (campaignResult != null)
                    {
                        campaignResult.Posts = posts;
                    }
                }, new { campaignId = id });
                return campaignResult;

            }
            catch (Exception ex)
            {
                Logger.Current.Error("Exception occurred while getting campaign.", ex);
                return null;
            }
        }

        /// <summary>
        /// Gets the contact campaign map identifier.
        /// </summary>
        /// <param name="campaignId">The campaign identifier.</param>
        /// <param name="contactId">The contact identifier.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public int GetContactCampaignMapId(int campaignId, int contactId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the unique campaign i ds by recipients.
        /// </summary>
        /// <param name="campaignRecipients">The campaign recipients.</param>
        /// <returns></returns>
        public IEnumerable<int> GetUniqueCampaignIDsByRecipients(IEnumerable<int> campaignRecipients, int accountId)
        {
            Logger.Current.Verbose("Request received to fetch campaign ids by campaign recipients");
            var db = ObjectContextFactory.Create();
            IEnumerable<int> campaignIds = db.CampaignRecipients.Where(c => campaignRecipients.Contains(c.CampaignRecipientID) && c.AccountID == accountId).Select(c => c.CampaignID);
            return campaignIds.Distinct();
        }

        /// <summary>
        /// Campaigns the contacts count.
        /// </summary>
        /// <param name="campaignId">The campaign identifier.</param>
        /// <returns></returns>
        public int CampaignContactsCount(int campaignId, int accountId)
        {
            var db = ObjectContextFactory.Create();
            return db.Get<int>("select count(1) from CampaignRecipients (nolock) where campaignid = @cid and accountId = @accountId", new { cid = campaignId, accountId = accountId }).FirstOrDefault();
        }

        /// <summary>
        /// Deletes the campaign.
        /// </summary>
        /// <param name="campaignId">The campaign identifier.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void DeleteCampaign(int campaignId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Finds the by.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public override Campaign FindBy(int id)
        {
            var db = ObjectContextFactory.Create();
            CampaignsDb campaignDatabase = db.Campaigns.SingleOrDefault(c => c.CampaignID == id);

            if (campaignDatabase != null)
            {
                Campaign contactDatabaseConvertedToDomain = ConvertToDomain(campaignDatabase);
                return contactDatabaseConvertedToDomain;
            }
            return null;
        }

        /// <summary>
        /// Determines whether [is campaign name unique] [the specified campaign].
        /// </summary>
        /// <param name="campaign">The campaign.</param>
        /// <returns></returns>
        public bool IsCampaignNameUnique(Campaign campaign)
        {
            var db = ObjectContextFactory.Create();
            var campaignFound = db.Campaigns.Where(c => c.Name == campaign.Name && c.AccountID == campaign.AccountID && c.IsDeleted == false).Select(c => c).FirstOrDefault();
            if (campaignFound != null && campaign.Id != campaignFound.CampaignID)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Gets the templates.
        /// </summary>
        /// <param name="campaignTemplateId">The campaign template identifier.</param>
        /// <returns></returns>
        public IEnumerable<CampaignTemplate> GetTemplates(string campaignTemplateId)
        {
            var db = ObjectContextFactory.Create();
            IEnumerable<CampaignTemplatesDb> campaignTemplatesDb = db.CampaignTemplates.Where(c => c.CampaignTemplateID.Equals(campaignTemplateId)).ToList();
            if (campaignTemplatesDb != null)
                return convertToDomainTemplates(campaignTemplatesDb);
            return null;
        }

        /// <summary>
        /// Gets the template names.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        public IEnumerable<CampaignTemplate> GetTemplateNames(int accountId)
        {
            var db = ObjectContextFactory.Create();
            IEnumerable<CampaignTemplate> Templates = new List<CampaignTemplate>();
            Templates = db.CampaignTemplates.Where(a => (a.AccountId == accountId || a.AccountId == null)).Select(s => new { Name = s.Name, Id = s.CampaignTemplateID }).ToList().Select(s => new CampaignTemplate() { Id = s.Id, Name = s.Name });
            return Templates;
        }

        /// <summary>
        /// Converts to domain templates.
        /// </summary>
        /// <param name="campaignTemplatesDb">The campaign templates database.</param>
        /// <returns></returns>
        IEnumerable<CampaignTemplate> convertToDomainTemplates(IEnumerable<CampaignTemplatesDb> campaignTemplatesDb)
        {
            foreach (CampaignTemplatesDb template in campaignTemplatesDb)
            {
                yield return new CampaignTemplate() { Id = template.CampaignTemplateID, Type = template.Type };
            }
        }

        /// <summary>
        /// Converts the type of to database.
        /// </summary>
        /// <param name="domainType">Type of the domain.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">Invalid campaign id has been passed. Suspected Id forgery.</exception>
        public override CampaignsDb ConvertToDatabaseType(Campaign domainType, CRMDb context)
        {
            CampaignsDb campaignsDb;
            if (domainType.Id > 0)
            {
                campaignsDb = context.Campaigns.SingleOrDefault(c => c.CampaignID == domainType.Id);

                if (campaignsDb == null)
                    throw new ArgumentException("Invalid campaign id has been passed. Suspected Id forgery.");

                campaignsDb = Mapper.Map<Campaign, CampaignsDb>(domainType, campaignsDb);
            }
            else
            {
                campaignsDb = Mapper.Map<Campaign, CampaignsDb>(domainType);
            }
            return campaignsDb;
        }

        /// <summary>
        /// Converts to domain.
        /// </summary>
        /// <param name="databaseType">Type of the database.</param>
        /// <returns></returns>
        public override Campaign ConvertToDomain(CampaignsDb databaseType)
        {
            //var campaignDb = GetCampaignsDb(databaseType.CampaignID);
            Campaign campaign = new Campaign();
            Mapper.Map<CampaignsDb, Campaign>(databaseType, campaign);

            if (databaseType.Contacts != null)
            {
                IList<Contact> contacts = new List<Contact>();
                foreach (ContactsDb contactDb in databaseType.Contacts)
                {
                    if (contactDb.ContactType == ContactType.Person)
                        contacts.Add(Mapper.Map<ContactsDb, Person>(contactDb));
                    else
                        contacts.Add(Mapper.Map<ContactsDb, Company>(contactDb));
                }
                campaign.Contacts = contacts;

            }
            if (databaseType.CampaignRecipients != null)
            {
                IList<CampaignRecipient> campaignRecipients = new List<CampaignRecipient>();
                foreach (var campaignRecipientDb in databaseType.CampaignRecipients)
                {
                    campaignRecipients.Add(Mapper.Map<CampaignRecipientsDb, CampaignRecipient>(campaignRecipientDb));
                }
                campaign.CampaignRecipients = campaignRecipients;
            }
            campaign.SentCount = CampaignContactsCount(databaseType.CampaignID, databaseType.AccountID);
            return campaign;
        }

        /// <summary>
        /// Persists the value objects.
        /// </summary>
        /// <param name="domainType">Type of the domain.</param>
        /// <param name="dbType">Type of the database.</param>
        /// <param name="context">The context.</param>
        public override void PersistValueObjects(Campaign domainType, CampaignsDb dbType, CRMDb context)
        {
            PersistCampaignContactTags(domainType, dbType, context);
            PersistCampaignTags(domainType, dbType, context);
            PersistCampaignSearchDefinitions(domainType, dbType, context);
            PersistCampaignLinks(domainType, dbType, context);
            PersistUserPosts(domainType, dbType, context);
            PersistPlainTextContent(domainType, dbType, context);
        }

        private void PersistPlainTextContent(Campaign domainType, CampaignsDb dbType, CRMDb context)
        {
            Logger.Current.Verbose("Persisting Plain Text Content");

            if (dbType.CampaignID > 0)
            {
                var plainTextContentDb = context.CampaignPlainTextContent.Where(c => c.CampaignID == dbType.CampaignID).FirstOrDefault();
                if (plainTextContentDb != null)
                {
                    Logger.Current.Informational("PlainTextContend id : " + plainTextContentDb.CampaignPlainTextContentMapID);
                    plainTextContentDb.PlainTextContent = domainType.PlainTextContent;
                    context.Entry(plainTextContentDb).State = System.Data.Entity.EntityState.Modified;
                }
                else
                {
                    Logger.Current.Informational("PlainTextContend not found. Adding now.");
                    plainTextContentDb = new CampaignPlainTextContentDb()
                    {
                        CampaignID = dbType.CampaignID,
                        PlainTextContent = domainType.PlainTextContent
                    };
                    context.Entry(plainTextContentDb).State = System.Data.Entity.EntityState.Added;
                }
            }
            else
            {
                var plainTextContentDb = new CampaignPlainTextContentDb()
                {
                    CampaignID = dbType.CampaignID,
                    PlainTextContent = domainType.PlainTextContent ?? string.Empty
                };
                context.Entry(plainTextContentDb).State = System.Data.Entity.EntityState.Added;
            }

        }
        /// <summary>
        /// Persists the user posts.
        /// </summary>
        /// <param name="domainType">Type of the domain.</param>
        /// <param name="dbType">Type of the database.</param>
        /// <param name="context">The context.</param>
        private void PersistUserPosts(Campaign domainType, CampaignsDb dbType, CRMDb context)
        {
            var by = (dbType.CampaignID > 0 && dbType.LastUpdatedBy.HasValue) ? dbType.LastUpdatedBy.Value : dbType.CreatedBy;
            var posts = context.UserSocialMediaPosts.Where(post => post.CampaignID == dbType.CampaignID && post.UserID == by)
                .Select(post => new UserSocialMediaPosts()
                {
                    UserSocialMediaPostID = post.UserSocialMediaPostID,
                    CommunicationType = post.CommunicationType,
                    Post = post.Post,
                    AttachmentPath = post.AttachmentPath,
                    CampaignID = post.CampaignID,
                    UserID = post.UserID
                });
            var dbPosts = Mapper.Map<IEnumerable<UserSocialMediaPostsDb>>(posts);
            dbPosts.ForEach(post =>
            {
                if (dbType.Posts.Any(spost => spost.UserID == post.UserID && spost.CampaignID == post.CampaignID && spost.CommunicationType == post.CommunicationType))
                {
                    var vpost = dbType.Posts.Single(spost => spost.UserID == post.UserID && spost.CampaignID == post.CampaignID && spost.CommunicationType == post.CommunicationType);
                    vpost.UserSocialMediaPostID = post.UserSocialMediaPostID;
                    context.Entry(vpost).State = System.Data.Entity.EntityState.Modified;
                }
                else
                {
                    context.Entry(post).State = System.Data.Entity.EntityState.Deleted;
                }
            });
        }

        /// <summary>
        /// Persists the campaign links.
        /// </summary>
        /// <param name="campaign">The campaign.</param>
        /// <param name="campaignsDb">The campaigns database.</param>
        /// <param name="db">The database.</param>
        /// <exception cref="LandmarkIT.Enterprise.Utilities.ExceptionHandling.UnsupportedOperationException">[|The campaign cannot be saved because the deleted links are associated with automation workflow|]</exception>
        public void PersistCampaignLinks(Campaign campaign, CampaignsDb campaignsDb, CRMDb db)
        {
            Logger.Current.Informational("Request received to update the links & campaignId is : " + campaign.Id);
            var campaignLinks = db.CampaignLinks.Where(c => c.CampaignID == campaign.Id).ToList();
            if (campaign.Links != null)
            {
                foreach (var link in campaign.Links)
                {
                    if (campaign.Id == 0)
                    {
                        link.CampaignLinkId = 0;
                    }
                    if (link.CampaignLinkId != 0)
                    {
                        var campaignLinkMap = campaignLinks.SingleOrDefault(r => r.CampaignLinkID == link.CampaignLinkId);

                        if (campaignLinkMap != null)
                        {
                            campaignLinkMap.URL = link.URL.URL;
                            campaignLinkMap.LinkIndex = link.LinkIndex;
                        }
                        else
                        {
                            campaignLinkMap = new CampaignLinksDb();
                            campaignLinkMap.CampaignLinkID = 0;
                            campaignLinkMap.URL = link.URL.URL;
                            campaignLinkMap.CampaignID = campaign.Id;
                            campaignLinkMap.LinkIndex = link.LinkIndex;
                            db.CampaignLinks.Add(campaignLinkMap);
                        }
                    }
                    else
                    {
                        CampaignLinksDb map = new CampaignLinksDb();
                        map.URL = link.URL.URL;
                        map.CampaignID = campaign.Id;
                        map.LinkIndex = link.LinkIndex;
                        //map.LinkText = link.LinkText;
                        db.CampaignLinks.Add(map);
                    }
                }
                IList<int> linkIDs = campaign.Links.Where(n => n.CampaignLinkId > 0).Select(n => n.CampaignLinkId).ToList();
                var unMapCampaignLinks = campaignLinks.Where(n => !linkIDs.Contains(n.CampaignLinkID));

                //var workFlowLinks = db.WorkflowTriggers.Join(db.Workflows,
                //               wt => wt.WorkflowID,
                //               wf => wf.WorkflowID,
                //               (wt, wf) => new { WorkFlow = wf, WorkflowTrigger = wt })
                //               .Where(c => c.WorkflowTrigger.CampaignID == campaign.Id && c.WorkflowTrigger.SelectedLinks != null && c.WorkFlow.IsDeleted == false)
                //               .Select(l => l.WorkflowTrigger.SelectedLinks).ToArray();
                var db1 = ObjectContextFactory.Create();
                var sql = @"select wcal.LinkID from  Workflows (NOLOCK) W
                            INNER JOIN WorkflowActions (NOLOCK) WA ON WA.WorkflowID = W.WorkflowID
                            INNER JOIN WorkflowCampaignActions (NOLOCK) WCA ON WCA.WorkflowActionID = WA.WorkflowActionID
                            INNER JOIN WorkflowCampaignActionLinks (NOLOCK) WCAL ON WCAL.ParentWorkflowActionID = WCA.WorkflowCampaignActionID
                            WHERE WCA.CampaignID = @campaignID AND W.IsDeleted=0";
                IEnumerable<int> WorkflowAssociatedLinkIds = db1.Get<int>(sql, new { campaignID = campaign.Id }).ToList();
                //IEnumerable<string> workflowAssociatedLinks = string.Join(",", workFlowLinks).Split(',').ToList().Distinct();

                foreach (CampaignLinksDb campaignLinkUnMapDb in unMapCampaignLinks)
                {
                    foreach (int workFlowLink in WorkflowAssociatedLinkIds)
                    {
                        if (workFlowLink != campaignLinkUnMapDb.CampaignLinkID)
                        {
                            Logger.Current.Informational("Requesting to delete the Link & LinkId Is: " + campaignLinkUnMapDb.CampaignLinkID);
                            db.CampaignLinks.Remove(campaignLinkUnMapDb);
                        }
                        else
                        {
                            Logger.Current.Informational("This link is associated with workflow & LinkId id: " + campaignLinkUnMapDb.CampaignLinkID);
                            throw new UnsupportedOperationException("[|The campaign cannot be saved because the deleted links are associated with automation workflow|]");
                        }
                    }
                }
            }
            else if (campaignLinks != null && campaignLinks.Any())
            {
                foreach (var campaignLink in campaignLinks)
                {
                    db.Entry(campaignLink).State = System.Data.Entity.EntityState.Modified;
                }
            }
        }

        /// <summary>
        /// Persists the campaign contact tags.
        /// </summary>
        /// <param name="campaign">The campaign.</param>
        /// <param name="campaignsDb">The campaigns database.</param>
        /// <param name="db">The database.</param>
        public void PersistCampaignContactTags(Campaign campaign, CampaignsDb campaignsDb, CRMDb db)
        {
            if (campaign.ContactTags != null)
            {
                var tagIds = campaign.ContactTags.Select(ct => ct.Id).ToList();

                var dapperDb = ObjectContextFactory.Create();
                string tagsSql = @"SELECT * FROM vTags
                                   WHERE TagID IN @tagIds AND IsDeleted = 0";
                var tagDbs = dapperDb.Get<TagsDb>(tagsSql, new { tagIds = tagIds });

                var mappedTagIds = db.CampaignContactTags.Where(cc => tagIds.Contains(cc.TagID) && cc.CampaignID == campaign.Id)
                    .Select(cc => cc.TagID).Distinct().ToList();
                foreach (TagsDb tagDb in tagDbs)
                {
                    if (mappedTagIds.Contains(tagDb.TagID))
                        continue;
                    var campaignContactTag = new CampaignContactTagMapDb()
                    {
                        CampaignID = campaignsDb.CampaignID,
                        TagID = tagDb.TagID
                    };
                    db.CampaignContactTags.Add(campaignContactTag);
                }

                var unMappedTags = db.CampaignContactTags.Where(cc => !tagIds.Contains(cc.TagID) && cc.CampaignID == campaign.Id)
                    .Select(cc => cc).Distinct().ToList();

                foreach (CampaignContactTagMapDb ccTagMapdb in unMappedTags)
                    db.CampaignContactTags.Remove(ccTagMapdb);
            }
        }

        /// <summary>
        /// Persists the campaign search definitions.
        /// </summary>
        /// <param name="campaign">The campaign.</param>
        /// <param name="campaignsDb">The campaigns database.</param>
        /// <param name="db">The database.</param>
        public void PersistCampaignSearchDefinitions(Campaign campaign, CampaignsDb campaignsDb, CRMDb db)
        {
            var searchDefinitionIds = campaign.SearchDefinitions.Select(sd => sd.Id).ToList();
            if (searchDefinitionIds != null && searchDefinitionIds.Count != 0)
            {
                var searchDefinitionDbs = db.SearchDefinitions.Where(sd => searchDefinitionIds.Contains(sd.SearchDefinitionID));
                var mappedSearchDefinitionIds = db.CampaignSearchDefinitions.Where(csd => searchDefinitionIds
                    .Contains(csd.SearchDefinitionID) && csd.CampaignID == campaign.Id)
                    .Select(csd => csd.SearchDefinitionID).Distinct().ToList();
                foreach (SearchDefinitionsDb searchDefinitionDb in searchDefinitionDbs)
                {
                    if (mappedSearchDefinitionIds.Contains(searchDefinitionDb.SearchDefinitionID))
                        continue;
                    var campaignSearchDefinitionDb = new CampaignSearchDefinitionsDb()
                    {
                        Campaign = campaignsDb,
                        SearchDefinition = searchDefinitionDb
                    };
                    db.CampaignSearchDefinitions.Add(campaignSearchDefinitionDb);
                }
            }
            var unMappedSearchDefinitions = db.CampaignSearchDefinitions.Where(csd => !searchDefinitionIds
                .Contains(csd.SearchDefinitionID) && csd.CampaignID == campaign.Id)
                .Select(csd => csd).Distinct().ToList();
            foreach (CampaignSearchDefinitionsDb campaignSearchDefinitionDb in unMappedSearchDefinitions)
            {
                db.CampaignSearchDefinitions.Remove(campaignSearchDefinitionDb);
            }
        }

        /// <summary>
        /// Persists the campaign tags.
        /// </summary>
        /// <param name="campaign">The campaign.</param>
        /// <param name="campaignsDb">The campaigns database.</param>
        /// <param name="db">The database.</param>
        public void PersistCampaignTags(Campaign campaign, CampaignsDb campaignsDb, CRMDb db)
        {
            var campaignTags = db.CampaignTags.Where(a => a.CampaignID == campaign.Id).ToList();
            if (campaignTags != null)
            {
                foreach (Tag tag in campaign.Tags)
                {
                    if (tag.Id == 0)
                    {
                        var dapperDb = ObjectContextFactory.Create();
                        string tagsSql = @"SELECT * FROM vTags WHERE TagName = @tagName AND AccountID = @accountId AND IsDeleted = 0";
                        var tagDb = dapperDb.Get<TagsDb>(tagsSql, new { tagName = tag.TagName, accountId = tag.AccountID }).FirstOrDefault();

                        if (tagDb == null)
                        {
                            tagDb = Mapper.Map<Tag, TagsDb>(tag);
                            tagDb.IsDeleted = false;
                            tagDb = db.Tags.Add(tagDb);
                        }
                        var campaignTag = new CampaignTagMapDb()
                        {
                            Campaign = campaignsDb,
                            Tag = tagDb
                        };

                        db.CampaignTags.Add(campaignTag);
                    }
                    else if (campaignTags.Any(a => a.TagID == tag.Id) == false)
                    {
                        db.CampaignTags.Add(new CampaignTagMapDb() { CampaignID = campaignsDb.CampaignID, TagID = tag.Id });
                        db.RefreshAnalytics.Add(new RefreshAnalyticsDb() { EntityID = tag.Id, EntityType = 5, Status = 1, LastModifiedOn = DateTime.Now.ToUniversalTime() });
                    }
                }

                IList<int> tagIds = campaign.Tags.Where(a => a.Id > 0).Select(a => a.Id).ToList();
                var unMapCampaignTags = campaignTags.Where(a => !tagIds.Contains(a.TagID));
                foreach (CampaignTagMapDb campaignTagMapDb in unMapCampaignTags)
                {
                    db.CampaignTags.Remove(campaignTagMapDb);
                    db.RefreshAnalytics.Add(new RefreshAnalyticsDb() { EntityID = campaignTagMapDb.TagID, EntityType = 5, Status = 1, LastModifiedOn = DateTime.Now.ToUniversalTime() });
                }
            }
        }

        /// <summary>
        /// Saves the resend campaign.
        /// </summary>
        /// <param name="parentCampaignId">The parent campaign identifier.</param>
        /// <param name="campaignId">The campaign identifier.</param>
        /// <param name="CampaignResentTo">The campaign resent to.</param>
        public void SaveResendCampaign(int parentCampaignId, int campaignId, CampaignResentTo CampaignResentTo)
        {
            using (var db = ObjectContextFactory.Create())
            {
                ResentCampaignDb resentcampaign = new ResentCampaignDb();
                resentcampaign.CampaignID = campaignId;
                resentcampaign.ParentCampaignID = parentCampaignId;
                resentcampaign.CamapignResentDate = DateTime.Now.ToUniversalTime();
                resentcampaign.CampaignResentTo = (short)CampaignResentTo;
                db.ResentCampaigns.Add(resentcampaign);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// Gets the resent campaign count.
        /// </summary>
        /// <param name="parentCampaignId">The parent campaign identifier.</param>
        /// <param name="CampaignResentTo">The campaign resent to.</param>
        /// <returns></returns>
        public int GetResentCampaignCount(int parentCampaignId, CampaignResentTo CampaignResentTo)
        {
            var db = ObjectContextFactory.Create();
            int count = db.ResentCampaigns.Where(p => p.ParentCampaignID == parentCampaignId && p.CampaignResentTo == (short)CampaignResentTo).Count();
            return count;
        }

        /// <summary>
        /// Gets the resent campaigns data.
        /// </summary>
        /// <param name="childCampaignId">The child campaign identifier.</param>
        /// <returns></returns>
        public bool GetResentCampaignsData(int childCampaignId)
        {
            var db = ObjectContextFactory.Create();
            bool count = db.ResentCampaigns.Where(p => p.CampaignID == childCampaignId).Count() > 0 ? false : true;
            return count;
        }

        /// <summary>
        /// Gets the templates.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        public IEnumerable<CampaignTemplate> GetTemplates(int accountId)
        {
            var predicate = PredicateBuilder.True<CampaignTemplatesDb>();
            var db = ObjectContextFactory.Create();
            var sql = @"SELECT CT.CampaignTemplateID Id, CT.Name, CT.Type, I.ImageID Id,
                        I.FriendlyName,I.StorageName,I.OriginalName,I.CreatedBy,I.CreatedDate,I.ImageCategoryID,I.AccountID,I.FriendlyName,I.StorageName,I.OriginalName,I.CreatedBy,I.CreatedDate,I.ImageCategoryID,I.AccountID FROM CampaignTemplates CT (NOLOCK) 
                        INNER JOIN Images (NOLOCK) I ON I.ImageId = CT.ThumbnailImage
                        WHERE (CT.AccountId IS NULL OR CT.AccountID = @AccountId) AND CT.Status = 0 
                        ORDER BY CT.Name ASC";
            var templates = db.Get<CampaignTemplate, Image>(sql, (ct, i) =>
                {
                    ct.ThumbnailImage = i;
                    return ct;
                }, new { AccountId = accountId }, splitOn: "Id");
            return templates;
        }

        /// <summary>
        /// Gets the template.
        /// </summary>
        /// <param name="campaignTemplateID">The campaign template identifier.</param>
        /// <returns></returns>
        public CampaignTemplate GetTemplate(int campaignTemplateID)
        {
            var db = ObjectContextFactory.Create();
            var template = db.CampaignTemplates.Where(ct => ct.CampaignTemplateID == campaignTemplateID).FirstOrDefault();
            var domainTemplates = Mapper.Map<CampaignTemplatesDb, CampaignTemplate>(template);
            return domainTemplates;
        }

        /// <summary>
        /// Finds the campaigns summary.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <returns></returns>
        IEnumerable<CampaignsDb> findCampaignsSummary(System.Linq.Expressions.Expression<Func<CampaignsDb, bool>> predicate)
        {
            IEnumerable<CampaignsDb> campaigns = ObjectContextFactory.Create().Campaigns
                .AsExpandable()
                .Where(predicate).OrderByDescending(c => c.CampaignID).Select(a =>
                    new
                    {
                        CampaignID = a.CampaignID,
                    }).ToList().Select(x => new CampaignsDb
                    {
                        CampaignID = x.CampaignID,
                    });
            return campaigns;
        }

        /// <summary>
        /// Finds all.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="limit">The limit.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="status">The status.</param>
        /// <param name="AccountID">The account identifier.</param>
        /// <returns></returns>
        public IEnumerable<Campaign> FindAll(string name, int limit, int pageNumber, byte status, int AccountID)
        {
            var predicate = PredicateBuilder.True<CampaignsDb>();
            var records = (pageNumber - 1) * limit;
            if (!string.IsNullOrEmpty(name))
            {
                name = name.ToLower();
                predicate = predicate.And(a => a.Name.Contains(name));
            }
            if (status != 0)
            {
                predicate = predicate.And(a => a.CampaignStatusID == status);
            }
            predicate = predicate.And(a => a.IsDeleted == false);
            predicate = predicate.And(a => a.AccountID == AccountID);
            IEnumerable<CampaignsDb> campaigns = findCampaignsSummary(predicate).Skip(records).Take(limit);
            foreach (CampaignsDb da in campaigns)
            {
                yield return ConvertToDomain(da);
            }
        }

        /// <summary>
        /// Finds all.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="status">The status.</param>
        /// <param name="AccountID">The account identifier.</param>
        /// <returns></returns>
        public IEnumerable<Campaign> FindAll(string name, byte status, int AccountID)
        {
            var predicate = PredicateBuilder.True<CampaignsDb>();
            if (!string.IsNullOrEmpty(name))
            {
                name = name.ToLower();
                predicate = predicate.And(a => a.Name.Contains(name));
            }
            if (status != 0)
            {
                predicate = predicate.And(a => a.CampaignStatusID == status);
            }
            predicate = predicate.And(a => a.IsDeleted == false);
            predicate = predicate.And(a => a.AccountID == AccountID);

            IEnumerable<CampaignsDb> campaigns = findCampaignsSummary(predicate);
            foreach (CampaignsDb da in campaigns)
            {
                yield return ConvertToDomain(da);
            }
        }

        /// <summary>
        /// Gets the campaign public images.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Image> GetCampaignPublicImages()
        {
            var db = ObjectContextFactory.Create();
            var images = db.Images.Where(i => i.AccountID == null && i.ImageCategoryID == ImageCategory.Campaigns).ToList();
            return Mapper.Map<IEnumerable<ImagesDb>, IEnumerable<Image>>(images);
        }

        /// <summary>
        /// Inserts the campaign image.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <returns></returns>
        public int InsertCampaignImage(Image image)
        {
            var db = ObjectContextFactory.Create();
            var imageDb = Mapper.Map<Image, ImagesDb>(image);
            db.Images.Add(imageDb);
            db.SaveChanges();
            return imageDb.ImageID;
        }

        /// <summary>
        /// Inserts the campaign templates.
        /// </summary>
        /// <param name="campaignTemplate">The campaign template.</param>
        /// <returns></returns>
        public int InsertCampaignTemplates(CampaignTemplate campaignTemplate)
        {
            var db = ObjectContextFactory.Create();
            //  var templateDb = Mapper.Map<CampaignTemplate, CampaignTemplatesDb>(campaignTemplate);
            var templateDb = new CampaignTemplatesDb()
            {
                CampaignTemplateID = campaignTemplate.Id,
                Name = campaignTemplate.Name,
                AccountId = campaignTemplate.AccountId,
                CreatedBy = campaignTemplate.CreatedBy,
                CreatedOn = DateTime.Now.ToUniversalTime(),
                ThumbnailImage = campaignTemplate.ThumbnailImageId,
                Description = campaignTemplate.Description,
                HTMLContent = campaignTemplate.HTMLContent,
                Type = campaignTemplate.Type
            };
            db.CampaignTemplates.Add(templateDb);
            //  db.Images.Add(imageDb);
            db.SaveChanges();
            return templateDb.CampaignTemplateID;
        }

        /// <summary>
        /// Deletes the campaign image.
        /// </summary>
        /// <param name="imageId">The image identifier.</param>
        /// <param name="accountId">The account identifier.</param>
        public void DeleteCampaignImage(int imageId, int accountId)
        {
            var db = ObjectContextFactory.Create();
            var imageDb = db.Images.Where(n => n.ImageID == imageId && n.AccountID == accountId).FirstOrDefault();
            if (imageDb != null)
            {
                db.Images.Remove(imageDb);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// Gets the campaign images.
        /// </summary>
        /// <param name="accountID">The account identifier.</param>
        /// <returns></returns>
        public List<Image> GetCampaignImages(int? accountID)
        {
            var db = ObjectContextFactory.Create();
            var images = db.Images.Where(n => (n.AccountID == accountID || n.AccountID == null)
                && n.ImageCategoryID == ImageCategory.Campaigns).ToList();
            return Mapper.Map<List<ImagesDb>, List<Image>>(images);
        }

        /// <summary>
        /// Finds all images.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="limit">The limit.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="accountID">The account identifier.</param>
        /// <returns></returns>
        public IEnumerable<Image> FindAllImages(string name, int limit, int pageNumber, int? accountID)
        {
            var predicate = PredicateBuilder.True<ImagesDb>();
            var records = (pageNumber - 1) * limit;
            IEnumerable<ImagesDb> images = null;

            if (!string.IsNullOrEmpty(name))
            {
                name = name.ToLower();
                predicate = predicate.And(a => a.FriendlyName.ToLower().Contains(name));
            }
            predicate = predicate.And(a => (a.AccountID == accountID && a.ImageCategoryID == ImageCategory.Campaigns));
            //predicate = predicate.And(a => a.ImageCategoryID == ImageCategory.Campaigns);
            if (limit == -1)
                images = findImagesSummary(predicate);
            else
                images = findImagesSummary(predicate).Skip(records).Take(limit);
            //if (images != null)
            //    images = images.Where(Id => Id.ImageCategoryID == ImageCategory.Campaigns);
            foreach (ImagesDb image in images)
            {
                yield return ConvertToDomain(image);
            }
            //return Mapper.Map<IEnumerable<ImagesDb>, IEnumerable<Image>>(images);
        }

        /// <summary>
        /// Converts to domain.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <returns></returns>
        private Image ConvertToDomain(ImagesDb image)
        {
            return Mapper.Map<ImagesDb, Image>(image);
        }

        /// <summary>
        /// Finds all images.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="AccountID">The account identifier.</param>
        /// <returns></returns>
        public IEnumerable<Image> FindAllImages(string name, int? AccountID)
        {
            var predicate = PredicateBuilder.True<ImagesDb>();
            if (!string.IsNullOrEmpty(name))
            {
                name = name.ToLower();
                predicate = predicate.And(a => a.FriendlyName.Contains(name));
            }

            predicate = predicate.And(a => a.AccountID == AccountID).And(a => a.ImageCategoryID == ImageCategory.Campaigns);
            //predicate = predicate.And(a => a.ImageCategoryID == ImageCategory.Campaigns);

            IEnumerable<ImagesDb> images = findImagesSummary(predicate);
            return Mapper.Map<IEnumerable<ImagesDb>, IEnumerable<Image>>(images);
        }

        /// <summary>
        /// Finds the images summary.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <returns></returns>
        IEnumerable<ImagesDb> findImagesSummary(System.Linq.Expressions.Expression<Func<ImagesDb, bool>> predicate)
        {
            IEnumerable<ImagesDb> images = ObjectContextFactory.Create().Images
                .AsExpandable()
                .Where(predicate).OrderByDescending(c => c.ImageID).Select(i =>
                    new
                    {
                        AccountID = i.AccountID,
                        FriendlyName = i.FriendlyName,
                        ImageCategoryID = i.ImageCategoryID,
                        ImageID = i.ImageID,
                        OriginalName = i.OriginalName,
                        StorageName = i.StorageName
                    }).ToList().Select(x => new ImagesDb
                    {
                        AccountID = x.AccountID,
                        FriendlyName = x.FriendlyName,
                        ImageCategoryID = x.ImageCategoryID,
                        ImageID = x.ImageID,
                        OriginalName = x.OriginalName,
                        StorageName = x.StorageName
                    });
            return images;
        }

        /// <summary>
        /// Campaigns recipients count.
        /// </summary>
        /// <param name="tags">The tags.</param>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        public int CampaignRecipientsCount(IEnumerable<Tag> tags, int accountId)
        {
            var db = ObjectContextFactory.Create();
            var contactTagIds = tags.Select(c => c.Id).Distinct().ToList();
            var tagContacts = db.ContactTags.Where(ct => contactTagIds.Contains(ct.TagID))
            .GroupJoin(db.Contacts.Where(c => c.IsDeleted == false && c.AccountID == accountId), ct => ct.ContactID,
            c => c.ContactID, (ct, c) => new
            {
                Count = db.ContactEmails.Count(ce => ce.AccountID == accountId
                    && ce.ContactID == ct.ContactID &&
                    (ce.EmailStatus == (byte)EmailStatus.NotVerified || ce.EmailStatus == (byte)EmailStatus.Verified
                    || ce.EmailStatus == (byte)EmailStatus.SoftBounce) && ce.IsPrimary == true)
            }).ToList();
            return tagContacts.Sum(c => c.Count);
        }

        public List<int> GetSearchDefinitionIds(int campaignId)
        {
            var db = ObjectContextFactory.Create();
            List<int> ids = db.CampaignSearchDefinitions.Where(p => p.CampaignID == campaignId).Select(p => p.SearchDefinitionID).ToList();
            return ids;
        }

        /// <summary>
        /// Campaigns unique recipients count.
        /// </summary>
        /// <param name="tags">The tags.</param>
        /// <param name="accountId">The account identifier.</param>
        /// <param name="contactsFromSearchs">The contacts from searchs.</param>
        /// <param name="toTagStatus">To tag status.</param>
        /// <returns></returns>
        [Obsolete("Not using this method")]
        public CampaignRecipientTypes CampaignUniqueRecipientsCount(IEnumerable<Tag> tags, int accountId, IEnumerable<int> contactsFromSearchs, long toTagStatus)
        {
            var db = ObjectContextFactory.Create();
            CampaignRecipientTypes recipients = new CampaignRecipientTypes();

            var contactEmailsCount = 0;
            var activeContactEmailCount = 0;
            IEnumerable<int> AllContactListBySS = Enumerable.Empty<int>();
            IEnumerable<int> ActiveContactListBySS = Enumerable.Empty<int>();
            IEnumerable<int> AllContactListByTag = Enumerable.Empty<int>();
            IEnumerable<int> ActiveContactListByTag = Enumerable.Empty<int>();


            var contactsFromSearch = contactsFromSearchs.Distinct();
            var emailstatuses = new List<byte>()
                {
                    (byte)EmailStatus.NotVerified, (byte)EmailStatus.Verified, (byte)EmailStatus.SoftBounce
                };

            DateTime today = DateTime.Now.ToUniversalTime();
            DateTime Before4Months = today.AddDays(-120);

            if (contactsFromSearch.Any())
            {
                var contactemailsql1 = @"select c.ContactID from contacts c 
											inner join contactemails (nolock) ce on ce.contactid = c.contactid	 
											inner join CampaignRecipients (nolock) cr on cr.ContactID = c.ContactID
											inner join CampaignStatistics (nolock) cs on cs.CampaignRecipientID = cr.CampaignRecipientID and cs.ActivityDate > @date
											where c.isdeleted = 0  and ce.isprimary = 1  and c.accountid = @accountid and cr.accountId = @accountid and cs.accountId = @accountid
											and ce.EmailStatus in (select datavalue from dbo.SPlit(@emailstatuses,',') )
											union
											select c.ContactID from contacts c 
											inner join contactemails (nolock) ce on ce.contactid = c.contactid
											inner join Contacts_Audit (nolock) ca on ca.ContactID = c.ContactID and ca.AuditAction = 'I' and  ca.LastUpdatedOn > @date
											where c.isdeleted = 0  and ce.isprimary = 1  and c.accountid = @accountid
										   and ce.EmailStatus in (select datavalue from dbo.SPlit(@emailstatuses,',') )";


                ActiveContactListBySS = db.Get<RawContact>(contactemailsql1, new { AccountId = accountId, emailstatuses = string.Join(",", emailstatuses), date = Before4Months }, true)
                    .Join(contactsFromSearch, c => c.ContactID, cf => cf, (c, cf) => new { c.ContactID }).Select(p => p.ContactID).Distinct();

                recipients.ActiveContactsBySS = ActiveContactListBySS.Count();

                var contactemailsql = @"select c.ContactID from contacts c (nolock) 
										inner join contactemails (nolock) ce on ce.contactid = c.contactid
										where c.isdeleted = 0
										and ce.isprimary = 1 
										and c.accountid = @accountid
										and ce.EmailStatus in (select datavalue from dbo.SPlit(@emailstatuses,',') ) ";

                AllContactListBySS = db.Get<RawContact>(contactemailsql, new { AccountId = accountId, emailstatuses = string.Join(",", emailstatuses) }, true)
                  .Join(contactsFromSearch, c => c.ContactID, cf => cf, (c, cf) => new { c.ContactID }).Select(p => p.ContactID).Distinct();

                recipients.AllContactsBySS = AllContactListBySS.Count();
            }


            var contactTagIds = tags.Select(c => c.Id).Distinct().ToList();

            if (contactTagIds.Any())
            {
                var contacttagsql = @"select c.ContactID from ContactTagMap (nolock) ctm
										inner join Contacts (nolock) c on c.ContactID =  ctm.ContactID
										inner join ContactEmails ce (nolock) on ce.ContactID = c.ContactID
										inner join CampaignRecipients (nolock) cr on cr.ContactID = c.ContactID
										inner join CampaignStatistics (nolock) cs on cs.CampaignRecipientID = cr.CampaignRecipientID and (cs.ActivityDate > @date)
										where c.IsDeleted = 0 and c.AccountID =  @accountId and cr.accountId = @accountId and cs.accountId = @accountId and ce.EmailStatus in (select datavalue from dbo.SPlit(@emailstatuses,','))
										and ctm.TagID in (select datavalue from dbo.SPlit(@tagIds,','))
										union 
										select c.ContactID from ContactTagMap ctm (nolock) 
										inner join Contacts (nolock) c on c.ContactID =  ctm.ContactID
										inner join ContactEmails (nolock) ce on ce.ContactID = c.ContactID
										inner join Contacts_Audit (nolock) ca on ca.ContactID = c.ContactID and ca.AuditAction = 'I'  and ca.LastUpdatedOn > @date
										where c.IsDeleted = 0 and c.AccountID =  @accountId and ce.EmailStatus in (select datavalue from dbo.SPlit(@emailstatuses,','))
										and ctm.TagID in (select datavalue from dbo.SPlit(@tagIds,','))";


                ActiveContactListByTag = db.Get<RawContact>(contacttagsql, new
                {
                    accountId = accountId,
                    emailstatuses = string.Join(",", emailstatuses),
                    tagIds = string.Join(",", contactTagIds),
                    date = Before4Months
                }, true).Select(p => new { p.ContactID }).Select(c => c.ContactID).Distinct();

                recipients.ActiveContactsByTag = ActiveContactListByTag.Count();

                var contacttagsql2 = @"select c.ContactID from ContactTagMap ctm (nolock) 
									inner join Contacts (nolock) c on c.ContactID =  ctm.ContactID
									inner join ContactEmails (nolock) ce on ce.ContactID = c.ContactID
									where c.IsDeleted = 0 and c.AccountID = @accountId and ce.EmailStatus in (select datavalue from dbo.SPlit(@emailstatuses,','))
									and ctm.TagID in (select datavalue from dbo.SPlit(@tagIds,','))";

                AllContactListByTag = db.Get<RawContact>(contacttagsql2, new
                {
                    accountId = accountId,
                    emailstatuses = string.Join(",", emailstatuses),
                    tagIds = string.Join(",", contactTagIds)
                }, true).Select(p => new { p.ContactID }).Select(p => p.ContactID).Distinct();

                recipients.AllContactsByTag = AllContactListByTag.Count();
            }

            if (tags.Any() == false && contactsFromSearch.Any())
            {
                contactEmailsCount = (int)recipients.AllContactsBySS;
                activeContactEmailCount = (int)recipients.ActiveContactsBySS;
                recipients.CampaignACTIVEandALLRecipientsCount = ActiveContactListByTag.Union(AllContactListBySS).Distinct().Count();
                recipients.CampaignALLandACTIVERecipientsCount = AllContactListByTag.Union(ActiveContactListBySS).Distinct().Count();

            }
            else if (tags.Any() && contactsFromSearch.Any() == false)
            {
                contactEmailsCount = (int)recipients.AllContactsByTag;
                activeContactEmailCount = (int)recipients.ActiveContactsByTag;
                recipients.CampaignACTIVEandALLRecipientsCount = ActiveContactListByTag.Union(AllContactListBySS).Distinct().Count();
                recipients.CampaignALLandACTIVERecipientsCount = AllContactListByTag.Union(ActiveContactListBySS).Distinct().Count();
            }
            else if (tags.Any() && contactsFromSearch.Any())
            {
                contactEmailsCount = AllContactListByTag.Union(AllContactListBySS).Distinct().Count();
                activeContactEmailCount = ActiveContactListByTag.Union(ActiveContactListBySS).Distinct().Count();
                recipients.CampaignACTIVEandALLRecipientsCount = ActiveContactListByTag.Union(AllContactListBySS).Distinct().Count();
                recipients.CampaignALLandACTIVERecipientsCount = AllContactListByTag.Union(ActiveContactListBySS).Distinct().Count();
            }


            recipients.CampaignActiveRecipientsCount = activeContactEmailCount;
            recipients.CampaignRecipientsCount = contactEmailsCount;
            return recipients;
        }

        /// <summary>
        /// Get campaign unique recipients count by user
        /// </summary>
        /// <param name="tags"></param>
        /// <param name="accountId"></param>
        /// <param name="contactsFromSearch"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public int CampaignUniqueRecipientsCount(IEnumerable<Tag> tags, int accountId, IEnumerable<int> contactsFromSearch, int userId)
        {
            var db = ObjectContextFactory.Create();
            var contactTagIds = tags.Select(c => c.Id).Distinct().ToList();
            var contactEmails = db.ContactTags.Where(ct => contactTagIds.Contains(ct.TagID))
                .GroupJoin(db.Contacts.Where(c => c.IsDeleted == false && c.AccountID == accountId && c.OwnerID == userId),
                ct => ct.ContactID,
                c => c.ContactID,
                   (ct, c) => new
                   {
                       ContactEmails = c
                       .Join(db.ContactEmails.Where(ce => ce.AccountID == accountId && (ce.EmailStatus == (byte)EmailStatus.NotVerified
                            || ce.EmailStatus == (byte)EmailStatus.Verified
                            || ce.EmailStatus == (byte)EmailStatus.SoftBounce) && ce.IsPrimary == true),
                           cc => cc.ContactID,
                           ce => ce.ContactID,
                       (cc, ce) => new { ContactID = cc.ContactID, EmailId = ce.Email })
                   }).SelectMany(a => a.ContactEmails)
                   .Union(db.Contacts.Where(c => c.IsDeleted == false && contactsFromSearch.Contains(c.ContactID) && c.OwnerID == userId && c.AccountID == accountId)
                   .Join(db.ContactEmails.Where(ce => ce.AccountID == accountId && (ce.EmailStatus == (byte)EmailStatus.NotVerified
                            || ce.EmailStatus == (byte)EmailStatus.Verified
                            || ce.EmailStatus == (byte)EmailStatus.SoftBounce) && ce.IsPrimary == true),
                   c => c.ContactID,
                   ce => ce.ContactID,
                   (c, ce) => new { ContactID = c.ContactID, EmailId = ce.Email })).Distinct();
            return contactEmails.Count();
        }
        /// <summary>
        /// Add recipients when contacts module is in private mode
        /// </summary>
        /// <param name="campaignId"></param>
        /// <param name="accountId"></param>
        /// <param name="contactsFromSearch"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        private int PersistCampaignRecipients(int campaignId, int accountId, IEnumerable<int> contactsFromSearch, int userId, int? tagRecipients, int? sSRecipients)
        {
            CRMDb db = ObjectContextFactory.Create();
            var objectContext = (db as IObjectContextAdapter).ObjectContext;
            objectContext.CommandTimeout = 1800;
            Func<IEnumerable<int>, System.Data.DataTable> ConvertListToDataTable = (list) =>
            {
                // New table.
                System.Data.DataTable table = new System.Data.DataTable();
                table.Columns.Add();
                foreach (var array in list)
                {
                    table.Rows.Add(array);
                }
                return table;
            };
            System.Data.DataTable contacts = ConvertListToDataTable(contactsFromSearch);
            List<SqlParameter> sqlParameters = new List<SqlParameter>();
            sqlParameters.Add(new SqlParameter("@CampaignId", campaignId));
            sqlParameters.Add(new SqlParameter("@OwnerId", userId));
            sqlParameters.Add(new SqlParameter { ParameterName = "@SSContacts", Value = contacts, SqlDbType = System.Data.SqlDbType.Structured, TypeName = "dbo.Contact_List" });

            var result = db.ExecuteStoredProcedure("dbo.HandleNextCampaign", sqlParameters);
            return result;
        }

        /// <summary>
        /// Creates a list of campaign recipients and queues the campaign. This method is out of UOW because of performance implications.
        /// </summary>
        /// <param name="campaignId"></param>
        /// <param name="accountId"></param>
        /// <param name="contactsFromSearch"></param>
        /// <param name="userId"></param>
        /// <param name="tagRecipients"></param>
        /// <param name="sSRecipients"></param>
        /// <returns></returns>
        public int AddCampaignRecipients(int campaignId, int accountId, IEnumerable<int> contactsFromSearch, int userId, int? tagRecipients, int? sSRecipients)
        {
            int totalRecipients = PersistCampaignRecipients(campaignId, accountId, contactsFromSearch, userId, tagRecipients, sSRecipients);
            Logger.Current.Verbose("Total Recipients:" + totalRecipients + " CampaignId:" + campaignId);
            return totalRecipients;
        }

        /// <summary>
        /// Deactivates the campaign.
        /// </summary>
        /// <param name="campaignIds">The campaign ids.</param>
        /// <param name="updatedBy">The updated by.</param>
        public void DeactivateCampaign(int[] campaignIds, int updatedBy)
        {
            var db = ObjectContextFactory.Create();
            foreach (int campaignId in campaignIds)
            {
                var campaign = db.Campaigns.SingleOrDefault(c => c.CampaignID == campaignId);
                campaign.IsDeleted = true;
                campaign.LastUpdatedBy = updatedBy;
                campaign.LastUpdatedOn = DateTime.Now.ToUniversalTime();
            }
            db.SaveChanges();
        }

        /// <summary>
        /// Deletes the campaigns.
        /// </summary>
        /// <param name="campaignIds">The campaign ids.</param>
        /// <param name="updatedBy">The updated by.</param>
        /// <returns></returns>
        public string DeleteCampaigns(int[] campaignIds, int updatedBy, int accountId)
        {
            var db = ObjectContextFactory.Create();
            IEnumerable<short> workflows = db.Workflows.Where(p => p.AccountID == accountId && p.IsDeleted == false).Select(p => p.WorkflowID).ToArray();
            var associatedWorkflow = db.WorkflowTriggers.Where(c => workflows.Contains(c.WorkflowID) && campaignIds.Contains((int)c.CampaignID)).FirstOrDefault();
            IEnumerable<int> workflowactions = db.WorkflowActions.Where(p => workflows.Contains(p.WorkflowID) && p.IsDeleted == false).Select(p => p.WorkflowActionID).ToArray();
            var associatedWorkflowsCampaignActions = db.WorkflowCampaignActions.Where(c => workflowactions.Contains(c.WorkflowActionID) && campaignIds.Contains((int)c.CampaignID)).FirstOrDefault();
            string message = "";
            if (associatedWorkflow != null)
            {
                var campaignName = db.Campaigns.Where(c => c.CampaignID == associatedWorkflow.CampaignID).Select(p => p.Name).FirstOrDefault();
                message = campaignName
                    + " [| is associated with an automation workflow.\n DELETE operation cancelled.|]";
                return message;
            }
            else if (associatedWorkflowsCampaignActions != null)
            {
                var campaignName = db.Campaigns.Where(c => c.CampaignID == associatedWorkflowsCampaignActions.CampaignID).Select(p => p.Name).FirstOrDefault();
                message = campaignName
                    + "[| is associated with an automation workflow.\n DELETE operation cancelled.|]";
                return message;
            }
            var campaigns = db.Campaigns.Where(c => campaignIds.Contains(c.CampaignID));
            campaigns.ForEach(c => { c.IsDeleted = true; });
            db.SaveChanges();
            return message;
        }

        /// <summary>
        /// Isworks the flow attached campaign.
        /// </summary>
        /// <param name="campaignID">The campaign identifier.</param>
        /// <returns></returns>
        public bool IsworkFlowAttachedCampaign(int campaignID)
        {
            var db = ObjectContextFactory.Create();
            var associatedWorkflow = db.WorkflowTriggers.Where(c => c.CampaignID == campaignID)
                .Include(c => c.Campaigns).FirstOrDefault();
            if (associatedWorkflow != null)
                return true;
            else
                return false;
        }
        /// <summary>
        /// Archives the campaigns.
        /// </summary>
        /// <param name="campaignIds">The campaign ids.</param>
        /// <param name="updatedBy">The updated by.</param>
        /// <returns></returns>
        public IEnumerable<Campaign> ArchiveCampaigns(int[] campaignIds, int updatedBy)
        {
            var db = ObjectContextFactory.Create();
            IEnumerable<CampaignsDb> campaigns = db.Campaigns.Where(c => campaignIds.Contains(c.CampaignID));
            campaigns.ForEach(c => { c.CampaignStatusID = (short)CampaignStatus.Archive; });
            db.SaveChanges();

            foreach (CampaignsDb da in campaigns)
            {
                yield return ConvertToDomain(da);
            }

        }

        /// <summary>
        /// Gets the next campaign to trigger.
        /// </summary>
        /// <returns></returns>
        public Campaign GetNextCampaignToTrigger()
        {
            var campaign = default(Campaign);
            CampaignsDb campaignDb = default(CampaignsDb);
            using (var db = ObjectContextFactory.Create())
            {
                try
                {
                    Logger.Current.Verbose("Request received to fetch NextCampaignToTrigger");
                    var eligibleCampaignStatuses = new List<short> { (short)CampaignStatus.Scheduled, (short)CampaignStatus.Queued };
                    var currentUtc = DateTime.Now.ToUniversalTime();

                    db.QueryStoredProc("[GetNextCampaignToTrigger]", (reader) =>
                    {
                        campaignDb = reader.Read<CampaignsDb>().FirstOrDefault();
                    });
                    if (campaignDb != null)
                    {
                        Logger.Current.Informational("NextCampaignToTrigger Id: " + campaignDb.CampaignID);
                        if (campaignDb.IncludePlainText == true)
                        {
                            var plainTextDb = db.Get<CampaignPlainTextContentDb>(
                                    @"SELECT * FROM CampaignPlaintextContentMap (NOLOCK) WHERE Campaignid = @campaignId",
                                    new { campaignId = campaignDb.CampaignID }).FirstOrDefault();
                            campaignDb.PlainTextContent = plainTextDb != null ? plainTextDb.PlainTextContent ?? "" : "";
                        }

                        campaign = ConvertToDomain(campaignDb);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Current.Error("Error occured in campaign repository in GetNextCampaignToTrigger method: ", ex);
                    if (campaignDb != null)
                    {
                        campaignDb.CampaignStatusID = (short)CampaignStatus.Failure;
                        campaign = Mapper.Map<CampaignsDb, Campaign>(campaignDb);
                    }
                }
            }
            return campaign;
        }

        /// <summary>
        /// Gets the campaign recipients information.
        /// </summary>
        /// <param name="campaignId">The campaign identifier.</param>
        /// <returns></returns>
        public IEnumerable<IDictionary<string, object>> GetCampaignRecipientsInfo(int campaignId, bool isLinkedToWorkflow = false)
        {
            var db = ObjectContextFactory.Create();
            var sql = "getCampaignRecipientInfo";
            var cr = new List<dynamic>();
            db.QueryStoredProc(sql, (d) =>
            {
                cr = d.Read().ToList();
            }, new { CampaignId = campaignId, IsLinkedToWorkflow = isLinkedToWorkflow }, commandTimeout: 1200);

            //var cr = db.Get(sql, new { cid = campaignId, isLinkedWorkflow = isLinkedToWorkflow }).ToList();
            foreach (dynamic m in cr)
            {
                yield return m;
            }
        }

        public int GetCampaignRecipientCount(int campaignId)
        {
            using (var db = ObjectContextFactory.Create())
            {
                var sql = @"SELECT COUNT(*) FROM CampaignRecipients (NOLOCK) CR 
                            INNER JOIN Campaigns (NOLOCK) C ON C.AccountID = CR.AccountID AND CR.CampaignID = C.CampaignID
                            WHERE CR.CampaignID = @campaignId";
                var recipientCount = db.Get<int>(sql, new { campaignId = campaignId }).FirstOrDefault();
                return recipientCount;
            }
        }

        /// <summary>
        /// Gets the campaign recipients.
        /// </summary>
        /// <param name="campaignId">The campaign identifier.</param>
        /// <returns></returns>
        public IEnumerable<CampaignRecipient> GetCampaignRecipients(int campaignId, int accountId)
        {
            var db = ObjectContextFactory.Create();
            Logger.Current.Verbose("Getting receipietns1");
            var campaignRecipientDb = db.CampaignRecipients.Where(c => c.CampaignID == campaignId && c.AccountID == accountId).ToList();
            Logger.Current.Verbose("Fetched Recipients");
            return Mapper.Map<IEnumerable<CampaignRecipientsDb>, IEnumerable<CampaignRecipient>>(campaignRecipientDb);
        }

        /// <summary>
        /// Updates the campaign trigger status.
        /// </summary>
        /// <param name="campaignId">The campaign identifier.</param>
        /// <param name="status">The status.</param>
        /// <param name="sentDateTime">The sent date time.</param>
        /// <param name="serviceProviderCampaignId">The service provider campaign identifier.</param>
        /// <param name="finalRecipients">The final recipients.</param>
        /// <param name="remarks">The remarks.</param>
        /// <param name="serviceProviderId">The service provider identifier.</param>
        /// <param name="lasttouched">The lasttouched.</param>
        /// <param name="recipients">The recipients.</param>
        /// <returns></returns>
        public Campaign UpdateCampaignTriggerStatus
            (int campaignId, CampaignStatus status, DateTime sentDateTime, string serviceProviderCampaignId,
            List<string> finalRecipients, string remarks, int? serviceProviderId, bool isDelayedCampaign, out List<LastTouchedDetails> lasttouched, IEnumerable<int> recipients = null)
        {
            using (CRMDb db = new CRMDb())
            {
                lasttouched = null;
                var campaignsDb = db.Campaigns.Where(c => c.CampaignID == campaignId).FirstOrDefault();
                if (campaignsDb != null)
                {
                    if (!campaignsDb.IsLinkedToWorkflows)
                    {
                        campaignsDb.CampaignStatusID = (byte)status;
                        campaignsDb.Remarks = remarks;
                    }
                    campaignsDb.ServiceProviderCampaignID = serviceProviderCampaignId;
                    campaignsDb.ServiceProviderID = serviceProviderId ?? campaignsDb.ServiceProviderID;
                    db.Entry<CampaignsDb>(campaignsDb).State = System.Data.Entity.EntityState.Modified;
                    if (status == CampaignStatus.Sent && !isDelayedCampaign)
                    {
                        var allRecipients = db.CampaignRecipients.Where(c => c.CampaignID == campaignId && c.AccountID == campaignsDb.AccountID).Select(ct => ct.ContactID).ToList();
                        List<LastTouchedDetails> lastTouchedDetails = new List<LastTouchedDetails>();
                        campaignsDb.ProcessedDate = sentDateTime;
                        allRecipients
                                .ForEach(c =>
                                {
                                    LastTouchedDetails lastTouched = new LastTouchedDetails()
                                    {
                                        LastTouchedDate = sentDateTime,
                                        ContactID = c
                                    };
                                    lastTouchedDetails.Add(lastTouched);
                                });
                        lasttouched = lastTouchedDetails;
                    }
                    else if (isDelayedCampaign && recipients.Any())
                    {
                        Logger.Current.Verbose(string.Format("Accumulating the last touched details for delayed recipients for campaign: {0}", campaignId));
                        var allRecipients = db.CampaignRecipients.Where(c => c.CampaignID == campaignId && recipients.Contains(c.CampaignRecipientID) && c.AccountID == campaignsDb.AccountID).Select(ct => ct.ContactID).ToList();
                        List<LastTouchedDetails> lastTouchedDetails = new List<LastTouchedDetails>();
                        campaignsDb.ProcessedDate = sentDateTime;
                        allRecipients
                                .ForEach(c =>
                                {
                                    LastTouchedDetails lastTouched = new LastTouchedDetails()
                                    {
                                        LastTouchedDate = sentDateTime,
                                        ContactID = c
                                    };
                                    lastTouchedDetails.Add(lastTouched);
                                });
                        lasttouched = lastTouchedDetails;
                    }

                    else if (status == CampaignStatus.Failure)
                    {
                        campaignsDb.ProcessedDate = DateTime.Now.ToUniversalTime();
                    }
                    else if (status == CampaignStatus.Sending)
                        campaignsDb.ServiceProviderCampaignID = serviceProviderCampaignId;

                    else if (status == CampaignStatus.Delayed)
                        campaignsDb.ProcessedDate = DateTime.Now.ToUniversalTime();

                    if (isDelayedCampaign == true)
                    {
                        Logger.Current.Verbose(string.Format("Updating the retry audit for campaign: {0}", campaignId));
                        var sql = @"UPDATE CampaignRetryAudit SET CampaignStatus = @campaignStatus,Remarks = @remarks
	                                        WHERE CampaignRetryAuditID IN 
                                                    (SELECT TOP 1 CampaignRetryAuditID FROM CampaignRetryAudit WHERE CampaignID = @campaignId ORDER BY CampaignRetryAuditID DESC)";
                        using (var dbNew = ObjectContextFactory.Create())
                        {
                            dbNew.Execute(sql, new { campaignStatus = status, remarks = remarks, campaignId = campaignId });
                        }
                    }

                    try
                    {
                        db.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        var message = string.Format("Campaign Id : {0} Remarks: {1}", campaignId, string.IsNullOrWhiteSpace(remarks) ? "n/a" : remarks);
                        Logger.Current.Error(message, ex);
                        throw;
                    }
                }

                return Mapper.Map<CampaignsDb, Campaign>(campaignsDb);
            }
        }

        /// <summary>
        /// This method is used for workflows.
        /// TODO
        /// Merge this method with UpdateCampaignTriggerStatus
        /// </summary>
        /// <param name="campaignId"></param>
        /// <param name="status"></param>
        /// <param name="sentDateTime"></param>
        /// <param name="serviceProviderCampaignId"></param>
        /// <param name="finalRecipients"></param>
        /// <param name="remarks"></param>
        /// <param name="serviceProviderId"></param>
        /// <param name="lasttouched"></param>
        /// <returns></returns>
        public Campaign UpdateCampaignTriggerStatusForWorkflow
            (int campaignId, CampaignStatus status, DateTime sentDateTime, string serviceProviderCampaignId,
            IEnumerable<int> finalRecipients, string remarks, int? serviceProviderId, out List<LastTouchedDetails> lasttouched)
        {
            using (CRMDb db = new CRMDb())
            {
                var objectContext = (db as IObjectContextAdapter).ObjectContext;
                objectContext.CommandTimeout = 1600;
                lasttouched = null;
                var campaignsDb = db.Campaigns.Where(c => c.CampaignID == campaignId).FirstOrDefault();
                if (campaignsDb != null)
                {
                    if (!campaignsDb.IsLinkedToWorkflows)
                    {
                        campaignsDb.CampaignStatusID = (byte)status;
                        campaignsDb.Remarks = remarks;
                    }
                    campaignsDb.ServiceProviderCampaignID = serviceProviderCampaignId;
                    campaignsDb.ServiceProviderID = serviceProviderId ?? campaignsDb.ServiceProviderID;
                    db.Entry<CampaignsDb>(campaignsDb).State = System.Data.Entity.EntityState.Modified;
                    if (status == CampaignStatus.Sent)
                    {
                        List<LastTouchedDetails> lastTouchedDetails = new List<LastTouchedDetails>();
                        db.CampaignRecipients.Where(c => c.CampaignID == campaignId && c.AccountID == campaignsDb.AccountID
                            && c.DeliveryStatus != (short)CampaignDeliveryStatus.HardBounce && finalRecipients.Contains(c.CampaignRecipientID))
                                                   .ForEach(c =>
                                                   {
                                                       c.SentOn = sentDateTime;
                                                       c.DeliveredOn = sentDateTime;
                                                       c.DeliveryStatus = (short)CampaignDeliveryStatus.Delivered;
                                                       c.Remarks = remarks;
                                                       c.LastModifiedOn = DateTime.UtcNow;
                                                       LastTouchedDetails lastTouched = new LastTouchedDetails()
                                                       {
                                                           LastTouchedDate = sentDateTime,
                                                           ContactID = c.ContactID
                                                       };
                                                       lastTouchedDetails.Add(lastTouched);
                                                   });
                        campaignsDb.ProcessedDate = sentDateTime;
                        lasttouched = lastTouchedDetails;
                    }

                    else if (status == CampaignStatus.Failure)
                    {
                        if (!campaignsDb.IsLinkedToWorkflows)
                            db.CampaignRecipients.RemoveRange(db.CampaignRecipients.Where(c => c.CampaignID == campaignId && c.AccountID == campaignsDb.AccountID));
                        else
                        {
                            db.CampaignRecipients.Where(c => c.CampaignID == campaignId && c.AccountID == campaignsDb.AccountID && finalRecipients.Contains(c.CampaignRecipientID)).ForEach(c =>
                            {
                                c.DeliveryStatus = (short)CampaignStatus.Failure;
                                c.Remarks = remarks;
                                c.LastModifiedOn = DateTime.UtcNow;
                            });
                        }
                        lasttouched = null;
                        campaignsDb.ProcessedDate = DateTime.Now.ToUniversalTime();
                    }
                    db.SaveChanges();
                }
                return Mapper.Map<CampaignsDb, Campaign>(campaignsDb);
            }
        }

        public void UpdateCampaignStatus(int campaignId, CampaignStatus status)
        {
            if (campaignId != 0)
            {
                var db = ObjectContextFactory.Create();
                string sql = @"UPDATE Campaigns
                           SET CampaignStatusID = @Status
                           WHERE CampaignID = @CampaignID";
                db.Execute(sql, new { CampaignID = campaignId, Status = (int)status });
            }
        }

        /// <summary>
        /// Updates the campaign recipient delivery status.
        /// </summary>
        /// <param name="campaignRecipientId">The campaign recipient identifier.</param>
        /// <param name="status">The status.</param>
        /// <returns></returns>
        public CampaignRecipient UpdateCampaignRecipientDeliveryStatus(int campaignRecipientId, CampaignDeliveryStatus status, int accountId)
        {
            using (CRMDb db = new CRMDb())
            {
                var campaignRecipient = db.CampaignRecipients.Where(c => c.CampaignRecipientID == campaignRecipientId && c.AccountID == accountId).FirstOrDefault();
                if (campaignRecipient != null)
                {
                    campaignRecipient.DeliveryStatus = (short)status;
                    campaignRecipient.LastModifiedOn = DateTime.UtcNow;
                }

                db.SaveChanges();
                return Mapper.Map<CampaignRecipientsDb, CampaignRecipient>(campaignRecipient);
            }
        }

        /// <summary>
        /// Updates the campaign delivery status.
        /// </summary>
        /// <param name="mailChimpCampaingId">The mail chimp campaing identifier.</param>
        /// <param name="status">The status.</param>
        /// <param name="sentDateTime">The sent date time.</param>
        /// <param name="reason">The reason.</param>
        /// <returns></returns>
        public Campaign UpdateCampaignDeliveryStatus(string mailChimpCampaingId, CampaignDeliveryStatus status, DateTime sentDateTime, string reason)
        {
            using (CRMDb db = new CRMDb())
            {
                var campaignDb = db.Campaigns.Where(c => c.ServiceProviderCampaignID == mailChimpCampaingId).FirstOrDefault();
                db.CampaignRecipients.Where(c => c.CampaignID == campaignDb.CampaignID && c.AccountID == campaignDb.AccountID)
                    .ForEach(c =>
                    {
                        c.DeliveryStatus = (short)status;
                        c.DeliveredOn = sentDateTime;
                        c.Remarks = reason;

                    });

                db.SaveChanges();
                Campaign campaign = Mapper.Map<CampaignsDb, Campaign>(campaignDb);
                return campaign;
            }
        }

        /// <summary>
        /// Updates the campaign failed status.
        /// </summary>
        /// <param name="campaingId">The campaing identifier.</param>
        /// <param name="status">The status.</param>
        /// <param name="remarks">The remarks.</param>
        /// <returns></returns>
        public Campaign UpdateCampaignFailedStatus(int campaingId, CampaignStatus status, string remarks)
        {
            using (CRMDb db = new CRMDb())
            {
                var campaign = db.Campaigns.Where(c => c.CampaignID == campaingId).First();
                campaign.CampaignStatusID = (byte)status; //sent
                db.Entry<CampaignsDb>(campaign).State = System.Data.Entity.EntityState.Modified;

                campaign.Remarks = remarks;
                db.SaveChanges();
                return Mapper.Map<CampaignsDb, Campaign>(campaign);
            }
        }

        /// <summary>
        /// Cancels the campaign.
        /// </summary>
        /// <param name="campaignId">The campaign identifier.</param>
        public Campaign CancelCampaign(int campaignId)
        {
            var db = ObjectContextFactory.Create();
            var campaign = db.Campaigns.SingleOrDefault(c => c.CampaignID == campaignId);
            if (campaign != null)
                campaign.CampaignStatusID = (byte)CampaignStatus.Cancelled;
            db.SaveChanges();
            return Mapper.Map<CampaignsDb, Campaign>(campaign);

        }

        /// <summary>
        /// Gets the campaign search definitions map.
        /// </summary>
        /// <param name="campaignId">The campaign identifier.</param>
        /// <returns></returns>
        public IEnumerable<SearchDefinition> GetCampaignSearchDefinitionsMap(int campaignId)
        {
            var searchDefinitionsDb = ObjectContextFactory.Create().CampaignSearchDefinitions
                .Include(c => c.SearchDefinition).Where(c => c.CampaignID == campaignId).Select(c => c.SearchDefinition).ToList();

            return Mapper.Map<IEnumerable<SearchDefinitionsDb>, IEnumerable<SearchDefinition>>(searchDefinitionsDb);
        }

        /// <summary>
        /// Record a campaign open or click entry
        /// </summary>
        /// <param name="campaignId"></param>
        /// <param name="linkId"></param>
        /// <param name="campaignRecipientId"></param>
        public CampaignRecipient InsertCampaignOpenEntry(int campaignId, byte? linkId, int campaignRecipientId)
        {
            var procedureName = "[dbo].[INSERT_CAMPAIGN_OPENENTRY]";
            var parms = new List<SqlParameter>
                {
                    new SqlParameter{ParameterName ="@LinkID", Value = linkId.HasValue ? (int)linkId.Value : -1 },
                    new SqlParameter{ParameterName ="@CampaignId", Value = campaignId},
                    new SqlParameter{ParameterName ="@CampaignRecipientId ", Value = campaignRecipientId}
                };
            CRMDb context = new CRMDb();
            var objectContext = (context as IObjectContextAdapter).ObjectContext;
            IEnumerable<CampaignRecipient> campaignRecipient = context.ExecuteStoredProcedure<CampaignRecipient>(procedureName, parms).ToList();

            return campaignRecipient.FirstOrDefault();
        }

        /// <summary>
        /// Gets the campaign statistics.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <param name="campaignId">The campaign identifier.</param>
        /// <returns></returns>
        public CampaignStatistics GetCampaignStatistics(int accountId, int campaignId)
        {
            CampaignStatistics campaignStatistics = new CampaignStatistics();
            var db = ObjectContextFactory.Create();

            var sql = @"SELECT CampaignId, Recipients as RecipientCount, [Sent], Delivered, Bounced, Blocked, Opened,Clicked, Complained,OptedOut AS UnSubscribed FROM CampaignAnalytics (NOLOCK) WHERE CampaignID = @cid; 
            SELECT CampaignID Id,Name,HTMLContent, CampaignStatusID CampaignStatus, [Subject], ProcessedDate, SenderName, [From],ServiceProviderCampaignID,AccountId, ServiceProviderId,TagRecipients, SSRecipients,CampaignTypeID,IsLinkedToWorkflows FROM Campaigns (NOLOCK) WHERE CampaignID = @cid
                        SELECT CL.CampaignId, CL.CampaignLinkID, COUNT(DISTINCT CampaignRecipientID) UniqueClickCount,  CL.LinkIndex, CL.Url LinkUrl FROM CampaignStatistics (NOLOCK) CS
                        INNER JOIN CampaignLinks (NOLOCK) CL ON CL.CampaignLinkID = CS.CampaignLinkID
                        WHERE CL.CampaignID = @cid AND CS.AccountID = @accountId
                        GROUP BY CL.CampaignId, CL.CampaignLinkId,CL.Url, CL.LinkIndex";
            db.GetMultiple(sql, (r) =>
            {
                var cs = r.Read<CampaignStatistics>().ToList();
                List<Campaign> cp = r.Read<Campaign>().ToList();
                campaignStatistics = cs.Any() ? cs.FirstOrDefault() : campaignStatistics;
                Campaign cpEntity = cp.Any() ? cp.FirstOrDefault() : cp.FirstOrDefault();
                campaignStatistics.UniqueClicks = r.Read<CampaignLinkContent, string, CampaignLinkContent>((cl, s) =>
                {
                    cl.Link = new Url();
                    cl.Link.URL = s;
                    return cl;
                }, splitOn: "linkUrl").ToList();
                campaignStatistics.Campaign = cpEntity;
                campaignStatistics.SentOn = cpEntity.ProcessedDate;
                campaignStatistics.NotViewed = campaignStatistics.Delivered - campaignStatistics.Opened;
            }, new { cid = campaignId, accountId = accountId });

            /*
             * Re-initiate db connection
             * **/
            db = ObjectContextFactory.Create();
            var serviceProvider = db.ServiceProviders.Where(c => c.ServiceProviderID == campaignStatistics.Campaign.ServiceProviderID).FirstOrDefault();
            if (serviceProvider == null)
            {
                var defaultServiceprovider = db.ServiceProviders.Where(c => c.AccountID == campaignStatistics.Campaign.AccountID && c.IsDefault == true).FirstOrDefault();
                if (defaultServiceprovider != null)
                {
                    campaignStatistics.MailProvider = defaultServiceprovider.ProviderName;
                    campaignStatistics.ServiceProviderGuid = defaultServiceprovider.LoginToken;
                }
            }
            else
            {
                campaignStatistics.MailProvider = serviceProvider.ProviderName;
                campaignStatistics.ServiceProviderGuid = serviceProvider.LoginToken;
            }
            return campaignStatistics;
        }

        /// <summary>
        /// Gets the campaign statistics.
        /// </summary>
        /// <param name="campaignId">The campaign identifier.</param>
        /// <returns></returns>
        //public CampaignStatistics GetCampaignStatistics(int campaignId, int accountId)
        //{
        //    CampaignStatistics campaignStatistics = new CampaignStatistics();
        //    var db = ObjectContextFactory.Create();
        //    campaignStatistics.RecipientCount = db.CampaignRecipients.Count(cr => cr.CampaignID == campaignId && cr.AccountID == accountId);
        //    campaignStatistics.Sent = db.CampaignRecipients
        //        .Where(c => (c.DeliveryStatus != null || c.DeliveryStatus == (short?)CampaignDeliveryStatus.Failed) && c.CampaignID == campaignId && c.AccountID == accountId).Count();
        //    CampaignsDb campaignDb = db.Campaigns.Where(c => c.CampaignID == campaignId).FirstOrDefault();
        //    campaignDb.Links = new List<CampaignLinksDb>();
        //    try
        //    {
        //        campaignDb.Links = db.CampaignLinks.Where(c => c.CampaignID == campaignId).ToList();
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.Current.Error("Exception occurred while getting campaign links.", ex);
        //        return null;
        //    }

        //    campaignStatistics.Campaign = Mapper.Map<CampaignsDb, Campaign>(campaignDb);
        //    campaignStatistics.Opens = db.vCampaignStatistics.Where(cs => cs.CampaignID == campaignId)
        //                 .Select(cs => new CampaignLinkContent() { RecipientId = cs.CampaignRecipientId, LinkIndex = cs.LinkIndex }).ToList();
        //    campaignStatistics.Clicks = campaignStatistics.Opens.Where(c => c.LinkIndex != null).Select(x => new CampaignLinkContent() { RecipientId = x.RecipientId }).Distinct().ToList();

        //    campaignStatistics.UniqueClicks = db.vCampaignStatistics.Where(cs => cs.CampaignID == campaignId && cs.LinkIndex != null && cs.CampaignLinkID != null)
        //                                                           .Join(db.CampaignLinks, i => i.CampaignLinkID, j => j.CampaignLinkID, (i, j) => new
        //                                                           {
        //                                                               RecipientID = i.CampaignRecipientId,
        //                                                               CampaignID = i.CampaignID,
        //                                                               CampaignLinkID = i.CampaignLinkID,
        //                                                               LinkIndex = i.LinkIndex,
        //                                                               Link = j.URL,

        //                                                           })
        //                                                           .GroupBy(g => new { g.CampaignLinkID })
        //                                                           .Select(cs => new CampaignLinkContent()
        //                                                           {

        //                                                               CampaignLinkID = cs.Key.CampaignLinkID,
        //                                                               Link = cs.Select(g => new Url() { URL = g.Link }).FirstOrDefault(),
        //                                                               LinkIndex = cs.Select(g => g.LinkIndex).FirstOrDefault(),
        //                                                               UniqueClickCount = cs.Select(x => x.RecipientID).Distinct().Count()
        //                                                           }).Distinct().ToList();


        //    var campaignRecipients = db.CampaignRecipients.Where(cr => cr.CampaignID == campaignId && cr.AccountID == accountId);
        //    //campaignStatistics.Bounced = db.CampaignRecipients.Where(cr)
        //    campaignStatistics.UnSubscribed = campaignRecipients.Where(cr => cr.HasUnsubscribed == true).Count();
        //    campaignStatistics.Complained = campaignRecipients.Where(cr => cr.HasComplained == true).Count();
        //    campaignStatistics.Bounced = campaignRecipients.Where(cr => cr.DeliveryStatus == (short)CampaignDeliveryStatus.SoftBounce
        //                                                         || cr.DeliveryStatus == (short)CampaignDeliveryStatus.HardBounce).Count();
        //    campaignStatistics.Delivered = campaignRecipients.Where(cr => cr.DeliveryStatus == (short)CampaignDeliveryStatus.Delivered).Count();
        //    campaignStatistics.CampaignId = campaignId;
        //    campaignStatistics.Clicked = campaignStatistics.Clicks.Select(x => x.RecipientId).Distinct().Count();
        //    campaignStatistics.Opened = campaignStatistics.Opens.Select(x => x.RecipientId).Distinct().Count();
        //    campaignStatistics.SentOn = db.CampaignRecipients.Where(c => c.DeliveryStatus == (short)CampaignDeliveryStatus.Delivered && c.CampaignID == campaignId && c.AccountID == accountId).Select(c => c.SentOn).FirstOrDefault();

        //    campaignStatistics.MailProvider = db.ServiceProviders.Where(c => c.ServiceProviderID == campaignDb.ServiceProviderID).Select(c => c.LoginToken).FirstOrDefault().ToString();
        //    return campaignStatistics;
        //}

        /// <summary>
        /// Gets the link URL.
        /// </summary>
        /// <param name="campaignId">The campaign identifier.</param>
        /// <param name="linkIndex">Index of the link.</param>
        /// <returns></returns>
        public CampaignLink GetLinkUrl(int campaignId, byte? linkIndex)
        {
            var db = ObjectContextFactory.Create();
            CampaignLinksDb campaignLinksDb = db.CampaignLinks.Where(cl => cl.CampaignID == campaignId && cl.LinkIndex == linkIndex).FirstOrDefault();
            CampaignLink campaignLink = Mapper.Map<CampaignLinksDb, CampaignLink>(campaignLinksDb);
            return campaignLink;

        }

        /// <summary>
        /// Gets the campaign link urls.
        /// </summary>
        /// <param name="campaignId">The campaign identifier.</param>
        /// <returns></returns>
        public IEnumerable<CampaignLink> GetCampaignLinkUrls(IEnumerable<int> campaignId)
        {
            var db = ObjectContextFactory.Create();
            IEnumerable<CampaignLinksDb> campaignLinksDb = db.CampaignLinks.Where(cl => campaignId.Contains(cl.CampaignID));
            IEnumerable<CampaignLink> campaignLink = Mapper.Map<IEnumerable<CampaignLinksDb>, IEnumerable<CampaignLink>>(campaignLinksDb);
            return campaignLink;
        }

        /// <summary>
        /// Gets the campaig linkn by campaignlink identifier.
        /// </summary>
        /// <param name="CampaignLinkId">The campaign link identifier.</param>
        /// <returns></returns>
        public CampaignLink GetCampaigLinknByCampaignlinkID(int CampaignLinkId)
        {
            var db = ObjectContextFactory.Create();
            CampaignLinksDb campaignLinksDb = db.CampaignLinks.Where(cl => cl.CampaignLinkID == CampaignLinkId).SingleOrDefault();
            return Mapper.Map<CampaignLinksDb, CampaignLink>(campaignLinksDb);
        }

        /// <summary>
        /// Determines whether [is image friendly name unique] [the specified image].
        /// </summary>
        /// <param name="image">The image.</param>
        /// <returns></returns>
        public bool IsImageFriendlyNameUnique(Image image)
        {
            var db = ObjectContextFactory.Create();
            bool campaignFound = db.Images.Where(c => c.FriendlyName == image.FriendlyName && c.AccountID == image.AccountID).Select(c => c).Count() > 0;
            return campaignFound;
        }

        /// <summary>
        /// Gets the campaign status.
        /// </summary>
        /// <param name="campaignId">The campaign identifier.</param>
        /// <returns></returns>
        public CampaignStatus? GetCampaignStatus(int campaignId)
        {
            var db = ObjectContextFactory.Create();
            var campaignStatus = db.Campaigns.Where(c => c.CampaignID == campaignId && c.IsDeleted == false).Select(s => s.CampaignStatusID).FirstOrDefault();
            return (CampaignStatus)campaignStatus;
        }

        /// <summary>
        /// Updates the campaign recipientopt out status.
        /// </summary>
        /// <param name="campaignId">The campaign identifier.</param>
        /// <param name="contactId">The contact identifier.</param>
        public void UpdateCampaignRecipientoptOutStatus(int campaignId, int contactId, int workflowId=0)
        {
            using (var db = new CRMDb())
            {
                var dapperDb = ObjectContextFactory.Create();
                string sql = string.Empty;
                
                if(workflowId > 0)
                {
                    sql = @"SELECT TOP 1 CR.* FROM CampaignRecipients (NOLOCK) CR 
                            INNER JOIN Campaigns (NOLOCK) C ON C.AccountID = CR.AccountID AND CR.CampaignID = C.CampaignID
                            WHERE CR.CampaignID = @campaignId AND CR.ContactID = @contactId AND WorkflowID = @workflowId";

                }
                else
                {
                    sql = @"SELECT TOP 1 CR.* FROM CampaignRecipients (NOLOCK) CR 
                            INNER JOIN Campaigns (NOLOCK) C ON C.AccountID = CR.AccountID AND CR.CampaignID = C.CampaignID
                            WHERE CR.CampaignID = @campaignId AND CR.ContactID = @contactId";
                }

                var recipient = dapperDb.Get<CampaignRecipientsDb>(sql, new { campaignId = campaignId, contactId = contactId, workflowId = workflowId }).FirstOrDefault();
                if (recipient != null)
                {
                    db.Entry<CampaignRecipientsDb>(recipient).State = System.Data.Entity.EntityState.Modified;
                    recipient.OptOutStatus = (short?)CampaignOptOutStatus.Unsubscribed;
                    recipient.HasUnsubscribed = true;
                    recipient.UnsubscribedOn = DateTime.Now.ToUniversalTime();
                    recipient.LastModifiedOn = DateTime.Now.ToUniversalTime();
                    db.SaveChanges();
                }
            }
        }

        /// <summary>
        /// Updates the campaign recipient status.
        /// </summary>
        /// <param name="campaignRecipientId">The campaign recipient identifier.</param>
        /// <param name="deliveryStatus">The delivery status.</param>
        /// <param name="deliveredOn">The delivered on.</param>
        /// <param name="remarks">The remarks.</param>
        /// <param name="serviceProviderId">The service provider identifier.</param>
        /// <param name="timeLogged">The time logged.</param>
        /// <param name="optOutStatus">The opt out status.</param>
        public void UpdateCampaignRecipientStatus(int campaignRecipientId, CampaignDeliveryStatus deliveryStatus, DateTime deliveredOn, string remarks, int? serviceProviderId, DateTime timeLogged, int accountId, short? optOutStatus)
        {
            using (var db = new CRMDb())
            {
                var recipient = db.CampaignRecipients.Where(cr => cr.CampaignRecipientID == campaignRecipientId && cr.AccountID == accountId).FirstOrDefault();
                if (recipient != null)
                {
                    db.Entry<CampaignRecipientsDb>(recipient).State = System.Data.Entity.EntityState.Modified;
                    recipient.DeliveredOn = deliveredOn;
                    recipient.DeliveryStatus = (short)deliveryStatus;
                    recipient.Remarks = remarks;
                    if (optOutStatus.HasValue)
                    {
                        if (optOutStatus.Value == (short)CampaignOptOutStatus.Unsubscribed)
                        {
                            recipient.HasUnsubscribed = true;
                            recipient.UnsubscribedOn = timeLogged.ToUniversalTime();
                        }
                        else
                        {
                            recipient.HasComplained = true;
                            recipient.ComplainedOn = timeLogged.ToUniversalTime();
                        }
                    }
                    if (serviceProviderId.HasValue)
                        recipient.ServiceProviderID = serviceProviderId.Value;
                    recipient.LastModifiedOn = DateTime.Now.ToUniversalTime();
                    db.SaveChanges();
                }
            }
        }

        /// <summary>
        /// Gets the campaign recipients by last modified date.
        /// </summary>
        /// <param name="lastModified">The last modified.</param>
        /// <returns></returns>
        public IEnumerable<CampaignRecipient> GetCampaignRecipientsByLastModifiedDate(DateTime lastModified, int accountId)
        {
            var db = ObjectContextFactory.Create();
            IEnumerable<CampaignRecipient> recipientdata = db.CampaignRecipients.Where(cr => cr.LastModifiedOn >= lastModified && cr.AccountID == accountId)
                                 .Select(c => new CampaignRecipient() { ContactID = c.ContactID, To = c.To }).ToArray();
            return recipientdata;
        }

        /// <summary>
        /// Gets the delivery statuses by mail identifier.
        /// </summary>
        /// <param name="mailId">The mail identifier.</param>
        /// <param name="take">The take.</param>
        /// <returns></returns>
        public IEnumerable<CampaignDeliveryStatus> GetDeliveryStatusesByMailId(string mailId, int accountId, int take = 5)
        {
            var db = ObjectContextFactory.Create();
            var statuses = db.CampaignRecipients
                .Where(cr => cr.To == mailId && cr.DeliveryStatus != null && cr.AccountID == accountId).OrderByDescending(cr => cr.LastModifiedOn)
                .Select(cr => (CampaignDeliveryStatus)cr.DeliveryStatus)
                .Take(take)
                .ToList();
            return statuses;
        }

        /// <summary>
        /// Gets the campaign recipient.
        /// </summary>
        /// <param name="mailChimpCampaingId">The mail chimp campaing identifier.</param>
        /// <param name="email">The email.</param>
        /// <returns></returns>
        public CampaignRecipient GetCampaignRecipient(string mailChimpCampaingId, string email)
        {
            using (var db = ObjectContextFactory.Create())
            {
                var campaign = db.Campaigns.Where(c => c.ServiceProviderCampaignID == mailChimpCampaingId).Select(c => new { CampaignId = c.CampaignID, AccountId = c.AccountID }).FirstOrDefault();
                CampaignRecipientsDb campaignRecipientsDb = db.CampaignRecipients.Where(cr => cr.CampaignID == campaign.CampaignId && cr.To == email && cr.AccountID == campaign.AccountId).FirstOrDefault();
                CampaignRecipient campaignRecipient = Mapper.Map<CampaignRecipientsDb, CampaignRecipient>(campaignRecipientsDb);
                return campaignRecipient;
            }
        }

        /// <summary>
        /// Gets the campaign links.
        /// </summary>
        /// <param name="CampaignID">The campaign identifier.</param>
        /// <returns></returns>
        public IEnumerable<CampaignLink> GetCampaignLinks(int CampaignID)
        {
            var db = ObjectContextFactory.Create();
            var sql = @"SELECT * FROM CampaignLinks (NOLOCK)
                        WHERE CampaignID=@CampaignId";
            IEnumerable<CampaignLinksDb> campaignLinks = db.Get<CampaignLinksDb>(sql, new { CampaignId = CampaignID }).ToList();
            foreach (CampaignLinksDb campaignlink in campaignLinks)
                yield return Mapper.Map<CampaignLinksDb, CampaignLink>(campaignlink);
        }

        /// <summary>
        /// Gets the campaign recipient.
        /// </summary>
        /// <param name="campaignRecipientID">The campaign recipient identifier.</param>
        /// <returns></returns>
        //public CampaignRecipient GetCampaignRecipient(int campaignRecipientID, int accountId)
        //{
        //    using (var db = ObjectContextFactory.Create())
        //    {
        //        CampaignRecipientsDb campaignRecipientsDb = db.CampaignRecipients.Where(cr => cr.CampaignRecipientID == campaignRecipientID && cr.AccountID == accountId).FirstOrDefault();
        //        CampaignRecipient campaignRecipient = Mapper.Map<CampaignRecipientsDb, CampaignRecipient>(campaignRecipientsDb);
        //        return campaignRecipient;
        //    }
        //}

        /// <summary>
        /// Gets the campaign recipient.
        /// </summary>
        /// <param name="campaignRecipientID">The campaign recipient identifier.</param>
        /// <returns></returns>
        public CampaignRecipient GetCampaignRecipient(int campaignRecipientID, int accountId)
        {
            Logger.Current.Verbose(string.Format("In CampaignRepository/GetCampaignRecipient accountid: {0} crid: {1}", accountId, campaignRecipientID));
            using (var db = ObjectContextFactory.Create())
            {
                var sql = string.Empty;
                if (accountId != 0)
                    sql = @"SELECT vcr.* FROM CampaignRecipients (NOLOCK) vcr
                                WHERE vcr.CampaignRecipientID = @recipientId AND vcr.AccountId = @accountId";
                else
                    sql = @"SELECT vcr.* FROM CampaignRecipients (NOLOCK) vcr
                                WHERE vcr.CampaignRecipientID = @recipientId";
                CampaignRecipient campaignRecipient = db.Get<CampaignRecipient>(sql, new { recipientId = campaignRecipientID, accountId = accountId }, timeoutSeconds: 60).FirstOrDefault();
                return campaignRecipient;
            }
        }
        /// <summary>
        /// Gets the campaign recipientby identifier.
        /// </summary>
        /// <param name="campaignID">The campaign identifier.</param>
        /// <param name="email">The email.</param>
        /// <returns></returns>
        public CampaignRecipient GetCampaignRecipientbyID(int campaignID, string email, int accountId)
        {
            using (var db = ObjectContextFactory.Create())
            {
                CampaignRecipientsDb campaignRecipientsDb = db.CampaignRecipients.Where(cr => cr.CampaignID == campaignID && cr.AccountID == accountId && cr.To == email).FirstOrDefault();
                CampaignRecipient campaignRecipient = Mapper.Map<CampaignRecipientsDb, CampaignRecipient>(campaignRecipientsDb);
                return campaignRecipient;
            }
        }

        /// <summary>
        /// Gets the campaign list by clicks.
        /// </summary>
        /// <param name="customStartDate">The custom start date.</param>
        /// <param name="customEnddate">The custom enddate.</param>
        /// <param name="accountID">The account identifier.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="limit">The limit.</param>
        /// <param name="isAccountAdmin">if set to <c>true</c> [is account admin].</param>
        /// <param name="userID">The user identifier.</param>
        /// <param name="TotalHits">The total hits.</param>
        /// <param name="isReputationReport">if set to <c>true</c> [is reputation report].</param>
        /// <returns></returns>
        public IEnumerable<CampaignReportData> GetCampaignListByClicks(DateTime customStartDate, DateTime customEnddate, int accountID,
            int pageNumber, long limit, bool isAccountAdmin, int userID, out int TotalHits, bool isReputationReport)
        {
            var procedureName = "[dbo].[GET_Account_Campagin_Reports]";
            var parms = new List<SqlParameter>
                {
                    new SqlParameter{ParameterName ="@AccountID", Value= accountID},
                    new SqlParameter{ParameterName ="@StartDate", Value= customStartDate.Date},
                    new SqlParameter{ParameterName="@EndDate ", Value = customEnddate.Date},
                    new SqlParameter{ParameterName="@IsAdmin", Value = isAccountAdmin},
                    new SqlParameter{ParameterName="@UserID", Value = userID},

                };
            CRMDb context = new CRMDb();
            var objectContext = (context as IObjectContextAdapter).ObjectContext;
            objectContext.CommandTimeout = 1600;
            IEnumerable<CampaignReportData> reportData = context.ExecuteStoredProcedure<CampaignReportData>(procedureName, parms).ToList();
            if (isReputationReport == true)
                reportData = reportData.Where(p => p.TotalSends >= 10);
            TotalHits = reportData.Count();
            return reportData;           //.Skip(records).Take((int)limit);
        }

        public IEnumerable<string> GetWorkflwosForCampaignReport(int campaignId)
        {
            var procedureName = "[dbo].[GET_Account_Campagin_Reports]";
            var parms = new List<SqlParameter>
                {
                    new SqlParameter{ParameterName ="@AccountID", Value = 0},
                    new SqlParameter{ParameterName ="@StartDate", Value= DateTime.UtcNow},
                    new SqlParameter{ParameterName="@EndDate ", Value = DateTime.UtcNow},
                    new SqlParameter{ParameterName="@IsAdmin", Value = 1},
                    new SqlParameter{ParameterName="@UserID", Value = 0},
                    new SqlParameter{ParameterName="@campaignId", Value = campaignId},
                };
            CRMDb context = new CRMDb();
            var objectContext = (context as IObjectContextAdapter).ObjectContext;
            objectContext.CommandTimeout = 1600;
            IEnumerable<string> workflowNames = context.ExecuteStoredProcedure<string>(procedureName, parms);
            return workflowNames;
        }

        public DataTable GetCampaignReportExport(DateTime customStartDate, DateTime customEndDate, bool isAccountAdmin, int accountId, int? userId)
        {
            var procedureName = "[dbo].[GET_Account_Campagin_Reports]";
            var parms = new List<SqlParameter>
                {
                    new SqlParameter{ParameterName ="@AccountID", Value= accountId},
                    new SqlParameter{ParameterName ="@StartDate", Value= customStartDate.Date},
                    new SqlParameter{ParameterName="@EndDate ", Value = customEndDate.Date},
                    new SqlParameter{ParameterName="@IsAdmin", Value = isAccountAdmin},
                    new SqlParameter{ParameterName="@UserID", Value = userId}
                };
            var db = ObjectContextFactory.Create();
            db.Database.Connection.Open();
            var cmd = db.Database.Connection.CreateCommand();
            cmd.CommandText = procedureName;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandTimeout = 1600;
            List<SqlParameter> parameters = parms;
            parameters.ForEach(p =>
            {
                cmd.Parameters.Add(p);
            });
            DbDataReader reader = cmd.ExecuteReader();
            DataTable table = new DataTable();
            table.Load(reader);
            return table;
        }

        /// <summary>
        /// Gets the campaign list by clicks.
        /// </summary>
        /// <param name="customStartDate">The custom start date.</param>
        /// <param name="customEnddate">The custom enddate.</param>
        /// <param name="accountID">The account identifier.</param>
        /// <param name="isAccountAdmin">if set to <c>true</c> [is account admin].</param>
        /// <param name="userID">The user identifier.</param>
        /// <returns></returns>
        public IEnumerable<CampaignReportData> GetCampaignListByClicks(DateTime customStartDate, DateTime customEnddate, int accountID,
               bool isAccountAdmin, int userID)
        {

            var procedureName = "[dbo].[GET_Account_Campagin_Reports]";
            var parms = new List<SqlParameter>
                {
                new SqlParameter{ParameterName ="@AccountID", Value= accountID},
                    new SqlParameter{ParameterName ="@StartDate", Value= customStartDate.Date},
                    new SqlParameter{ParameterName="@EndDate ", Value = customEnddate.Date},
                    new SqlParameter{ParameterName="@IsAdmin", Value = isAccountAdmin},
                    new SqlParameter{ParameterName="@UserID", Value = userID},
                };
            CRMDb context = new CRMDb();
            var objectContext = (context as IObjectContextAdapter).ObjectContext;
            objectContext.CommandTimeout = 1600;
            IEnumerable<CampaignReportData> reportData = context.ExecuteStoredProcedure<CampaignReportData>(procedureName, parms).ToList();

            return reportData = reportData.OrderByDescending(cn => cn.TotalClicks).Take(10);
        }

        /// <summary>
        /// Updates the automation campaign recipients.
        /// </summary>
        /// <param name="contactId">The contact identifier.</param>
        /// <param name="campaignId">The campaign identifier.</param>
        /// <param name="workflowId">The workflow identifier.</param>
        public void UpdateAutomationCampaignRecipients(int contactId, int campaignId, int workflowId, int accountId)
        {
            var db = ObjectContextFactory.Create();
            if (contactId != 0 && campaignId != 0)
            {
                var sql = @"DECLARE @ContactEmail VARCHAR(250) = ''
                            SELECT @ContactEmail = CE.Email FROM ContactEmails (NOLOCK) CE WHERE CE.ContactID = @ContactID AND CE.AccountID = @AccountID AND CE.IsPrimary = 1  
                            AND (CE.EmailStatus = 51 OR CE.EmailStatus = 50 OR CE.EmailStatus = 52)
                            IF (@ContactEmail IS NOT NULL AND @ContactEmail != '')
                            BEGIN
	                            INSERT INTO CampaignRecipients 
	                            (CampaignID,ContactID,CreatedDate,[To],ScheduleTime,SentOn,[GUID],DeliveredOn,DeliveryStatus,LastModifiedOn,OptOutStatus,Remarks
	                            ,ServiceProviderID,HasUnsubscribed,UnsubscribedOn,HasComplained,ComplainedOn,WorkflowID, AccountId) 
	                            SELECT @CampaignId, @ContactId, GETUTCDATE(), @ContactEmail, GETUTCDATE(), NULL,NULL,NULL,NULL,GETUTCDATE(),0,NULL,
	                            NULL, 0, NULL, 0 ,NULL, @WorkflowId, @AccountId
                            END";
                db.Execute(sql, new { CampaignId = campaignId, ContactId = contactId, WorkflowId = (short)workflowId, AccountId = accountId });
            }
        }

        /// <summary>
        /// Gets the next automation campaign recipients.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<CampaignRecipient> GetNextAutomationCampaignRecipients()
        {
            var db = ObjectContextFactory.Create();
            List<CampaignRecipient> recipients = new List<CampaignRecipient>();
            Logger.Current.Informational("Request received to get next workflow campaign recipient.");
            var sql = @"SELECT * FROM CampaignRecipients(NOLOCK) WHERE ScheduleTime < GETUTCDATE() AND DeliveryStatus IS NULL AND WorkflowID IS NOT NULL";
            var campaignReipients = db.Get<CampaignRecipientsDb>(sql);
            if (campaignReipients != null && campaignReipients.Count() > 0)
                recipients = Mapper.Map<List<CampaignRecipientsDb>, List<CampaignRecipient>>(campaignReipients.ToList());
            return recipients;
        }

        /// <summary>
        /// Gets the automation campaigns.
        /// </summary>
        /// <param name="campaignIds">The campaign ids.</param>
        /// <returns></returns>
        public IEnumerable<Campaign> GetAutomationCampaigns(IEnumerable<int> campaignIds)
        {
            IList<Campaign> campaigns = new List<Campaign>();
            if (campaignIds != null)
            {
                var db = ObjectContextFactory.Create();
                var sql = @"select c.*, ISNULL(cptc.PlainTextContent,'') PlainTextContent from campaigns (nolock) c
                                left join CampaignPlainTextContentMap cptc on c.CampaignID=cptc.CampaignID
                                where c.IsLinkedToWorkflows = 1 
                                and c.CampaignStatusID = 107 
                                and c.CampaignID in  (select datavalue from dbo.SPlit(@campaignIds,','))";

                var campaignsDb = db.Get<CampaignsDb>(sql, new { campaignIds = string.Join(",", campaignIds) }).ToList();
                foreach (CampaignsDb campaignDb in campaignsDb)
                    campaigns.Add(Mapper.Map<CampaignsDb, Campaign>(campaignDb));
            }
            return campaigns;
        }

        /// <summary>
        /// Gets the posts.
        /// </summary>
        /// <param name="campaignID">The campaign identifier.</param>
        /// <param name="userID">The user identifier.</param>
        /// <param name="communicationType">Type of the communication.</param>
        /// <returns></returns>
        public IEnumerable<UserSocialMediaPosts> GetPosts(int campaignID, int userID, string communicationType)
        {
            var db = ObjectContextFactory.Create();
            var predicate = PredicateBuilder.True<UserSocialMediaPostsDb>();
            predicate = predicate.And(post => post.CampaignID == campaignID);
            if (userID != 0)
                predicate = predicate.And(post => post.UserID == userID);
            if (!string.IsNullOrEmpty(communicationType))
                predicate = predicate.And(post => post.CommunicationType == communicationType);
            var posts = db.UserSocialMediaPosts.AsExpandable().Where(predicate);
            return Mapper.Map<IEnumerable<UserSocialMediaPosts>>(posts);
        }

        /// <summary>
        /// Gets the campaign ids seeking analysis.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Campaign> GetCampaignIdsSeekingAnalysis()
        {
            var db = ObjectContextFactory.Create();
            var campaigns = db.Campaigns
                .Where(c => c.CampaignStatusID == (short)CampaignStatus.Analyzing)
                .Select(c => new Campaign { Id = c.CampaignID, ServiceProviderCampaignID = c.ServiceProviderCampaignID, AccountID = c.AccountID, ServiceProviderID = c.ServiceProviderID });
            return campaigns;
        }

        /// <summary>
        /// Gets the campaign themes.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        public IEnumerable<CampaignTheme> GetCampaignThemes(int userId, int accountId)
        {
            var db = ObjectContextFactory.Create();
            var campaignThemesDb = db.CampaignThemes.Where(t => t.AccountID == accountId).ToList();
            var campaignThemes = Mapper.Map<IEnumerable<CampaignThemesDb>, IEnumerable<CampaignTheme>>(campaignThemesDb);
            return campaignThemes;
        }

        /// <summary>
        /// Determines whether [is duplicate campaign layout] [the specified name].
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        public bool IsDuplicateCampaignLayout(string name, int accountId)
        {
            var db = ObjectContextFactory.Create();
            return db.CampaignTemplates.Where(ct => (ct.AccountId == null || ct.AccountId == accountId) && (ct.Name == name)).Any();
        }

        /// <summary>
        /// Gets the templates for contact emails.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        public IEnumerable<CampaignTemplate> GetTemplatesForContactEmails(int accountId)
        {
            var predicate = PredicateBuilder.True<CampaignTemplatesDb>();
            predicate = predicate.And(i => i.AccountId == null || i.AccountId == accountId);
            predicate = predicate.And(i => i.Type != CampaignTemplateType.Layout);
            predicate = predicate.And(i => i.Status == 0);
            var db = ObjectContextFactory.Create();
            var templates = db.CampaignTemplates.Include(c => c.Image)
                              .AsExpandable()
                              .Where(predicate).Select(c =>
                                new
                                {
                                    CampaignTemplateID = c.CampaignTemplateID,
                                    Name = c.Name,
                                    Type = c.Type,
                                    Image = c.Image
                                }).AsEnumerable().Select(x => new CampaignTemplatesDb
                                {
                                    CampaignTemplateID = x.CampaignTemplateID,
                                    Name = x.Name,
                                    Type = x.Type,
                                    Image = x.Image
                                });

            var sentCampaigns = db.Campaigns.Where(i => i.CampaignStatusID == (short)CampaignStatus.Sent && i.AccountID == accountId)
                                   .AsEnumerable().Select(c => new CampaignTemplatesDb
                                   {
                                       CampaignTemplateID = c.CampaignID,
                                       Name = c.Name,
                                       Type = CampaignTemplateType.SentCampaigns,
                                       Image = null
                                   });

            var alltemplates = templates.Union(sentCampaigns);
            var AllTemplates = Mapper.Map<IEnumerable<CampaignTemplatesDb>, IEnumerable<CampaignTemplate>>(alltemplates);
            return AllTemplates;
        }

        /// <summary>
        /// Gets the template HTML.
        /// </summary>
        /// <param name="ID">The identifier.</param>
        /// <param name="Type">The type.</param>
        /// <returns></returns>
        public string GetTemplateHTML(int ID, CampaignTemplateType Type)
        {
            using (var db = ObjectContextFactory.Create())
            {
                string query = string.Empty;
                if (Type == CampaignTemplateType.SentCampaigns)
                    query = @"SELECT HTMLContent FROM Campaigns(NOLOCK) WHERE CampaignID=@Id";
                else
                    query = @"SELECT HTMLContent FROM CampaignTemplates (NOLOCK) WHERE CampaignTemplateID=@Id";

                return db.Get<string>(query,new { Id = ID }).FirstOrDefault();
            }
        }

        /// <summary>
        /// Updates the campaign recipients status.
        /// </summary>
        /// <param name="campaignRecipientIDs">The campaign recipient i ds.</param>
        /// <param name="remarks">The remarks.</param>
        /// <param name="deliveredOn">The delivered on.</param>
        /// <param name="sentOn">The sent on.</param>
        /// <param name="deliveryStatus">The delivery status.</param>
        public void UpdateCampaignRecipientsStatus(List<int> campaignRecipientIDs, string remarks, DateTime deliveredOn, DateTime sentOn, CampaignDeliveryStatus deliveryStatus)
        {
            using (var db = ObjectContextFactory.Create())
            {
                var parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter { ParameterName = "@campaignRecipientIDs", Value = campaignRecipientIDs.ToContactIdTypeDataTable(), SqlDbType = System.Data.SqlDbType.Structured, TypeName = "dbo.Contact_List" });
                parameters.Add(new SqlParameter("@remarks", remarks));
                parameters.Add(new SqlParameter("@deliveredOn", deliveredOn));
                parameters.Add(new SqlParameter("@sentOn", sentOn));
                parameters.Add(new SqlParameter("@deliveryStatus", (short)deliveryStatus));

                db.ExecuteStoredProcedure("UpdateCampaignRecipientsStatus", parameters, 360);
            }
        }

        public IEnumerable<CampaignUniqueRecipient> GetCampaignUniqueRecipients(IEnumerable<int> tags, IEnumerable<SearchDefinitionContact> searchDefinitions, int accountId, int roleId, int owner, bool isDataSharingOn)
        {
            IEnumerable<CampaignUniqueRecipient> recipients = new List<CampaignUniqueRecipient>();
            using (var db = ObjectContextFactory.Create())
            {
                db.QueryStoredProc("dbo.GetUniqueRecpients", (reader) =>
                {
                    recipients = reader.Read<CampaignUniqueRecipient>().ToList();
                }, new
                {
                    Tags = tags.AsTableValuedParameter("dbo.Contact_List"),
                    SDefinitions = searchDefinitions.AsTableValuedParameter("dbo.SavedSearchContacts", new string[] { "GroupId", "SearchDefinitionId", "ContactId" }),
                    AccountId = accountId,
                    RoleId = roleId,
                    OwnerId = owner,
                    IsDataSharingOn = isDataSharingOn
                });
                return recipients;
            }
        }

        public int GetUniqueContactscount(IEnumerable<int> tags, IEnumerable<SearchDefinitionContact> searchDefinitions, int accountId)
        {
            int count = searchDefinitions.Count();
            using (var db = ObjectContextFactory.Create())
            {
                db.QueryStoredProc("dbo.GetUniqueContacts", (reader) =>
                {
                    count = reader.Read<int>().FirstOrDefault();
                }, new
                {
                    Tags = tags.AsTableValuedParameter("dbo.Contact_List"),
                    SDefinitions = searchDefinitions.AsTableValuedParameter("dbo.SavedSearchContacts", new string[] { "GroupId", "SearchDefinitionId", "ContactId" }),
                    AccountId = accountId,
                    EntityType = tags.IsAny() ? 1 : 2
                });
            }
            return count;
        }

        public void InsertBulkRecipients(IEnumerable<TemporaryRecipient> temporaryRecipients)
        {
            using (var db = ObjectContextFactory.Create())
            {
                var recipients = Mapper.Map<IEnumerable<TemporaryRecipient>, IEnumerable<MomentaryCampaignRecipientsDb>>(temporaryRecipients).ToList();
                Logger.Current.Informational("Momentary Recipients count " + recipients.Count());
                db.BulkInsert<MomentaryCampaignRecipientsDb>(recipients);
            }
        }

        public void InsertCampaignLogDetails(IEnumerable<CampaignLogDetails> campaignLogDetails)
        {
            var db = ObjectContextFactory.Create();
            List<CampaignLogDetailsDb> CampaignLogDetails = Mapper.Map<IEnumerable<CampaignLogDetails>, IEnumerable<CampaignLogDetailsDb>>(campaignLogDetails).ToList();
            db.BulkInsert<CampaignLogDetailsDb>(CampaignLogDetails);
        }

        public Campaign GetCampaignBasicInfoById(int campaignId)
        {
            using (var db = ObjectContextFactory.Create())
            {
                var campaignDb = db.Campaigns.Where(c => c.CampaignID == campaignId).FirstOrDefault();
                return Mapper.Map<CampaignsDb, Campaign>(campaignDb);
            }
        }

        /// <summary>
        /// Get Campaign Name By Id
        /// </summary>
        /// <param name="campaignId"></param>
        /// <returns></returns>
        public string GetCampaignNameById(int campaignId)
        {
            var db = ObjectContextFactory.Create();
            var sql = @"SELECT Name FROM Campaigns(NOLOCK) WHERE CampaignID=@campaignID";
            string campaignName = db.Get<string>(sql, new { campaignID = campaignId }).FirstOrDefault();
            return campaignName;
        }

        /// <summary>
        /// Get URL By LinkId
        /// </summary>
        /// <param name="linkId"></param>
        /// <returns></returns>
        public string GetCampaignLinkURLByLinkId(int linkId)
        {
            var db = ObjectContextFactory.Create();
            var sql = @"SELECT URL FROM CampaignLinks (NOLOCK) WHERE CampaignLinkID=@linkID";
            string URL = db.Get<string>(sql, new { linkID = linkId }).FirstOrDefault();
            return URL;
        }

        public IEnumerable<CampaignLinkInfo> GetWorkflowCampaignActionLinks(int accountId)
        {
            using (var db = ObjectContextFactory.Create())
            {
                var sql = @"exec GetWorkflowCampaignActionLinks @accountId";
                IEnumerable<CampaignLinkInfo> campaignLinks = db.Get<CampaignLinkInfo>(sql, new { accountId = accountId }).ToList();
                return campaignLinks;
            }
        }

        public IEnumerable<CampaignReEngagementInfo> GetReEngagementSummary(DateTime startDate, DateTime endDate, bool isDefaultDateRange, int accountId, IEnumerable<int> linkIds)
        {
            using (var db = ObjectContextFactory.Create())
            {
                var procedureName = "[dbo].[Get_ReEngamentClickSummaryReport]";
                var minDate = (DateTime)System.Data.SqlTypes.SqlDateTime.MinValue;
                if (isDefaultDateRange == false)
                {
                    endDate = endDate.AddDays(1).AddSeconds(-1);
                    Logger.Current.Verbose("Start Date: " + startDate.ToString());
                    Logger.Current.Verbose("End Date: " + endDate.ToString());
                }
                var parms = new List<SqlParameter>
                {
                    new SqlParameter{ParameterName ="@AccountID", Value= accountId},
                    new SqlParameter{ParameterName ="@FromDate", Value = startDate ==  DateTime.MinValue ? minDate.AddDays(1) : startDate},
                    new SqlParameter{ParameterName="@ToDate", Value = endDate ==  DateTime.MinValue ? minDate.AddDays(1) : endDate },
                    new SqlParameter{ParameterName="@Entities",Value = linkIds != null && linkIds.Count()>0 ? string.Join(",",linkIds) : ""},
                    new SqlParameter { ParameterName = "@IsDefaultDateRange", Value = isDefaultDateRange }
                };

                var results = db.ExecuteStoredProcedure<CampaignReEngagementInfo>(procedureName, parms);
                return results;
            }
        }

        public IEnumerable<int> GetReEngagedContacts(int accountId, int campaignId, DateTime startDate, DateTime endDate, bool isDefaultDateRange, bool hasSelectedLinks, IEnumerable<int> linkIds, byte drillDownPeriod)
        {
            Logger.Current.Verbose("In GetReEngagedContactsByCampaign. CampaignId: " + campaignId);
            using (var db = ObjectContextFactory.Create())
            {
                var minDate = (DateTime)System.Data.SqlTypes.SqlDateTime.MinValue;
                var procedureName = "[dbo].[Get_ReengagedContacts]";
                var parms = new List<SqlParameter>
                {
                    new SqlParameter { ParameterName = "@AccountId", Value= accountId },
                    new SqlParameter { ParameterName = "@StartDate", Value = startDate ==  DateTime.MinValue ? minDate : startDate },
                    new SqlParameter { ParameterName = "@EndDate", Value = endDate ==  DateTime.MinValue ? minDate : endDate },
                    new SqlParameter { ParameterName = "@CampaignId", Value= campaignId },
                    new SqlParameter { ParameterName = "@LinkIds",Value = linkIds != null && linkIds.Count()>0 ? string.Join(",",linkIds) : "" },
                    new SqlParameter { ParameterName = "@IsDefaultDateRange", Value = isDefaultDateRange },
                    new SqlParameter { ParameterName = "@HasSelectedLinks", Value = hasSelectedLinks },
                      new SqlParameter { ParameterName = "@DrilldownPeriod", Value = drillDownPeriod }
                };

                var results = db.ExecuteStoredProcedure<int>(procedureName, parms);
                var totalTotacts = results.Any() ? results.Count() : 0;
                Logger.Current.Informational(totalTotacts + " found.");
                return results ?? new List<int>();
            }

        }

        /// <summary>
        /// Getting Campaign UTM Information.
        /// </summary>
        /// <param name="campaignId"></param>
        /// <returns></returns>
        public Campaign GetCampaignUTMInformation(int campaignId)
        {
            var db = ObjectContextFactory.Create();

            var sql = @"SELECT NAME , Subject , CampaignStatusID AS CampaignStatus FROM Campaigns (NOLOCK) WHERE CampaignID=@campaignID";
            Campaign campaign = db.Get<Campaign>(sql, new { campaignID = campaignId }).FirstOrDefault();
            return campaign;
        }

        public IEnumerable<PrimitiveContactValue> GetContactMergeFields(IEnumerable<string> mergeFields, int contactId)
        {
            using (var db = ObjectContextFactory.Create())
            {
                Logger.Current.Verbose("Fetching contact values for " + contactId);
                var parameters = new List<SqlParameter>
                {
                    new SqlParameter{ParameterName="@ContactID", Value = contactId}
                };
                var contactMergeFields = db.ExecuteStoredProcedure<PrimitiveContactValue>("[dbo].[GetContactPrimaryDetails]", parameters);
                Logger.Current.Informational(contactMergeFields.Count() + " values found");
                return contactMergeFields;
            }

        }

        public IEnumerable<CampaignLitmusMap> GetPendingLitmusRequests()
        {
            using (var db = ObjectContextFactory.Create())
            {
                Logger.Current.Verbose("Fetching Campaign Litmus Map");

                var sql = "SELECT * FROM CampaignLitmusMap (NOLOCK) WHERE ProcessingStatus = @processingStatus";
                return db.Get<CampaignLitmusMap>(sql, new { processingStatus = LitmusCheckStatus.ReadyToProcesLitmusCheck });
            }
        }

        public void UpdateLitmusId(CampaignLitmusMap map)
        {
            using (var db = ObjectContextFactory.Create())
            {
                var sql = "UPDATE CampaignLitmusMap SET LitmusId = @litmusId, LastModifiedOn = GETUTCDATE(), ProcessingStatus = @status, Remarks = @remarks WHERE CampaignLitmusMapId = @campaignLitmusMapId";
                db.Execute(sql, new
                {
                    litmusId = map.LitmusId,
                    status = map.ProcessingStatus,
                    remarks = map.Remarks,
                    campaignLitmusMapId = map.CampaignLitmusMapId
                });
            }
        }

        public void RequestLitmusCheck(int campaignId)
        {
            using (var db = ObjectContextFactory.Create())
            {
                var sql = "INSERT INTO CampaignLitmusMap (CampaignId, LitmusId, ProcessingStatus, CreatedOn, LastModifiedOn, Remarks ) VALUES (@campaignId, NULL,@processingStatus,GETUTCDATE(),GETUTCDATE(),'litmus test requested')";
                db.Execute(sql, new { campaignId = campaignId, processingStatus = LitmusCheckStatus.ReadyToProcesLitmusCheck });
            }
        }

        public void InsertMailTesterRequest(int campaignId, Guid guid, int userId)
        {
            using (var db = ObjectContextFactory.Create())
            {
                var sql = "INSERT INTO CampaignMailTest (CampaignId, UniqueID, Status,  CreatedBy, CreatedOn) VALUES (@campaignId, @guid, @processingStatus, @createdBy, GETUTCDATE())";
                db.Execute(sql, new { campaignId = campaignId, guid = guid, processingStatus = LitmusCheckStatus.ReadyToProcesLitmusCheck, createdBy = userId });
            }
        }

        public IEnumerable<CampaignMailTester> GetMailTesterRequests()
        {
            using (var db = ObjectContextFactory.Create())
            {
                Logger.Current.Informational("Db request for fetching MailTester requests");

                var sql = @"SELECT CMT.*, C.Name FROM CampaignMailTest (NOLOCK) CMT
                            JOIN Campaigns (NOLOCK) C ON C.CampaignID = CMT.CampaignID
                            WHERE CMT.Status = 131 AND C.IsDeleted = 0";
                return db.Get<CampaignMailTester>(sql, new { processingStatus = LitmusCheckStatus.ReadyToProcesLitmusCheck });
            }
        }

        public void UpdateCampaignMailTester(IEnumerable<CampaignMailTester> mailTester)
        {
            if (mailTester.IsAny())
            { 
                var db = ObjectContextFactory.Create();
                IEnumerable<int> Ids = mailTester.Select(s => s.CampaignMailTestID);
                var mailTesterDb = db.CampaignMailTest.Where(w => Ids.Contains(w.CampaignMailTestID));
                if (mailTesterDb.IsAny())
                {
                    foreach (var test in mailTesterDb)
                    { 
                        CampaignMailTester mailTest = mailTester.Where(w => w.CampaignMailTestID == test.CampaignMailTestID).FirstOrDefault();
                        test.RawData = mailTest.RawData;
                        test.LastUpdatedOn = DateTime.UtcNow;
                        test.Status = mailTest.Status;
                    }
                    db.SaveChanges();
                }
            }
        }

        public Guid GetMailTesterGuid(int campaignId)
        {
            using (var db = ObjectContextFactory.Create())
            {
                string sql = @"SELECT TOP 1 UniqueID FROM CampaignMailTest WHERE CampaignID = @CampaignId AND Status = 132
                               ORDER BY CampaignMailTestID DESC";
                return db.Get<Guid>(sql, new { CampaignId = campaignId }).FirstOrDefault();
            }
        }

        public IEnumerable<CampaignLitmusMap> GetLitmusIdByCampaignId(int campaignId, int accountId, int ownerId)
        {
            string sql = string.Empty;

            using (var db = ObjectContextFactory.Create())
            {
                sql = @"SELECT CLM.* FROM CampaignLitmusMap (NOLOCK) CLM 
                            INNER JOIN Campaigns (NOLOCK) C ON C.CampaignId = CLM.CampaignId WHERE C.CampaignId = @campaignId AND C.AccountId = @accountId
                            AND CLM.ProcessingStatus = @processingStatus";
                if (ownerId > 0)
                {
                    sql = @"SELECT CLM.* FROM CampaignLitmusMap (NOLOCK) CLM 
                            INNER JOIN Campaigns (NOLOCK) C ON C.CampaignId = CLM.CampaignId WHERE C.CampaignId = @campaignId AND C.AccountId = @accountId
                            AND C.OwnerId = @ownerId AND CLM.ProcessingStatus = @processingStatus";
                }
                //TODO
                // We need to pass Litmus check notified as status to bring litmus id from db
                return db.Get<CampaignLitmusMap>(sql, new { campaignId = campaignId, accountId = accountId, ownerId = ownerId, processingStatus = LitmusCheckStatus.LitmusNotified });
            }
        }

        public void NotifyLitmusCheck()
        {
            using (var db = ObjectContextFactory.Create())
            {
                InsertListmusResultsNotification();
                var sql = @"UPDATE CampaignLitmusMap
                            SET ProcessingStatus = 133, Remarks = 'Ready to view'
                            WHERE LitmusID IS NOT NULL AND DATEDIFF(MINUTE, LastModifiedOn, GETUTCDATE()) >2 AND ProcessingStatus = 132";

                db.Execute(sql);
            }
        }

        public bool CampaignHasLitmusResults(int campaignId)
        {
            using (var db = ObjectContextFactory.Create())
            {
                var sql = @"SELECT COUNT(1) FROM CampaignLitmusMap WHERE CampaignId=@campaignId AND ProcessingStatus IN (131,132)";
                int count = db.Get<int>(sql, new { campaignId = campaignId }).FirstOrDefault();

                if (count > 0)
                    return true;
                else
                    return false;

            }
        }

        public void InsertListmusResultsNotification()
        {
            var db = ObjectContextFactory.Create();
            List<Notification> notifications = new List<Notification>() { };
            var sql = @"SELECT CLM.CampaignId,CLM.LastModifiedOn,C.NAME,C.LastUpdatedBy FROM CampaignLitmusMap CLM
					 JOIN Campaigns(nolock) C ON C.CampaignID=CLM.CampaignId
				     WHERE LitmusID IS NOT NULL AND DATEDIFF(MINUTE, LastModifiedOn, GETUTCDATE()) >2 AND ProcessingStatus = 132";

            IEnumerable<CampaignLitmusNotification> litmusCampaigs = db.Get<CampaignLitmusNotification>(sql).ToList();

            if (litmusCampaigs.IsAny())
            {
                litmusCampaigs.ForEach(l =>
                {
                    Notification notification = new Notification();
                    notification.EntityId = l.CampaignId;
                    notification.Subject = string.Format("{0} - Litmus&#x00AE; Results", l.Name);
                    notification.Details = string.Format("{0} - Litmus&#x00AE; Results", l.Name);
                    notification.Time = l.LastModifiedOn;
                    notification.Status = NotificationStatus.New;
                    notification.UserID = l.LastUpdatedBy;
                    notification.ModuleID = (byte)AppModules.LitmusTest;
                    notification.DownloadFile = null;
                    notifications.Add(notification);
                });

                IEnumerable<NotificationDb> notificationDb = Mapper.Map<IEnumerable<Notification>, IEnumerable<NotificationDb>>(notifications);
                db.Notifications.AddRange(notificationDb);
                db.SaveChanges();
            }
        }

        public Dictionary<byte, string> GetCampaignLinkURLsByCampaignId(int campaignId)
        {
            var db = ObjectContextFactory.Create();
            Dictionary<byte,string> urls = new Dictionary<byte, string>();
            var sql = @"SELECT DISTINCT C.LinkIndex,C.URL  FROM CampaignLinks (NOLOCK) C WHERE C.CampaignID=@CampaignId";
            urls = db.Get<CampaignLinksDb>(sql, new { CampaignId = campaignId }).ToDictionary(k => k.LinkIndex,v => v.URL);
            return urls;
        }

        /// <summary> // Added by Ram on 9th May 2018  for Ticket NEXG-3004
        /// Update the campaign table with mail sent flag
        /// </summary>
        /// <param name="_campaignId"></param>
        /// <param name="_MailsentFlag"></param>
        public void UpdateCampaignSentFlagStatus(Int32 _campaignId, bool _MailsentFlag)
        {
            using (var db = ObjectContextFactory.Create())
            {
                var parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@CampaignId", _campaignId));
                parameters.Add(new SqlParameter("@CampaignSentStatus", _MailsentFlag));
                db.ExecuteStoredProcedure("UpdateCampaignSentFlagStatus", parameters, 360);
            }
        }

        //NEXG-3005
        public void InsertCampaignRecipients(Campaign _campaign, CampaignRecipient _campRecp)
        {
            using (var db = ObjectContextFactory.Create())
            {
                //var parameters = new List<SqlParameter>();
                //parameters.Add(new SqlParameter("@CampaignID", _campaign.Id));
                //parameters.Add(new SqlParameter("@ContactID", _campRecp.ContactID));
                //parameters.Add(new SqlParameter("@To", _campRecp.To));
                //parameters.Add(new SqlParameter("@ScheduleTime", _campRecp.ScheduleTime));
                //parameters.Add(new SqlParameter("@SentOn", null));
                //parameters.Add(new SqlParameter("@AccountId", _campRecp.AccountID));
                //db.ExecuteStoredProcedure("InsertCampaignRecipients_new", parameters, 360);

                InsertAllSScontacts(_campRecp);
            }
        }

        public void InsertAllSScontacts(CampaignRecipient _campRecp)
        {
            using (var db = ObjectContextFactory.Create())
            {
                var parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@CampaignID", _campRecp.CampaignID));
                parameters.Add(new SqlParameter("@ContactID", _campRecp.ContactID));
                parameters.Add(new SqlParameter("@To", _campRecp.To));
                parameters.Add(new SqlParameter("@ScheduleTime", _campRecp.ScheduleTime));
                db.ExecuteStoredProcedure("InsertCampaignRecipients_new1", parameters, 360);
            }
        }
    }


}

using AutoMapper;
using LandmarkIT.Enterprise.Utilities.Logging;
using LandmarkIT.Enterprise.Extensions;
using LinqKit;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.Opportunities;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using SmartTouch.CRM.Repository.Database;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using SmartTouch.CRM.Domain.Images;
using System.ComponentModel;

namespace SmartTouch.CRM.Repository.Repositories
{
    public class OpportunityRepository : Repository<Opportunity, int, OpportunitiesDb>, IOpportunityRepository
    {
        public OpportunityRepository(IUnitOfWork unitOfWork, IObjectContextFactory objectContextFactory)
            : base(unitOfWork, objectContextFactory)
        { }

        /// <summary>
        /// Finds all by owner.
        /// </summary>
        /// <param name="ownerids">The ownerids.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns></returns>
        public IEnumerable<Opportunity> FindAllByOwner(int[] ownerids, DateTime startDate, DateTime endDate)
        {
            IEnumerable<OpportunitiesDb> opportunities = ObjectContextFactory.Create().Opportunities
                .Include(o => o.OpportunitiesRelations).Where(c => c.IsDeleted != true && ownerids.Contains(c.Owner) && c.CreatedOn > startDate && c.CreatedOn < endDate);
            foreach (OpportunitiesDb opportunity in opportunities)
                yield return Mapper.Map<OpportunitiesDb, Opportunity>(opportunity);
        }

        /// <summary>
        /// Finds all.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Opportunity> FindAll()
        {
            IEnumerable<OpportunitiesDb> opportunities = ObjectContextFactory.Create().Opportunities.Include(o => o.OpportunitiesRelations).Where(c => c.IsDeleted != true);
            foreach (OpportunitiesDb opportunity in opportunities)
                yield return Mapper.Map<OpportunitiesDb, Opportunity>(opportunity);
        }

        /// <summary>
        /// Finds all.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="limit">The limit.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        public IEnumerable<Opportunity> FindAll(string name, int limit, int pageNumber, int accountId)
        {
            var predicate = PredicateBuilder.True<OpportunitiesDb>();
            var records = (pageNumber - 1) * limit;
            if (!string.IsNullOrEmpty(name))
            {
                name = name.ToLower();
                predicate = predicate.And(a => a.OpportunityName.Contains(name));
            }
            predicate = predicate.And(a => a.AccountID == accountId && a.IsDeleted != true);

            IEnumerable<OpportunitiesDb> opportunities = findOpportunitiesSummary(predicate).Skip(records).Take(limit);

            foreach (OpportunitiesDb da in opportunities)
            {
                yield return ConvertToDomain(da);
            }
        }

        /// <summary>
        /// Converts to domain.
        /// </summary>
        /// <param name="userDbObject">The user database object.</param>
        /// <returns></returns>
        public override Opportunity ConvertToDomain(OpportunitiesDb userDbObject)
        {
            var opportunityDb = getOpportunitiesDb(userDbObject.OpportunityID);
            Opportunity opportunity = new Opportunity();
            Mapper.Map<OpportunitiesDb, Opportunity>(opportunityDb, opportunity);
            return opportunity;
        }

        /// <summary>
        /// Gets the opportunities database.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        OpportunitiesDb getOpportunitiesDb(int id)
        {
            var db = ObjectContextFactory.Create();
            OpportunitiesDb opportunityDb = new OpportunitiesDb();//db.Opportunities.Where(op => op.IsDeleted == false).Include("ContactsMap").SingleOrDefault(k => k.OpportunityID == id);
            var sql= @"SELECT * FROM Opportunities(NOLOCK) WHERE IsDeleted=0 AND OpportunityID=@opportunityId
                       SELECT OCM.ContactID FROM OpportunityContactMap (NOLOCK) OCM
                       JOIN Contacts (NOLOCK) C ON C.ContactID= OCM.ContactID
                       WHERE OCM.OpportunityID=@opportunityId AND OCM.IsDeleted=0 AND C.IsDeleted=0 ORDER BY CreatedOn DESC
                       SELECT U.* FROM Opportunities(NOLOCK) O
                       JOIN Users(NOLOCK) U ON U.UserID = O.Owner
                       WHERE O.OpportunityID=@opportunityId AND O.IsDeleted=0";

            db.GetMultiple(sql, (r) =>
            {
                opportunityDb = r.Read<OpportunitiesDb>().FirstOrDefault();
                opportunityDb.ContactsMap =  r.Read<OpportunityContactMap>().ToList();
                opportunityDb.Users = r.Read<UsersDb>().FirstOrDefault();
            }, new { opportunityId = id });

            //   OpportunitiesDb opportunityDb = db.Opportunities.SingleOrDefault(k => k.OpportunityID == id);

            //ICollection<OpportunityContactMap> Contacts = db.OpportunityContactMap.Where(i => i.OpportunityID == id).ToList();
            // opportunityDb.ContactsMap = Contacts;
            //ICollection<OpportunityTagMap> OpportunityTags = db.OpportunityTagMap.Where(i => i.OpportunityID == id).ToList();
            //opportunityDb.OpportunityTags = OpportunityTags;

            //ICollection<OpportunitiesRelationshipMapDb> OpportunityRelations =
            //    ObjectContextFactory.Create().OpportunityRelationMap
            //    .Include("RelationshipTypes").Where(i => i.OpportunityID == id)
            //    .AsExpandable()
            //    .Select(a =>
            //        new
            //        {
            //            OpportunityRelationshipMapId = a.OpportunityRelationshipMapID,
            //            RelationShipTypeID = a.RelationshipTypeID,
            //            ContactID = a.ContactID,
            //            Contacts = a.Contacts,
            //            RelationshipTypes = a.RelationshipTypes

            //        }).ToList().Select(x => new OpportunitiesRelationshipMapDb
            //        {
            //            OpportunityRelationshipMapID = x.OpportunityRelationshipMapId,
            //            RelationshipTypeID = x.RelationShipTypeID,
            //            ContactID = x.ContactID,
            //            Contacts = x.Contacts,
            //            RelationshipTypes = x.RelationshipTypes
            //        }).ToList();

            //foreach (var rel in OpportunityRelations)
            //{
            //    var email = db.ContactEmails.Where(p => p.ContactID == rel.ContactID && p.IsPrimary == true).FirstOrDefault();
            //    if (email != null)
            //        rel.Contacts.PrimaryEmail = email.Email;
            //}

            //if (opportunityDb != null)
            //    opportunityDb.OpportunitiesRelations = OpportunityRelations;
            return opportunityDb;
        }

        /// <summary>
        /// Finds all.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        public IEnumerable<Opportunity> FindAll(string name, int accountId)
        {
            var predicate = PredicateBuilder.True<OpportunitiesDb>();
            if (!string.IsNullOrEmpty(name))
            {
                name = name.ToLower();
                predicate = predicate.And(a => a.OpportunityName.Contains(name));
            }
            predicate = predicate.And(a => a.AccountID == accountId && a.IsDeleted != true);
            IEnumerable<OpportunitiesDb> opportunities = findOpportunitiesSummary(predicate);
            foreach (OpportunitiesDb da in opportunities)
            {
                yield return ConvertToDomain(da);
            }
        }

        /// <summary>
        /// Gets the related contacts.
        /// </summary>
        /// <param name="opportunityId">The opportunity identifier.</param>
        /// <returns></returns>
        public IEnumerable<int> GetRelatedContacts(int opportunityId)
        {
            var contacts = ObjectContextFactory.Create().OpportunityContactMap
                .Where(oc => oc.OpportunityID == opportunityId).Select(o => o.ContactID).ToList();
            return contacts;
        }

        /// <summary>
        /// Finds the opportunities summary.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <returns></returns>
        IEnumerable<OpportunitiesDb> findOpportunitiesSummary(System.Linq.Expressions.Expression<Func<OpportunitiesDb, bool>> predicate)
        {
            IEnumerable<OpportunitiesDb> opportunities = ObjectContextFactory.Create().Opportunities.Where(predicate)
                .Include("OpportunityContactMap").Include("Statuses").Include("OpportunitiesRelationshipMap")
                .AsExpandable()
                .Select(a =>
                    new
                    {
                        OpportunityID = a.OpportunityID,
                        AccountID = a.AccountID,
                        OpportunityName = a.OpportunityName,
                        Potential = a.Potential,
                        StageID = a.StageID,
                        CloseDate = a.ExpectedClosingDate,
                        Statuses = a.Statuses

                    }).ToList().Select(x => new OpportunitiesDb
                    {
                        OpportunityID = x.OpportunityID,
                        AccountID = x.AccountID,
                        OpportunityName = x.OpportunityName,
                        Potential = x.Potential,
                        StageID = x.StageID,
                        ExpectedClosingDate = x.CloseDate,
                        Statuses = x.Statuses
                    });
            return opportunities;
        }

        /// <summary>
        /// Finds the by.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public override Opportunity FindBy(int id)
        {
            var opportunityDb = getOpportunitiesDb(id);
            if (opportunityDb == null)
                return null;
            Opportunity opportunity = new Opportunity();
            Mapper.Map<OpportunitiesDb, Opportunity>(opportunityDb, opportunity);
            return opportunity;
        }

        /// <summary>
        /// Gets the opportunity database.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        OpportunitiesDb getOpportunityDb(int id)
        {
            var db = ObjectContextFactory.Create();
            return db.Opportunities.SingleOrDefault(c => c.OpportunityID == id && c.IsDeleted != true);
        }

        /// <summary>
        /// Converts the type of to database.
        /// </summary>
        /// <param name="domainType">Type of the domain.</param>
        /// <param name="db">The database.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">Invalid opportunity id has been passed. Suspected Id forgery.</exception>
        public override OpportunitiesDb ConvertToDatabaseType(Opportunity domainType, CRMDb db)
        {
            OpportunitiesDb opportunitiesDb;

            //Existing Contact
            int OpportunityID = domainType.Id;
            if (OpportunityID > 0)
            {
                opportunitiesDb = db.Opportunities.SingleOrDefault(c => c.OpportunityID == OpportunityID);
                db.Entry<OpportunitiesDb>(opportunitiesDb).State = System.Data.Entity.EntityState.Modified;
                if (opportunitiesDb == null)
                    throw new ArgumentException("Invalid opportunity id has been passed. Suspected Id forgery.");

                opportunitiesDb = Mapper.Map<Opportunity, OpportunitiesDb>(domainType, opportunitiesDb);
            }
            else //New Contact
            {

                opportunitiesDb = Mapper.Map<Opportunity, OpportunitiesDb>(domainType);
            }
            return opportunitiesDb;
            //throw new NotImplementedException();
        }

        /// <summary>
        /// Persists the value objects.
        /// </summary>
        /// <param name="domainType">Type of the domain.</param>
        /// <param name="dbType">Type of the database.</param>
        /// <param name="db">The database.</param>
        public override void PersistValueObjects(Opportunity domainType, OpportunitiesDb dbType, CRMDb db)
        {
            PersistOpportunityContacts(domainType, dbType, db);
            PersistRelationships(domainType, dbType, db);
        }

        /// <summary>
        /// Persists the opportunity contacts.
        /// </summary>
        /// <param name="opportunity">The opportunity.</param>
        /// <param name="OpportunitiesDb">The opportunities database.</param>
        /// <param name="db">The database.</param>
        void PersistOpportunityContacts(Opportunity opportunity, OpportunitiesDb OpportunitiesDb, CRMDb db)
        {
            var opportunityContacts = db.OpportunityContactMap.Where(a => a.OpportunityID == opportunity.Id).ToList();

            foreach (int contactId in opportunity.Contacts)
            {
                if (!opportunityContacts.Any(a => a.ContactID == contactId))
                {
                    db.OpportunityContactMap.Add(new OpportunityContactMap() { OpportunityID = OpportunitiesDb.OpportunityID, ContactID = contactId });
                }
            }

            IList<int> contactIds = opportunity.Contacts.ToList();
            var unMapOpportunityContacts = opportunityContacts.Where(a => !contactIds.Contains(a.ContactID));
            foreach (OpportunityContactMap opportunityContactMap in unMapOpportunityContacts)
            {
                db.OpportunityContactMap.Remove(opportunityContactMap);
            }
        }

        /// <summary>
        /// Persists the relationships.
        /// </summary>
        /// <param name="opportunity">The opportunity.</param>
        /// <param name="OpportunitiesDb">The opportunities database.</param>
        /// <param name="db">The database.</param>
        void PersistRelationships(Opportunity opportunity, OpportunitiesDb OpportunitiesDb, CRMDb db)
        {
            var opportunityRelationships = db.OpportunityRelationMap.Where(a => a.OpportunityID == opportunity.Id).ToList();

            foreach (PeopleInvolved people in opportunity.PeopleInvolved)
            {
                if (!opportunityRelationships.Any(a => a.ContactID == people.ContactID && a.RelationshipTypeID == people.RelationshipTypeID && a.OpportunityID == OpportunitiesDb.OpportunityID))
                {
                    db.OpportunityRelationMap.Add(new OpportunitiesRelationshipMapDb()
                    {
                        OpportunityID = OpportunitiesDb.OpportunityID,
                        RelationshipTypeID = people.RelationshipTypeID,
                        ContactID = people.ContactID
                    });
                }
            }
            IList<int> opportunityRelationMapIDs = opportunity.PeopleInvolved.Where(a => a.PeopleInvolvedID > 0).Select(a => a.PeopleInvolvedID).ToList();
            var unMapOpportunityRelationship = opportunityRelationships.Where(a => !opportunityRelationMapIDs.Contains(a.OpportunityRelationshipMapID));
            foreach (OpportunitiesRelationshipMapDb opportunityrelationmap in unMapOpportunityRelationship)
            {
                db.OpportunityRelationMap.Remove(opportunityrelationmap);
            }
        }

        /// <summary>
        /// Determines whether [is opportunity unique] [the specified opportunity].
        /// </summary>
        /// <param name="opportunity">The opportunity.</param>
        /// <returns></returns>
        public bool IsOpportunityUnique(Opportunity opportunity)
        {
            var db = ObjectContextFactory.Create();
            var opportunityFound = db.Opportunities.Where(c => c.OpportunityName == opportunity.OpportunityName && c.AccountID == opportunity.AccountID && c.IsDeleted != true)
                              .Select(c => c).FirstOrDefault();
            if (opportunityFound != null && opportunity.Id != opportunityFound.OpportunityID)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Deletes the opportunities.
        /// </summary>
        /// <param name="OpportunityIDs">The opportunity i ds.</param>
        /// <param name="modifiedBy">The modified by.</param>
        public void DeleteOpportunities(int[] OpportunityIDs, int modifiedBy)
        {
            var db = ObjectContextFactory.Create();
            IEnumerable<OpportunitiesDb> opportunities = db.Opportunities.Include("OpportunityContactMap")
                                                                         .Include("OpportunityTagMap")
                                                                         .Include("OpportunityRelationMap")
                                                                         .Where(u => OpportunityIDs.Contains(u.OpportunityID))
                                                                         .AsExpandable()
                                                                         .Select(a =>
                                                                            new
                                                                            {
                                                                                OpportunityID = a.OpportunityID,
                                                                                IsDeleted = a.IsDeleted,
                                                                                ContactMap = a.ContactsMap,
                                                                                OpportunityTag = a.OpportunityTags,
                                                                                OpportunityRelations = a.OpportunitiesRelations
                                                                            }).ToList().Select(x => new OpportunitiesDb
                                                                            {
                                                                                OpportunityID = x.OpportunityID,
                                                                                OpportunitiesRelations = x.OpportunityRelations,
                                                                                OpportunityTags = x.OpportunityTag,
                                                                                IsDeleted = x.IsDeleted,
                                                                                ContactsMap = x.ContactMap
                                                                            });
            foreach (OpportunitiesDb opportunity in opportunities)
            {
                var opp = db.Opportunities.Where(i => i.OpportunityID == opportunity.OpportunityID).SingleOrDefault();
                opp.IsDeleted = true;
                opp.LastModifiedBy = modifiedBy;
                opp.LastModifiedOn = DateTime.Now.ToUniversalTime();
                if (opportunity.OpportunityTags != null)
                {
                    db.OpportunityTagMap.RemoveRange(opportunity.OpportunityTags);
                }
                if (opportunity.ContactsMap != null)
                {
                    db.OpportunityContactMap.RemoveRange(opportunity.ContactsMap);
                }
                if (opportunity.OpportunitiesRelations != null)
                {
                    db.OpportunityRelationMap.RemoveRange(opportunity.OpportunitiesRelations);
                }
            }
            db.SaveChanges();
        }

        /// <summary>
        /// Oppertunities the pipelinefunnel chart details.
        /// </summary>
        /// <param name="accountID">The account identifier.</param>
        /// <param name="userID">The user identifier.</param>
        /// <param name="isAccountAdmin">if set to <c>true</c> [is account admin].</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns></returns>
        public IEnumerable<DashboardPieChartDetails> OppertunityPipelinefunnelChartDetails(int accountID, int[] userIds, bool isAccountAdmin, DateTime startDate, DateTime endDate)
        {
            using (var db = ObjectContextFactory.Create())
            {
                Logger.Current.Informational("Created the procedure name to get the OppertunitypipelineChartDetails  for Dashboard ");
                var procedureName = "[dbo].[GET_Account_Opportunity_Pipeline_FunnelChart]";
                Logger.Current.Informational("Created the parametes for the procedure");

                var parms = new List<SqlParameter>
                {
                    new SqlParameter{ParameterName ="@AccountID", Value= accountID},
                    new SqlParameter{ParameterName ="@FromDate", Value=startDate.Date},
                    new SqlParameter{ParameterName="@ToDate ", Value = endDate.Date.AddDays(1)},
                    new SqlParameter{ParameterName="@IsAdmin", Value = isAccountAdmin},
                    new SqlParameter{ParameterName="@OwnerID", Value = string.Join(",",userIds )},
                };
                // var lstcontacts = context.ExecuteStoredProcedure<int>(procedureName, parms);
                return db.ExecuteStoredProcedure<DashboardPieChartDetails>(procedureName, parms);
            }
        }

        /// <summary>
        /// Finds the by contact.
        /// </summary>
        /// <param name="ContactID">The contact identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns></returns>
        public IEnumerable<Opportunity> FindByContact(int ContactID, int userId)
        {
            var db = ObjectContextFactory.Create();
            var ownerSql = userId != 0 ? "AND O.Owner = @UserId" : "";
            var opportunitySql = @"SELECT O.* FROM Opportunities(NOLOCK) O
                                        INNER JOIN OpportunityContactMap(NOLOCK) OCM ON OCM.OpportunityID =  O.OpportunityID
                                        WHERE OCM.ContactID = @ContactId " + ownerSql;

            List<OpportunitiesDb> oppDb = db.Get<OpportunitiesDb>(opportunitySql, new { contactId = ContactID, UserId = userId }).ToList();
            if (oppDb.IsAny())
            {
                foreach (var item in oppDb)
                {
                    Opportunity opportunity = new Opportunity();
                    Mapper.Map<OpportunitiesDb, Opportunity>(item, opportunity);
                    yield return opportunity;
                }
            }
        }

        /// <summary>
        /// Deletes the opportunity contact.
        /// </summary>
        /// <param name="OpportunityID">The opportunity identifier.</param>
        /// <param name="ContactID">The contact identifier.</param>
        public void DeleteOpportunityContact(int OpportunityID, int ContactID)
        {
            var db = ObjectContextFactory.Create();
            OpportunityContactMap opportunitycontactmap = db.OpportunityContactMap.Where(i => i.OpportunityID == OpportunityID && i.ContactID == ContactID).FirstOrDefault();
            if (opportunitycontactmap != null)
                db.OpportunityContactMap.Remove(opportunitycontactmap);
            db.SaveChanges();
        }

        /// <summary>
        /// Gets the opportunity stage contacts.
        /// </summary>
        /// <param name="opportunityStage">The opportunity stage.</param>
        /// <returns></returns>
        public IEnumerable<int> GetOpportunityStageContacts(short opportunityStage)
        {
            var db = ObjectContextFactory.Create();
            IEnumerable<Person> contacts = db.Opportunities.Where(o => o.StageID == opportunityStage && o.IsDeleted == false).Join(db.OpportunityContactMap, o => o.OpportunityID, oc => oc.OpportunityID, (o, oc) => new Person
            {
                Id = oc.ContactID,
            });
            return contacts.Select(s => s.Id).Distinct();
        }

        public OpportunityTableType InsertOpportunity(OpportunityTableType opportunity)
        {
            IEnumerable<Image> images = new List<Image>();
            IEnumerable<OpportunityTableType> opportunities = new List<OpportunityTableType>()
            {
                opportunity
            };

            if (opportunity.OpportunityImage != null)
                images = new List<Image>() { opportunity.OpportunityImage };

            using (var db = ObjectContextFactory.Create())
            {
                db.QueryStoredProc("dbo.SaveOpportunity", (reader) =>
                {
                    opportunity.OpportunityID = reader.Read<int>().FirstOrDefault();

                }, new
                {
                    opportunity = opportunities.AsTableValuedParameter("dbo.OpportunityTableType", new string[] { "OpportunityID", "OpportunityName", "Potential", "StageID", "ExpectedClosingDate", "Description", "Owner", "AccountID", "CreatedBy", "CreatedOn", "LastModifiedBy", "LastModifiedOn", "IsDeleted", "OpportunityType", "ProductType", "Address", "ImageID" }),
                    image = images.AsTableValuedParameter("dbo.ImageType", new string[] { "ImageID", "FriendlyName", "StorageName", "OriginalName", "CreatedBy", "CreatedDate", "CategoryId", "AccountID" }),
                });

                return opportunity;
            }
        }

        public Image GetOpportunityProfileImage(int imageId)
        {
            using (var db = ObjectContextFactory.Create())
            {
                var sql = @"SELECT ImageID,FriendlyName,StorageName,OriginalName,CreatedBy,CreatedDate,ImageCategoryID AS CategoryId,AccountID FROM Images(NOLOCK) WHERE ImageID=@imageId";
                Image image = db.Get<Image>(sql, new { imageId = imageId }).FirstOrDefault();
                return image;
            }
        }

        public IEnumerable<Opportunity> SearchByOpportunityName(int accountId, string name)
        {
            using (var db = ObjectContextFactory.Create())
            {
                var sql = @"SELECT TOP 10 O.OpportunityID AS Id,O.OpportunityName FROM Opportunities(NOLOCK) O WHERE O.AccountID=@accountId AND O.IsDeleted=0 AND O.OpportunityName LIKE '%" + name + "%'";
                IEnumerable<Opportunity> opportunities = db.Get<Opportunity>(sql, new { accountId = accountId }).ToList();
                return opportunities;
            }
        }

        public void InsertAndUpdateOpportunityBuyers(IEnumerable<OpportunityContactMapTableType> buyers)
        {
            using (var db = ObjectContextFactory.Create())
            {
                db.QueryStoredProc("dbo.OpportunityBuyersSave", (r) => { }, 
                    new
                    {
                        opportunitycontactmap = buyers.AsTableValuedParameter("dbo.OpportunityContactMapTableType", new string[] { "OpportunityContactMapID", "OpportunityID", "ContactID", "Potential", "ExpectedToClose", "Comments", "Owner", "StageID", "IsDeleted", "CreatedOn", "CreatedBy" })
                    });
            }
        }

        public void UpdateOpportunityName(int opportunityId, string oppName, int lastUpdateBy)
        {
            using (var db = ObjectContextFactory.Create())
            {
                var sql = @"UPDATE Opportunities SET OpportunityName=@opportunityName, LastModifiedBy=@updatedBy,LastModifiedOn=GETUTCDATE() WHERE OpportunityID=@opportunityId";
                db.Execute(sql, new { opportunityName = oppName, updatedBy = lastUpdateBy, opportunityId = opportunityId });
            }
        }

        public  void UpdateOpportunityStage(int opportunityId, int stageId)
        {
            using (var db = ObjectContextFactory.Create())
            {
                var sql = @"UPDATE Opportunities SET StageID=@stageID WHERE OpportunityID=@opportunityId";
                db.Execute(sql, new { stageID = stageId, opportunityId = opportunityId });
            }
        }

        public void UpdateOpportunityOwner(int opportunityId, int ownerId)
        {
            using (var db = ObjectContextFactory.Create())
            {
                var sql = @"UPDATE Opportunities SET Owner=@ownerID WHERE OpportunityID=@opportunityId";
                db.Execute(sql, new { ownerID = ownerId, opportunityId = opportunityId });
            }
        }

        public void UpdateOpportunityDescription(int opportunityId, string oppDescription, int lastUpdateBy)
        {
            using (var db = ObjectContextFactory.Create())
            {
                var sql = @"UPDATE Opportunities SET Description=@description, LastModifiedBy=@updatedBy,LastModifiedOn=GETUTCDATE() WHERE OpportunityID=@opportunityId";
                db.Execute(sql, new { description = oppDescription, updatedBy = lastUpdateBy, opportunityId = opportunityId });
            }
        }

        public void UpdateOpportunityPotential(int opportunityId, decimal potential, int lastUpdateBy)
        {
            using (var db = ObjectContextFactory.Create())
            {
                var sql = @"UPDATE Opportunities SET Potential=@potentail, LastModifiedBy=@updatedBy,LastModifiedOn=GETUTCDATE() WHERE OpportunityID=@opportunityId";
                db.Execute(sql, new { potentail = potential, updatedBy = lastUpdateBy, opportunityId = opportunityId });
            }
        }

        public void UpdateOpportunityExpectedCloseDate(int opportunityId, DateTime closedDate)
        {
            using (var db = ObjectContextFactory.Create())
            {
                var sql = @"UPDATE Opportunities SET ExpectedClosingDate=@expCloseDate WHERE OpportunityID=@opportunityId";
                db.Execute(sql, new { expCloseDate = closedDate, opportunityId = opportunityId });
            }
        }

        public void UpdateOpportunityType(int opportunityId, string opptype, int lastUpdateBy)
        {
            using (var db = ObjectContextFactory.Create())
            {
                var sql = @"UPDATE Opportunities SET OpportunityType=@opportunityType, LastModifiedBy=@updatedBy,LastModifiedOn=GETUTCDATE() WHERE OpportunityID=@opportunityId";
                db.Execute(sql, new { opportunityType = opptype, updatedBy = lastUpdateBy, opportunityId = opportunityId });
            }
        }
        public void UpdateOpportunityProductType(int opportunityId, string productType, int lastUpdateBy)
        {
            using (var db = ObjectContextFactory.Create())
            {
                var sql = @"UPDATE Opportunities SET ProductType=@productType, LastModifiedBy=@updatedBy,LastModifiedOn=GETUTCDATE() WHERE OpportunityID=@opportunityId";
                db.Execute(sql, new { productType = productType, updatedBy = lastUpdateBy, opportunityId = opportunityId });
            }
        }

        public void UpdateOpportunityAddress(int opportunityId, string oppAddress, int lastUpdatedBy)
        {
            using (var db = ObjectContextFactory.Create())
            {
                var sql = @"UPDATE Opportunities SET Address=@address, LastModifiedBy=@updatedBy,LastModifiedOn=GETUTCDATE() WHERE OpportunityID=@opportunityId";
                db.Execute(sql, new { address = oppAddress, updatedBy = lastUpdatedBy, opportunityId = opportunityId });
            }
        }

        public void UpdateOpportunityImage(int opportunityId, Image image,int lastUpdatedBy)
        {
            using (var db = ObjectContextFactory.Create())
            {
                var sql = @"DECLARE @imageId INT 
                            INSERT INTO Images(FriendlyName,StorageName,OriginalName,CreatedBy,CreatedDate,ImageCategoryID,AccountID)
                            VALUES(@friendlyName,@storageName,@originalName,1,GETUTCDATE(),@imageCategoryID,@accountID)
                            SET @imageId = SCOPE_IDENTITY()
                            UPDATE Opportunities SET ImageID=@imageId, LastModifiedBy=@updatedBy,LastModifiedOn=GETUTCDATE() WHERE OpportunityID=@opportunityId";
                db.Execute(sql, new {
                    friendlyName = image.FriendlyName,
                    storageName = image.StorageName,
                    originalName = image.OriginalName,
                    imageCategoryID = image.CategoryId,
                    accountID = image.AccountID,
                    updatedBy = lastUpdatedBy,
                    opportunityId = opportunityId
                });
            }
        }

        public IEnumerable<OpportunityBuyer> GetAllOpportunityBuyers(int opportunityId, int accountId, int pageNumber, int pageSize)
        {
            using(var db = ObjectContextFactory.Create())
            {
                IEnumerable<OpportunityBuyer> buyers = null;

                db.QueryStoredProc("[dbo].[Get_Opportunity_Buyers]", (reader) =>
                                      buyers = (reader).Read<OpportunityBuyer>().ToList(),
                                     new
                                     {
                                         opportunityID = opportunityId,
                                         pageNumber = pageNumber,
                                         pageSize = pageSize,
                                         accountID = accountId
                                     });
                return buyers;
            }
        }

        public OpportunityBuyer GetOpportunityBuyerDetailsById(int buyerId)
        {
            using (var db = ObjectContextFactory.Create())
            {
                var sql = @"SELECT OpportunityID,ContactID,Potential,ExpectedToClose,Comments,Owner,StageID FROM OpportunityContactMap(nolock) where OpportunityContactMapID=@oppcontmapId";
                OpportunityBuyer buyer = db.Get<OpportunityBuyer>(sql, new { oppcontmapId = buyerId }).FirstOrDefault();
                return buyer;
            }
        }

        public int DeleteOpportunityBuyer(int buyerId)
        {
            int opportunityId = 0;
            using (var db = ObjectContextFactory.Create())
            {
                var sql = @"UPDATE OpportunityContactMap SET IsDeleted=1 WHERE OpportunityContactMapID=@oppcontmapid";
                db.Execute(sql, new { oppcontmapid = buyerId });
                sql = @"SELECT OpportunityID FROM OpportunityContactMap(NOLOCK) WHERE OpportunityContactMapID=@oppcontmapid";
                opportunityId = db.Get<int>(sql, new { oppcontmapid = buyerId }).FirstOrDefault();
            }
            return opportunityId;
        }

        public IEnumerable<OpportunityBuyer> GetAllContactOpportunities(int contactId)
        {
            using (var db = ObjectContextFactory.Create())
            {
                var sql = @"SELECT OCTM.OpportunityContactMapID, O.OpportunityID,O.OpportunityName AS Name, DV.DropdownValue AS Stage,OCTM.ExpectedToClose,OCTM.Potential,OCTM.Comments,O.Description FROM OpportunityContactMap(NOLOCK) OCTM
                        JOIN Opportunities(NOLOCK) O ON O.OpportunityID = OCTM.OpportunityID
                        JOIN DropdownValues(NOLOCK) DV ON DV.DropdownValueID = OCTM.StageID
                        WHERE OCTM.ContactID=@contactId AND OCTM.IsDeleted=0
                        GROUP BY OCTM.OpportunityContactMapID,O.OpportunityName, DV.DropdownValue,OCTM.ExpectedToClose,OCTM.Potential,OCTM.Comments,OCTM.CreatedOn, O.OpportunityID, O.Description
                        ORDER BY OCTM.CreatedOn DESC";
                IEnumerable<OpportunityBuyer> opportunites = db.Get<OpportunityBuyer>(sql, new { contactId = contactId }).ToList();
                return opportunites;
            }
   
        }

        public string GetOpportunityNameByBuyerId(int buyerId)
        {
            using (var db = ObjectContextFactory.Create())
            {
                var sql = @"SELECT O.OpportunityName FROM OpportunityContactMap(NOLOCK) OCTM
                            JOIN Opportunities(NOLOCK) O ON O.OpportunityID = OCTM.OpportunityID
                            WHERE OCTM.OpportunityContactMapID=@buyerID";
                string opportunityName = db.Get<string>(sql, new { buyerID = buyerId }).FirstOrDefault();
                return opportunityName;
            }
        }

        public IEnumerable<OpportunityBuyer> GetAllOpportunityBuyerNames(int opportunityId, int accountId)
        {
            using (var db = ObjectContextFactory.Create())
            {
                IEnumerable<OpportunityBuyer> buyers = null;
                var sql = @"SELECT C.ContactID,C.ContactType, CASE WHEN (LEN(C.FirstName) > 0 AND LEN(C.LastName) >0) THEN C.FirstName + ' ' + C.LastName
					                     WHEN LEN(C.Company) > 0 THEN C.Company
					                     ELSE CE.Email	 END
				                      AS Name
				            FROM OpportunityContactMap(NOLOCK) OCM
		                    JOIN Contacts(NOLOCK) C ON C.ContactID = OCM.ContactID AND C.AccountID=@AccountId AND C.IsDeleted=0
		                    LEFT OUTER JOIN ContactEmails (nolock) CE on CE.ContactID = C.ContactID and CE.IsPrimary = 1 and CE.IsDeleted = 0
		                    WHERE OCM.IsDeleted=0 and OCM.OpportunityID=@OpportunityId";
                buyers = db.Get<OpportunityBuyer>(sql, new { AccountId = accountId, OpportunityId = opportunityId }).ToList();
                return buyers;
            }
        }

        /// <summary>
        /// Get All Opportunities With Buyers
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <param name="query"></param>
        /// <param name="sortField"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public IEnumerable<Opportunity> GetOpportunitiesWithBuyersList(int accountId, int pageNumber, int pageSize,string query, string sortField,int[] userIds,DateTime? startDate,DateTime? endDate, ListSortDirection direction = ListSortDirection.Descending)
        {
            using (var db = ObjectContextFactory.Create())
            {
                List<Opportunity> Opportunities = new List<Opportunity>();
                db.QueryStoredProc("[dbo].[Get_Opportunities_List]", (reader) =>
                                     {
                                         Opportunities = reader.Read<Opportunity>().ToList();
                                     },
                                     new
                                     {
                                         AccountId = accountId,
                                         pageNumber = pageNumber,
                                         pageSize = pageSize,
                                         Query = query,
                                         SortField = sortField,
                                         Users = userIds.AsTableValuedParameter("dbo.Contact_List"),
                                         StartDate = startDate,
                                         EndDate = endDate,
                                         SortDirection = direction == ListSortDirection.Ascending ? "ASC" : "DESC"
                                     });

                return Opportunities;
            }
        }

        public IList<int> GetOpportunityContactIds(int opportunityId)
        {
            using(var db = ObjectContextFactory.Create())
            {
                var sql = @"SELECT ContactID FROM OpportunityContactMap (NOLOCK) WHERE IsDeleted=0 AND OpportunityID=@OpportunityId";
                return db.Get<int>(sql, new { OpportunityId = opportunityId }).ToList();
            }
        }

    }
}

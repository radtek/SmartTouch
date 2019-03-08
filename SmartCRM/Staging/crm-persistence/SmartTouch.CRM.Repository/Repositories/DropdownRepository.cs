using AutoMapper;
using LandmarkIT.Enterprise.Utilities.ExceptionHandling;
using LandmarkIT.Enterprise.Utilities.Logging;
using LinqKit;
using SmartTouch.CRM.Domain.Dropdowns;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using SmartTouch.CRM.Repository.Database;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace SmartTouch.CRM.Repository.Repositories
{
    /// <summary>
    /// 
    /// </summary>
    public class DropdownRepository : Repository<Dropdown, int, DropdownDb>, IDropdownRepository
    {
        public DropdownRepository(IUnitOfWork unitOfWork, IObjectContextFactory objectContextFactory)
            : base(unitOfWork, objectContextFactory) { }

        /// <summary>
        /// Finds all.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Dropdown> FindAll()
        {
            var db = ObjectContextFactory.Create();
            IEnumerable<DropdownDb> dropdownsDb = db.Dropdowns.Include(ls => ls.DropdownValues).ToList();
            foreach (DropdownDb dv in dropdownsDb)
            {
                yield return Mapper.Map<DropdownDb, Dropdown>(dv);
            }
        }

        /// <summary>
        /// Finds all.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="limit">The limit.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        public IEnumerable<Dropdown> FindAll(string name, int limit, int pageNumber, int accountId)
        {
            var predicate = PredicateBuilder.True<DropdownDb>();
            var records = (pageNumber - 1) * limit;
            if (!string.IsNullOrEmpty(name))
            {
                name = name.ToLower();
                predicate = predicate.And(a => a.DropdownName.Contains(name));
                // predicate=predicate.And()
            }

            IEnumerable<DropdownDb> dropdownValuesDb = findDropdownValuesSummary(predicate, accountId).Skip(records).Take(limit);
            Logger.Current.Informational("Fetched dropdownvalues for accountId : " + accountId);
            foreach (DropdownDb dv in dropdownValuesDb)
            {
                foreach (var item in dv.DropdownValues)
                {

                    ICollection<OpportunityStageGroupsDb> OpportunityStageGroups = ObjectContextFactory.Create().OpportunityStageGroupsDb.Where(p => p.DropdownValueID == item.DropdownValueID).AsExpandable()
                          .Select(a => new
                          {
                              AccountID = a.AccountID,
                              DropdownValueID = a.DropdownValueID,
                              OpportunityGroupID = a.OpportunityGroupID,
                              OpportunityStageGroupID = a.OpportunityStageGroupID
                          }).ToList().Select(x => new OpportunityStageGroupsDb
                          {
                              AccountID = x.AccountID,
                              DropdownValueID = x.DropdownValueID,
                              OpportunityGroupID = x.OpportunityGroupID,
                              OpportunityStageGroupID = x.OpportunityStageGroupID
                          }).ToList();
                    item.OpportunityStageGroups = OpportunityStageGroups;
                    Logger.Current.Informational("Fetched opportunity stage groups for dropdownvalueid : " + item.DropdownValueID);
                }

                yield return ConvertToDomain(dv);
            }
        }

        /// <summary>
        /// Gets the oppoertunity stage groups.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<OpportunityGroups> GetOppoertunityStageGroups()
        {
            var db = ObjectContextFactory.Create();
            IEnumerable<OpportunityGroupsDb> opportunityGroupsDb;
            opportunityGroupsDb = db.OpportunityGroupsDb.Select(p => p).ToList();
            return convertToDomainOpportunityGroups(opportunityGroupsDb);
        }

        /// <summary>
        /// Converts to domain opportunity groups.
        /// </summary>
        /// <param name="groups">The groups.</param>
        /// <returns></returns>
        IEnumerable<OpportunityGroups> convertToDomainOpportunityGroups(IEnumerable<OpportunityGroupsDb> groups)
        {
            foreach (OpportunityGroupsDb group in groups)
            {
                yield return new OpportunityGroups() { OpportunityGroupID = group.OpportunityGroupID, OpportunityGroupName = group.OpportunityGroupName };
            }
        }

        /// <summary>
        /// Finds the by.
        /// </summary>
        /// <param name="DropdownID">The dropdown identifier.</param>
        /// <param name="AccountID">The account identifier.</param>
        /// <returns></returns>
        public Dropdown FindBy(int DropdownID, int AccountID)
        {

            var db = ObjectContextFactory.Create();


            DropdownDb dropdownValues = db.Dropdowns.Include("DropdownValues")
               .AsExpandable()
               .Where(i => i.DropdownID == DropdownID).OrderBy(c => c.DropdownID).Select(dv =>
                   new
                   {
                       DropdownID = dv.DropdownID,
                       DropdownName = dv.DropdownName,
                       DropdownValues = dv.DropdownValues.Where(i => (i.AccountID == AccountID) && i.IsDeleted == false).OrderBy(d => d.SortID).ToList()

                   }).ToList().Select(x => new DropdownDb
                   {
                       DropdownName = x.DropdownName,
                       DropdownID = x.DropdownID,
                       DropdownValues = x.DropdownValues
                   }).FirstOrDefault();
            foreach (var item in dropdownValues.DropdownValues)
            {
                ICollection<OpportunityStageGroupsDb> OpportunityStageGroups = ObjectContextFactory.Create().OpportunityStageGroupsDb.Where(p => p.DropdownValueID == item.DropdownValueID).AsExpandable()
                      .Select(a => new
                      {
                          AccountID = a.AccountID,
                          DropdownValueID = a.DropdownValueID,
                          OpportunityGroupID = a.OpportunityGroupID,
                          OpportunityStageGroupID = a.OpportunityStageGroupID
                      }).ToList().Select(x => new OpportunityStageGroupsDb
                      {
                          AccountID = x.AccountID,
                          DropdownValueID = x.DropdownValueID,
                          OpportunityGroupID = x.OpportunityGroupID,
                          OpportunityStageGroupID = x.OpportunityStageGroupID
                      }).ToList();
                item.OpportunityStageGroups = OpportunityStageGroups;
            }
            return ConvertToDomain(dropdownValues);
        }

        /// <summary>
        /// Finds the dropdown values summary.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        IEnumerable<DropdownDb> findDropdownValuesSummary(System.Linq.Expressions.Expression<Func<DropdownDb, bool>> predicate, int? accountId)
        {
            IEnumerable<DropdownDb> dropdownValues = ObjectContextFactory.Create().Dropdowns.Include("DropdownValues")
                .AsExpandable()
                .Where(predicate).OrderBy(c => (c.DropdownID)).Select(dv =>
                    new
                    {
                        DropdownID = dv.DropdownID,
                        DropdownName = dv.DropdownName,
                        DropdownValues = dv.DropdownValues.Where(i => (i.AccountID == accountId) && i.IsDeleted == false).OrderBy(d => d.SortID).ToList()
                    }).ToList().Select(x => new DropdownDb
                    {
                        DropdownName = x.DropdownName,
                        DropdownID = x.DropdownID,
                        DropdownValues = x.DropdownValues
                    });

            return dropdownValues;
        }

        /// <summary>
        /// Finds the dropdown values summary.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        IEnumerable<DropdownDb> findDropdownValuesSummary(string name, int? accountId)
        {
            var where = string.IsNullOrEmpty(name) ? "" : "where dropdownname = @name";
            var sql = string.Format(@"SELECT D.DropdownID,D.DropdownName,COUNT(1) OVER() AS TotalDropdownCount  FROM Dropdowns(NOLOCK) D {0} SELECT * FROM dbo.dropdownvalues (NOLOCK) where Coalesce(accountid,0) = Coalesce(@Id,0) ORDER BY SortID", where);

            var db = ObjectContextFactory.Create();
            var dropdowns = new List<DropdownDb>();
            var dvl = new List<DropdownValueDb>();
            db.GetMultiple(sql, (r) =>
                {
                    dropdowns = r.Read<DropdownDb>().ToList();
                    dvl = r.Read<DropdownValueDb>().ToList();
                }, new { Id = accountId, Name = name });

            foreach (var dv in dropdowns)
            {
                dv.DropdownValues = dvl.Where(id => id.DropdownID == dv.DropdownID && id.IsDeleted == false).ToList();
            }
            return dropdowns;
        }

        /// <summary>
        /// Gets all dropdown values.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Dropdown> GetAllDropdownValues()
        {
            IEnumerable<DropdownDb> dropdownValues = ObjectContextFactory.Create().Dropdowns.
                Include(d => d.DropdownValues);
            foreach (DropdownDb dc in dropdownValues)
            {
                yield return Mapper.Map<DropdownDb, Dropdown>(dc);
            }

        }

        /// <summary>
        /// Finds all.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        public IEnumerable<Dropdown> FindAll(string name, int? accountId)
        {
            if (!string.IsNullOrEmpty(name))
                name = name.ToLower();
            IEnumerable<DropdownDb> dropdownValues = findDropdownValuesSummary(name, accountId);
            var db = ObjectContextFactory.Create();

            foreach (DropdownDb dv in dropdownValues)
            {

                //foreach (var value in dv.DropdownValues)
                //{
                //    ICollection<OpportunityStageGroupsDb> OpportunityStageGroups = db.OpportunityStageGroupsDb.Where(p => p.DropdownValueID == value.DropdownValueID).AsExpandable()
                //          .Select(a => new
                //          {
                //              AccountID = a.AccountID,
                //              DropdownValueID = a.DropdownValueID,
                //              OpportunityGroupID = a.OpportunityGroupID,
                //              OpportunityStageGroupID = a.OpportunityStageGroupID
                //          }).ToList().Select(x => new OpportunityStageGroupsDb
                //          {
                //              AccountID = x.AccountID,
                //              DropdownValueID = x.DropdownValueID,
                //              OpportunityGroupID = x.OpportunityGroupID,
                //              OpportunityStageGroupID = x.OpportunityStageGroupID
                //          }).ToList();
                //    value.OpportunityStageGroups = OpportunityStageGroups;
                //}


                yield return ConvertToDomain(dv);
            }
        }

        /// <summary>
        /// Finds the dropdown by.
        /// </summary>
        /// <param name="dropdownId">The dropdown identifier.</param>
        /// <returns></returns>
        public Dropdown FindDropdownBy(byte dropdownId)
        {
            DropdownDb dropdownsDatabase = ObjectContextFactory.Create().Dropdowns.Include(c => c.DropdownValues).Where(d => d.DropdownID == dropdownId).FirstOrDefault();
            if (dropdownsDatabase != null)
                return Mapper.Map<DropdownDb, Dropdown>(dropdownsDatabase);
            return null;
        }

        public override Dropdown FindBy(int id)
        {
            //  DropdownValueDb dropdownsDatabase = ObjectContextFactory.Create().Dropdowns.Include(c => c.Dropdowns.DropdownID == id).Include(a => a.Accounts.AccountID == id).FirstOrDefault();
            // DropdownDb dropdownsDatabase = ObjectContextFactory.Create().Dropdowns.Include(c => c.DropdownValues).Include(d => d.DropdownID == Convert.ToByte(id)).FirstOrDefault();
            //  DropdownDb dropdownsDatabase = ObjectContextFactory.Create().Dropdowns.Include("DropdownValues").Where(d => d.DropdownID == Convert.ToByte(id)).FirstOrDefault();
            //DropdownValueDb dropdownsDatabase = ObjectContextFactory.Create().DropdownValues.Include("Dropdowns").Where(d => d.DropdownID == id).SingleOrDefault();
            //if (dropdownsDatabase != null)
            //    return Mapper.Map<DropdownValueDb,DropdownValue>(dropdownsDatabase);
            //return null;
            throw new NotImplementedException();
        }

        /// <summary>
        /// Converts the type of to database.
        /// </summary>
        /// <param name="domainType">Type of the domain.</param>
        /// <param name="db">The database.</param>
        /// <returns></returns>
        /// <exception cref="LandmarkIT.Enterprise.Utilities.ExceptionHandling.UnsupportedOperationException">Invalid dropdown id has been passed. Suspected Id forgery.</exception>
        public override DropdownDb ConvertToDatabaseType(Dropdown domainType, CRMDb db)
        {
            DropdownDb dropdownDb;

            var dropdownId = Convert.ToInt16(domainType.Id);
            if (dropdownId > 0)
            {
                dropdownDb = db.Dropdowns.Include(d => d.DropdownValues)
                    .Join(db.DropdownValues.Where(dd => dd.AccountID == domainType.AccountID && dd.DropdownID == dropdownId), o => o.DropdownID, i => i.DropdownID, (o, i) => o).First();

                if (dropdownDb == null)
                    throw new UnsupportedOperationException("Invalid dropdown id has been passed. Suspected Id forgery.");
            }
            else
            {

                dropdownDb = Mapper.Map<Dropdown, DropdownDb>(domainType);
            }
            return dropdownDb;
        }

        /// <summary>
        /// Converts to domain.
        /// </summary>
        /// <param name="dropdown">The dropdown.</param>
        /// <returns></returns>
        public override Dropdown ConvertToDomain(DropdownDb dropdown)
        {
            //Dropdown data = Mapper.Map<DropdownDb, Dropdown>(dropdown);
            return Mapper.Map<DropdownDb, Dropdown>(dropdown);
        }

        /// <summary>
        /// Persists the value objects.
        /// </summary>
        /// <param name="dropdown">The dropdown.</param>
        /// <param name="dropdowndb">The dropdowndb.</param>
        /// <param name="db">The database.</param>
        public override void PersistValueObjects(Dropdown dropdown, DropdownDb dropdowndb, CRMDb db)
        {
            PersistDropdownValue(dropdown, dropdowndb, db);
        }

        /// <summary>
        /// Persists the dropdown value.
        /// </summary>
        /// <param name="dropdown">The dropdown.</param>
        /// <param name="dropdownDb">The dropdown database.</param>
        /// <param name="db">The database.</param>
        /// <exception cref="LandmarkIT.Enterprise.Utilities.ExceptionHandling.UnsupportedOperationException"></exception>
        private void PersistDropdownValue(Dropdown dropdown, DropdownDb dropdownDb, CRMDb db)
        {
            var dropDownValues = db.DropdownValues.Where(c => (c.DropdownID == dropdown.Id) && (c.AccountID == dropdown.AccountID) && c.IsDeleted == false).ToList();
            if (dropdown.DropdownValues != null)
            {
                foreach (var dropdwn in dropdown.DropdownValues)
                {
                    if (dropdwn.Id != 0)
                    {
                        var dropdownValueMap = dropDownValues.SingleOrDefault(r => r.DropdownValueID == dropdwn.Id);
                        dropdownValueMap.DropdownValue = dropdwn.Value;
                        dropdownValueMap.IsDefault = dropdwn.IsDefault;
                        dropdownValueMap.SortID = dropdwn.SortID;
                        dropdownValueMap.IsActive = dropdwn.IsActive;
                        dropdownValueMap.DropdownValueTypeID = dropdwn.DropdownValueTypeID;
                        if (dropdown.Id == 6)
                        {
                            var opoortunityStageGroup = db.OpportunityStageGroupsDb.Where(c => c.DropdownValueID == dropdwn.Id && c.AccountID == dropdwn.AccountID).FirstOrDefault();
                            if (opoortunityStageGroup != null)
                                opoortunityStageGroup.OpportunityGroupID = dropdwn.OpportunityGroupID;
                        }
                    }
                    else
                    {
                        DropdownValueDb map = new DropdownValueDb();
                        map.DropdownID = dropdwn.DropdownID;
                        map.AccountID = dropdwn.AccountID;
                        map.IsDefault = dropdwn.IsDefault;
                        map.DropdownValue = dropdwn.Value;
                        map.SortID = dropdwn.SortID;
                        map.IsActive = dropdwn.IsActive;
                        map.IsDeleted = false;
                        map.DropdownValueTypeID = dropdwn.DropdownValueTypeID;


                        if (dropdown.Id == 6)
                        {
                            OpportunityStageGroupsDb grp = new OpportunityStageGroupsDb()
                            {
                                DropdownValues = map,
                                AccountID = (int)dropdwn.AccountID,
                                OpportunityGroupID = dropdwn.OpportunityGroupID
                            };
                            map.OpportunityStageGroups = new List<OpportunityStageGroupsDb>();
                            map.OpportunityStageGroups.Add(grp);
                        }
                        db.DropdownValues.Add(map);

                    }
                }
                IList<short> dropdownvalues = dropdown.DropdownValues.Where(d => d.Id > 0).Select(d => d.Id).ToList();
                var unMapDropdownValues = dropDownValues.Where(n => !dropdownvalues.Contains(n.DropdownValueID));
                foreach (DropdownValueDb dropdownValueMapDb in unMapDropdownValues)
                {
                    string temp = DeleteDropdownvalues(dropdownValueMapDb, db);
                    if (temp == "")
                    {
                        dropdownValueMapDb.IsDeleted = true;
                        db.SaveChanges();
                        var GroupDb = db.OpportunityStageGroupsDb.Where(p => p.DropdownValueID == dropdownValueMapDb.DropdownValueID).FirstOrDefault();
                        if (GroupDb != null)
                            db.OpportunityStageGroupsDb.Remove(GroupDb);
                    }
                    else
                        throw new UnsupportedOperationException(temp);
                }
            }
        }

        /// <summary>
        /// Deletes the dropdownvalues.
        /// </summary>
        /// <param name="dropdownValueMapDb">The dropdown value map database.</param>
        /// <param name="db">The database.</param>
        /// <returns></returns>
        private string DeleteDropdownvalues(DropdownValueDb dropdownValueMapDb, CRMDb crmdb)
        {
            var db = ObjectContextFactory.Create();
            string response = "";
            if (dropdownValueMapDb.DropdownID == (short)DropdownFieldTypes.AddressType)
            {
                var sql = @"select count(1) from Addresses(nolock) a 
                            join contactaddressmap(nolock) cam on cam.AddressID = a.AddressID
                            join contacts(nolock) c on c.ContactID = cam.ContactID
                            where c.AccountID = @AccountID  and c.IsDeleted = 0 and a.AddressTypeID = @AddressTypeID";
                var count = db.Get<int>(sql, new { AccountID = dropdownValueMapDb.AccountID, AddressTypeID = dropdownValueMapDb.DropdownValueID }).FirstOrDefault();
                if (count > 0)
                    response = "The Address type you are trying to delete is currently associated to one or more Contacts and cannot be deleted. Please set to Inactive if you no longer wish to use it.";
            }
            else if (dropdownValueMapDb.DropdownID == (short)DropdownFieldTypes.PhoneNumberType)
            {
                var sql = @"select count(1) from ContactPhoneNumbers(nolock) cp
                            join contacts(nolock) c on c.ContactID = cp.ContactID
                            where c.AccountID = @AccountID  and c.IsDeleted = 0 and cp.PhoneType = @PhoneTypeID";
                var count = db.Get<int>(sql, new { AccountID = dropdownValueMapDb.AccountID, PhoneTypeID = dropdownValueMapDb.DropdownValueID }).FirstOrDefault();
                if (count > 0)
                    response = "The Phone number type you are trying to delete is currently associated to one or more Contacts and cannot be deleted. Please set to Inactive if you no longer wish to use it.";
            }
            else if (dropdownValueMapDb.DropdownID == (short)DropdownFieldTypes.LifeCycle)
            {
                var isLifecycleStage = db.Contacts.Where(at => at.LifecycleStage == dropdownValueMapDb.DropdownValueID && at.AccountID == dropdownValueMapDb.AccountID && !at.IsDeleted).Count();
                if (isLifecycleStage > 0)
                    response = "The Life Cycle you are trying to delete is currently associated to one or more Contacts and cannot be deleted. Please set to Inactive if you no longer wish to use it.";
            }
            else if (dropdownValueMapDb.DropdownID == (short)DropdownFieldTypes.LeadSources)
            {
                var sql = @"select count(1) from ContactLeadSourceMap(nolock) CLM
                            join contacts(nolock) c on c.ContactID = CLM.ContactID
                            where c.AccountID = @AccountID  and c.IsDeleted = 0 and CLM.LeadSouceID = @LeadSourceID";
                var count = db.Get<int>(sql, new { AccountID = dropdownValueMapDb.AccountID, LeadSourceID = dropdownValueMapDb.DropdownValueID }).FirstOrDefault();
                if (count > 0)
                    response = "The Lead Source you are trying to delete is currently associated to one or more Contacts and cannot be deleted. Please set to Inactive if you no longer wish to use it.";
            }
            else if (dropdownValueMapDb.DropdownID == (short)DropdownFieldTypes.Community)
            {
                int count = 0;
                var sql = @"select count(1) from ContactCommunityMap(nolock) CCM
                            join contacts(nolock) c on c.ContactID = CCM.ContactID
                            where c.AccountID = @AccountID  and c.IsDeleted = 0 and CCM.CommunityID = @CommunityID AND
                            CCM.IsDeleted = 0";
                count = db.Get<int>(sql, new { AccountID = dropdownValueMapDb.AccountID, CommunityID = dropdownValueMapDb.DropdownValueID }).FirstOrDefault();
                if (count == 0)
                {
                    var sql1 = @"select count(1) from Tours(nolock) T
                                join contacttourmap(nolock) ctm on ctm.TourID = T.TourID
                                join contacts(nolock) c on c.ContactID = ctm.ContactID
                                where c.AccountID = @AccountID  and c.IsDeleted = 0 and T.CommunityID = @CommunityID";
                    count += db.Get<int>(sql1, new { AccountID = dropdownValueMapDb.AccountID, CommunityID = dropdownValueMapDb.DropdownValueID }).FirstOrDefault();
                }
                if (count > 0)
                    response = "The Community you are trying to delete is currently associated to one or more Contacts and cannot be deleted. Please set to Inactive if you no longer wish to use it.";
            }
            else if (dropdownValueMapDb.DropdownID == (short)DropdownFieldTypes.TourType)
            {
                var sql = @"select count(1) from Tours(nolock) T
                            join contacttourmap(nolock) ctm on ctm.TourID = T.TourID
                            join contacts(nolock) c on c.ContactID = ctm.ContactID
                            where c.AccountID = @AccountID  and c.IsDeleted = 0 and T.TourType = @TourTypeID";
                var count = db.Get<int>(sql, new { AccountID = dropdownValueMapDb.AccountID, TourTypeID = dropdownValueMapDb.DropdownValueID }).FirstOrDefault();
                if (count > 0)
                    response = "The Tour type you are trying to delete is currently associated to one or more Contacts and cannot be deleted. Please set to Inactive if you no longer wish to use it.";
            }
            else if (dropdownValueMapDb.DropdownID == (short)DropdownFieldTypes.ActionType)
            {
                var sql = @"select count(1) from Actions(nolock) A
                            join ContactActionMap(nolock) CAM ON CAM.ActionID = A.ActionID
                            join contacts(nolock) c on c.ContactID = CAM.ContactID
                            where c.AccountID = @AccountID  and c.IsDeleted = 0 and A.ActionType = @ActionTypeID";
                var count = db.Get<int>(sql, new { AccountID = dropdownValueMapDb.AccountID, ActionTypeID = dropdownValueMapDb.DropdownValueID }).FirstOrDefault();
                if (count > 0)
                    response = "The Action type you are trying to delete is currently associated to one or more Contacts and cannot be deleted. Please set to Inactive if you no longer wish to use it.";
            }
            else if (dropdownValueMapDb.DropdownID == (short)DropdownFieldTypes.OpportunityStage)
            {
                var sql = @"select count(1) from OpportunityContactMap(nolock) T
                            join contacts(nolock) c on c.ContactID = T.ContactID
                            where c.AccountID = @AccountID  and c.IsDeleted = 0 and T.StageID = @StageID";
                var count = db.Get<int>(sql, new { AccountID = dropdownValueMapDb.AccountID, StageID = dropdownValueMapDb.DropdownValueID }).FirstOrDefault();
                if (count > 0)
                    response = "The Opportunity Stage you are trying to delete is currently associated to one or more Contacts and cannot be deleted. Please set to Inactive if you no longer wish to use it.";
            }
            else if (dropdownValueMapDb.DropdownID == (short)DropdownFieldTypes.RelationshipType)
            {
                var sql = @"SELECT Count(1) FROM [dbo].[ContactRelationshipMap] (NOLOCK) CRM
                            JOIN Contacts (NOLOCK) C ON C.ContactID = CRM.ContactID
                            WHERE C.IsDeleted = 0 AND C.AccountID = @AccountID AND CRM.RelationshipType = @RelationshipTypeID";
                var count = db.Get<int>(sql, new { AccountID = dropdownValueMapDb.AccountID, RelationshipTypeID = dropdownValueMapDb.DropdownValueID }).FirstOrDefault();
                if (count > 0)
                    response = "The Relationship type you are trying to delete is currently associated to one or more Contacts and cannot be deleted. Please set to Inactive if you no longer wish to use it.";
            }
            else if (dropdownValueMapDb.DropdownID == (short)DropdownFieldTypes.PartnerType)
            {
                var isPartnerType = db.Contacts.Where(at => at.PartnerType == dropdownValueMapDb.DropdownValueID && at.AccountID == dropdownValueMapDb.AccountID && !at.IsDeleted).Count();
                if (isPartnerType > 0)
                    response = "The Partner type you are trying to delete is currently associated to one or more Contacts and cannot be deleted. Please set to Inactive if you no longer wish to use it.";
            }
            else if (dropdownValueMapDb.DropdownID == (short)DropdownFieldTypes.NoteCategory)
            {
                var sql = @"select count(1) from Notes(nolock) N
                            join ContactNoteMap(nolock) cnm ON cnm.NoteID = N.NoteID
                            join contacts(nolock) c on c.ContactID = cnm.ContactID
                            where c.AccountID = @AccountID  and c.IsDeleted = 0 and N.NoteCategory = @NoteCategory";
                var count = db.Get<int>(sql, new { AccountID = dropdownValueMapDb.AccountID, NoteCategory = dropdownValueMapDb.DropdownValueID }).FirstOrDefault();
                if (count > 0)
                    response = "The Note Category you are trying to delete is currently associated to one or more Contacts and cannot be deleted. Please set to Inactive if you no longer wish to use it.";
            }

            return response;
        }
        //public void UpdateDropdownValue(Dropdown dropdown)
        //{
        //    var db = ObjectContextFactory.Create();
        //    DropdownDb dropdownDb = Mapper.Map<Dropdown, DropdownDb>(dropdown);
        //    foreach (DropdownValueDb DVDb in dropdownDb.DropdownValues)
        //    {

        //        var dropdownValueRecord = db.DropdownValues.Where(d => d.DropdownValueID == DVDb.DropdownValueID).FirstOrDefault();
        //        dropdownValueRecord.IsDefault = DVDb.IsDefault;
        //    }
        //    // var dropdownValueRecord = db.DropdownValues.Where(d => d.DropdownValueID == dropdownValue.Id).FirstOrDefault();
        //    //  dropdownValueRecord.IsDefault = dropdownValue.IsDefault;
        //    db.SaveChanges();
        //}

        /// <summary>
        /// Gets the communities.
        /// </summary>
        /// <param name="dropdownID">The dropdown identifier.</param>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        public IEnumerable<dynamic> GetCommunities(int dropdownID, int accountId)
        {
            var db = ObjectContextFactory.Create();
            IEnumerable<dynamic> dropdownFields = db.DropdownValues.Where(df => df.DropdownID == dropdownID && df.AccountID == accountId).Select(df => new { DropdownValueID = df.DropdownValueID, DropdownID = df.DropdownID, DropdownValue = df.DropdownValue, IsDefault = df.IsDefault }).ToList();

            if (dropdownFields != null)
                return convertToDomainCommunity(dropdownFields);
            return null;
        }

        /// <summary>
        /// Converts to domain community.
        /// </summary>
        /// <param name="dropdownFields">The dropdown fields.</param>
        /// <returns></returns>
        private IEnumerable<dynamic> convertToDomainCommunity(IEnumerable<dynamic> dropdownFields)
        {
            foreach (dynamic dropdownField in dropdownFields)
            {
                yield return new DropdownValue() { Id = dropdownField.DropdownValueID, Value = dropdownField.DropdownValue, IsDefault = dropdownField.IsDefault };
            }
        }

        /// <summary>
        /// Gets the lead sources.
        /// </summary>
        /// <param name="dropdownID">The dropdown identifier.</param>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        public IEnumerable<DropdownValue> GetLeadSources(int dropdownID, int accountId)
        {
            var db = ObjectContextFactory.Create();
            IEnumerable<DropdownValueDb> dropdownFields = db.DropdownValues.Where(df => df.DropdownID == dropdownID && df.AccountID == accountId && df.IsDeleted == false).ToList();

            if (dropdownFields != null)
                return convertToDomainLeadSource(dropdownFields);
            return null;
        }

        /// <summary>
        /// Converts to domain lead source.
        /// </summary>
        /// <param name="dropdownFields">The dropdown fields.</param>
        /// <returns></returns>
        private IEnumerable<DropdownValue> convertToDomainLeadSource(IEnumerable<DropdownValueDb> dropdownFields)
        {
            foreach (DropdownValueDb dropdownField in dropdownFields)
            {
                yield return new DropdownValue() { Id = dropdownField.DropdownValueID, Value = dropdownField.DropdownValue, IsDefault = dropdownField.IsDefault, AccountID = dropdownField.AccountID };
            }
        }

        /// <summary>
        /// Gets the address types.
        /// </summary>
        /// <param name="dropdownID">The dropdown identifier.</param>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        public IEnumerable<DropdownValue> GetAddressTypes(int dropdownID, int accountId)
        {
            var db = ObjectContextFactory.Create();
            IEnumerable<DropdownValueDb> dropdownFields = db.DropdownValues.Where(df => df.DropdownID == dropdownID && df.AccountID == accountId).ToList();

            if (dropdownFields != null)
                return convertToDomainLeadSource(dropdownFields);
            return null;
        }

        /// <summary>
        /// Inserts the default dropdown values.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        public void InsertDefaultDropdownValues(int accountId)
        {
            var db = ObjectContextFactory.Create();
            var dropdownValues = db.DropdownValues.Where(r => r.AccountID == null).ToList();
            if (dropdownValues != null)
            {
                dropdownValues.ForEach(s => s.AccountID = accountId);
                db.DropdownValues.AddRange(dropdownValues);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// Inserts the default opportunity stage groups.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        public void InsertDefaultOpportunityStageGroups(int accountId)
        {
            var db = ObjectContextFactory.Create();
            var dropdownValues = db.DropdownValues.Where(r => r.AccountID == accountId && r.DropdownID == 6).ToList();
            if (dropdownValues != null)
            {
                foreach (var dropdown in dropdownValues)
                {
                    OpportunityStageGroupsDb stageGroup = new OpportunityStageGroupsDb();
                    stageGroup.AccountID = accountId;
                    stageGroup.DropdownValueID = dropdown.DropdownValueID;
                    stageGroup.OpportunityGroupID = (dropdown.DropdownValue == "Interested" || dropdown.DropdownValue == "Offer") ? 1 :
                        ((dropdown.DropdownValue == "Contract" || dropdown.DropdownValue == "Closed") ? 2 : 3);
                    db.OpportunityStageGroupsDb.Add(stageGroup);
                    db.SaveChanges();
                }
            }
        }

        /// <summary>
        /// Gets the dropdown field value by.
        /// </summary>
        /// <param name="dropdownValueID">The dropdown value identifier.</param>
        /// <returns></returns>
        public string GetDropdownFieldValueBy(Int16 dropdownValueID)
        {
            var db = ObjectContextFactory.Create();
            if (dropdownValueID > 0)
            {
                var dropdownValues = db.DropdownValues.Where(r => r.DropdownValueID == dropdownValueID).FirstOrDefault();
                return dropdownValues.DropdownValue;
            }
            return string.Empty;
        }

        /// <summary>
        /// Gets the dropdown value.
        /// </summary>
        /// <param name="dropdownValueID">The dropdown value identifier.</param>
        /// <returns></returns>
        public DropdownValue GetDropdownValue(Int16 dropdownValueID)
        {
            var db = ObjectContextFactory.Create();
            var dropdownValues = db.DropdownValues.Where(r => r.DropdownValueID == dropdownValueID).FirstOrDefault();
            return Mapper.Map<DropdownValueDb, DropdownValue>(dropdownValues);
        }
    }
}

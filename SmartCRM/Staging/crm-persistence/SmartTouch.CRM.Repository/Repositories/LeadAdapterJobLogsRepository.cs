using AutoMapper;
using LinqKit;
using SmartTouch.CRM.Domain.LeadAdapters;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using SmartTouch.CRM.Repository.Database;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace SmartTouch.CRM.Repository.Repositories
{
    public class LeadAdapterJobLogsRepository : Repository<LeadAdapterJobLogs, int, LeadAdapterJobLogsDb>, ILeadAdaptersJobLogsRepository
    {
        public LeadAdapterJobLogsRepository(IUnitOfWork unitOfWork, IObjectContextFactory objectContextFactory)
            : base(unitOfWork, objectContextFactory)
        { }

        /// <summary>
        /// Finds all.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public IEnumerable<LeadAdapterJobLogs> FindAll()
        {
            throw new NotImplementedException(" ");
        }

        /// <summary>
        /// Gets the lead adapter job details.
        /// </summary>
        /// <param name="leadAdapterJobID">The lead adapter job identifier.</param>
        /// <returns></returns>
        public IEnumerable<LeadAdapterJobLogs> GetLeadAdapterJobDetails(int leadAdapterJobID)
        {
            return null;
        }

        /// <summary>
        /// Converts to import domain.
        /// </summary>
        /// <param name="leadAdapterJobLogsDb">The lead adapter job logs database.</param>
        /// <returns></returns>
        public LeadAdapterJobLogs ConvertToImportDomain(LeadAdapterJobLogsDb leadAdapterJobLogsDb)
        {
            LeadAdapterJobLogs leadAdapterJobLogs = new LeadAdapterJobLogs();
            Mapper.Map<LeadAdapterJobLogsDb, LeadAdapterJobLogs>(leadAdapterJobLogsDb, leadAdapterJobLogs);
            //leadAdapter.LeadAdapterName = GetDescription(leadAdapter.LeadAdapterTypeID, leadAdapter.LeadAdapterTypeID.ToString());
            return leadAdapterJobLogs;
        }

        /// <summary>
        /// Converts to domain.
        /// </summary>
        /// <param name="leadAdapterJobLogsDb">The lead adapter job logs database.</param>
        /// <returns></returns>
        public override LeadAdapterJobLogs ConvertToDomain(LeadAdapterJobLogsDb leadAdapterJobLogsDb)
        {
            LeadAdapterJobLogs leadAdapterJobLogs = new LeadAdapterJobLogs();
            Mapper.Map<LeadAdapterJobLogsDb, LeadAdapterJobLogs>(leadAdapterJobLogsDb, leadAdapterJobLogs);
            //leadAdapter.LeadAdapterName = GetDescription(leadAdapter.LeadAdapterTypeID, leadAdapter.LeadAdapterTypeID.ToString());
            return leadAdapterJobLogs;
        }

        /// <summary>
        /// Converts to lead adapter job log details domain.
        /// </summary>
        /// <param name="leadAdapterJobLogsDb">The lead adapter job logs database.</param>
        /// <returns></returns>
        public LeadAdapterJobLogDetails ConvertToLeadAdapterJobLogDetailsDomain(LeadAdapterJobLogDetailsDb leadAdapterJobLogsDb)
        {
            LeadAdapterJobLogDetails leadAdapterJobLogDetails = new LeadAdapterJobLogDetails();
            Mapper.Map<LeadAdapterJobLogDetailsDb, LeadAdapterJobLogDetails>(leadAdapterJobLogsDb, leadAdapterJobLogDetails);
            //leadAdapter.LeadAdapterName = GetDescription(leadAdapter.LeadAdapterTypeID, leadAdapter.LeadAdapterTypeID.ToString());
            return leadAdapterJobLogDetails;
        }

        /// <summary>
        /// Converts the type of to database.
        /// </summary>
        /// <param name="domainType">Type of the domain.</param>
        /// <param name="db">The database.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">Invalid lead adapter id has been passed. Suspected Id forgery.</exception>
        public override LeadAdapterJobLogsDb ConvertToDatabaseType(LeadAdapterJobLogs domainType, CRMDb db)
        {
            LeadAdapterJobLogsDb leadAdapterDb;
            if (domainType.Id > 0)
            {
                leadAdapterDb = db.LeadAdapterJobLogs.SingleOrDefault(Id => Id.LeadAdapterJobLogID == domainType.Id);
                if (leadAdapterDb == null)
                {
                    throw new ArgumentException("Invalid lead adapter id has been passed. Suspected Id forgery.");
                }
                else
                {
                    leadAdapterDb = Mapper.Map<LeadAdapterJobLogs, LeadAdapterJobLogsDb>(domainType as LeadAdapterJobLogs, leadAdapterDb);
                }
            }
            else
            {
                leadAdapterDb = Mapper.Map<LeadAdapterJobLogs, LeadAdapterJobLogsDb>(domainType as LeadAdapterJobLogs);
            }
            return leadAdapterDb;
        }

        /// <summary>
        /// Converts the type of to lead adapter job log details database.
        /// </summary>
        /// <param name="domainType">Type of the domain.</param>
        /// <param name="db">The database.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">Invalid lead adapter id has been passed. Suspected Id forgery.</exception>
        public LeadAdapterJobLogDetailsDb ConvertToLeadAdapterJobLogDetailsDatabaseType(LeadAdapterJobLogDetails domainType, CRMDb db)
        {
            LeadAdapterJobLogDetailsDb leadAdapterDb;
            if (domainType.Id > 0)
            {
                leadAdapterDb = db.LeadAdapterJobLogDetails.SingleOrDefault(Id => Id.LeadAdapterJobLogID == domainType.Id);
                if (leadAdapterDb == null)
                {
                    throw new ArgumentException("Invalid lead adapter id has been passed. Suspected Id forgery.");
                }
                else
                {
                    leadAdapterDb = Mapper.Map<LeadAdapterJobLogDetails, LeadAdapterJobLogDetailsDb>(domainType as LeadAdapterJobLogDetails, leadAdapterDb);
                }
            }
            else
            {
                leadAdapterDb = Mapper.Map<LeadAdapterJobLogDetails, LeadAdapterJobLogDetailsDb>(domainType as LeadAdapterJobLogDetails);
            }
            return leadAdapterDb;
        }

        /// <summary>
        /// Persists the value objects.
        /// </summary>
        /// <param name="domainType">Type of the domain.</param>
        /// <param name="dbType">Type of the database.</param>
        /// <param name="context">The context.</param>
        public override void PersistValueObjects(LeadAdapterJobLogs domainType, LeadAdapterJobLogsDb dbType, CRMDb context)
        {
            persistLeadAdapterJobLogDetails(domainType, dbType, context);
        }

        /// <summary>
        /// Persists the lead adapter job log details.
        /// </summary>
        /// <param name="leadAdapterJobLogs">The lead adapter job logs.</param>
        /// <param name="leadAdapterJobLogsDb">The lead adapter job logs database.</param>
        /// <param name="context">The context.</param>
        void persistLeadAdapterJobLogDetails(LeadAdapterJobLogs leadAdapterJobLogs, LeadAdapterJobLogsDb leadAdapterJobLogsDb, CRMDb context)
        {
            IEnumerable<LeadAdapterJobLogDetailsDb> leadAdapterJobLogDetailsDb = Mapper.Map<IEnumerable<LeadAdapterJobLogDetails>, IEnumerable<LeadAdapterJobLogDetailsDb>>(leadAdapterJobLogs.LeadAdapterJobLogDetails);
            leadAdapterJobLogsDb.LeadAdapterJobLogDetails = leadAdapterJobLogDetailsDb;
        }

        /// <summary>
        /// Finds the by.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public override LeadAdapterJobLogs FindBy(int id)
        {
            LeadAdapterJobLogsDb leadAdapterDataBase = getLeadAdapterJobLogsDb(id);
            if (leadAdapterDataBase != null)
            {
                LeadAdapterJobLogs leadAdapterJobLogsDatabaseConvertedToDomain = ConvertToDomain(leadAdapterDataBase);
                return leadAdapterJobLogsDatabaseConvertedToDomain;
            }
            return null;
        }

        /// <summary>
        /// Gets the lead adapter job logs database.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        LeadAdapterJobLogsDb getLeadAdapterJobLogsDb(int id)
        {
            var db = ObjectContextFactory.Create();
            return db.LeadAdapterJobLogs.SingleOrDefault(c => c.LeadAdapterJobLogID == id);
        }

        /// <summary>
        /// Finds the by lead adapter job log details.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public LeadAdapterJobLogDetails FindByLeadAdapterJobLogDetails(int id)
        {
            LeadAdapterJobLogDetailsDb leadAdapterDataBase = getLeadAdapterJobLogDetailsDb(id);
            if (leadAdapterDataBase != null)
            {
                LeadAdapterJobLogDetails leadAdapterJobLogDetailsDatabaseConvertedToDomain = ConvertToLeadAdapterJobLogDetailsDomain(leadAdapterDataBase);
                return leadAdapterJobLogDetailsDatabaseConvertedToDomain;
            }
            return null;
        }

        /// <summary>
        /// Gets the lead adapter job log details database.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        LeadAdapterJobLogDetailsDb getLeadAdapterJobLogDetailsDb(int id)
        {
            var db = ObjectContextFactory.Create();
            return db.LeadAdapterJobLogDetails.SingleOrDefault(c => c.LeadAdapterJobLogDetailID == id);
        }

        /// <summary>
        /// Finds the lead adapter job log details all.
        /// </summary>
        /// <param name="limit">The limit.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="leadAdapterJobLogID">The lead adapter job log identifier.</param>
        /// <param name="status">if set to <c>true</c> [status].</param>
        /// <returns></returns>
        public IEnumerable<LeadAdapterJobLogDetails> FindLeadAdapterJobLogDetailsAll(int limit, int pageNumber, int leadAdapterJobLogID, bool status)
        {
            var predicate = PredicateBuilder.True<LeadAdapterJobLogDetailsDb>();
            var records = (pageNumber - 1) * limit;
            predicate = predicate.And(a => a.LeadAdapterJobLogID == leadAdapterJobLogID);

            if (status)
            {
                predicate = predicate.And(a => a.LeadAdapterRecordStatusID == LeadAdapterRecordStatus.Added
                                            || a.LeadAdapterRecordStatusID == LeadAdapterRecordStatus.Updated);
            }
            else
            {
                predicate = predicate.And(a => a.LeadAdapterRecordStatusID == LeadAdapterRecordStatus.Duplicate
                                            || a.LeadAdapterRecordStatusID == LeadAdapterRecordStatus.ValidationFailed
                                            || a.LeadAdapterRecordStatusID == LeadAdapterRecordStatus.BuilderNumberFailed
                                            || a.LeadAdapterRecordStatusID == LeadAdapterRecordStatus.DuplicateFromFile
                                            || a.LeadAdapterRecordStatusID == LeadAdapterRecordStatus.CommunityNumberFailed
                                            );
            }
            IEnumerable<LeadAdapterJobLogDetailsDb> leadAdapterJobLogDetailsDb = findLeadAdapterJobLogDetailsSummary(predicate).Skip(records).Take(limit);
            foreach (LeadAdapterJobLogDetailsDb da in leadAdapterJobLogDetailsDb)
            {
                yield return ConvertToLeadAdapterJobLogDetailsDomain(da);
            }
        }

        /// <summary>
        /// Finds the lead adapter job log details all.
        /// </summary>
        /// <param name="leadAdapterAndAccountMapID">The lead adapter and account map identifier.</param>
        /// <param name="status">if set to <c>true</c> [status].</param>
        /// <returns></returns>
        public IEnumerable<LeadAdapterJobLogDetails> FindLeadAdapterJobLogDetailsAll(int leadAdapterAndAccountMapID, bool status)
        {
            var predicate = PredicateBuilder.True<LeadAdapterJobLogDetailsDb>();
            predicate = predicate.And(a => a.LeadAdapterJobLogID == leadAdapterAndAccountMapID);
            predicate = predicate.And(a => a.RowData != null);
            if (status)
            {
                predicate = predicate.And(a => a.LeadAdapterRecordStatusID == LeadAdapterRecordStatus.Added 
                                            || a.LeadAdapterRecordStatusID == LeadAdapterRecordStatus.Updated);
            }
            else
            {
                predicate = predicate.And(a => a.LeadAdapterRecordStatusID == LeadAdapterRecordStatus.Duplicate
                                            || a.LeadAdapterRecordStatusID == LeadAdapterRecordStatus.DuplicateFromFile
                                            || a.LeadAdapterRecordStatusID == LeadAdapterRecordStatus.BuilderNumberFailed
                                            || a.LeadAdapterRecordStatusID == LeadAdapterRecordStatus.ValidationFailed);
            }
            IEnumerable<LeadAdapterJobLogDetailsDb> leadAdapterJobLogDetailsDb = findLeadAdapterJobLogDetailsSummary(predicate);
            foreach (LeadAdapterJobLogDetailsDb da in leadAdapterJobLogDetailsDb)
            {
                yield return ConvertToLeadAdapterJobLogDetailsDomain(da);
            }
        }

        /// <summary>
        /// Finds the lead adapter job log details summary.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <returns></returns>
        IEnumerable<LeadAdapterJobLogDetailsDb> findLeadAdapterJobLogDetailsSummary(System.Linq.Expressions.Expression<Func<LeadAdapterJobLogDetailsDb, bool>> predicate)
        {
            IEnumerable<LeadAdapterJobLogDetailsDb> leadAdapterJobLogs = ObjectContextFactory.Create()
                .LeadAdapterJobLogDetails
                .Include(c=>c.LeadAdapterRecordStatus)
                .AsExpandable()                
                .Where(predicate).OrderByDescending(c => c.LeadAdapterJobLogDetailID).Select(a =>
                    new
                    {
                        LeadAdapterJobLogDetailID = a.LeadAdapterJobLogDetailID,
                        ReferenceId = a.ReferenceId,
                        CreatedDateTime = a.CreatedDateTime,
                        LeadAdapterRecordStatusID = a.LeadAdapterRecordStatusID,
                        RowData = a.RowData,
                        Remarks = a.Remarks,
                        LeadAdapterRecordStatus = a.LeadAdapterRecordStatus

                    }).ToList().Select(x => new LeadAdapterJobLogDetailsDb
                    {
                        LeadAdapterJobLogDetailID = x.LeadAdapterJobLogDetailID,
                        ReferenceId = x.ReferenceId,
                        CreatedDateTime = x.CreatedDateTime,
                        LeadAdapterRecordStatusID = x.LeadAdapterRecordStatusID,
                        RowData = x.RowData,
                        Remarks = x.Remarks,
                        LeadAdapterRecordStatus = x.LeadAdapterRecordStatus
                    });
            return leadAdapterJobLogs;
        }

        /// <summary>
        /// Finds the lead adapter job log all.
        /// </summary>
        /// <param name="limit">The limit.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="leadAdapterAndAccountMapID">The lead adapter and account map identifier.</param>
        /// <returns></returns>
        public IEnumerable<LeadAdapterJobLogs> FindLeadAdapterJobLogAll(int limit, int pageNumber, int leadAdapterAndAccountMapID)
        {
            var predicate = PredicateBuilder.True<LeadAdapterJobLogsDb>();
            var records = (pageNumber - 1) * limit;
            predicate = predicate.And(a => a.LeadAdapterAndAccountMapID == leadAdapterAndAccountMapID);
            IEnumerable<LeadAdapterJobLogsDb> leadadapters = findLeadAdapterJobLogsSummary(predicate).Skip(records).Take(limit);
            foreach (LeadAdapterJobLogsDb da in leadadapters)
            {
                yield return ConvertToDomain(da);
            }
        }

        /// <summary>
        /// Finds the lead adapter job log all.
        /// </summary>
        /// <param name="leadAdapterAndAccountMapID">The lead adapter and account map identifier.</param>
        /// <returns></returns>
        public IEnumerable<LeadAdapterJobLogs> FindLeadAdapterJobLogAll(int leadAdapterAndAccountMapID)
        {
            var predicate = PredicateBuilder.True<LeadAdapterJobLogsDb>();
            predicate = predicate.And(a => a.LeadAdapterAndAccountMapID == leadAdapterAndAccountMapID);
            IEnumerable<LeadAdapterJobLogsDb> leadadapters = findLeadAdapterJobLogsSummary(predicate);
            foreach (LeadAdapterJobLogsDb da in leadadapters)
            {
                yield return ConvertToDomain(da);
            }
        }

        /// <summary>
        /// Finds the lead adapter job logs summary.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <returns></returns>
        IEnumerable<LeadAdapterJobLogsDb> findLeadAdapterJobLogsSummary(System.Linq.Expressions.Expression<Func<LeadAdapterJobLogsDb, bool>> predicate)
        {
            IEnumerable<LeadAdapterJobLogsDb> leadAdapterJobLogs = ObjectContextFactory.Create().LeadAdapterJobLogs
                .AsExpandable()
                .Where(predicate).OrderByDescending(c => c.LeadAdapterAndAccountMapID).Select(a =>
                    new
                    {
                        LeadAdapterAndAccountMapID = a.LeadAdapterAndAccountMapID,
                        LeadAdapterJobLogID = a.LeadAdapterJobLogID,
                        CreatedDateTime = a.CreatedDateTime,
                        FileName = a.FileName,
                        Remarks = a.Remarks,
                        LeadAdapterJobStatusID = a.LeadAdapterJobStatusID

                    }).ToList().Select(x => new LeadAdapterJobLogsDb
                    {
                        LeadAdapterAndAccountMapID = x.LeadAdapterAndAccountMapID,
                        LeadAdapterJobLogID = x.LeadAdapterJobLogID,
                        CreatedDateTime = x.CreatedDateTime,
                        FileName = x.FileName,
                        Remarks = x.Remarks,
                        LeadAdapterJobStatusID = x.LeadAdapterJobStatusID
                    });
            return leadAdapterJobLogs;
        }

        /// <summary>
        /// Finds the lead adapter job log details count.
        /// </summary>
        /// <param name="leadAdapterAndAccountMapID">The lead adapter and account map identifier.</param>
        /// <param name="status">if set to <c>true</c> [status].</param>
        /// <returns></returns>
        public int FindLeadAdapterJobLogDetailsCount(int leadAdapterAndAccountMapID, bool status)
        {
            var db = ObjectContextFactory.Create();
            var predicate = PredicateBuilder.True<LeadAdapterJobLogDetailsDb>();
            predicate = predicate.And(a => a.LeadAdapterJobLogID == leadAdapterAndAccountMapID);

            if (status)
            {
                predicate = predicate.And(a => a.LeadAdapterRecordStatusID == LeadAdapterRecordStatus.Added
                                        || a.LeadAdapterRecordStatusID == LeadAdapterRecordStatus.Updated);
            }
            else {
                predicate = predicate.And(a => a.LeadAdapterRecordStatusID == LeadAdapterRecordStatus.Duplicate
                                            || a.LeadAdapterRecordStatusID == LeadAdapterRecordStatus.ValidationFailed
                                            || a.LeadAdapterRecordStatusID == LeadAdapterRecordStatus.BuilderNumberFailed
                                            || a.LeadAdapterRecordStatusID == LeadAdapterRecordStatus.DuplicateFromFile);
            }


            int count = db.LeadAdapterJobLogDetails.AsExpandable().Where(predicate).Count();
            return count;
        }

        public LeadAdapterData GetLeadAdapterSubmittedData(int? leadAdapterId)
        {

            LeadAdapterData leadAdapterData = new LeadAdapterData();
            if(leadAdapterId != null)
            {
                var db = ObjectContextFactory.Create();
                var sql = @"select ld.SubmittedData , lt.Name LeadAdapterType from Contacts c (nolock) 
                         inner join LeadAdapterJobLogDetails ld (nolock) on c.ReferenceID = ld.ReferenceID
                         inner join LeadAdapterJobLogs ljl (nolock) on ld.LeadAdapterJobLogID = ljl.LeadAdapterJobLogID
                         inner join LeadAdapterAndAccountMap ldm (nolock) on ljl.LeadAdapterAndAccountMapID = ldm.LeadAdapterAndAccountMapID
                         inner join LeadAdapterTypes lt (nolock) on ldm.LeadAdapterTypeID = lt.LeadAdapterTypeID
                         where c.ContactID=@ContactID order by ld.CreatedDateTime desc";
               leadAdapterData = db.Get<LeadAdapterData>(sql, new { ContactID = leadAdapterId }).FirstOrDefault();
            }
            
            return leadAdapterData;
            
        }

        public string GetLeadAdapterName(int? leadAdapterTypeId)
        {
            var db = ObjectContextFactory.Create();
            var sql = @"SELECT lt.Name FROM Contacts c (NOLOCK) 
                        INNER JOIN LeadAdapterJobLogDetails ld (NOLOCK) on c.ReferenceID = ld.ReferenceID
                        INNER JOIN LeadAdapterJobLogs ljl (NOLOCK) on ld.LeadAdapterJobLogID = ljl.LeadAdapterJobLogID
                        INNER JOIN LeadAdapterAndAccountMap ldm (NOLOCK) on ljl.LeadAdapterAndAccountMapID = ldm.LeadAdapterAndAccountMapID
                        INNER JOIN LeadAdapterTypes lt (NOLOCK) on ldm.LeadAdapterTypeID = lt.LeadAdapterTypeID
                        WHERE c.ContactID=@ContactID ORDER BY ld.CreatedDateTime DESC";
            string leadAdapterName = db.Get<string>(sql, new { ContactID = leadAdapterTypeId }).FirstOrDefault();
            return leadAdapterName;

        }

        /// <summary>
        /// Getting FaceBook Host Name By ContactId
        /// </summary>
        /// <param name="contactId"></param>
        /// <returns></returns>
        public string GetFaceBookHostName(int contactId)
        {
            using (var db = ObjectContextFactory.Create())
            {
                var sql = @"SELECT fl.Name FROM Contacts c (NOLOCK) 
                            INNER JOIN LeadAdapterJobLogDetails ld (NOLOCK) on c.ReferenceID = ld.ReferenceID
                            INNER JOIN LeadAdapterJobLogs ljl (NOLOCK) on ld.LeadAdapterJobLogID = ljl.LeadAdapterJobLogID
                            INNER JOIN FacebookLeadAdapter fl (NOLOCK) on fl.LeadAdapterAndAccountMapID = ljl.LeadAdapterAndAccountMapID
                            WHERE c.ContactID=@ContactID ORDER BY ld.CreatedDateTime DESC";
                return db.Get<string>(sql, new { ContactID = contactId }).FirstOrDefault();
            }
        }


    }
}

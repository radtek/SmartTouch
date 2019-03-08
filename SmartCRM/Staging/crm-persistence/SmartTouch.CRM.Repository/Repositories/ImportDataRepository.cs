using AutoMapper;
using LandmarkIT.Enterprise.Utilities.Logging;
using LinqKit;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.ImportData;
using SmartTouch.CRM.Domain.LeadAdapters;
using SmartTouch.CRM.Domain.Tags;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using SmartTouch.CRM.Repository.Database;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SmartTouch.CRM.Repository.Repositories
{
    /// <summary>
    /// 
    /// </summary>
    public class ImportDataRepository : Repository<ImportData, int, GetImportDataDb>, IImportDataRepository
    {

        public ImportDataRepository(IUnitOfWork unitOfWork, IObjectContextFactory objectContextFactory)
            : base(unitOfWork, objectContextFactory)
        {
        }

        /// <summary>
        /// Finds all.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ImportData> FindAll()
        {
            IEnumerable<GetImportDataDb> importData = ObjectContextFactory.Create().GetImportData;
            foreach (GetImportDataDb dc in importData)
            {
                yield return Mapper.Map<GetImportDataDb, ImportData>(dc);
            }
        }

        /// <summary>
        /// Finds the by.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public override ImportData FindBy(int id)
        {
            GetImportDataDb importDataDatabase = ObjectContextFactory.Create().GetImportData.FirstOrDefault(c => c.LeadAdapterJobLogID == id);

            if (importDataDatabase != null)
                return ConvertToDomain(importDataDatabase);
            return null;
        }

        /// <summary>
        /// Gets the import data database.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        GetImportDataDb getImportDataDb(int id)
        {
            var db = ObjectContextFactory.Create();
            return db.GetImportData.SingleOrDefault(c => c.LeadAdapterJobLogID == id);
        }

        /// <summary>
        /// Persists the value objects.
        /// </summary>
        /// <param name="domainType">Type of the domain.</param>
        /// <param name="importdataDb">The importdata database.</param>
        /// <param name="db">The database.</param>
        public override void PersistValueObjects(ImportData domainType, GetImportDataDb importdataDb, CRMDb db)
        {
            //for future use
        }
        
        /// <summary>
        /// InsertImportsData
        /// </summary>
        /// <param name="contact">contact.</param>
        public void InsertImportsData(ImportContactsData contact)
        {
            var db = ObjectContextFactory.Create();

            if (contact.ContactData != null && contact.ContactData.Any())
            {
                List<ImportContactData> TempDb = (Mapper.Map<IEnumerable<RawContact>, IEnumerable<ImportContactData>>(contact.ContactData)).ToList();
                int jobid = contact.ContactData.Select(k => k.JobID).First();
                db.BulkInsert<ImportContactData>(TempDb);
                db.SaveChanges();

                Logger.Current.Informational("Successfully inserted contacts in Import Data,contacts count: " + contact.ContactData.Count());

                InsertImportCustomFieldData(contact.ContactCustomData);
                InsertImportPhoneData(contact.ContactPhoneData);
                this.UpdateLeadAdapterStatus(jobid, null);
            }
        }

        /// <summary>
        /// Gets the source by job identifier.
        /// </summary>
        /// <param name="jobId">The job identifier.</param>
        /// <returns></returns>
        public string GetSourceByJobId(int jobId)
        {
            var db = ObjectContextFactory.Create();
            var source = db.LeadAdapterJobLogs.Where(p => p.LeadAdapterJobLogID == jobId).Join(db.LeadAdapters, p => p.LeadAdapterAndAccountMapID, q => q.LeadAdapterAndAccountMapId, (p, q) => new
            {
                q.LeadAdapterTypeID

            }).Join(db.LeadAdapterTypes, a => a.LeadAdapterTypeID, b => b.LeadAdapterTypeID, (a, b) => new { b.Name }).FirstOrDefault();
            if (source != null)
                return source.Name;
            else
                return "";
        }

        /// <summary>
        /// Gets the lead adapter type by reference identifier.
        /// </summary>
        /// <param name="guid">The unique identifier.</param>
        /// <returns></returns>
        public string GetLeadAdapterTypeByReferenceId(Guid guid)
        {
            var db = ObjectContextFactory.Create();
            var source = db.LeadAdapterJobLogDetails.Where(p => p.ReferenceId == guid).Join(db.LeadAdapterJobLogs, k => k.LeadAdapterJobLogID, l => l.LeadAdapterJobLogID, (k, l) => new { l.LeadAdapterAndAccountMapID })
                  .Join(db.LeadAdapters, m => m.LeadAdapterAndAccountMapID, n => n.LeadAdapterAndAccountMapId, (m, n) => new { n.LeadAdapterTypeID }).
                  Join(db.LeadAdapterTypes, a => a.LeadAdapterTypeID, b => b.LeadAdapterTypeID, (a, b) => new { b.Name }).FirstOrDefault();
            if (source != null)
                return source.Name;
            else
                return "";
        }

        /// <summary>
        /// Gets the import contact by data identifier.
        /// </summary>
        /// <param name="dataId">The data identifier.</param>
        /// <returns></returns>
        public RawContact GetImportContactByDataID(int dataId)
        {
            var db = ObjectContextFactory.Create();
            ImportContactData data = db.ImportContactData.Where(p => p.ImportContactDataID == dataId).FirstOrDefault();
            if (data != null)
            {
                RawContact contact = Mapper.Map<ImportContactData, RawContact>(data);
                return contact;
            }
            return null;
        }

        /// <summary>
        /// Inserts the import custom field data.
        /// </summary>
        /// <param name="customData">The custom data.</param>
        public void InsertImportCustomFieldData(IList<SmartTouch.CRM.Domain.ImportData.ImportCustomData> customData)
        {
            //  var db = ObjectContextFactory.Create();
            Logger.Current.Informational("Request received for bulk customfielddata inserting");
            using (var db = ObjectContextFactory.Create())
            {
                List<SmartTouch.CRM.Repository.Database.ImportCustomData> TempDb = (Mapper.Map<IEnumerable<SmartTouch.CRM.Domain.ImportData.ImportCustomData>, IEnumerable<SmartTouch.CRM.Repository.Database.ImportCustomData>>(customData)).ToList();
                db.BulkInsert<SmartTouch.CRM.Repository.Database.ImportCustomData>(TempDb);
                db.SaveChanges();
            }

            Logger.Current.Informational("Successfully inserted bulk customfielddata,count:" + customData.Count());
        }

        /// <summary>
        /// Gets the import custom fields by reference identifier.
        /// </summary>
        /// <param name="refID">The reference identifier.</param>
        /// <returns></returns>
        public List<ContactCustomField> GetImportCustomFieldsByRefID(Guid refID)
        {
            var db = ObjectContextFactory.Create();
            List<ContactCustomField> data = db.ImportCustomData.Where(p => p.ReferenceID == refID).Select(s => new ContactCustomField()
            {
                CustomFieldId = s.FieldID,
                Value = s.FieldValue
            }).ToList();
            return data;
        }

        /// <summary>
        /// Merges the duplicate import data.
        /// </summary>
        /// <param name="customData">The custom data.</param>
        /// <param name="phoneData">The phone data.</param>
        /// <param name="referenceId">The reference identifier.</param>
        /// <param name="contactRefId">The contact reference identifier.</param>
        public void MergeDuplicateImportData(string customData, string phoneData, Guid referenceId, Guid contactRefId)
        {
            Logger.Current.Informational("started...");
            var db = ObjectContextFactory.Create();
            var query = string.Format("Update ImportCustomData Set ReferenceID = '{0}' where ReferenceId = '{1}'", contactRefId, referenceId);
            db.Database.ExecuteSqlCommand(query);
            query = string.Format("Update ImportPhoneData Set ReferenceID = '{0}' where ReferenceId = '{1}'", contactRefId, referenceId);
            db.Database.ExecuteSqlCommand(query);
        }

        /// <summary>
        /// Inserts the import phone data.
        /// </summary>
        /// <param name="phoneData">The phone data.</param>
        public void InsertImportPhoneData(IList<SmartTouch.CRM.Domain.ImportData.ImportPhoneData> phoneData)
        {
            Logger.Current.Informational("Request received for bulk phonecontactdat inserting");
            using (var db = ObjectContextFactory.Create())
            {
                List<SmartTouch.CRM.Repository.Database.ImportPhoneData> TempDb = (Mapper.Map<IEnumerable<SmartTouch.CRM.Domain.ImportData.ImportPhoneData>, IEnumerable<SmartTouch.CRM.Repository.Database.ImportPhoneData>>(phoneData)).ToList();
                db.BulkInsert<SmartTouch.CRM.Repository.Database.ImportPhoneData>(TempDb);
                db.SaveChanges();
            }

            Logger.Current.Informational("Successfully inserted phone data,Count:"+ phoneData.Count());
        }

        /// <summary>
        /// Updates the lead adapter status.
        /// </summary>
        /// <param name="jobid">The jobid.</param>
        public void UpdateLeadAdapterStatus(int jobid, LeadAdapterJobStatus? status)
        {
            Logger.Current.Informational("Request received to update LeadAdapterJobLogs to some x Status.");
            var db = ObjectContextFactory.Create();
            var job = db.LeadAdapterJobLogs.Where(p => p.LeadAdapterJobLogID == jobid).Select(p => p).FirstOrDefault();
            if (job != null && !status.HasValue)
                job.LeadAdapterJobStatusID = LeadAdapterJobStatus.ReadyToProcess;
            else if (job != null && status.HasValue)
                job.LeadAdapterJobStatusID = status.Value;
            db.SaveChanges();

            Logger.Current.Informational("Successfully updated LeadAdapterJobLogs to Status:"+ (status.HasValue?status.Value.ToString():(0).ToString()));
        }

        /// <summary>
        /// InsertImportContactEmailStatuses
        /// </summary>
        /// <param name="contact">contact.</param>
        public void InsertImportContactEmailStatuses(List<RawContact> contact)
        {
            List<ImportContactsEmailStatusesDb> TempDb = (Mapper.Map<IEnumerable<RawContact>, IEnumerable<ImportContactsEmailStatusesDb>>(contact)).ToList();
            var db = ObjectContextFactory.Create();
            db.BulkInsert<ImportContactsEmailStatusesDb>(TempDb);
            db.SaveChanges();
        }

        /// <summary>
        /// Finds the accounts summary.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <returns></returns>
        IEnumerable<GetImportDataDb> findAccountsSummary(System.Linq.Expressions.Expression<Func<GetImportDataDb, bool>> predicate)
        {
            IEnumerable<GetImportDataDb> getImportData = ObjectContextFactory.Create().GetImportData
                .AsExpandable()
                .Where(predicate).OrderByDescending(c => c.LeadAdapterJobLogID).Select(a =>
                    new
                    {
                        LeadAdapterJobLogID = a.LeadAdapterJobLogID,
                        FileName = a.FileName,
                        LeadAdapterAndAccountMapID = a.LeadAdapterAndAccountMapID,
                        LeadAdapterJobStatusID = a.LeadAdapterJobStatusID,
                        StartDate = a.StartDate,
                        EndDate = a.EndDate,
                        RecordCreated = a.RecordCreated,
                        RecordUpdated = a.RecordUpdated,
                        TotalRecords = a.TotalRecords,
                        CreatedDateTime = a.CreatedDateTime,
                        Validation = a.IsValidated,
                        BadEmails = a.BadEmailsData,
                        GoodEmails = a.GoodEmailsData,
                        NeverBounceRequestID = a.NeverBounceRequestID
                    }).ToList().Select(x => new GetImportDataDb
                    {
                        LeadAdapterJobLogID = x.LeadAdapterJobLogID,
                        FileName = x.FileName,
                        LeadAdapterAndAccountMapID = x.LeadAdapterAndAccountMapID,
                        LeadAdapterJobStatusID = x.LeadAdapterJobStatusID,
                        StartDate = x.StartDate,
                        EndDate = x.EndDate,
                        RecordCreated = x.RecordCreated,
                        RecordUpdated = x.RecordUpdated,
                        TotalRecords = x.TotalRecords,
                        CreatedDateTime = x.CreatedDateTime,
                        IsValidated = x.Validation,
                        BadEmailsData = x.BadEmails,
                        GoodEmailsData = x.GoodEmails,
                        NeverBounceRequestID = x.NeverBounceRequestID
                    });
            return getImportData;
        }

        /// <summary>
        /// Finds all.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="limit">The limit.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="leadaccountmapId">The leadaccountmap identifier.</param>
        /// <returns></returns>
        public IEnumerable<ImportData> FindAll(string name, int limit, int pageNumber, int leadaccountmapId)
        {
            var predicate = PredicateBuilder.True<GetImportDataDb>();
            var records = (pageNumber - 1) * limit;
            if (!string.IsNullOrEmpty(name))
            {
                name = name.ToLower();
                predicate = predicate.And(a => a.FileName.Contains(name));
            }
            predicate = predicate.And(a => a.LeadAdapterAndAccountMapID == leadaccountmapId);

            IEnumerable<GetImportDataDb> accounts = findAccountsSummary(predicate).Skip(records).Take(limit);
            foreach (GetImportDataDb da in accounts)
            {
                yield return ConvertToDomain(da);
            }
        }

        /// <summary>
        /// Finds all.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="leadaccountmapId">The leadaccountmap identifier.</param>
        /// <returns></returns>
        public IEnumerable<ImportData> FindAll(string name, int leadaccountmapId)
        {
            var predicate = PredicateBuilder.True<GetImportDataDb>();
            if (!string.IsNullOrEmpty(name))
            {
                name = name.ToLower();
                predicate = predicate.And(a => a.FileName.Contains(name));
            }
            predicate = predicate.And(a => a.LeadAdapterAndAccountMapID == leadaccountmapId);
            //if (status != 0)
            //{
            //    predicate = predicate.And(a => a.Status == status);
            //}
            IEnumerable<GetImportDataDb> accounts = findAccountsSummary(predicate);
            foreach (GetImportDataDb da in accounts)
            {
                yield return ConvertToDomain(da);
            }
        }

        /// <summary>
        /// Converts to domain.
        /// </summary>
        /// <param name="importDbObject">The import database object.</param>
        /// <returns></returns>
        public override ImportData ConvertToDomain(GetImportDataDb importDbObject)
        {
            ImportData import = new ImportData();
            Mapper.Map<GetImportDataDb, ImportData>(importDbObject, import);
            return import;

        }

        /// <summary>
        /// Converts the type of to database.
        /// </summary>
        /// <param name="domainType">Type of the domain.</param>
        /// <param name="db">The database.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">Invalid import id has been passed. Suspected Id forgery.</exception>
        public override GetImportDataDb ConvertToDatabaseType(ImportData domainType, CRMDb db)
        {
            GetImportDataDb importDb;

            //Existing Contact
            if (domainType.Id > 0)
            {
                importDb = db.GetImportData.SingleOrDefault(c => c.LeadAdapterJobLogID == domainType.Id);
                //accountsDb = db.Accounts.SingleOrDefault(c => c.AccountID == domainType.Id);

                if (importDb == null)
                    throw new ArgumentException("Invalid import id has been passed. Suspected Id forgery.");

                importDb = Mapper.Map<ImportData, GetImportDataDb>(domainType, importDb);
            }
            else //New
            {

                importDb = Mapper.Map<ImportData, GetImportDataDb>(domainType);
            }
            return importDb;
        }

        /// <summary>
        /// Gets the states.
        /// </summary>
        /// <param name="statename">The statename.</param>
        /// <returns></returns>
        public string GetStates(string statename)
        {
            var stateId = "";
            var db = ObjectContextFactory.Create();
            IEnumerable<StatesDb> statesDb = db.States.Where(c => c.StateName.Equals(statename)).ToList();
            if (statesDb != null && statesDb.Any())
                stateId = statesDb.FirstOrDefault().StateID;
            return stateId;
        }

        /// <summary>
        /// Gets the lead adapter account map.
        /// </summary>
        /// <param name="AccountId">The account identifier.</param>
        /// <returns></returns>
        public int GetLeadAdapterAccountMap(int AccountId)
        {
            var db = ObjectContextFactory.Create();
            return db.LeadAdapters.Where(c => c.AccountID == AccountId && c.LeadAdapterTypeID == 11)
                                                            .Select(x => x.LeadAdapterAndAccountMapId)
                                                            .SingleOrDefault();
        }

        /// <summary>
        /// Gets the countries.
        /// </summary>
        /// <param name="countryname">The countryname.</param>
        /// <returns></returns>
        public string GetCountries(string countryname)
        {
            var countryId = "";
            var db = ObjectContextFactory.Create();
            IEnumerable<CountriesDb> countriesDb = db.Countries.Where(c => c.CountryName.Equals(countryname)).ToList();
            if (countriesDb != null)
                countryId = countriesDb.FirstOrDefault().CountryID;
            return countryId;
        }

        /// <summary>
        /// To the data table.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items">The items.</param>
        /// <returns></returns>
        public static DataTable ToDataTable<T>(List<T> items)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);
            //Get all the properties
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                switch (prop.PropertyType.Name)
                {
                    case "String":
                        dataTable.Columns.Add(prop.Name, typeof(string));
                        break;
                    case "Byte":
                        dataTable.Columns.Add(prop.Name, typeof(byte));
                        break;
                    case "DateTime":
                        dataTable.Columns.Add(prop.Name, typeof(DateTime));
                        break;
                    case "Int32":
                        dataTable.Columns.Add(prop.Name, typeof(Int32));
                        break;
                    default:
                        dataTable.Columns.Add(prop.Name);
                        break;
                }
                //Setting column names as Property names
            }
            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    //inserting property values to datatable rows
                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }
            //put a breakpoint here and check datatable
            return dataTable;
        }

        /// <summary>
        /// Inserts the lead adapterjob.
        /// </summary>
        /// <param name="leadAdapterJobLog">The lead adapter job log.</param>
        /// <param name="uniqueIdentifier">The unique identifier.</param>
        /// <param name="updateOnDuplicate">if set to <c>true</c> [update on duplicate].</param>
        /// <param name="isFromImport">if set to <c>true</c> [is from import].</param>
        /// <param name="AccountID">The account identifier.</param>
        /// <param name="UserID">The user identifier.</param>
        /// <param name="DuplicteLogicID">The duplicte logic identifier.</param>
        /// <returns></returns>
        public int InsertLeadAdapterjob(LeadAdapterJobLogs leadAdapterJobLog, Guid uniqueIdentifier, bool updateOnDuplicate, bool isFromImport, int AccountID, int UserID, int DuplicteLogicID,int ownerId, bool IncludeInReports)
        {
            Logger.Current.Informational("ToDataTable method called with lead adapter job logs domain class");
            var JobLogID = default(int);
            DataTable dtLogDetails = ToDataTable<LeadAdapterJobLogDetails>(leadAdapterJobLog.LeadAdapterJobLogDetails.ToList());
            try
            {
                Logger.Current.Informational("Created the procedure name insert job log details");
                var procedureName = "[dbo].[INSERT_JobLogDetails]";
                Logger.Current.Informational("Created the parametes for the procedure");
                var outputParam = new SqlParameter { ParameterName = "@JobLogID", SqlDbType = SqlDbType.Int, Direction = ParameterDirection.Output };
                var parms = new List<SqlParameter>
                {   
                    new SqlParameter{ParameterName="@leadAdapterAndAccountMapID", Value = leadAdapterJobLog.LeadAdapterAndAccountMapID},
                    new SqlParameter{ParameterName="@remarks", Value = leadAdapterJobLog.Remarks},
                    new SqlParameter{ParameterName="@filename", Value = leadAdapterJobLog.FileName},
                    new SqlParameter{ParameterName = "@importuniqueidentifier", Value= uniqueIdentifier},
                    new SqlParameter{ParameterName = "@updateonduplicate", Value= updateOnDuplicate},
                    new SqlParameter{ParameterName="@isfromimport", Value = isFromImport},
                    new SqlParameter{ParameterName="@AccountID", Value = AccountID},
                    new SqlParameter{ParameterName="@UserID", Value = UserID},
                    new SqlParameter{ParameterName="@DuplicateLogic", Value = DuplicteLogicID},
                    new SqlParameter {ParameterName="@OwnerId",Value = ownerId },                  					
                    new SqlParameter{ParameterName="@LeadAdapterDetails", Value=dtLogDetails, SqlDbType= SqlDbType.Structured, TypeName="dbo.LeadAdapterDetails" },
                    new SqlParameter {ParameterName="@IncludeInReports", Value =IncludeInReports },
                   outputParam
                };

                // parms.Add(outputParam);
                Logger.Current.Informational("Created the instance for the crmdb method and called the execute stored proecedure method with the instance");
                CRMDb context = new CRMDb();
                Logger.Current.Informational("Logging Error while inserting Before Sp Excution:");
                context.ExecuteStoredProcedure(procedureName, parms);
                Logger.Current.Informational("Logging Error while inserting Import Data: After Sp Excution:" + outputParam);
                try
                {
                    JobLogID = (int)outputParam.Value;
                }
                catch(Exception ex)
                {
                    Logger.Current.Informational("error while getting joblogid", ex.StackTrace);
                    JobLogID = 0;
                }
                
                return JobLogID;
            }
            catch (Exception ex)
            {
                Logger.Current.Informational("An execpetion has occuered in the InsertLeadAdapterJob method in the InsertDateRepository.cs file : " + ex);
                throw;
            }
        }

        /// <summary>
        /// Uploads the contacts.
        /// </summary>
        /// <param name="contacts">The contacts.</param>
        /// <param name="jobLog">The job log.</param>
        /// <param name="importedFrom">The imported from.</param>
        /// <param name="uniqueidentifier">The uniqueidentifier.</param>
        /// <param name="AccountID">The account identifier.</param>
        /// <param name="leadAdapterAccountMapID">The lead adapter account map identifier.</param>
        /// <param name="leadAdapterJobLogID">The lead adapter job log identifier.</param>
        /// <param name="OwnerID">The owner identifier.</param>
        /// <returns></returns>
        public IList<int> UploadContacts(List<RawContact> contacts, LeadAdapterJobLogs jobLog, byte importedFrom, Guid uniqueidentifier,
                                         int AccountID, int leadAdapterAccountMapID, int leadAdapterJobLogID, int? OwnerID)
        {
            try
            {
                DataTable dtContacts = ToDataTable<RawContact>(contacts);
                var joblogdetails = jobLog.LeadAdapterJobLogDetails.ToList();
                DataTable dtLogDetails = ToDataTable<LeadAdapterJobLogDetails>(joblogdetails);
                LeadAdapterErrorStatus errorstatus = default(LeadAdapterErrorStatus);

                byte[] successstatuses = new byte[] { 1, 3 };
                byte[] errorstatuses = new byte[] { 2, 4, 5, 6, 7, 8 };
                int successcount = joblogdetails.Where(x => successstatuses.Contains(x.LeadAdapterRecordStatusID)).Count();
                int failurecount = joblogdetails.Where(x => errorstatuses.Contains(x.LeadAdapterRecordStatusID)).Count();

                if (successcount == joblogdetails.Count())
                {
                    errorstatus = LeadAdapterErrorStatus.Running;
                }
                else if (failurecount == joblogdetails.Count())
                {
                    errorstatus = LeadAdapterErrorStatus.Error;
                }
                else
                {
                    errorstatus = LeadAdapterErrorStatus.Warning;
                }

                var procedureName = "[dbo].[IMPORT_Contacts_Data]";

                var parms = new List<SqlParameter>
                {   
                    new SqlParameter{ParameterName="@ImportData", Value=dtContacts, SqlDbType= SqlDbType.Structured, TypeName="dbo.IMPORT_ContactDetails" },
                    new SqlParameter{ParameterName="@LeadAdapterDetails", Value=dtLogDetails, SqlDbType= SqlDbType.Structured, TypeName="dbo.LeadAdapterDetails" },
                    new SqlParameter{ParameterName="@filename", Value = jobLog.FileName == null? jobLog.StorageName: jobLog.FileName},
                    new SqlParameter{ParameterName="@GUID", Value = uniqueidentifier.ToString() },
                    // if value = 2 means it is from lead adapter and if the value is 1 then it is from import
                    new SqlParameter{ParameterName="@FileUpload", Value = importedFrom },
                    new SqlParameter{ParameterName="@AccountID", Value = AccountID },
                    new SqlParameter{ParameterName="@LeadAdapterAndAccountMapID", Value = leadAdapterAccountMapID },
                    new SqlParameter{ParameterName="@ErrorStatus", Value = (byte)errorstatus },
                    new SqlParameter{ParameterName="@LeadAdapterJobLogID", Value = leadAdapterJobLogID },
                    new SqlParameter{ParameterName="@OwnerID", Value = OwnerID == null?0:OwnerID }
                };

                CRMDb context = new CRMDb();
                var objectContext = (context as IObjectContextAdapter).ObjectContext;
                objectContext.CommandTimeout = 1600;
                var lstcontacts = context.ExecuteStoredProcedure<int>(procedureName, parms);
                List<int> contactids = new List<int>();
                foreach (int da in lstcontacts)
                {
                    contactids.Add(da);
                }
                return contactids;
            }
            catch (SqlException ex)
            {
                Logger.Current.Error("An error occurred in Import Data Stored Proc Insertion sql exception", ex);
                throw;
            }
            catch (Exception ex)
            {
                Logger.Current.Error("An error occurred in Import Data Stored Proc Insertion", ex);
                throw;
            }
        }

        /// <summary>
        /// Gets the importdata setting.
        /// </summary>
        /// <param name="uniqueidentifier">The uniqueidentifier.</param>
        /// <returns></returns>
        public ImportDataSettings getImportdataSetting(string uniqueidentifier)
        {
            var db = ObjectContextFactory.Create();
            ImportDataSettingsDb importdatasettings = db.ImpoortDataSettings.Where(c => c.UniqueImportIdentifier.ToString() == uniqueidentifier).FirstOrDefault();
            if (importdatasettings != null)
                return Mapper.Map<ImportDataSettingsDb, ImportDataSettings>(importdatasettings);
            return null;
        }

        /// <summary>
        /// Gets the import for account identifier.
        /// </summary>
        /// <param name="AccountID">The account identifier.</param>
        /// <returns></returns>
        public LeadAdapterAndAccountMap GetImportForAccountID(int AccountID)
        {
            var db = ObjectContextFactory.Create();
            LeadAdapterAndAccountMapDb leadadapterforaccountid = db.LeadAdapters.FirstOrDefault(c => c.AccountID == AccountID && c.LeadAdapterTypeID == 11);

            if (leadadapterforaccountid != null)
                return Mapper.Map<LeadAdapterAndAccountMapDb, LeadAdapterAndAccountMap>(leadadapterforaccountid);
            return null;
        }

        /// <summary>
        /// Owners the i dfor imported contacts.
        /// </summary>
        /// <param name="AccountID">The account identifier.</param>
        /// <returns></returns>
        public IEnumerable<int> OwnerIDforImportedContacts(int AccountID)
        {
            var db = ObjectContextFactory.Create();
            short AccountAdminRoleID = db.RoleModules.Include(c => c.Role).Where(i => i.ModuleID == 29).Select(i => new
            {
                RoleID = i.RoleID,
                AccountId = i.Role.AccountID
            }).Where(i => i.AccountId == AccountID).Select(i => i.RoleID).FirstOrDefault();
            return db.Users.Where(g => g.RoleID == AccountAdminRoleID && g.IsDeleted == false && g.Status == Status.Active).Select(x => x.UserID);
        }

        /// <summary>
        /// Updates the leadadapters file status.
        /// </summary>
        /// <param name="JobLogID">The job log identifier.</param>
        /// <param name="UserIDs">The user i ds.</param>
        /// <param name="AccountID">The account identifier.</param>
        /// <param name="storageFileName">Name of the storage file.</param>
        /// <param name="errorstatus">The errorstatus.</param>
        /// <param name="LeadAdapterID">The lead adapter identifier.</param>
        public void UpdateLeadadaptersFileStatus(int JobLogID, IEnumerable<int> UserIDs, int AccountID, string storageFileName, LeadAdapterErrorStatus errorstatus, int LeadAdapterID)
        {
            var db = ObjectContextFactory.Create();
            LeadAdapterJobLogsDb jobLog = db.LeadAdapterJobLogs.Include(x => x.LeadAdapter).FirstOrDefault(i => i.LeadAdapterJobLogID == JobLogID);
            jobLog.LeadAdapterJobStatusID = LeadAdapterJobStatus.Completed;
            jobLog.StorageName = storageFileName;

            db.Entry(jobLog).State = System.Data.Entity.EntityState.Modified;

            var filename = jobLog.FileName;
            if (!string.IsNullOrEmpty(filename))
                filename = Path.GetFileName(filename);

            foreach (int UserID in UserIDs)
            {
                NotificationDb notification = new NotificationDb()
                {
                    EntityID = JobLogID,
                    Subject = "A file " + filename + " has been successfully completed",
                    Details = "A file " + filename + " has been successfully completed",
                    NotificationTime = DateTime.Now.ToUniversalTime(),
                    Status = NotificationStatus.New,
                    UserID = UserID,
                    ModuleID = jobLog.LeadAdapter.LeadAdapterTypeID == (byte)LeadAdapterTypes.Import ? (byte)AppModules.ImportData : (byte)AppModules.LeadAdapter
                };
                db.Notifications.Add(notification);
                db.Entry(notification).State = System.Data.Entity.EntityState.Added;
            }

            LeadAdapterServiceStatus? ServiceStatus = default(LeadAdapterServiceStatus?);
            if (errorstatus == LeadAdapterErrorStatus.Error)
                ServiceStatus = LeadAdapterServiceStatus.AllContactsFailed;
            else if (errorstatus == LeadAdapterErrorStatus.Running)
                ServiceStatus = LeadAdapterServiceStatus.AllContactsInserted;
            else if (errorstatus == LeadAdapterErrorStatus.Warning)
                ServiceStatus = LeadAdapterServiceStatus.PartialContactsInserted;

            LeadAdapterAndAccountMapDb leadadapter = db.LeadAdapters.FirstOrDefault(i => i.LeadAdapterAndAccountMapId == LeadAdapterID);
            leadadapter.LeadAdapterErrorStatusID = errorstatus;
            leadadapter.LeadAdapterServiceStatusID = (short)ServiceStatus;
            leadadapter.LastProcessed = DateTime.Now.ToUniversalTime();
            db.Entry(leadadapter).State = System.Data.Entity.EntityState.Modified;

            db.SaveChanges();
        }

        /// <summary>
        /// Inserts the import tags.
        /// </summary>
        /// <param name="JobLogID">The job log identifier.</param>
        /// <param name="Tags">The tags.</param>
        public void InsertImportTags(int JobLogID, IEnumerable<Tag> Tags)
        {
            var db = ObjectContextFactory.Create();
            foreach (Tag tag in Tags)
            {
                if (tag.Id == 0)
                {
                    var tagDb = db.Tags.SingleOrDefault(t => t.TagName.Equals(tag.TagName) && t.AccountID.Equals(tag.AccountID) && t.IsDeleted != true);
                    if (tagDb == null)
                    {
                        tagDb = Mapper.Map<Tag, TagsDb>(tag);
                        tagDb.IsDeleted = false;
                        db.Tags.Add(tagDb);
                    }
                    var importTag = new ImportTagMapDb()
                    {
                        LeadAdapterJobLogID = JobLogID,
                        Tag = tagDb
                    };

                    db.ImportTagMap.Add(importTag);
                }
                else
                {
                    db.ImportTagMap.Add(new ImportTagMapDb() { LeadAdapterJobLogID = JobLogID, TagID = tag.Id });
                    db.RefreshAnalytics.Add(new RefreshAnalyticsDb() { EntityID = tag.Id, EntityType = 5, Status = 1, LastModifiedOn = DateTime.Now.ToUniversalTime() });
                }
            }
            db.SaveChanges();
        }

        /// <summary>
        /// Gets the import tags.
        /// </summary>
        /// <param name="JobID">The job identifier.</param>
        /// <returns></returns>
        public List<int> GetImportTags(DateTime lastModifiedDate)
        {
            var db = ObjectContextFactory.Create();
            return db.LeadAdapterJobLogs.Where(p => p.CreatedDateTime > lastModifiedDate).Join(db.ImportTagMap, k => k.LeadAdapterJobLogID, i => i.LeadAdapterJobLogID, (k, i) => new { i.TagID }).Select(p => p.TagID).ToList();
        }

        /// <summary>
        /// Gets the lead adapter tags.
        /// </summary>
        /// <param name="LeadAdapterAndAccountMapID">The lead adapter and account map identifier.</param>
        /// <returns></returns>
        public IEnumerable<int> GetLeadAdapterTags(int LeadAdapterAndAccountMapID)
        {
            var db = ObjectContextFactory.Create();
            return db.LeadAdapterTags.Where(i => i.LeadAdapterID == LeadAdapterAndAccountMapID).Select(x => x.TagID);
        }

        /// <summary>
        /// Gets the Imported Contacts.
        /// </summary>
        /// <param name="lastModifiedDate">last modified date.</param>
        /// <returns></returns>
        public List<int> GetImportedContacts(DateTime lastModifiedDate)
        {
            var db = ObjectContextFactory.Create();
            var ids = db.LeadAdapterJobLogs.Where(p => p.LeadAdapterJobStatusID == LeadAdapterJobStatus.SQLCompleted).
                      Join(db.LeadAdapterJobLogDetails, jl => jl.LeadAdapterJobLogID, jld => jld.LeadAdapterJobLogID, (jl, jld) => new { jld.ReferenceId }).
                      Join(db.Contacts, l => l.ReferenceId, c => c.ReferenceId, (l, c) => new { c.ContactID });
            return ids.Select(p => p.ContactID).ToList();
        }

        /// <summary>
        /// Gets the column mappings.
        /// </summary>
        /// <param name="jobId">The job identifier.</param>
        /// <returns></returns>
        public IEnumerable<ImportColumnMappings> GetColumnMappings(int jobId)
        {
            List<ImportColumnMappings> columns = new List<ImportColumnMappings>();
            if (jobId != 0)
            {
                var db = ObjectContextFactory.Create();
                var dbObjects = db.ImportColumnMappigns.Where(w => w.JobID == jobId).ToList();
                return Mapper.Map<List<ImportColumnMappingsDb>, List<ImportColumnMappings>>(dbObjects);
            }
            else
                return null;
        }

        /// <summary>
        /// Inserts the column mappings.
        /// </summary>
        /// <param name="columnMappings">The column mappings.</param>
        public void InsertColumnMappings(IEnumerable<ImportColumnMappings> columnMappings)
        {
            if (columnMappings != null && columnMappings.Any())
            {
                var db = ObjectContextFactory.Create();
                IEnumerable<ImportColumnMappingsDb> dbObjects = mapColumnFields(columnMappings);
                db.ImportColumnMappigns.AddRange(dbObjects);
                int count = db.SaveChanges();
            }
        }

        /// <summary>
        /// Maps the column fields.
        /// </summary>
        /// <param name="mappings">The mappings.</param>
        /// <returns></returns>
        private IEnumerable<ImportColumnMappingsDb> mapColumnFields(IEnumerable<ImportColumnMappings> mappings)
        {
            List<ImportColumnMappingsDb> dbobjects = new List<ImportColumnMappingsDb>();
            if (mappings != null && mappings.Any())
            {
                foreach (var map in mappings)
                {
                    ImportColumnMappingsDb obj = new ImportColumnMappingsDb() { ContactFieldName = map.ContactFieldName, ImportColumnMappingID = map.Id, IsCustomField = map.IsCustomField,
                        IsDropDownField = map.IsDropDownField, JobID = map.JobID, SheetColumnName = map.SheetColumnName };
                    dbobjects.Add(obj);
                }
            }
            return dbobjects;
        }

        public void UpdateLeadAdapterJobLogsWithProcessedFileName(int jobId, string processedFileName)
        {
            var db = ObjectContextFactory.Create();
            var sql = @"UPDATE LeadAdapterJobLogs SET ProcessedFileName=@processedFile where LeadAdapterJobLogID=@jobId";
            db.Execute(sql, new { processedFile= processedFileName , jobId = jobId });
        }

        public void InsertNeverBounceRequest(int accountId, int createdBy, byte entityType, List<int> entityIds, int totalCount)
        {
            using (var db = ObjectContextFactory.Create())
            {
                NeverBounceRequestDb dbObject = new NeverBounceRequestDb();
                dbObject.AccountID = accountId;
                dbObject.ServiceStatus = entityType == (byte)NeverBounceEntityTypes.Imports ? (Int16)NeverBounceStatus.LeadAdapterQueued : (Int16)NeverBounceStatus.Queued;
                dbObject.CreatedBy = createdBy;
                dbObject.CreatedOn = DateTime.UtcNow;
                dbObject.EmailsCount = totalCount;
                db.NeverBounceRequests.Add(dbObject);
                db.SaveChanges();

                int neverBounceRequestID = dbObject.NeverBounceRequestID;
                var newDb = ObjectContextFactory.Create();
                if(entityType != 0 && entityIds != null && entityIds.Any())
                {
                    entityIds.ForEach(f => 
                    {
                        NeverBounceMappingsDb mapping = new NeverBounceMappingsDb();
                        mapping.NeverBounceRequestID = neverBounceRequestID;
                        mapping.NeverBounceEntityType = entityType;
                        mapping.EntityID = f;
                        newDb.NeverBounceMappings.Add(mapping);
                    });
                    newDb.SaveChanges();
                }
            }
        }

        public void UpdateRequest(int Id, int userId, NeverBounceStatus status)
        {
            using (var db = ObjectContextFactory.Create())
            {
                var request = db.NeverBounceRequests.Where(w => w.NeverBounceRequestID == Id).FirstOrDefault();
                if (request != null)
                {
                    request.ServiceStatus = (short)status;
                    request.ReviewedOn = DateTime.UtcNow;
                    request.ReviewedBy = userId;
                }
                db.SaveChanges();
            }
        }

        public IEnumerable<NeverBounceQueue> GetNeverBounceRequests(int accountId,int pazeNumber,int pageSize)
        { 
            IEnumerable<NeverBounceQueue> queue = new List<NeverBounceQueue>();
            using (var db = ObjectContextFactory.Create())
            {
                var skip = (pazeNumber - 1) * pageSize;
                var take = pageSize;

                db.QueryStoredProc("GET_NeverBounce_Requests", (r) => 
                {
                    queue = r.Read<NeverBounceQueue>().ToList();
                }, new { AccountID = accountId, Skip = skip, Take = take });

                if (queue != null && queue.Any())
                {
                    foreach (var item in queue)
                    {
                        item.EntityName = item.EntityType == 1 ? "Import" : item.EntityType == 2 ? "Tag" : item.EntityType == 3 ? "Saved Search" : "";
                        item.StatusName = item.Status == 901 ? "Queued" : item.Status == 902 ? "Accepted" : item.Status == 903 ? "Rejected" : item.Status == 904 ? "CSV Generated" : item.Status == 905 ? "Polling" : item.Status == 906 ? "Polling Completed" :
                            item.Status == 907 ? "Processed" : "Failed";
                    }
                }
            }
            return queue;
        }

        public IEnumerable<NeverBounceRequest> GetNeverBounceAcceptedRequests(NeverBounceStatus status)
        { 
            IEnumerable<NeverBounceRequest> requests = new List<NeverBounceRequest>();
            using( var db = ObjectContextFactory.Create())
            {
                var sql = @"SELECT NBR.*, STUFF((SELECT distinct ', ' + CAST(NBM.EntityID AS nvarchar)
                             FROM NeverBounceMappings (NOLOCK) NBM
                             WHERE NBM.NeverBounceRequestID = NBR.NeverBounceRequestID
                                FOR XML PATH(''), TYPE
                                ).value('.', 'NVARCHAR(MAX)') 
                            ,1,2,'') EntityIds, (SELECT TOP 1 NeverBounceEntityType FROM NeverBounceMappings NB WHERE NB.NeverBounceRequestID = NBR.NeverBounceRequestID) AS EntityType 
                    FROM NeverBounceRequests (NOLOCK) NBR 
                    WHERE ServiceStatus = @status and ReviewedBy IS NOT NULL AND ReviewedOn IS NOT NULL ";

                if (status == NeverBounceStatus.CSVGenerated)
                    sql = sql + "AND ScheduledPollingTime < GETUTCDATE()";
                requests = db.Get<NeverBounceRequest>(sql, new { status = (short)status});
            }
            return requests;
        }

        public IEnumerable<ReportContact> GetContactEmails(NeverBounceEntityTypes entityType, string entityIds)
        { 
            IEnumerable<ReportContact> Data = new List<ReportContact>();
            using (var db = ObjectContextFactory.Create())
            {
                var sql = string.Empty;
                if (entityType == NeverBounceEntityTypes.Imports)
                    sql = @"SELECT ICD.ContactID, CE.Email AS email, CE.ContactEmailID FROM ImportContactData (NOLOCK) ICD
                            JOIN ContactEmails (NOLOCK) CE ON CE.ContactID = ICD.ContactID
                            WHERE JobID =  @jobId AND ValidEmail = 1 AND IsDuplicate = 0 AND CE.IsDeleted = 0 AND 
                            CE.IsPrimary = 1 AND CE.Email IS NOT NULL";

                else if (entityType == NeverBounceEntityTypes.Tags)
                    sql = @"SELECT DISTINCT CE.ContactID, CE.Email AS email, CE.ContactEmailID FROM ContactTagMap (NOLOCK) CTM
		                    JOIN Contacts (NOLOCK) C ON C.ContactID = CTM.ContactID
		                    JOIN ContactEmails (NOLOCK) CE ON CE.ContactID = C.ContactID
		                    WHERE CTM.TagID IN (SELECT DATAVALUE FROM dbo.Split(@EntityIDS,',')) AND CE.IsDeleted = 0
		                    AND CE.IsPrimary = 1 AND CE.Email IS NOT NULL 
		                    AND CE.Email != '' AND C.IsDeleted=0";
                else if (entityType == NeverBounceEntityTypes.SavedSearches)
                    sql = "";

                Data = db.Get<ReportContact>(sql, new { JobId = entityIds, EntityIDS = entityIds });
            }
            return Data;
        }

        public void UpdateNeverBounceRequest(NeverBounceRequest request)
        {
            if (request != null)
            {
                Logger.Current.Informational("Request received to update NeverBounce requests");
                using (var db = ObjectContextFactory.Create())
                {
                    NeverBounceRequestDb dbObject = db.NeverBounceRequests.Where(w => w.NeverBounceRequestID == request.NeverBounceRequestID).FirstOrDefault();
                    if (dbObject != null)
                    {
                        dbObject.ServiceStatus = (short)request.ServiceStatus;
                        dbObject.Remarks = request.Remarks;
                        dbObject.ScheduledPollingTime = request.ScheduledPollingTime;
                        dbObject.EmailsCount = request.EmailsCount;
                        dbObject.NeverBounceJobID = request.NeverBounceJobID;
                    }
                    db.Entry<NeverBounceRequestDb>(dbObject).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    Logger.Current.Informational("NeverBounce requests updated successfully");
                }
            }
        }

        public void UpdateNeverBouncePollingResponse(NeverBounceRequest request)
        {
            if (request != null)
            {
                Logger.Current.Informational("Request received for updating NeverBounce polling response");
                using (var db = ObjectContextFactory.Create())
                {
                    NeverBounceRequestDb dbObject = db.NeverBounceRequests.Where(w => w.NeverBounceRequestID == request.NeverBounceRequestID).FirstOrDefault();
                    if (dbObject != null)
                    {
                        dbObject.PollingRemarks = request.PollingRemarks;
                        dbObject.PollingStatus = request.PollingStatus;
                        dbObject.ScheduledPollingTime = request.ScheduledPollingTime;
                        dbObject.ServiceStatus = (short)request.ServiceStatus;
                    }
                    db.Entry<NeverBounceRequestDb>(dbObject).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }
            }
        }

        public void InsertNeverBounceResults(IEnumerable<NeverBounceResult> results)
        {
            Logger.Current.Informational("Request received for updating contact emails from NeverBounce processor");
            if (results != null && results.Any())
            {
                Logger.Current.Informational("No of records for update : " + results.Count());
                using (var db = ObjectContextFactory.Create())
                {
                    List<NeverBounceEmailStatusDb> dbObjects = Mapper.Map<List<NeverBounceResult>, List<NeverBounceEmailStatusDb>>(results.ToList());
                    db.BulkInsert<NeverBounceEmailStatusDb>(dbObjects);
                    db.SaveChanges();
                    Logger.Current.Informational("successfully inserted bulk data");
                }
            }
        }

        public NeverBounceEmailData GetEmailData(int neverBounceRequestID)
        {
            NeverBounceEmailData data = new NeverBounceEmailData();
            if (neverBounceRequestID != 0)
            {
                using (var db = ObjectContextFactory.Create())
                {
                    db.QueryStoredProc("[dbo].[GetNeverBounceSendMailData]", (reader) =>
                    {
                        data = reader.Read<NeverBounceEmailData>().FirstOrDefault();
                    }, new { NeverBounceRequestId = neverBounceRequestID });
                    
                }
            }
            return data;
        }
    }
}

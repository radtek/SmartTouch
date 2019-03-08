using AutoMapper;
using Dapper;
using LandmarkIT.Enterprise.Extensions;
using LinqKit;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.Reports;
using SmartTouch.CRM.Domain.Users;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using SmartTouch.CRM.Repository.Database;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;

namespace SmartTouch.CRM.Repository.Repositories
{
    public class ReportRepository : Repository<Report, int, ReportsDb>, IReportRepository
    {
        public ReportRepository(IUnitOfWork unitOfWork, IObjectContextFactory objectContextFactory)
            : base(unitOfWork, objectContextFactory)
        { }

        /// <summary>
        /// Finds all.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="accountId">The account identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="pageSize">Size of the page.</param>
        /// <param name="totalHits">The total hits.</param>
        /// <param name="sortField">The sort field.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="moduleIds">The module ids.</param>
        /// <param name="direction">The direction.</param>
        /// <returns></returns>
        public IEnumerable<Report> FindAll(string name, int accountId, int userId, int pageNumber, int pageSize, out int totalHits
            , string sortField, string filter, IEnumerable<byte> moduleIds, ListSortDirection direction = ListSortDirection.Descending)
        {
            var db = ObjectContextFactory.Create();
            if (pageNumber == 0) pageNumber = 1;
            var recordsLimit = pageSize;
            var records = (pageNumber - 1) * recordsLimit;
            IEnumerable<Report> reports = null;
            var modules = string.Join(",", moduleIds.ToArray());
            db.QueryStoredProc("[dbo].[GetEntitledReports]", (reader) =>
            {
                reports = reader.Read<Report>().ToList();
            }, new { AccountID = accountId, Modules = modules, UserID = userId });

            if(filter == "1")
                reports = reports.Where(r => r.ReportType == Reports.Custom).ToArray();
            else
                reports = reports.Where(r => r.ReportType != Reports.Custom).ToArray();

            if (!string.IsNullOrEmpty(name))
            {
                reports = reports.Where(r => r.ReportName.ToLower().Contains(name.ToLower()));
            }
            if (!string.IsNullOrEmpty(sortField))
            {
                if(sortField == "ReportName")
                {
                    if (direction == ListSortDirection.Descending)
                        reports = reports.OrderByDescending(r => r.ReportName);
                    else
                        reports = reports.OrderBy(r => r.ReportName);
                }
                else
                {
                    if (direction == ListSortDirection.Descending)
                        reports = reports.OrderByDescending(r => r.LastRunOn);
                    else
                        reports = reports.OrderBy(r => r.LastRunOn);
                }
            }

            totalHits = reports.Count();
            var finalreports = reports.Skip(records).Take(recordsLimit).ToList();
            return finalreports;

        }

        /// <summary>
        /// Finds all custom reports.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="accountId">The account identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="pageSize">Size of the page.</param>
        /// <param name="totalHits">The total hits.</param>
        /// <param name="sortField">The sort field.</param>
        /// <param name="direction">The direction.</param>
        /// <returns></returns>
        public IEnumerable<Report> FindAllCustomReports(string name, int accountId, int userId, int pageNumber, int pageSize, out int totalHits, string sortField, ListSortDirection direction = ListSortDirection.Descending)
        {
            var db = ObjectContextFactory.Create();
            if (pageNumber == 0)
                pageNumber = 1;
            var recordsLimit = pageSize;
            var records = (pageNumber - 1) * recordsLimit;
            var predicate = PredicateBuilder.True<AccountCustomReportsDb>();
            if (!string.IsNullOrEmpty(name))
            {
                name = name.ToLower();
                predicate = predicate.And(a => a.ReportName.Contains(name));
            }
            predicate = predicate.And(a => a.AccountID == accountId);


            var reports = db.AccountCustomReports.AsExpandable().Where(predicate)
                            .GroupJoin(db.UserActivitiesLog.Where(u => u.UserID == userId && u.AccountID == accountId && u.ModuleID == (byte)AppModules.Reports && u.UserActivityID == (byte)UserActivityType.LastRunOn),
                            r => r.ReportID, ul => ul.EntityID,
                            (r, ul) => new { Report = r, Log = ul.DefaultIfEmpty() })
                            .Select(g => new Report()
                            {
                                Id = g.Report.ReportID,
                                ReportName = g.Report.ReportName,
                                LastRunOn = g.Log.OrderByDescending(l => l.LogDate).FirstOrDefault().LogDate
                            }).OrderBy(sortField, direction);

            totalHits = reports.Count();
            var finalreports = reports.Skip(records).Take(recordsLimit).ToList();

            return finalreports;
        }

        /// <summary>
        /// Gets the custom reports.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        public bool GetCustomReports(int accountId)
        {
            var db = ObjectContextFactory.Create();
            bool hasCustomReports = db.AccountCustomReports.Where(p => p.AccountID == accountId && p.IsDeleted == false).Count() > 0;
            return hasCustomReports;
        }

        /// <summary>
        /// Finds the reports summary.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <param name="sortField">The sort field.</param>
        /// <param name="direction">The direction.</param>
        /// <returns></returns>
        public IEnumerable<ReportsDb> FindReportsSummary(Expression<Func<ReportsDb, bool>> predicate, string sortField, ListSortDirection direction)
        {
            return ObjectContextFactory.Create().Reports.OrderBy(sortField, direction)
                .AsExpandable()
                .Where(predicate).ToList();
        }

        /// <summary>
        /// Finds all.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public IEnumerable<Report> FindAll()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Finds the type of the report by.
        /// </summary>
        /// <param name="ReportType">Type of the report.</param>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        public Report FindReportByType(Reports ReportType, int accountId)
        {
            using(var db = ObjectContextFactory.Create())
            {
                var sql = @"SELECT R.* FROM Reports (NOLOCK) R WHERE R.AccountID=@AccountId AND R.ReportType=@ReportTypeId";
                var reportsDb = db.Get<ReportsDb>(sql,new { AccountId= accountId , ReportTypeId = ReportType }).FirstOrDefault();
                return Mapper.Map<ReportsDb, Report>(reportsDb);

            }
        }

        /// <summary>
        /// Gets the new leads visuvalization.
        /// </summary>
        /// <param name="filters">The filters.</param>
        /// <returns></returns>
        [Obsolete("This method uses storedprocedure", true)]
        public ReportResult GetNewLeadsVisuvalization(ReportFilters filters)
        {
            var procedureName = "[dbo].[GET_Account_NewLeads_AreaChart_V1]";
            var outputParam = new SqlParameter { ParameterName = "@PreviousValue", SqlDbType = System.Data.SqlDbType.Int, Direction = System.Data.ParameterDirection.Output };
            var parms = new List<SqlParameter>
				{   
					new SqlParameter{ParameterName="@AccountID", Value= filters.AccountId},
					new SqlParameter{ParameterName="@Top5Only", Value= filters.Top5Only },
					new SqlParameter{ParameterName="@IsAdmin", Value= filters.IsAdmin},
					 new SqlParameter{ParameterName="@DateRange", Value= filters.DateRange },
					new SqlParameter{ParameterName="@FromDate", Value= filters.StartDate.Date},
					new SqlParameter{ParameterName="@ToDate", Value= filters.EndDate.Date},
					new SqlParameter{ParameterName="@OwnerIDs", Value =string.Join(",",filters.SelectedOwners)},                 
					new SqlParameter{ParameterName="@StartDatePrev", Value = filters.StartDatePrev},
					new SqlParameter{ParameterName="@EndDatePrev", Value = filters.EndDatePrev},
					 outputParam
				};
            return getStoredProcedureData(procedureName, parms, filters.IsComparedTo);
        }

        /// <summary>
        /// Gets the first lead source report.
        /// </summary>
        /// <param name="filters">The filters.</param>
        /// <returns></returns>
        public ReportResult GetFirstLeadSourceReport(ReportFilters filters)
        {
            var procName = "[dbo].[Get_Account_LeadSource_Report]";
            string entities = string.Empty;
            if (filters.Type == 1)
                entities = string.Join(",", filters.SelectedOwners);
            else if (filters.Type == 2)
                entities = string.Join(",", filters.TrafficLifeCycle);
            else if (filters.Type == 3)
                entities = string.Join(",", filters.SelectedCommunities);

            DateTime endDate = filters.EndDate.Date.AddDays(1).AddMinutes(-1);

            var parms = new List<SqlParameter>
				{   
					new SqlParameter{ParameterName="@AccountID", Value= filters.AccountId},
					new SqlParameter{ParameterName="@GroupType", Value= filters.Type },
					new SqlParameter{ParameterName="@DateRange", Value= filters.DateRange },
					new SqlParameter{ParameterName="@StartDate", Value= filters.StartDate.Date},
					new SqlParameter{ParameterName="@EndDate", Value= endDate},
					new SqlParameter{ParameterName="@Entities", Value = entities },                 
				};

            var db = ObjectContextFactory.Create();

            db.Database.Connection.Open();
            var cmd = db.Database.Connection.CreateCommand();
            cmd.CommandText = procName;
            cmd.CommandType = CommandType.StoredProcedure;
            parms.ForEach(p =>
            {
                cmd.Parameters.Add(p);
            });

            DbDataReader reader = cmd.ExecuteReader();
            List<AreaChartData> areaChartData = null;
            List<PieChartData> pieChartData = null;
            var dbReader = (DbDataReader)reader.MapTo<AreaChartData>(out areaChartData).MoveNext().MapTo<PieChartData>(out pieChartData).MoveNext();
            IQueryable<ReportData> result = null;

            Func<DbDataRecord, IEnumerable<DropDownValue>> GetLeadSources = (row) =>
            {
                List<DropDownValue> drpdwnvalues = new List<DropDownValue>();
                Random random = new Random();
                for (int index = 1; index < row.FieldCount; index++)
                {
                    DropDownValue drpdwn = new DropDownValue();
                    var val = row.GetValue(index);
                    var text = row.GetName(index);
                    drpdwn.DropdownValueName = text;
                    drpdwn.DropdownValue = DBNull.Value.Equals(val) ? 0 : (int)val;
                    drpdwnvalues.Add(drpdwn);
                }
                return drpdwnvalues;
            };

            result = dbReader.Cast<DbDataRecord>()
                  .Select(r => new ReportData()
                  {
                      ID = (int)(r["Id"]),
                      DropdownValues = GetLeadSources(r)
                  }).AsQueryable();

            ReportResult procdata = new ReportResult() { GridData = result, AreaChartData = areaChartData, PieChartData = pieChartData };
            return procdata;
        }

        /// <summary>
        /// Gets the first l ead source report contacts.
        /// </summary>
        /// <param name="filters">The filters.</param>
        /// <returns></returns>
        public IEnumerable<ReportContact> GetFirstLEadSourceReportContacts(ReportFilters filters)
        {
            var procedureName = "[dbo].[Get_Account_LeadSource_Report_ContactInfo]";
            String entities = string.Empty;
            if (filters.Type == 1)
                entities = string.Join(",", filters.SelectedOwners);
            else if (filters.Type == 2)
                entities = string.Join(",", filters.TrafficLifeCycle);
            else if (filters.Type == 3)
                entities = string.Join(",", filters.SelectedCommunities);

            DateTime endDate = filters.EndDate.Date.AddDays(1).AddMinutes(-1);

            var parms = new List<SqlParameter>
				{   
					new SqlParameter{ParameterName="@AccountID", Value= filters.AccountId},
					new SqlParameter{ParameterName="@GroupType", Value= filters.Type },
					new SqlParameter{ParameterName="@StartDate", Value= filters.StartDate.Date},
					new SqlParameter{ParameterName="@EndDate", Value= endDate},
					new SqlParameter{ParameterName="@Entities", Value = entities },             
                    new SqlParameter{ParameterName="@LeadSourceID",Value = filters.TrafficSource != null && filters.TrafficSource.IsAny() ? string.Join(",", filters.TrafficSource) : string.Empty }
				};
            var db = ObjectContextFactory.Create();
            db.Database.Connection.Open();
            var cmd = db.Database.Connection.CreateCommand();
            cmd.CommandText = procedureName;
            cmd.CommandType = CommandType.StoredProcedure;
            parms.ForEach(p =>
            {
                cmd.Parameters.Add(p);
            });
            List<ReportContact> contactReport = new List<ReportContact>();
            DbDataReader reader = cmd.ExecuteReader();
            var dbReader = (DbDataReader)reader.MapTo<ReportContact>(out contactReport);
            return contactReport;
        }

        /// <summary>
        /// Gets all lead source report.
        /// </summary>
        /// <param name="filters">The filters.</param>
        /// <returns></returns>
        public ReportResult GetAllLeadSourceReport(ReportFilters filters)
        {
            var procName = "[dbo].[Get_Account_All_LeadSource_Report]";
            string entities = string.Empty;
            if (filters.Type == 1)
                entities = string.Join(",", filters.TrafficSource);
            else if (filters.Type == 2)
                entities = string.Join(",", filters.TrafficLifeCycle);
            else if (filters.Type == 3)
                entities = string.Join(",", filters.SelectedCommunities);

            DateTime endDate = filters.EndDate.Date.AddDays(1).AddMinutes(-1);

            var parms = new List<SqlParameter>
                {   
                    new SqlParameter{ParameterName="@AccountID", Value= filters.AccountId},
                    new SqlParameter{ParameterName="@FilterType", Value= filters.Type },
                    new SqlParameter{ParameterName="@DateRange", Value= filters.DateRange },
                    new SqlParameter{ParameterName="@StartDate", Value= filters.StartDate.Date},
                    new SqlParameter{ParameterName="@EndDate", Value= endDate},
                    new SqlParameter{ParameterName="@Entities", Value = entities },                 
                };

            var db = ObjectContextFactory.Create();

            db.Database.Connection.Open();
            var cmd = db.Database.Connection.CreateCommand();
            cmd.CommandText = procName;
            cmd.CommandType = CommandType.StoredProcedure;
            parms.ForEach(p =>
            {
                cmd.Parameters.Add(p);
            });

            DbDataReader reader = cmd.ExecuteReader();
            List<AreaChartData> areaChartData = null;
            List<PieChartData> pieChartData = null;
            List<AllLeadSourceReportGrid> grid = null;
            var dbReader = (DbDataReader)reader.MapTo<AreaChartData>(out areaChartData).MoveNext().MapTo<PieChartData>(out pieChartData).MoveNext().MapTo<AllLeadSourceReportGrid>(out grid);
            ReportResult procdata = new ReportResult() { AllLeadSourceData = grid, AreaChartData = areaChartData, PieChartData = pieChartData };
            return procdata;
        }

        /// <summary>
        /// Gets all lead source report contacts.
        /// </summary>
        /// <param name="filters">The filters.</param>
        /// <returns></returns>
        public IEnumerable<ReportContact> GetAllLeadSourceReportContacts(ReportFilters filters)
        {
            var procName = "[dbo].[Get_Account_All_LeadSource_Report_ContactInfo]";
            string entities = string.Empty;
            if (filters.Type == 1)
                entities = string.Join(",", filters.TrafficSource);
            else if (filters.Type == 2)
                entities = string.Join(",", filters.TrafficLifeCycle);
            else if (filters.Type == 3)
                entities = string.Join(",", filters.SelectedCommunities);

            DateTime endDate = filters.EndDate.Date.AddDays(1).AddMinutes(-1);

            var parms = new List<SqlParameter>
                {   
                    new SqlParameter{ParameterName="@AccountID", Value= filters.AccountId},
                    new SqlParameter{ParameterName="@ColIndex", Value= filters.ColumnIndex },
                    new SqlParameter{ParameterName="@GroupType", Value= filters.Type },
                    new SqlParameter{ParameterName="@StartDate", Value= filters.StartDate.Date},
                    new SqlParameter{ParameterName="@EndDate", Value= endDate},
                    new SqlParameter{ParameterName="@Entities", Value = entities },                 
                    new SqlParameter{ParameterName="@LeadSourceID", Value = filters.TrafficSource != null && filters.TrafficSource.IsAny() ? string.Join(",", filters.TrafficSource) : string.Empty }
                };

            var db = ObjectContextFactory.Create();

            db.Database.Connection.Open();
            var cmd = db.Database.Connection.CreateCommand();
            cmd.CommandText = procName;
            cmd.CommandType = CommandType.StoredProcedure;
            parms.ForEach(p =>
            {
                cmd.Parameters.Add(p);
            });

            List<ReportContact> contactReport = new List<ReportContact>();
            DbDataReader reader = cmd.ExecuteReader();
            var dbReader = (DbDataReader)reader.MapTo<ReportContact>(out contactReport);
            return contactReport;
        }

        /// <summary>
        /// Gets the traffic by source.
        /// </summary>
        /// <param name="filters">The filters.</param>
        /// <returns></returns>
        public ReportResult GetTrafficBySource(ReportFilters filters)
        {
            var procedureName = "[dbo].[GET_Account_Traffic_LeadSource_v2]";


            var parms = new List<SqlParameter>
				{   
					new SqlParameter{ParameterName="@AccountID", Value= filters.AccountId},
					new SqlParameter{ParameterName="@Type", Value= filters.Type },
					new SqlParameter{ParameterName="@Top5Only", Value= filters.Top5Only },
					new SqlParameter{ParameterName="@DateRange", Value= filters.DateRange },
					new SqlParameter{ParameterName="@StartDate", Value= filters.StartDate.Date},
					new SqlParameter{ParameterName="@EndDate", Value= filters.EndDate.Date},
					new SqlParameter{ParameterName="@SelectedList", Value = filters.Type == 0?string.Join(",",filters.SelectedOwners):string.Join(",",filters.SelectedCommunities)},                 
					new SqlParameter{ParameterName="@TrafficSource", Value = string.Join(",",filters.TrafficSource)},
					new SqlParameter{ParameterName="@StartDatePrev", Value = filters.StartDatePrev},
					new SqlParameter{ParameterName="@EndDatePrev", Value = filters.EndDatePrev}
							
				};


            return getStoredProcedureData(procedureName, parms, filters.IsComparedTo);
        }

        /// <summary>
        /// Gets the traffic by lifecycle.
        /// </summary>
        /// <param name="filters">The filters.</param>
        /// <returns></returns>
        public ReportResult GetTrafficByLifecycle(ReportFilters filters)
        {
            var procedureName = "[dbo].[GET_Account_Traffic_LifeCycle_v2]";

            var parms = new List<SqlParameter>
				{   
					new SqlParameter{ParameterName="@AccountID", Value= filters.AccountId},
					new SqlParameter{ParameterName="@Type", Value= filters.Type },
					new SqlParameter{ParameterName="@Top5Only", Value= filters.Top5Only },
					new SqlParameter{ParameterName="@DateRange", Value= filters.DateRange },
					new SqlParameter{ParameterName="@StartDate", Value= filters.StartDate.Date},
					new SqlParameter{ParameterName="@EndDate", Value= filters.EndDate.Date},
					new SqlParameter{ParameterName="@SelectedList", Value = filters.Type == 0?string.Join(",",filters.SelectedOwners):string.Join(",",filters.SelectedCommunities)},                 
					new SqlParameter{ParameterName="@TrafficLifeCycle", Value = string.Join(",",filters.TrafficLifeCycle)},
					new SqlParameter{ParameterName="@StartDatePrev", Value = filters.StartDatePrev},
					new SqlParameter{ParameterName="@EndDatePrev", Value = filters.EndDatePrev}
							
				};
            return getStoredProcedureData(procedureName, parms, filters.IsComparedTo);
        }

        /// <summary>
        /// Gets the type of the traffic by.
        /// </summary>
        /// <param name="filters">The filters.</param>
        /// <returns></returns>
        public ReportResult GetTrafficByType(ReportFilters filters)
        {
            var procedureName = "[dbo].[GET_Account_Traffic_TourType_v2]";

            DateTime endDate = filters.EndDate;
            if (filters.StartDate.Date != filters.EndDate.Date)
                endDate = filters.EndDate.Date.AddDays(1);

            var parms = new List<SqlParameter>
				{   
					new SqlParameter{ParameterName="@AccountID", Value= filters.AccountId},
					new SqlParameter{ParameterName="@Type", Value= filters.Type },
					new SqlParameter{ParameterName="@Top5Only", Value= filters.Top5Only },
					new SqlParameter{ParameterName="@DateRange", Value= filters.DateRange },
					new SqlParameter{ParameterName="@StartDate", Value= filters.StartDate.Date},
					new SqlParameter{ParameterName="@EndDate", Value= endDate.Date},
					new SqlParameter{ParameterName="@SelectedList", Value = filters.Type == 0?string.Join(",",filters.SelectedOwners):string.Join(",",filters.SelectedCommunities)},                 
					new SqlParameter{ParameterName="@TrafficType", Value = string.Join(",",filters.TrafficType)},
					new SqlParameter{ParameterName="@StartDatePrev", Value = filters.StartDatePrev},
					new SqlParameter{ParameterName="@EndDatePrev", Value = filters.EndDatePrev}
							
				};
            return getStoredProcedureData(procedureName, parms, filters.IsComparedTo);
        }

        /// <summary>
        /// Gets the opportunity pipeline.
        /// </summary>
        /// <param name="filters">The filters.</param>
        /// <returns></returns>
        public ReportResult GetOpportunityPipeline(ReportFilters filters)
        {
            //ReportResult result = new ReportResult();
            //using (var db = ObjectContextFactory.Create())
            //{
                //    IEnumerable<DashboardPieChartDetails> dropDownValues = null;
                //    IEnumerable<ReportData> reportData = null;
                //    db.QueryStoredProc("[dbo].[GET_Opportunity_Pipeline_Report]", (reader) =>
                //    {
                //        dropDownValues = reader.Read<DashboardPieChartDetails>().ToList();
                //        reportData = reader.Read<ReportData>().ToList();
                //    }, new
                //    {
                //        AccountID = filters.AccountId,
                //        FromDate = filters.StartDate.Date,
                //        ToDate = filters.EndDate.Date,
                //        AccountExecutives = string.Join(",", filters.SelectedOwners),
                //        OpportunityStages = string.Join(",", filters.TrafficLifeCycle),
                //        OwnerID = 0
                //    });
                //    int count = dropDownValues.Count();
                //    Func<IEnumerable<DropDownValue>, IEnumerable<DashboardPieChartDetails>> GetStages = (red) => {
                //        List<DropDownValue> drpdwnvalues = new List<DropDownValue>();
                //        for (int index = 4; index < count; index++)
                //        {
                //            DropDownValue drpdwn = new DropDownValue();
                //            var val = red.GetValue(index);
                //            var text = red.GetName(index);
                //            drpdwn.DropdownValueName = text;
                //            drpdwn.DropdownValue = (int)val;
                //            drpdwnvalues.Add(drpdwn);
                //        }
                //        return drpdwnvalues;
                //    };

                //    reportData.Each(itm => {
                //        itm.DropdownValues = GetStages(dropDownValues);
                //    })
                //    result.GridData = reportData;
                //    result.DashboardPieCharData = dropDownValues;
                //    return result;

                //}
                var procedureName = "[dbo].[GET_Opportunity_Pipeline_Report]";

                var parms = new List<SqlParameter>
                {
                    new SqlParameter{ParameterName="@AccountID", Value= filters.AccountId},
                    new SqlParameter{ParameterName="@FromDate", Value= filters.StartDate.Date },
                    new SqlParameter{ParameterName="@ToDate", Value= filters.EndDate.Date},
                    new SqlParameter{ParameterName="@AccountExecutives", Value= string.Join(",", filters.SelectedOwners)},
                    new SqlParameter{ParameterName="@OpportunityStages", Value = string.Join(",", filters.TrafficLifeCycle)},
                    new SqlParameter{ParameterName="@OwnerID", Value =0  }

                };
                return getStoredProcedureData(procedureName, parms);
            }

        /// <summary>
        /// Gets the activities by module.
        /// </summary>
        /// <param name="filters">The filters.</param>
        /// <returns></returns>
        public ReportResult GetActivitiesByModule(ReportFilters filters)
        {
            var procedureName = "[dbo].[GET_Account_Activity_Report_v1]";

            var parms = new List<SqlParameter>
				{   
					new SqlParameter{ParameterName="@AccountID", Value= filters.AccountId},
					new SqlParameter{ParameterName="@StartDate", Value= filters.StartDate.Date},
					new SqlParameter{ParameterName="@EndDate", Value= filters.EndDate.Date},
					new SqlParameter{ParameterName="@UserIDs", Value =string.Join(",",filters.SelectedOwners)},                 
					new SqlParameter{ParameterName="@ModuleIDs", Value = string.Join(",", filters.ModuleIDs) }                
					
				};
            return getStoredProcedureData(procedureName, parms);
        }

        /// <summary>
        /// Gets the traffic by type and life cycle.
        /// </summary>
        /// <param name="filters">The filters.</param>
        /// <returns></returns>
        public ReportResult GetTrafficByTypeAndLifeCycle(ReportFilters filters)
        {
            var db = ObjectContextFactory.Create();
            Func<IEnumerable<IDictionary<string,object>>,string ,bool,IEnumerable<ReportData>> GetLeadSources = (row,type, campareTo) =>
               {
                   IList<ReportData> actualData = new List<ReportData>();
                   foreach(IDictionary<string,object> d in row)
                   {
                       ReportData rowdata = new ReportData();
                       List<DropDownValue> drpdwnvalues = new List<DropDownValue>();
                       
                       if(campareTo == false)
                       {
                           rowdata.ID = Convert.ToInt32(d["ID"]);
                           rowdata.Name = d["Name"].ToString();
                           rowdata.CurrentTotal = type != "TT" ? Convert.ToInt32(d["Total"]) : 0;
                           foreach (string key in d.Keys)
                           {
                               DropDownValue drpdwn = new DropDownValue();
                               if ((key != "ID" && key != "Name" && key != "Total") || (type == "TT" && key == "Total"))
                               {
                                   drpdwn.DropdownValueName = (type == "TT" && key == "Total") ? "Total Traffic" : key;
                                   drpdwn.DropdownValue = (int)d[key];
                                   drpdwn.DropdownType = type;
                                   drpdwnvalues.Add(drpdwn);
                               }
                           }
                       }
                       else
                       {
                           rowdata.ID = Convert.ToInt32(d["ID"]);
                           rowdata.Name = d["Name"].ToString();
                           rowdata.CurrentTotal = type != "TT" ? Convert.ToInt32(d["Total (c)"]) : 0;
                           rowdata.PreviousTotal = type != "TT" ? Convert.ToInt32(d["Total (p)"]) : 0;
                           foreach (string key in d.Keys)
                           {
                               DropDownValue drpdwn = new DropDownValue();
                               if ((key != "ID" && key != "Name" && key != "Total (c)" && key != "Total (p)") || (type == "TT" && (key == "Total (c)" || key == "Total (p)")))
                               {
                                   drpdwn.DropdownValueName = (type == "TT" && key == "Total (c)") ? "Total Traffic (c)" : (type == "TT" && key == "Total (p)")? "Total Traffic (p)": key;
                                   drpdwn.DropdownValue = (int)d[key];
                                   drpdwn.DropdownType = type;
                                   drpdwnvalues.Add(drpdwn);
                               }
                           }
                       }
                     
                       
                       rowdata.DropdownValues = drpdwnvalues;
                       actualData.Add(rowdata);
                   }
                   return actualData.ToList();
               };
            List<AreaChartData> areaChartData = null;
            List<PieChartData> pieChartData = null;
            IList<dynamic> result = new List<dynamic>();
            IList<dynamic> result1 = new List<dynamic>();


            var minDate = (filters.StartDate.Date == null || filters.StartDate.Date == DateTime.MinValue) ? (DateTime)System.Data.SqlTypes.SqlDateTime.MinValue : filters.StartDate.Date;
            var maxDate = (filters.EndDate.Date == null || filters.EndDate.Date == DateTime.MinValue) ? (DateTime)System.Data.SqlTypes.SqlDateTime.MaxValue : filters.EndDate.Date;
            IEnumerable<ReportData> finalResult = new List<ReportData>();
          
              db.QueryStoredProc("[dbo].[GET_Account_TourType_Lifecycle_v2]", (reader) =>
            {
                areaChartData = reader.Read<AreaChartData>().ToList();
                pieChartData = reader.Read<PieChartData>().ToList();
                result = reader.Read().ToList();
                result1 = reader.Read().ToList(); ;
            }, new
            {
                AccountID = filters.AccountId,
                Type = filters.Type == 1 ? 0 : 1,
                Top5Only = filters.Top5Only,
                DateRange = filters.DateRange,
                StartDate = minDate,
                EndDate = maxDate,
                SelectedList = filters.Type == 1 ? string.Join(",", filters.SelectedOwners) : string.Join(",", filters.SelectedCommunities),
                TrafficLifeCycle = string.Join(",", filters.TrafficLifeCycle),
                TourType = string.Join(",", filters.TrafficType),
                StartDatePrev = filters.StartDatePrev,
                EndDatePrev = filters.EndDatePrev,
            });

            IEnumerable<IDictionary<string, object>> leadsourResult = ConvertToDictionaryObect(result.ToList());
            IEnumerable<IDictionary<string, object>> tourTypeResult = ConvertToDictionaryObect(result1.ToList());
            IEnumerable<ReportData> finalLeadSourceResult= GetLeadSources(leadsourResult, "LC", filters.IsComparedTo);
            IEnumerable<ReportData> finalTourTypeResult = GetLeadSources(tourTypeResult, "TT", filters.IsComparedTo);

            finalTourTypeResult.Each(tl =>
            {
                var leadsources = finalLeadSourceResult.Where(t => t.ID == tl.ID).Select(s => s.DropdownValues).FirstOrDefault();
                tl.DropdownValues = tl.DropdownValues.Concat(leadsources);
                tl.CurrentTotal = finalLeadSourceResult.Where(t => t.ID == tl.ID).Select(s => s.CurrentTotal).FirstOrDefault();
                tl.PreviousTotal = finalLeadSourceResult.Where(t => t.ID == tl.ID).Select(s => s.PreviousTotal).FirstOrDefault();

            });

            return new ReportResult { GridData = finalTourTypeResult, AreaChartData = areaChartData, PieChartData = pieChartData };

        }

        private IEnumerable<IDictionary<string,object>> ConvertToDictionaryObect(List<dynamic> data)
        {
            foreach (dynamic m in data)
            {
                yield return m;
            }
        }

        /// <summary>
        /// Gets the custom report data.
        /// </summary>
        /// <param name="filters">The filters.</param>
        /// <returns></returns>
        public ReportResult GetCustomReportData(ReportFilters filters)
        {
            var procedureName = "[dbo].[GET_Account_CustomReport_Info]";

            var parms = new List<SqlParameter>
				{   
					new SqlParameter{ParameterName="@AccountID", Value= filters.AccountId},
					new SqlParameter{ParameterName="@ReportID", Value= filters.ReportId },
					new SqlParameter{ParameterName="@StartDate", Value= filters.StartDate.Date},
					new SqlParameter{ParameterName="@EndDate", Value= filters.EndDate.Date}									
				};

            return getCustomReportStoredProcedureData(procedureName, parms);
        }

        /// <summary>
        /// Gets the traffic by source contacts.
        /// </summary>
        /// <param name="filters">The filters.</param>
        /// <returns></returns>
        public IEnumerable<ReportContact> GetTrafficBySourceContacts(ReportFilters filters)
        {
            var procedureName = "[dbo].[GET_Account_Traffic_LeadSource_v1]";
            int[] selectedOwners = filters.RowId == 0 ? filters.SelectedOwners : new int[] { filters.RowId };
            int[] selectedCommunities = filters.RowId == 0 ? filters.SelectedCommunities : new int[] { filters.RowId };

            var parms = new List<SqlParameter>
                {
                    new SqlParameter{ParameterName="@AccountID", Value= filters.AccountId},
                    new SqlParameter{ParameterName="@Type", Value= filters.Type },
                    new SqlParameter{ParameterName="@StartDate", Value= filters.StartDate.Date},
                    new SqlParameter{ParameterName="@EndDate", Value=filters.EndDate.Date },
                    new SqlParameter{ParameterName="@SelectedIDs", Value =  filters.Type == 0? string.Join(",",selectedOwners):string.Join(",",selectedCommunities)},
                    new SqlParameter{ParameterName="@TrafficSource", Value = string.Join(",", filters.TrafficSource)},
                    new SqlParameter{ParameterName="@ContactListID", Value = filters.RowId == 0?1:filters.RowId}
                };
            CRMDb context = new CRMDb();
            var objectContext = (context as IObjectContextAdapter).ObjectContext;
            objectContext.CommandTimeout = 400;
            return context.ExecuteStoredProcedure<ReportContact>(procedureName, parms);
        }

        /// <summary>
        /// Gets the opportunity pipeline contacts.
        /// </summary>
        /// <param name="filters">The filters.</param>
        /// <returns></returns>
        public IEnumerable<ReportContact> GetOpportunityPipelineContacts(ReportFilters filters)
        {
            var procedureName = "[dbo].[GET_Opportunity_Pipeline_Report]";

            int[] selectedOwners = filters.RowId == 0 ? filters.SelectedOwners : new int[] { filters.RowId };

            var parms = new List<SqlParameter>
                {
                    new SqlParameter{ParameterName="@AccountID", Value= filters.AccountId},
                    new SqlParameter{ParameterName="@FromDate", Value= filters.StartDate.Date },
                    new SqlParameter{ParameterName="@ToDate", Value= filters.EndDate.Date},
                    new SqlParameter{ParameterName="@AccountExecutives", Value= string.Join(",", selectedOwners)},
                    new SqlParameter{ParameterName="@OpportunityStages", Value = string.Join(",", filters.TrafficLifeCycle)},
                    new SqlParameter{ParameterName="@OwnerID", Value =filters.RowId == 0 ? 1 :filters.RowId }

                };

            CRMDb context = new CRMDb();
            var objectContext = (context as IObjectContextAdapter).ObjectContext;
            objectContext.CommandTimeout = 400;
            return context.ExecuteStoredProcedure<ReportContact>(procedureName, parms);
        }

        /// <summary>
        /// Gets the traffic by type contacts.
        /// </summary>
        /// <param name="filters">The filters.</param>
        /// <returns></returns>
        public IEnumerable<ReportContact> GetTrafficByTypeContacts(ReportFilters filters)
        {
            var procedureName = "[dbo].[GET_Account_Traffic_TourType_v1]";
            int[] selectedOwners = filters.RowId == 0 ? filters.SelectedOwners : new int[] { filters.RowId };
            int[] selectedCommunities = filters.RowId == 0 ? filters.SelectedCommunities : new int[] { filters.RowId };
            DateTime endDate = filters.EndDate;
            if (filters.StartDate.Date != filters.EndDate.Date)
                endDate = filters.EndDate.Date.AddDays(1);

            var parms = new List<SqlParameter>
                {
                    new SqlParameter{ParameterName="@AccountID", Value= filters.AccountId},
                    new SqlParameter{ParameterName="@Type", Value= filters.Type},
                    new SqlParameter{ParameterName="@StartDate", Value= filters.StartDate.Date},
                    new SqlParameter{ParameterName="@EndDate", Value= endDate.Date},
                    new SqlParameter{ParameterName="@SelectedIDs", Value = filters.Type == 0? string.Join(",",selectedOwners) :string.Join(",",selectedCommunities) },
                    new SqlParameter{ParameterName="@TrafficType", Value = string.Join(",",filters.TrafficType)},
                    new SqlParameter{ParameterName="@ContactListID", Value = filters.RowId == 0 ? 1:filters.RowId }
                };
            CRMDb context = new CRMDb();
            var objectContext = (context as IObjectContextAdapter).ObjectContext;
            objectContext.CommandTimeout = 400;
            return context.ExecuteStoredProcedure<ReportContact>(procedureName, parms).AsQueryable();
        }

        /// <summary>
        /// Gets the online registered contacts.
        /// </summary>
        /// <param name="filters">The filters.</param>
        /// <returns></returns>
        public IEnumerable<ReportContact> GetOnlineRegisteredContacts(ReportFilters filters)
        {
            var procedureName = "[dbo].[GET_Account_FormsCountSummary_Report]";

            var parms = new List<SqlParameter>
				{   
					new SqlParameter{ParameterName ="@AccountID", Value= filters.AccountId},
					new SqlParameter{ParameterName ="@FromDate", Value= filters.StartDate.Date},
					new SqlParameter{ParameterName="@ToDate ", Value = filters.EndDate.Date},
					new SqlParameter{ParameterName="@IsAdmin", Value = filters.IsAdmin},
					new SqlParameter{ParameterName="@OwnerID", Value = filters.UserId},
					new SqlParameter{ParameterName="@GroupID", Value = filters.Type},
					 new SqlParameter{ParameterName="@SelectedIDs", Value = filters.Type == 0?string.Join(",",filters.FormIDs):string.Join(",",filters.LeadAdapterIDs)},
						new SqlParameter{ParameterName="@ContactListID", Value = 1},
						new SqlParameter{ParameterName="@accountMapID", Value = filters.RowId}
				};
            CRMDb context = new CRMDb();
            var objectContext = (context as IObjectContextAdapter).ObjectContext;
            objectContext.CommandTimeout = 400;
            return context.ExecuteStoredProcedure<ReportContact>(procedureName, parms).AsQueryable();
        }

        /// <summary>
        /// Gets the traffic by life cycle contacts.
        /// </summary>
        /// <param name="filters">The filters.</param>
        /// <returns></returns>
        public IEnumerable<ReportContact> GetTrafficByLifeCycleContacts(ReportFilters filters)
        {
            var procedureName = "[dbo].[GET_Account_Traffic_LifeCycle_v1]";
            int[] selectedOwners = filters.RowId == 0 ? filters.SelectedOwners : new int[] { filters.RowId };
            int[] selectedCommunities = filters.RowId == 0 ? filters.SelectedCommunities : new int[] { filters.RowId };

            var parms = new List<SqlParameter>
                {
                    new SqlParameter{ParameterName="@AccountID", Value= filters.AccountId},
                    new SqlParameter{ParameterName="@Type", Value= filters.Type },
                    new SqlParameter{ParameterName="@StartDate", Value= filters.StartDate.Date},
                    new SqlParameter{ParameterName="@EndDate", Value= filters.EndDate.Date},
                    new SqlParameter{ParameterName="@SelectedIDs", Value = filters.Type == 0?string.Join(",", selectedOwners):string.Join(",",selectedCommunities)},
                    new SqlParameter{ParameterName="@TrafficLifeCycle", Value = string.Join(",",filters.TrafficLifeCycle) },
                    new SqlParameter{ParameterName="@ContactListID", Value = filters.RowId == 0 ? 1:filters.RowId }

                };
            CRMDb context = new CRMDb();
            var objectContext = (context as IObjectContextAdapter).ObjectContext;
            objectContext.CommandTimeout = 400;
            return context.ExecuteStoredProcedure<ReportContact>(procedureName, parms);
        }

        /// <summary>
        /// This method gets hotlist data by executing query using dapper.
        /// </summary>
        /// <param name="filters"></param>
        /// <returns></returns>
        public HotlistGridData GetHotlistReportData(ReportFilters filters)
        {
            var procedureName = "[dbo].[GET_HotList_Report]";
            int start = (filters.PageNumber - 1) * filters.PageLimit;
            int end = start + filters.PageLimit;

            var parms = new List<SqlParameter>
                {   
                    new SqlParameter{ParameterName="@ToDate", Value= filters.EndDate},
                    new SqlParameter{ParameterName="@FromDate", Value= filters.StartDate },
                    new SqlParameter{ParameterName="@OwnerID", Value= string.Join(",", filters.SelectedOwners)},
                    new SqlParameter{ParameterName="@AccountID", Value= filters.AccountId},
                    new SqlParameter{ParameterName="@IsAdmin", Value = 1},                
                    new SqlParameter{ParameterName="@LifeCycleStage",Value = string.Join(",", filters.TrafficLifeCycle) },
                    new SqlParameter{ParameterName="@LeadSources", Value = string.Join(",", filters.TrafficSource) },
                    new SqlParameter{ParameterName="@StartNo", Value = (start + 1) },
                    new SqlParameter{ParameterName="@EndNo", Value = end },
                    new SqlParameter{ParameterName="@IsDashboard", Value = filters.IsDasboardView }                    
                };
            var db = ObjectContextFactory.Create();

            db.Database.Connection.Open();
            var cmd = db.Database.Connection.CreateCommand();
            List<SqlParameter> parameters = parms;
            cmd.CommandText = procedureName;
            cmd.CommandType = CommandType.StoredProcedure;
            parameters.ForEach(p =>
            {
                cmd.Parameters.Add(p);
            });
            cmd.CommandTimeout = 600;

            DbDataReader reader = cmd.ExecuteReader();

            List<HotlistData> data = null;
            List<ReportAdvancedViewContact> contacts = new List<ReportAdvancedViewContact>();
            var dbReader = (DbDataReader)reader.MapTo<HotlistData>(out data).MoveNext().MapTo<ReportAdvancedViewContact>(out contacts).MoveNext();
            HotlistGridData griddata = new HotlistGridData();
            griddata.TotalHits = contacts.Count;
            griddata.HotlistData = data;
            griddata.Contacts = contacts.Select(c => c.ContactID);
            return griddata;

        }

        /// <summary>
        /// Gets the web visits report.
        /// </summary>
        /// <param name="filters">The filters.</param>
        /// <param name="TotalHits">The total hits.</param>
        /// <returns></returns>
        public IList<WebVisitReport> GetWebVisitsReport(ReportFilters filters, out int TotalHits)
        {
            CRMDb db = ObjectContextFactory.Create();
            var startDate = filters.StartDate;
            var endDate = filters.EndDate;
            var owners = string.Join(",", filters.SelectedOwners);
            var offset = filters.PageLimit * (filters.PageNumber - 1);
            var pageNumber = filters.PageNumber;
            var pageSize = filters.PageLimit;


            var sortFieldMappings = new Dictionary<string, string>()
                {
                    {"FirstName","c.FirstName"},
                    {"LastName", "c.LastName"},
                    {"Email","ep.Email"},
                    {"Phone","ep.PhoneNumber"},
                    {"Zip","ep.Zip"},
                    {"LifecycleStage","dv.DropdownValue"},      
                    {"PageViews","PageViews"},
                    {"VisitedOn","VisitedOn"},
                    {"Duration","Duration"},
                    {"Page1","vs.Page1"},
                    {"Page2","vs.page2"},
                    {"Page3","vs.page3"},
                    {"Source","Source"},
                    {"Location","wv.Location"},
                };

            Func<string, string> GetSortField = (s) =>
            {
                if (sortFieldMappings.ContainsKey(s))
                    return sortFieldMappings[s];
                else
                    return s;
            };
            string sortField = filters.SortField ?? "VisitedOn";
            string sortDirection = filters.SortDirection == "asc" ? "ASC" : "DESC";

            var accountId = filters.AccountId;
            var sql = string.Format(@"select 
	                    wv.VisitReference, ISNULL(c.FirstName,'') FirstName, ISNULL(c.LastName,'') LastName, ISNULL(ep.Email,'') Email, ISNULL(ep.PhoneNumber,'') Phone,c.ContactId ContactID,
                        ISNULL(ep.Zip,'') Zip, c.Lifecyclestage LifeCycleStageId, dv.DropdownValue LifeCycleStage, VisitedOn, PageViews, Duration, vs.Page1, ISNULL(vs.page2,'') Page2, ISNULL(vs.page3,'') Page3, ISNULL(wv.Location,'') Location, ISNULL(Source,'') Source
	                     from (select contactid, VisitReference, max(visitedon) VisitedOn, sum(duration) Duration, count(visitreference) PageViews,ISNULL(Max(city +', ' + Region),'') Location,
	                     ISNULL(Max(ISPName),'') Source
		                    from contactwebvisits cw 
		                    where  isnull(VisitReference, '000') <> '000' and  VisitedOn >= @startDate and VisitedOn <@endDate
		                    group by visitreference, contactid
	                    ) wv 
	                    left join GET_VisiStat_Top3_Pages vs on wv.VisitReference = vs.VisitReference 
	                    left join contacts c on wv.contactid = c.contactid 
                        left join DropdownValues dv on dv.DropdownValueID = c.LifecycleStage
	                    left join GET_Contact_Email_Phone_Zip ep on wv.ContactID = ep.contactid
	                    where c.ownerid in (select datavalue from dbo.split(@owners,','))  and c.accountId=@accountId
	                    ORDER BY {0} {1} 
	                    OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY", GetSortField(sortField), sortDirection);

            var sqlTotalHits = @"select count(1) from 
                        (select contactid, VisitReference, max(visitedon) VisitedOn	from contactwebvisits cw 
		                    where  isnull(VisitReference, '000') <> '000'  and  VisitedOn >= @startDate and VisitedOn <@endDate
		                    group by visitreference, contactid
	                    ) wv 

	                    left join contacts c on wv.contactid = c.contactid 

	                    where c.ownerid in (select datavalue from dbo.split(@owners,',')) and c.accountId=@accountId";

            var visits = db.Get<WebVisitReport>(sql, new { accountId = accountId, startDate = startDate, endDate = endDate, owners = owners, offset = offset, pageNumber = pageNumber, pageSize = pageSize }).ToList();
            TotalHits = db.Get<int>(sqlTotalHits, new { accountId = accountId, startDate = startDate, endDate = endDate, owners = owners }).Sum();

            return visits;
        }

        /// <summary>
        /// Gets the traffic by type and life cycle contacts.
        /// </summary>
        /// <param name="filters">The filters.</param>
        /// <returns></returns>
        public IEnumerable<ReportContact> GetTrafficByTypeAndLifeCycleContacts(ReportFilters filters)
        {
            var procedureName = "[dbo].[GET_Account_TourType_Lifecycle_v1]";
            int[] selectedOwners = filters.RowId == 0 ? filters.SelectedOwners : new int[] { filters.RowId };
            int[] selectedCommunities = filters.RowId == 0 ? filters.SelectedCommunities : new int[] { filters.RowId };
            var startDate = (filters.StartDate.Date == null || filters.StartDate.Date == DateTime.MinValue) ? (DateTime)System.Data.SqlTypes.SqlDateTime.MinValue : filters.StartDate.Date;
            var parms = new List<SqlParameter>
				{   
					new SqlParameter{ParameterName="@AccountID", Value= filters.AccountId},
					new SqlParameter{ParameterName="@Type", Value= filters.Type == 1 ? 0 : 1 },
					new SqlParameter{ParameterName="@StartDate", Value= startDate},
					new SqlParameter{ParameterName="@EndDate", Value= filters.EndDate.Date},
					new SqlParameter{ParameterName="@SelectedIDs", Value = filters.Type == 1?string.Join(",",selectedOwners):string.Join(",",selectedCommunities)},                
					new SqlParameter{ParameterName="@TourTypes",Value = string.Join(",",filters.TrafficType) },
					new SqlParameter{ParameterName="@TrafficLifeCycle", Value = string.Join(",",filters.TrafficLifeCycle) },
					new SqlParameter{ParameterName="@ContactListID", Value = filters.RowId==0?1:filters.RowId},
                    new SqlParameter{ParameterName="@DropdownType", Value = filters.DropdownType}

                };
            CRMDb context = new CRMDb();
            var objectContext = (context as IObjectContextAdapter).ObjectContext;
            objectContext.CommandTimeout = 400;
            return context.ExecuteStoredProcedure<ReportContact>(procedureName, parms);
        }

        public IEnumerable<dynamic> GetNightlyStatusReportData(ReportFilters filters)
        {
            var db = ObjectContextFactory.Create();

            var procName = filters.Type == (int)Reports.NightlyStatus ? "[dbo].[GetSenderRecipientInfoNightlyReport]" : filters.Type == (int)Reports.NightlyCampaign ? "[dbo].[DailyCampaignReport]" :
                filters.Type == (int)Reports.BouncedEmail ? "[dbo].[GET_BouncedEmailReport]" : "";
            var offset = filters.PageLimit * (filters.PageNumber - 1);
            var pageSize = filters.PageLimit;

            IEnumerable<dynamic> reportData = null;
            
            db.QueryStoredProc(procName, (reader) =>
            {
                if (filters.Type == (int)Reports.NightlyStatus)
                    reportData = reader.Read<NightlyStatusReport>().ToList();
                else if (filters.Type == (int)Reports.NightlyCampaign)
                    reportData = reader.Read<NightlyCampaignReport>().ToList();
                else if (filters.Type == (int)Reports.BouncedEmail)
                    reportData = reader.Read<BouncedEmailReport>().ToList();
            },
                new
                {
                    StartDate = filters.StartDate,
                    EndDate = filters.EndDate,
                    AccountIds = string.Join(",", filters.AccountIDs),
                    Take = pageSize,
                    Skip = offset
                });
            return reportData;
        }

        public IEnumerable<LoginFrequencyReport> GetLoginFrequencyReportData(ReportFilters filters)
        {
            var db = ObjectContextFactory.Create();
            var procName = "[dbo].[GET_LogIn_Frequency_Report]";

            IEnumerable<LoginFrequencyReport> data = new List<LoginFrequencyReport>();

            db.QueryStoredProc(procName, (r) =>
            {
                data = r.Read<LoginFrequencyReport>().ToList();
            }, new {
                StartDate = filters.StartDate,
                EndDate = filters.EndDate,
                AccountIds = string.Join(",", filters.AccountIDs),
                LoginReportType = filters.Type
            });
            return data;
        }

        /// <summary>
        /// Gets the stored procedure data.
        /// </summary>
        /// <param name="procedureName">Name of the procedure.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        ReportResult getStoredProcedureData(string procedureName, List<SqlParameter> parameters)
        {
            var db = ObjectContextFactory.Create();
            db.Database.Connection.Open();
            var cmd = db.Database.Connection.CreateCommand();
            cmd.CommandTimeout = 400;
            cmd.CommandText = procedureName;
            cmd.CommandType = CommandType.StoredProcedure;
            parameters.ForEach(p =>
            {
                cmd.Parameters.Add(p);
            });
            DbDataReader reader = cmd.ExecuteReader();
            Func<DbDataRecord, IEnumerable<DropDownValue>> GetLeadSources = (row) =>
            {
                List<DropDownValue> drpdwnvalues = new List<DropDownValue>();
                for (int index = 3; index < row.FieldCount; index++)
                {
                    DropDownValue drpdwn = new DropDownValue();
                    var val = row.GetValue(index);
                    var text = row.GetName(index);
                    drpdwn.DropdownValueName = text;
                    drpdwn.DropdownValue = (int)val;
                    drpdwnvalues.Add(drpdwn);
                }
                return drpdwnvalues;
            };

            IQueryable<ReportData> result = reader.Cast<DbDataRecord>()
                  .Select(r => new ReportData()
                  {
                      ID = (int)(r["ID"]),
                      Name = (string)r["Name"],
                      CurrentTotal = (int)r["Total"],
                      DropdownValues = GetLeadSources(r)
                  }).AsQueryable();
            return new ReportResult { GridData = result.AsQueryable() };
        }

        /// <summary>
        /// Gets the forms count summary data.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="formIds">The form ids.</param>
        /// <param name="leadAdapterIds">The lead adapter ids.</param>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="accountId">The account identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="isAdmin">if set to <c>true</c> [is admin].</param>
        /// <returns></returns>
        public IEnumerable<ForsCountSummary> GetFormsCountSummaryData(DateTime startDate, DateTime endDate, int[] formIds, int[] leadAdapterIds, int groupId, int accountId, int userId, bool isAdmin)
        {
            var procedureName = "[dbo].[GET_Account_FormsCountSummary_Report]";

            var parms = new List<SqlParameter>
				{   
					new SqlParameter{ParameterName ="@AccountID", Value= accountId},
					new SqlParameter{ParameterName ="@FromDate", Value= startDate.Date},
					new SqlParameter{ParameterName="@ToDate ", Value = endDate.Date},
					new SqlParameter{ParameterName="@IsAdmin", Value = isAdmin},
					new SqlParameter{ParameterName="@OwnerID", Value = userId},
					new SqlParameter{ParameterName="@GroupID", Value = groupId},
					 new SqlParameter{ParameterName="@SelectedIDs", Value = groupId == 0?string.Join(",",formIds):string.Join(",",leadAdapterIds)}
				};
            CRMDb context = new CRMDb();
            var objectContext = (context as IObjectContextAdapter).ObjectContext;
            objectContext.CommandTimeout = 400;
            return context.ExecuteStoredProcedure<ForsCountSummary>(procedureName, parms);
        }

        /// <summary>
        /// Gets the BDX freemium custom lead report data.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="accountId">The account identifier.</param>
        /// <param name="isAdmin">if set to <c>true</c> [is admin].</param>
        /// <param name="userID">The user identifier.</param>
        /// <returns></returns>
        public IEnumerable<ReportContactInfo> GetBDXFreemiumCustomLeadReportData(DateTime startDate, DateTime endDate, int accountId, bool isAdmin, int userID)
        {
            var procedureName = "[dbo].[Get_Account_BDX_FreemiumCustomLead_Report]";


            var parms = new List<SqlParameter>
				{   
					new SqlParameter{ParameterName ="@AccountID", Value= accountId},
					new SqlParameter{ParameterName ="@FromDate", Value= startDate.Date},
					new SqlParameter{ParameterName="@ToDate ", Value = endDate.Date},
					 new SqlParameter{ParameterName="@IsAdmin ", Value = isAdmin},
					  new SqlParameter{ParameterName="@OwnerID ", Value = userID}
				};
            CRMDb context = new CRMDb();
            var objectContext = (context as IObjectContextAdapter).ObjectContext;
            objectContext.CommandTimeout = 400;
            return context.ExecuteStoredProcedure<ReportContactInfo>(procedureName, parms);
        }

        /// <summary>
        /// Gets the custom report stored procedure data.
        /// </summary>
        /// <param name="procedureName">Name of the procedure.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        ReportResult getCustomReportStoredProcedureData(string procedureName, List<SqlParameter> parameters)
        {
            var db = ObjectContextFactory.Create();

            db.Database.Connection.Open();
            var cmd = db.Database.Connection.CreateCommand();
            cmd.CommandText = procedureName;
            cmd.CommandType = CommandType.StoredProcedure;
            parameters.ForEach(p =>
            {
                cmd.Parameters.Add(p);
            });

            DbDataReader reader = cmd.ExecuteReader();
            Func<DbDataRecord, IEnumerable<CustomColumns>> GetCustomData = (row) =>
            {
                List<CustomColumns> customcolumns = new List<CustomColumns>();
                for (int index = 0; index < row.FieldCount; index++)
                {
                    CustomColumns clms = new CustomColumns();
                    var val = row.GetValue(index);
                    var text = row.GetName(index);
                    clms.ColumnName = text;
                    clms.ColumnValue = val == null ? "" : Convert.ToString(val);
                    customcolumns.Add(clms);
                }
                return customcolumns;
            };
            IQueryable<CustomReportData> result = reader.Cast<DbDataRecord>()
                     .Select(r => new CustomReportData()
                     {
                         CustomData = GetCustomData(r)
                     }).AsQueryable();

            ReportResult reportResult = new ReportResult { CustomReportData = result };
            return reportResult;
        }

        /// <summary>
        /// Gets the stored procedure data.
        /// </summary>
        /// <param name="procedureName">Name of the procedure.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="isCamparedTo">if set to <c>true</c> [is campared to].</param>
        /// <returns></returns>
        ReportResult getStoredProcedureData(string procedureName, List<SqlParameter> parameters, bool isCamparedTo)
        {
            var db = ObjectContextFactory.Create();

            db.Database.Connection.Open();
            var cmd = db.Database.Connection.CreateCommand();
            //   cmd.CommandTimeout = 360;
            cmd.CommandText = procedureName;
            cmd.CommandType = CommandType.StoredProcedure;
            parameters.ForEach(p =>
            {
                cmd.Parameters.Add(p);
            });

            DbDataReader reader = cmd.ExecuteReader();

            List<AreaChartData> areaChartData = null;
            List<PieChartData> pieChartData = null;
            var dbReader = (DbDataReader)reader.MapTo<AreaChartData>(out areaChartData).MoveNext().MapTo<PieChartData>(out pieChartData).MoveNext();
            IQueryable<ReportData> result = null;
            if (isCamparedTo == false)
            {
                Func<DbDataRecord, IEnumerable<DropDownValue>> GetLeadSources = (row) =>
                {
                    List<DropDownValue> drpdwnvalues = new List<DropDownValue>();
                    for (int index = 3; index < row.FieldCount; index++)
                    {
                        DropDownValue drpdwn = new DropDownValue();
                        var val = row.GetValue(index);
                        var text = row.GetName(index);
                        drpdwn.DropdownValueName = text;
                        drpdwn.DropdownValue = (int)val;
                        drpdwnvalues.Add(drpdwn);
                    }
                    return drpdwnvalues;
                };

                result = dbReader.Cast<DbDataRecord>()
                      .Select(r => new ReportData()
                      {
                          ID = (int)(r["ID"]),
                          Name = (string)r["Name"],
                          CurrentTotal = (int)r["Total"],
                          DropdownValues = GetLeadSources(r)
                      }).AsQueryable();
            }
            else if (isCamparedTo == true)
            {
                Func<DbDataRecord, IEnumerable<DropDownValue>> GetLeadSources = (row) =>
                {
                    List<DropDownValue> drpdwnvalues = new List<DropDownValue>();
                    for (int index = 4; index < row.FieldCount; index++)
                    {
                        DropDownValue drpdwn = new DropDownValue();
                        var val = row.GetValue(index);
                        var text = row.GetName(index);
                        drpdwn.DropdownValueName = text;
                        drpdwn.DropdownValue = (int)val;
                        drpdwnvalues.Add(drpdwn);
                    }
                    return drpdwnvalues;
                };

                result = dbReader.Cast<DbDataRecord>()
                .Select(r => new ReportData()
                {
                    ID = (int)(r["ID"]),
                    Name = (string)r["Name"],
                    CurrentTotal = (int)r["Total (c)"],
                    PreviousTotal = (int)r["Total (p)"],
                    DropdownValues = GetLeadSources(r)
                }).AsQueryable();
            }
            return new ReportResult { GridData = result.AsQueryable(), AreaChartData = areaChartData, PieChartData = pieChartData };
        }

        /// <summary>
        /// Updates the last run activity.
        /// </summary>
        /// <param name="reportId">The report identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="accountId">The account identifier.</param>
        /// <param name="reportName">Name of the report.</param>
        public void UpdateLastRunActivity(int reportId, int userId, int accountId, string reportName)
        {
            if (reportId != 0 && userId != 0 && accountId != 0)
            {
                var db = ObjectContextFactory.Create();
                UserActivityLogsDb log = new UserActivityLogsDb();
                log.AccountID = accountId;
                log.UserID = userId;
                log.EntityID = reportId;
                log.EntityName = reportName;
                log.LogDate = DateTime.Now.ToUniversalTime();
                log.ModuleID = (byte)AppModules.Reports;
                log.UserActivityID = (byte)UserActivityType.LastRunOn;
                db.UserActivitiesLog.Add(log);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// Gets the activity report contact ids.
        /// </summary>
        /// <param name="filters">The filters.</param>
        /// <returns></returns>
        public List<int> GetActivityReportContactIds(ReportFilters filters)
        {
            CRMDb db = ObjectContextFactory.Create();
            var entityIDs = db.UserActivitiesLog.Where(u => u.AccountID == filters.AccountId && u.UserActivityID == 4 && u.ModuleID == 3 && u.UserID == filters.RowId
                 && (u.LogDate >= filters.StartDate.Date && u.LogDate <= filters.EndDate)).Select(e => e.EntityID).ToArray();
            var contactIds = db.UserActivitiesLog.Where(u => u.AccountID == filters.AccountId && u.UserActivityID == 1 && u.ModuleID == 3 && u.UserID == filters.RowId
                 && (u.LogDate >= filters.StartDate.Date && u.LogDate <= filters.EndDate)).Select(e => new { EntityID = e.EntityID }).Where(a => !entityIDs.Contains(a.EntityID)).ToList();
            return contactIds.Select(e => e.EntityID).ToList();
        }

        /// <summary>
        /// Gets the activity report note contacts ids.
        /// </summary>
        /// <param name="filters">The filters.</param>
        /// <returns></returns>
        public List<int> GetActivityReportNoteContactsIds(ReportFilters filters)
        {
            CRMDb db = ObjectContextFactory.Create();
            var entityIDs = db.UserActivitiesLog.Where(u => u.AccountID == filters.AccountId && u.UserActivityID == 4 && u.ModuleID == 6 && u.UserID == filters.RowId
                 && (u.LogDate >= filters.StartDate.Date && u.LogDate <= filters.EndDate)).Select(e => e.EntityID).ToArray();
            List<int> noteIds = db.UserActivitiesLog.Where(u => u.AccountID == filters.AccountId && u.UserActivityID == 1 && u.ModuleID == 6 && u.UserID == filters.RowId
                 && (u.LogDate >= filters.StartDate.Date && u.LogDate <= filters.EndDate)).Select(e => e.EntityID).Where(a => !entityIDs.Contains(a)).ToList();
            var contactIds = db.ContactNotes.Where(n => noteIds.Contains(n.NoteID)).Select(c => c.ContactID).ToList();
            return contactIds;
        }

        /// <summary>
        /// Gets the activity report tour contact ids.
        /// </summary>
        /// <param name="filters">The filters.</param>
        /// <returns></returns>
        public List<int> GetActivityReportTourContactIds(ReportFilters filters)
        {
            CRMDb db = ObjectContextFactory.Create();
            var entityIDs = db.UserActivitiesLog.Where(u => u.AccountID == filters.AccountId && u.UserActivityID == 4 && u.ModuleID == 7 && u.UserID == filters.RowId
                && (u.LogDate >= filters.StartDate.Date && u.LogDate <= filters.EndDate)).Select(e => e.EntityID).ToArray();
            List<int> tourIds = db.UserActivitiesLog.Where(u => u.AccountID == filters.AccountId && u.UserActivityID == 1 && u.ModuleID == 7 && u.UserID == filters.RowId
                 && (u.LogDate >= filters.StartDate.Date && u.LogDate <= filters.EndDate)).Select(e => e.EntityID).Where(a => !entityIDs.Contains(a)).ToList();
            var contactIds = db.ContactTours.Where(a => tourIds.Contains(a.TourID)).Select(c => c.ContactID).ToList();
            return contactIds;

        }

        /// <summary>
        /// Inserts the default reports.
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        public void InsertDefaultReports(int accountId)
        {
            var db = ObjectContextFactory.Create();
            var defaultReports = db.Reports.Where(r => r.AccountID == null);
            if (defaultReports != null)
            {
                foreach (var defaultReport in defaultReports)
                {
                    ReportsDb report = new ReportsDb();
                    report.AccountID = accountId;
                    report.ReportName = defaultReport.ReportName;
                    report.ReportType = defaultReport.ReportType;
                    db.Reports.Add(report);
                }
                db.SaveChanges();
            }
        }

        /// <summary>
        /// Finds the by.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override Report FindBy(int id)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Converts the type of to database.
        /// </summary>
        /// <param name="domainType">Type of the domain.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override ReportsDb ConvertToDatabaseType(Report domainType, CRMDb context)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Converts to domain.
        /// </summary>
        /// <param name="reportsDb">The reports database.</param>
        /// <returns></returns>
        public override Report ConvertToDomain(ReportsDb reportsDb)
        {
            return Mapper.Map<ReportsDb, Report>(reportsDb);

        }

        /// <summary>
        /// Persists the value objects.
        /// </summary>
        /// <param name="domainType">Type of the domain.</param>
        /// <param name="dbType">Type of the database.</param>
        /// <param name="context">The context.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public override void PersistValueObjects(Report domainType, ReportsDb dbType, CRMDb context)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Inserts the user dashboard settings.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="dashboardIds">The dashboard ids.</param>
        public void InsertUserDashboardSettings(int userId, IList<byte> dashboardIds)
        {
            var db = ObjectContextFactory.Create();
            var userSettingRecords = db.DashBoardUserSettingsMap.Where(u => u.UserID == userId).ToList();

            if (dashboardIds != null)
            {
                foreach (byte dashboard in dashboardIds)
                {
                    if (!(userSettingRecords.Exists(ds => ds.DashBoardID == dashboard)))
                    {
                        DashBoardUserSettingsMap dashboardUserSettingsMap = new DashBoardUserSettingsMap();
                        dashboardUserSettingsMap.UserID = (int)userId;
                        dashboardUserSettingsMap.DashBoardID = dashboard;
                        db.DashBoardUserSettingsMap.Add(dashboardUserSettingsMap);
                        db.SaveChanges();
                    }
                }
                var deletedSettings = userSettingRecords.Where(ds => ds.DashBoardID != 0 && !dashboardIds.Contains(ds.DashBoardID)).ToList();
                foreach (var userdashBoardSettings in deletedSettings)
                {
                    db.DashBoardUserSettingsMap.Remove(userdashBoardSettings);
                    db.SaveChanges();
                }
            }
        }

        /// <summary>
        /// Gets the dashboard items.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns></returns>
        public IEnumerable<DashboardItems> GetDashboardItems(int userId)
        {
            var db = ObjectContextFactory.Create();

            int count = db.DashBoardUserSettingsMap.Count(u => u.UserID == userId);
            if (count > 0)
            {
                IEnumerable<DashboardItems> query = db.DashboardSettings.GroupJoin(db.DashBoardUserSettingsMap.Where(i => i.UserID == userId),
                                                            setting => setting.DashBoardID, map => map.DashBoardID, (setting, map) => new { setting = setting, map = map })
                                                            .SelectMany(x => x.map.DefaultIfEmpty(), (d, s) => new DashboardItems()
                                                            {
                                                                Id = d.setting.DashBoardID,
                                                                Report = d.setting.Report,
                                                                Value = s == null ? false : true
                                                            });
                return query;
            }
            else
            {
                var dashboardSettings = db.DashboardSettings.ToList();
                IList<DashboardItems> items = new List<DashboardItems>();
                foreach (DashboardSettingsDb item in dashboardSettings)
                {
                    DashboardItems dItems = new DashboardItems();
                    dItems.Id = item.DashBoardID;
                    dItems.Value = item.Value;
                    dItems.Report = item.Report;
                    items.Add(dItems);
                }
                return items;
            }
        }

        /// <summary>
        /// Gets the BDX custom lead report contact information.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="accountId">The account identifier.</param>
        /// <param name="isAdmin">if set to <c>true</c> [is admin].</param>
        /// <param name="userID">The user identifier.</param>
        /// <returns></returns>
        public IEnumerable<BDXCustomLeadContactInfo> GetBDXCustomLeadReportContactInfo(DateTime startDate, DateTime endDate, int accountId, bool isAdmin, int userID)
        {
            var procedureName = "[dbo].[Get_Account_BDX_Custom_Lead_Report]";
            var parms = new List<SqlParameter>
                {   
                    new SqlParameter{ParameterName ="@AccountID", Value= accountId},
                    new SqlParameter{ParameterName ="@FromDate", Value= startDate.Date},
                    new SqlParameter{ParameterName="@ToDate ", Value = endDate.Date},
                    new SqlParameter{ParameterName="@IsAdmin ", Value = isAdmin},
                    new SqlParameter{ParameterName="@OwnerID ", Value = userID}
                };
            var db = ObjectContextFactory.Create();
            IEnumerable<BDXCustomLeadContactInfo> data = db.ExecuteStoredProcedure<BDXCustomLeadContactInfo>(procedureName, parms);
            return data;
        }

        public string GetReportNameByType(int accountId, byte reportType)
        {
            using (var db = ObjectContextFactory.Create())
            {
                var sql = @"select ReportName from reports where accountid = @accountId and reporttype = @reportType";
                var reportName = db.Get<string>(sql, new { accountId = accountId, reportType = reportType }, true).FirstOrDefault();
                return reportName;
            }
        }

        /// <summary>
        /// Getting Database Life Cycle Data Based on Filters
        /// </summary>
        /// <param name="filters"></param>
        /// <returns></returns>
        public ReportResult GetAllDatabaseLifeCycleData(ReportFilters filters)
        {
            var procName = "[dbo].[Get_DatabaseLifeCycleReport]";

            DateTime endDate = filters.EndDate.Date.AddDays(1).AddMinutes(-1);

            var parms = new List<SqlParameter>
                {
                    new SqlParameter{ParameterName ="@AccountID", Value= filters.AccountId},
                    new SqlParameter{ParameterName ="@Users", Value= filters.SelectedOwners.IsAny() ? string.Join(",", filters.SelectedOwners) : string.Empty},
                    new SqlParameter{ParameterName="@StartDate ", Value = filters.StartDate},
                    new SqlParameter{ParameterName="@EndDate ", Value = endDate},
                    new SqlParameter{ParameterName ="@DateRange", Value= filters.DateRange},
                    new SqlParameter{ParameterName="@LifeCycleStages ", Value = filters.TrafficLifeCycle.IsAny() ? string.Join(",", filters.TrafficLifeCycle) : string.Empty},
                    new SqlParameter{ParameterName="@IsAdmin ", Value = filters.IsAdmin}

                };

            var db = ObjectContextFactory.Create();

            db.Database.Connection.Open();
            var cmd = db.Database.Connection.CreateCommand();
            cmd.CommandText = procName;
            cmd.CommandType = CommandType.StoredProcedure;
            parms.ForEach(p =>
            {
                cmd.Parameters.Add(p);
            });

            DbDataReader reader = cmd.ExecuteReader();
            List<AreaChartData> areaChartData = null;
            List<DatabasePieChartData> pieChartData = null;
            List<DatabaseLifeCycleGridData> grid = null;
            var dbReader = (DbDataReader)reader.MapTo<AreaChartData>(out areaChartData).MoveNext().MapTo<DatabaseLifeCycleGridData>(out grid);
            pieChartData = grid.Select(s => new DatabasePieChartData { LifecycleStageId = s.LifecycleStageId, ContactsCount = s.ContactsCount }).OrderByDescending(s => s.ContactsCount).Take(5).ToList();
            ReportResult procdata = new ReportResult() { AllDatabaseLifeCycleData = grid.OrderByDescending(c => c.ContactsCount), AreaChartData = areaChartData, DatabasePiesChartData = pieChartData };
            return procdata;
        }

        /// <summary>
        /// Get Total Contacts from based on database lifecycle report
        /// </summary>
        /// <param name="filters"></param>
        /// <returns></returns>
        public IEnumerable<ReportContact> GetAllDatabaseLifeCycleReportContacts(ReportFilters filters)
        {
            var procName = "[dbo].[Get_Database_Lifecycle_Report_ContactInfo]";

            var parms = new List<SqlParameter>
                {
                    new SqlParameter{ParameterName="@Entities", Value= filters.TrafficLifeCycle.IsAny() ? string.Join(",", filters.TrafficLifeCycle) : string.Empty},
                    new SqlParameter{ParameterName="@AccountID", Value= filters.AccountId },
                    new SqlParameter{ParameterName="@OwnerIds", Value= filters.SelectedOwners.IsAny() ? string.Join(",", filters.SelectedOwners) : string.Empty },
                    new SqlParameter{ParameterName="@StartDate", Value= filters.StartDate},
                    new SqlParameter{ParameterName="@EndDate", Value= filters.EndDate},
                    new SqlParameter{ParameterName="@IsAdmin", Value= filters.IsAdmin}
                };

            var db = ObjectContextFactory.Create();

            db.Database.Connection.Open();
            var cmd = db.Database.Connection.CreateCommand();
            cmd.CommandText = procName;
            cmd.CommandType = CommandType.StoredProcedure;
            parms.ForEach(p =>
            {
                cmd.Parameters.Add(p);
            });

            List<ReportContact> contactReport = new List<ReportContact>();
            DbDataReader reader = cmd.ExecuteReader();
            var dbReader = (DbDataReader)reader.MapTo<ReportContact>(out contactReport);
            return contactReport;
        }

        public IEnumerable<TourByContactReportInfo> GetTourByContactsReportData(int accountId, DateTime startDate, DateTime endDate, int[] tourStatuses, int[] tourTypes, int[] tourCommunities, int pageSize, int pageNumber, string sortField, string sortDirection)
        {
            using (var db = ObjectContextFactory.Create())
            {
                DateTime toDate = endDate.Date.AddDays(1).AddMinutes(-1);
                IEnumerable<TourByContactReportInfo> contactCampaignDetails = null;
                db.QueryStoredProc("[dbo].[Get_ToursByContacts]", (reader) =>
                {
                    contactCampaignDetails = reader.Read<TourByContactReportInfo>().ToList();
                }, 
                new
                {
                    AccountId = accountId,
                    FromDate = startDate,
                    ToDate = toDate,
                    TourStatus = tourStatuses.IsAny() ? string.Join(",", tourStatuses) : string.Empty,
                    TourType = tourTypes.IsAny() ? string.Join(",", tourTypes) : string.Empty,
                    TourCommunity = tourCommunities.IsAny() ? string.Join(",", tourCommunities) : string.Empty,
                    PageSize = pageSize,
                    PageNumber = pageNumber,
                    SortColumn = sortField,
                    SortDirection = sortDirection
                });

                return contactCampaignDetails;
            }
        }

        public List<int> GetActivityReportDrildownModuleContactIds(ReportFilters filters)
        {
            var db = ObjectContextFactory.Create();
            List<int> contactIds = new List<int>() { };
            db.QueryStoredProc("GET_Account_Activity_Report_Contacts", r =>
            {
                contactIds = r.Read<int>().ToList();
            },
            new
            {
                AccountID = filters.AccountId,
                UserIDs = string.Join(",", filters.SelectedOwners),
                ModuleIDs = string.Join(",", filters.ModuleIDs) ,
                StartDate = filters.StartDate,
                EndDate = filters.EndDate
            });

            return contactIds;
        }

        public int GetImportDropdownValue(int accountId)
        {
            var db = ObjectContextFactory.Create();
            return db.DropdownValues.Where(w => w.AccountID == accountId && w.DropdownID == (byte)DropdownFieldTypes.LeadSources && w.DropdownValueTypeID == (short)DropdownValueTypes.Imports).Select(s => s.DropdownValueID).FirstOrDefault();
        }
    }
}
using LandmarkIT.Enterprise.Utilities.Logging;
using SmartTouch.CRM.Repository.Database;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using LandmarkIT.Enterprise.Extensions;
using Dapper;

namespace SmartTouch.CRM.Repository
{
    public static class DbContextExtensions
    {
        public static IList<T> ExecuteStoredProcedure<T>(this DbContext db, string procedureName, List<SqlParameter> parms)
        {
            var result = default(IList<T>);
            var sqlQuery = new StringBuilder(string.Format("EXECUTE {0} ", procedureName));
            var parmArray = default(SqlParameter[]);
            try
            {
                if (parms != null && parms.Count > 0)
                {
                    for (int i = 0; i < parms.ToArray().Length; i++)
                    {
                        sqlQuery.Append(string.Format("{0} {1},", parms[i].ParameterName, parms[i].Direction == ParameterDirection.Input ? string.Empty : "out"));
                    }
                    sqlQuery = sqlQuery.Remove(sqlQuery.Length - 1, 1);
                }
                if (parms == null || parms.Count == 0)
                {
                    result = db.Database.SqlQuery<T>(sqlQuery.ToString()).ToList();
                }
                else
                {
                    parmArray = parms.ToArray();
                    result = db.Database.SqlQuery<T>(sqlQuery.ToString(), parmArray).ToList();
                }

                if (null != parmArray) parms = parmArray.ToList();
            }
            catch (Exception ex)
            {
                Logger.Current.Error("An error occurred in executing stored procedure returning data, procedure name: " + procedureName, ex);
            }

            return result;
        }

        public static int ExecuteStoredProcedure(this DbContext db, string procedureName, List<SqlParameter> parms, int? timeout = null)
        {
            var result = default(int);
            var parmArray = default(SqlParameter[]);
            var sqlQuery = new StringBuilder(string.Format("EXECUTE {0} ", procedureName));
            try
            {
                if (parms != null && parms.Count > 0)
                {
                    for (int i = 0; i < parms.ToArray().Length; i++)
                    {
                        sqlQuery.Append(string.Format("{0} {1},", parms[i].ParameterName, parms[i].Direction == ParameterDirection.Input ? string.Empty : "out"));
                    }
                    sqlQuery = sqlQuery.Remove(sqlQuery.Length - 1, 1);
                }

                if (parms == null || parms.Count == 0)
                {
                    result = db.Database.ExecuteSqlCommand(sqlQuery.ToString(), null);
                }
                else
                {
                    db.Database.CommandTimeout = timeout;
                    parmArray = parms.ToArray();
                    result = db.Database.ExecuteSqlCommand(sqlQuery.ToString(), parmArray);
                }

                if (null != parmArray) parms = parmArray.ToList();
            }
            catch (Exception ex)
            {
                Logger.Current.Error("An error occurred in executing stored procedure returning int, procedure name: " + procedureName, ex);
            }

            return result;
        }
        
        public static void BulkInsert<T>(this CRMDb db, IList<T> list) where T : class
        {
            string connection = db.Database.Connection.ConnectionString;
            string tableName = db.GetTableName<T>();
            using (var _conn = new SqlConnection(connection))
            {
                _conn.Open();
                using (var bulkCopy = new SqlBulkCopy(_conn))
                {
                    bulkCopy.BatchSize = list.Count;
                    bulkCopy.DestinationTableName = tableName;
                    bulkCopy.BulkCopyTimeout = 6000;
                    var batchCount = 5000;
                    var table = new DataTable();
                    var props = TypeDescriptor.GetProperties(typeof(T))
                        //Dirty hack to make sure we only have system data types 
                        //i.e. filter out the relationships/collections
                                               .Cast<PropertyDescriptor>()
                                               .Where(propertyInfo => propertyInfo.PropertyType.Namespace.Equals("System"))
                                               .ToArray();
                    var iteratorCount = Math.Ceiling((float)list.Count() / (float)batchCount);
                    
                    foreach (var propertyInfo in props)
                    {
                        bulkCopy.ColumnMappings.Add(propertyInfo.Name, propertyInfo.Name);
                        table.Columns.Add(propertyInfo.Name, Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType);
                    }

                    var values = new object[props.Length];

                    for (var i = 0; i < iteratorCount; i++)
                    {
                        foreach (var item in list.Skip(i * batchCount).Take(batchCount))
                        {
                            for (var j = 0; j < values.Length; j++)
                            {
                                values[j] = props[j].GetValue(item);
                            }

                            table.Rows.Add(values);
                        }
                        bulkCopy.WriteToServer(table);
                        table.Rows.Clear();
                    }
                }
            }
        }
        public static string GetTableName<T>(this DbContext context) where T : class
        {
            return ((IObjectContextAdapter)context).ObjectContext.GetTableName<T>();
        }

        private static string GetTableName<T>(this ObjectContext context) where T : class
        {
            string sql = context.CreateObjectSet<T>().ToTraceString();
            Regex regex = new Regex("FROM (?<table>.*) AS");
            Match match = regex.Match(sql);

            return match.Groups["table"].Value;
        }
    }
}

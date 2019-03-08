using Dapper;
using LandmarkIT.Enterprise.Utilities.Caching;
using LandmarkIT.Enterprise.Utilities.Logging;
using SmartTouch.CRM.Repository.Database;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Web.Script.Serialization;

namespace SmartTouch.CRM.Repository
{
    public static class DapperExtensions
    {
        private static readonly int IntervalInMinutes = 3;
        /// <summary>
        /// gets data from given multiple sql and executes the action
        /// </summary>
        /// <param name="db"></param>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <param name="action"></param>
        /// <param name="getFromCache"></param>
        public static void GetMultiple
            (this CRMDb db, string sql, Action<SqlMapper.GridReader> action = null, object parameters = null, bool getFromCache = false, int commandTimeout = 300)
        {
            try
            {
                var cs = getConnectionString(db);
                using (var _con = GetSqlConnection(cs))
                {
                    _con.Open();
                    using (var mq = _con.QueryMultiple(sql, parameters, commandTimeout: commandTimeout))
                    {
                        action(mq);
                    }
                }
            }
            catch(Exception ex)
            {
                parameters = (parameters == null) ? new object() : parameters;
                ex.Data.Clear();
                ex.Data.Add("parameters", new JavaScriptSerializer().Serialize(parameters));
                ex.Data.Add("sql", sql);
                Logger.Current.Error("Error while querying using dapper", ex);
                throw ex;
            }
            
        }
        /// <summary>
        /// Get multiple datasets from stored procedure in the form of Reader
        /// </summary>
        /// <param name="db"></param>
        /// <param name="storedProcedure"></param>
        /// <param name="action"></param>
        /// <param name="parameters"></param>
        /// <param name="getFromCache"></param>
        public static void QueryStoredProc(this CRMDb db, string storedProcedure, Action<SqlMapper.GridReader> action = null, object parameters = null, bool getFromCache = false, int commandTimeout = 300)
        {
            try
            {
                var cs = getConnectionString(db);
                using (var _con = GetSqlConnection(cs))
                {
                    _con.Open();
                    using (var mq = _con.QueryMultiple(storedProcedure, parameters, commandType: CommandType.StoredProcedure, commandTimeout: commandTimeout))
                    {
                        action(mq);
                    }
                }
            }
            catch(Exception ex)
            {
                parameters = (parameters == null) ? new object() : parameters;
                ex.Data.Clear();
                ex.Data.Add("parameters", new JavaScriptSerializer().Serialize(parameters));
                ex.Data.Add("sql", storedProcedure);
                Logger.Current.Error("Error while querying using dapper", ex);
                throw ex;
            }
            
        }
        public static IEnumerable<dynamic> Get(this CRMDb db, string sql, object parameters = null, bool getFromCache = false, TimeSpan cacheInterval = default(TimeSpan))
        {
            try
            {
                var cache = new MemoryCacheManager();
                var key = GetHashedKey(sql + parameters);
                if (getFromCache && cache.IsExists(key))
                {
                    return cache.Get<IEnumerable<dynamic>>(key);
                }

                var cs = getConnectionString(db);
                using (var _con = GetSqlConnection(cs))
                {
                    _con.Open();
                    var result = _con.Query(sql, parameters);
                    return AddToCache(key, result, cache, getFromCache, cacheInterval);
                }
            }
            catch(Exception ex)
            {
                parameters = (parameters == null) ? new object() : parameters;
                ex.Data.Clear();
                ex.Data.Add("parameters", new JavaScriptSerializer().Serialize(parameters));
                ex.Data.Add("sql", sql);
                Logger.Current.Error("Error while querying using dapper", ex);
                throw ex;
            }
            
        }
        /// <summary>
        /// gets data from given sql and converts to <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="db"></param>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <param name="getFromCache"></param>
        /// <returns></returns>
        public static IEnumerable<T> Get<T>(this CRMDb db, string sql, object parameters = null, bool getFromCache = false, TimeSpan cacheInterval = default(TimeSpan), int timeoutSeconds = 30, 
            CommandType commandType=CommandType.Text)
        {
            try
            {
                var cache = new MemoryCacheManager();
                var key = GetHashedKey(sql + parameters);
                if (getFromCache && cache.IsExists(key))
                {
                    return cache.Get<IEnumerable<T>>(key);
                }

                var cs = getConnectionString(db);
                using (var _con = GetSqlConnection(cs))
                {
                    _con.Open();
                    var result = _con.Query<T>(sql, parameters, null, true, timeoutSeconds, commandType: commandType);
                    return AddToCache(key, result, cache, getFromCache, cacheInterval);
                }
            }
            catch (Exception ex)
            {
                parameters = (parameters == null) ? new object() : parameters;
                ex.Data.Clear();
                ex.Data.Add("parameters", new JavaScriptSerializer().Serialize(parameters));
                ex.Data.Add("sql", sql);
                Logger.Current.Error("Error while querying using dapper", ex);
                throw ex;
            }
            

        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="db"></param>
        /// <param name="sql"></param>
        /// <param name="map"></param>
        /// <param name="parameters"></param>
        /// <param name="splitOn"></param>
        /// <param name="getFromCache"></param>
        /// <returns></returns>
        public static IEnumerable<T1> Get<T1, T2>(this CRMDb db, string sql, Func<T1, T2, T1> map, object parameters = null, string splitOn = "Id", bool getFromCache = false, TimeSpan cacheInterval = default(TimeSpan),
            CommandType commandType = CommandType.Text, int timeoutSeconds = 30)
        {
            try
            {
                var cache = new MemoryCacheManager();
                var key = GetHashedKey(sql + parameters);
                if (getFromCache && cache.IsExists(key))
                {
                    return cache.Get<IEnumerable<T1>>(key);
                }
                var cs = getConnectionString(db);
                using (var _con = GetSqlConnection(cs))
                {
                    _con.Open();
                    var result = _con.Query<T1, T2, T1>(sql, map, parameters, splitOn: splitOn, commandType: commandType, commandTimeout: timeoutSeconds);
                    return AddToCache(key, result, cache, getFromCache, cacheInterval);
                }
            }
            catch(Exception ex)
            {
                parameters = (parameters == null) ? new object() : parameters;
                ex.Data.Clear();
                ex.Data.Add("parameters", new JavaScriptSerializer().Serialize(parameters));
                ex.Data.Add("sql", sql);
                Logger.Current.Error("Error while querying using dapper", ex);
                throw ex;
            }
            
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <param name="db"></param>
        /// <param name="sql"></param>
        /// <param name="map"></param>
        /// <param name="parameters"></param>
        /// <param name="splitOn"></param>
        /// <param name="getFromCache"></param>
        /// <returns></returns>
        public static IEnumerable<T1> Get<T1, T2, T3>(this CRMDb db, string sql, Func<T1, T2, T3, T1> map, object parameters = null, string splitOn = "Id", bool getFromCache = false, TimeSpan cacheInterval = default(TimeSpan),
            CommandType commandType = CommandType.Text, int timeoutSeconds = 30)
        {
            try
            {
                var cache = new MemoryCacheManager();
                var key = GetHashedKey(sql + parameters);
                if (getFromCache && cache.IsExists(key))
                {
                    return cache.Get<IEnumerable<T1>>(key);
                }
                var cs = getConnectionString(db);
                using (var _con = GetSqlConnection(cs))
                {
                    _con.Open();
                    var result = _con.Query<T1, T2, T3, T1>(sql, map, parameters, splitOn: splitOn, commandType: commandType, commandTimeout: timeoutSeconds);
                    return AddToCache(key, result, cache, getFromCache, cacheInterval);
                }
            }
            catch(Exception ex)
            {
                parameters = (parameters == null) ? new object() : parameters;
                ex.Data.Clear();
                ex.Data.Add("parameters", new JavaScriptSerializer().Serialize(parameters));
                ex.Data.Add("sql", sql);
                Logger.Current.Error("Error while querying using dapper", ex);
                throw ex;
            }
            
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <param name="db"></param>
        /// <param name="sql"></param>
        /// <param name="map"></param>
        /// <param name="parameters"></param>
        /// <param name="splitOn"></param>
        /// <param name="getFromCache"></param>
        /// <returns></returns>
        public static IEnumerable<T1> Get<T1, T2, T3, T4>(this CRMDb db, string sql, Func<T1, T2, T3, T4, T1> map, object parameters = null, string splitOn = "Id", bool getFromCache = false, TimeSpan cacheInterval = default(TimeSpan),
            CommandType commandType = CommandType.Text, int timeoutSeconds = 30)
        {
            try
            {
                var cache = new MemoryCacheManager();
                var key = GetHashedKey(sql + parameters);
                if (getFromCache && cache.IsExists(key))
                {
                    return cache.Get<IEnumerable<T1>>(key);
                }
                var cs = getConnectionString(db);
                using (var _con = GetSqlConnection(cs))
                {
                    _con.Open();
                    var result = _con.Query<T1, T2, T3, T4, T1>(sql, map, parameters, splitOn: splitOn, commandType: commandType, commandTimeout: timeoutSeconds);
                    return AddToCache(key, result, cache, getFromCache, cacheInterval);
                }
            }
            catch(Exception ex)
            {
                parameters = (parameters == null) ? new object() : parameters;
                ex.Data.Clear();
                ex.Data.Add("parameters", new JavaScriptSerializer().Serialize(parameters));
                ex.Data.Add("sql", sql);
                Logger.Current.Error("Error while querying using dapper", ex);
                throw ex;
            }
            
        }
        /// <summary>
        /// Scalar - use this method for no-return operations ex: insert/update/delete
        /// </summary>
        /// <param name="db"></param>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static bool Execute(this CRMDb db, string sql, object parameters = null, int timeoutSeconds = 180)
        {
            try
            {
                var cs = getConnectionString(db);
                bool isSuccess = false;
                using (var _con = GetSqlConnection(cs))
                {
                    _con.Open();
                    var transaction = _con.BeginTransaction();
                    try
                    {
                        _con.Execute(sql, parameters, transaction, timeoutSeconds);
                        transaction.Commit();
                        isSuccess = true;
                    }
                    catch (Exception e)
                    {
                        transaction.Rollback();
                        isSuccess = false;
                        e.Data.Add("sql", sql);
                        e.Data.Add("parameters", parameters);
                        Logger.Current.Error("Error while updating/inserting", e);
                    }
                }
                return isSuccess;
            }
            catch(Exception ex)
            {
                parameters = (parameters == null) ? new object() : parameters;
                ex.Data.Clear();
                ex.Data.Add("parameters", new JavaScriptSerializer().Serialize(parameters));
                ex.Data.Add("sql", sql);
                Logger.Current.Error("Error while querying using dapper", ex);
                throw ex;
            }
            
        }
        
        /// <summary>
        /// Execute query and return last inserted Identity value -3014
        /// </summary>
        /// <param name="db"></param>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <param name="timeoutSeconds"></param>
        /// <returns></returns>
        public static int ExecuteWithOutPut(this CRMDb db, string sql, object parameters = null, int timeoutSeconds = 180)
        {
            try
            {
                var cs = getConnectionString(db);
                int id = 0;
                //bool isSuccess = false;
                using (var _con = GetSqlConnection(cs))
                {
                    _con.Open();
                    var transaction = _con.BeginTransaction();
                    try
                    {
                        
                       id = _con.Query<int>(sql, parameters, transaction,buffered:false,commandTimeout:null,commandType:null).Single();                       
                        transaction.Commit();
                        //isSuccess = true;
                    }
                    catch (Exception e)
                    {
                        transaction.Rollback();
                        id = 0;
                        //isSuccess = false;
                        e.Data.Add("sql", sql);
                        e.Data.Add("parameters", parameters);
                        Logger.Current.Error("Error while updating/inserting", e);
                    }
                }
                return id;
            }
            catch (Exception ex)
            {
                parameters = (parameters == null) ? new object() : parameters;
                ex.Data.Clear();
                ex.Data.Add("parameters", new JavaScriptSerializer().Serialize(parameters));
                ex.Data.Add("sql", sql);
                Logger.Current.Error("Error while querying using dapper", ex);
                throw ex;
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        private static string getConnectionString(CRMDb db)
        {
            return db.Database.Connection.ConnectionString; ;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string GetHashedKey(string input)
        {
            HashAlgorithm hashAlgorithm = new SHA256CryptoServiceProvider();

            byte[] byteValue = System.Text.Encoding.UTF8.GetBytes(input);

            byte[] byteHash = hashAlgorithm.ComputeHash(byteValue);

            return Convert.ToBase64String(byteHash);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="result"></param>
        /// <param name="getFromCache"></param>
        /// <returns></returns>
        public static IEnumerable<T> AddToCache<T>(string key, IEnumerable<T> result, MemoryCacheManager cache , bool getFromCache = false, TimeSpan cacheInterval = default(TimeSpan))
        {
            if (getFromCache)
            {
                if (cacheInterval == default(TimeSpan))
                    cacheInterval = new TimeSpan(0, IntervalInMinutes, 0);
                cache.Add<IEnumerable<T>>(key, result, DateTime.Now.Add(cacheInterval));
            }
            return result;
        }
        public static SqlMapper.ICustomQueryParameter AsTableValuedParameter<T>(this IEnumerable<T> enumerable, string typeName, IEnumerable<string> orderedColumnNames = null)
        {
            var dataTable = new DataTable();
            
            if (typeof(T).IsValueType)
            {
                if (orderedColumnNames != null && orderedColumnNames.Any())
                {
                    foreach (var c in orderedColumnNames)
                    {
                        dataTable.Columns.Add(c);
                    }
                }
                else
                {
                    dataTable.Columns.Add("NONAME", typeof(T));
                }
                
                foreach (T obj in enumerable)
                {
                    dataTable.Rows.Add(obj);
                }                
            }
            else
            {
                PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                PropertyInfo[] readableProperties = properties.Where(w => w.CanRead).ToArray();
                if (readableProperties.Length > 1 && orderedColumnNames == null)
                    throw new ArgumentException("Ordered list of column names must be provided when TVP contains more than one column");
                var columnNames = (orderedColumnNames ?? readableProperties.Select(s => s.Name)).ToArray();
                foreach (string name in columnNames)
                {
                    var propType = readableProperties.Single(s => s.Name.Equals(name));
                    dataTable.Columns.Add(name, Nullable.GetUnderlyingType(propType.PropertyType) ?? propType.PropertyType);
                }

                foreach (T obj in enumerable)
                {
                    dataTable.Rows.Add(
                        columnNames.Select(s => readableProperties.Single(s2 => s2.Name.Equals(s)).GetValue(obj))
                            .ToArray());
                }
            }
            return dataTable.AsTableValuedParameter(typeName);
        }

        public static IDbConnection GetSqlConnection(string connectionString)
        {
            //var factory = DbProviderFactories.GetFactory("System.Data.SqlClient");
            //var cnn = factory.CreateConnection();
            //cnn.ConnectionString = connectionString;
            //return cnn;
            return new SqlConnection(connectionString);
        }
    }
}

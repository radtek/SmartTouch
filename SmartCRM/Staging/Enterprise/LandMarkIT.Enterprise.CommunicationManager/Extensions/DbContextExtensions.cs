using LandmarkIT.Enterprise.Utilities.Logging;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace LandmarkIT.Enterprise.CommunicationManager.Extensions
{
    public static class DbContextExtensions
    {
        public static IList<T> ExecuteStoredProcedure<T>(this DbContext db, string procedureName, List<SqlParameter> parms)
        {
            Logger.Current.Verbose("Executing stored procedure : "+ procedureName);
            var result = default(IList<T>);
            var sqlQuery = new StringBuilder(string.Format("EXECUTE {0} ", procedureName));
            var parmArray = default(SqlParameter[]);
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

            return result;
        }

        public static int ExecuteStoredProcedure(this DbContext db, string procedureName, List<SqlParameter> parms)
        {
            var result = default(int);
            var parmArray = default(SqlParameter[]);
            var sqlQuery = new StringBuilder(string.Format("EXECUTE {0} ", procedureName));
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
                parmArray = parms.ToArray();
                result = db.Database.ExecuteSqlCommand(sqlQuery.ToString(), parmArray);
            }

            if (null != parmArray) parms = parmArray.ToList();

            return result;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data.SqlClient;

namespace SmartTouchSqli18n
{
    public class DataManager
    {
        public static SqlConnection GetConnection(string connectionName)
        {
            return new SqlConnection(ConfigurationManager.ConnectionStrings[connectionName].ConnectionString);
        }

        public static SqlConnection GetConnection()
        {
            return GetConnection("CRMDB");
        }
    }
}

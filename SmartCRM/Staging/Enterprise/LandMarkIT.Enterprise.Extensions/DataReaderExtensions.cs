using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LandmarkIT.Enterprise.Extensions
{
    public static class DataReaderExtensions
    {
        public static IDataReader MapTo<T>(this IDataReader reader, out List<T> result)
        {
            result = new List<T>();
            while (reader.Read())
            {
                var obj = Activator.CreateInstance<T>();
                foreach (var prop in obj.GetType().GetProperties())
                {
                    if (!Equals(reader[prop.Name], DBNull.Value))
                    {
                        prop.SetValue(obj, reader[prop.Name], null);
                    }
                }
                result.Add(obj);
            }
          
            return reader;
        }

        public static IDataReader MoveNext(this IDataReader reader)
        {
            reader.NextResult();
            return reader;
        }
    }
}

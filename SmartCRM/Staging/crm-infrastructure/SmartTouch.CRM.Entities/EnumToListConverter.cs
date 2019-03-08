using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Entities
{
   public static class EnumToListConverter
    {
        public static Dictionary<int, string> convertEnumToDictionary(Type enumType)
        {
            return Enum.GetValues(enumType).Cast<int>().ToDictionary(e => e, e => Enum.GetName(enumType, e));
        }

        public static IEnumerable<dynamic> convertEnumToList<T>()
        {
            Type type = typeof(T);
            IEnumerable<dynamic> list = Enum.GetValues(type).Cast<byte>().Select(e => new { TypeId = e, Name = Enum.GetName(type, e) }).ToList();
            return list;
        }
    }
    public static class EnumUtils
    {
        public static IEnumerable<int> GetValuesByModule(this LeadScoreConditionType type,int module)
        {
            return typeof(LeadScoreConditionType).GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(m=>
                    {
                        var x = (MessageModuleAttribute)m.GetCustomAttributes(typeof(MessageModuleAttribute), false).FirstOrDefault();
                        return x.Modules.Contains(module) ? true : false;
                    }).Select(m=>
                        {
                            return (int)(LeadScoreConditionType)m.GetValue(null);
                        });
        }
    }
}

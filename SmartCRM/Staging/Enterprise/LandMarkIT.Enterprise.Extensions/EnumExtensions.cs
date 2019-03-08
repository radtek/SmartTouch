using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace LandmarkIT.Enterprise.Extensions
{
    public static class EnumExtensions
    {
        public static string GetDisplayName(this Enum enumValue)
        {
            var attribute = enumValue.GetType().GetMember(enumValue.ToString()).First().GetCustomAttributes<DisplayAttribute>().FirstOrDefault();
            string name = string.Empty;
            if(attribute != null)
            {
                name = attribute.GetName();
            }
            else
            {
                name = enumValue.ToString();
            }
            return name;
        }
    }
}

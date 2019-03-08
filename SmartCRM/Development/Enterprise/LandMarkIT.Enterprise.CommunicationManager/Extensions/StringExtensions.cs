using System.Collections.Generic;

namespace LandmarkIT.Enterprise.CommunicationManager.Extensions
{
    public static class StringExtensions
    {
        public static string ToPlainString(this List<string> values)
        {
            var result = string.Empty;
            if (null != values && values.Count > 0)
            {
                result = string.Join(",", values.ToArray());
            }
            return result;
        }
    }
}

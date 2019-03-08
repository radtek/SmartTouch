using SmartTouch.CRM.ApplicationServices.Exceptions;
using System;
using System.Collections.Generic;
using System.Net;

namespace SmartTouch.CRM.WebService.Helpers
{
    /// <summary>
    /// for Exception in Dictionary
    /// </summary>
    public static class ExceptionDictionary
    {
        /// <summary>
        /// Convert To Http Status Code
        /// </summary>
        /// <param name="exception">exception</param>
        /// <returns></returns>
        public static HttpStatusCode ConvertToHttpStatusCode(this Exception exception)
        {
            Dictionary<Type, HttpStatusCode> dict = GetExceptionDictionary();
            if (dict.ContainsKey(exception.GetType()))
            {
                return dict[exception.GetType()];
            }
            return dict[typeof(Exception)];
        }

        /// <summary>
        /// Get Exception Dictionary
        /// </summary>
        /// <returns></returns>
        private static Dictionary<Type, HttpStatusCode> GetExceptionDictionary()
        {
            Dictionary<Type, HttpStatusCode> dict = new Dictionary<Type, HttpStatusCode>();
            dict[typeof(ResourceNotFoundException)] = HttpStatusCode.NotFound;
            dict[typeof(Exception)] = HttpStatusCode.InternalServerError;
            return dict;
        }
    }
}
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;

namespace SmartTouch.CRM.Web.Utilities
{
    public static class HtmlHelperExtensions
    {
        public static MvcHtmlString EncodedActionLink(this HtmlHelper htmlHelper, string linkText, string actionName, string controllerName, object routeValues, object htmlAttributes)
        {
            string queryString = string.Empty;
            string htmlAttributesString = string.Empty;
            if (routeValues != null)
            {
                RouteValueDictionary d = new RouteValueDictionary(routeValues);
                for (int i = 0; i < d.Keys.Count; i++)
                {
                    if (i > 0)
                    {
                        queryString += "?";
                    }
                    queryString += d.Keys.ElementAt(i) + "=" + d.Values.ElementAt(i);
                }
            }

            if (htmlAttributes != null)
            {
                RouteValueDictionary d = new RouteValueDictionary(htmlAttributes);
                for (int i = 0; i < d.Keys.Count; i++)
                {
                    htmlAttributesString += " " + d.Keys.ElementAt(i) + "=" + d.Values.ElementAt(i);
                }
            }

            //<a href="/Answer?questionId=14">What is Entity Framework??</a>
            StringBuilder ancor = new StringBuilder();
            ancor.Append("<a ");
            if (!string.IsNullOrEmpty(htmlAttributesString))
            {
                ancor.Append(htmlAttributesString);
            }
            ancor.Append(" href='");
            if (!string.IsNullOrEmpty(controllerName))
            {
                ancor.Append("/" + controllerName);
            }

            if (!string.Equals(actionName,"Index",StringComparison.OrdinalIgnoreCase))
            {
                ancor.Append("/" + actionName);
            }
            if (!string.IsNullOrEmpty(queryString))
            {
                ancor.Append("?q=" + Encrypt(queryString));
            }
            ancor.Append("'");
            ancor.Append(">");
            ancor.Append(linkText);
            ancor.Append("</a>");
            return new MvcHtmlString(ancor.ToString());
        }

        private static string Encrypt(string plainText)
        {
            string key = "jdsg432387#";
            byte[] EncryptKey = { };
            byte[] IV = { 55, 34, 87, 64, 87, 195, 54, 21 };
            EncryptKey = System.Text.Encoding.UTF8.GetBytes(key.Substring(0, 8));
            using(DESCryptoServiceProvider des = new DESCryptoServiceProvider())
            {
                byte[] inputByte = Encoding.UTF8.GetBytes(plainText);
                MemoryStream mStream = new MemoryStream();
                CryptoStream cStream = new CryptoStream(mStream, des.CreateEncryptor(EncryptKey, IV), CryptoStreamMode.Write);
                cStream.Write(inputByte, 0, inputByte.Length);
                cStream.FlushFinalBlock();
                return Convert.ToBase64String(mStream.ToArray());
            }
        }
    }
}
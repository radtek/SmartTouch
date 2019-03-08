using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    /// <summary>
    /// 
    /// </summary>
    public static class GetDocumentationXML
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static XmlDocument DocumentationXML()
        {
            var documentLocation = System.Configuration.ConfigurationManager.AppSettings["xmldocument"];
            Assembly asm = Assembly.GetExecutingAssembly();
            

            XmlDocument doc = new XmlDocument();
            doc.Load(documentLocation);
            
            
            return doc;
        }
    }
}

using SmartTouch.CRM.Plugins.Utilities.ImplicitSync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http.Routing;
using System.Xml.Linq;
using System.Threading;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Plugins.Controllers;
using LandmarkIT.Enterprise.Utilities.Logging;

namespace SmartTouch.CRM.Plugins.Utilities
{
    public class RequestHandler : DelegatingHandler
    {

        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var requestInfo = await request.Content.ReadAsStringAsync();
           // Logger.Current.Informational("RequestInfo: " + requestInfo);
            var context = HttpContext.Current;
            string action = context.Request.Params["Action"] ?? string.Empty;
            string session = context.Request.Params["Session"] ?? string.Empty;
           // Logger.Current.Informational("Request received to perform : " + action);

            XDocument doc = new XDocument();
            if (!string.IsNullOrEmpty(requestInfo))
            {
                doc = XDocument.Parse(requestInfo);
                doc.Elements("Session").FirstOrDefault().Attribute("SessionKey").Value = session;                
            }
            else
            {
                doc.Add(new XElement("Session", new XAttribute("SessionKey", session)));
                doc = XDocument.Parse(doc.ToString());
            }
                        
            var response = Invoke<WASyncWSController>(action, doc);

            return response; 

        }

        private HttpResponseMessage Invoke<T>(string methodName, params object[] inputs) where T : new()
        {
            T instance = new T();
            MethodInfo method = typeof(T).GetMethod(methodName);
            Logger.Current.Informational("Method idenfied: " + method);
            ResponseXml response = new ResponseXml(string.Empty, methodName); ;
           // Logger.Current.Informational("Request Inputs: \n"+ inputs[0].ToString());
            try
            {
                response = (ResponseXml)method.Invoke(instance, inputs);
            }
            catch (WAException ex)
            {
                Logger.Current.Error("A WAException occured while processing " + method, ex);
                response.RetStatus = ex.Status;
                response.ErrorString = ex.Message;
            }
            catch (Exception ex)
            {
                Logger.Current.Error("An exception occured while processing " + method, ex);
                response.RetStatus = -1;
                response.ErrorString = ex.Message;
            }
            var xml = response.Xml.ToString();

            return new HttpResponseMessage
            {
                Content = new StringContent(xml, Encoding.UTF8, "text/xml")
            };

            
        }

    }
}
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web.Http;
using System.Web.Http.Hosting;
using System.Net.Http;

namespace SmartTouch.CRM.WebService.Tests
{
    [TestClass]
    public class ControllerTestBase
    {
        protected void SetupControllerTests(ApiController controller, string url, HttpMethod httpMethod)
        {
            var config = new HttpConfiguration();
            var request = new HttpRequestMessage(httpMethod, url);
            var route = config.Routes.MapHttpRoute("Default", "api/{controller}/{id}");

            controller.Request = request;
            controller.Request.Properties[HttpPropertyKeys.HttpConfigurationKey] = config;
        }
    }
}

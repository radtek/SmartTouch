using SmartTouch.CRM.WebService.Areas.HelpPage.ModelDescriptions;
using SmartTouch.CRM.WebService.Areas.HelpPage.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Mvc;
using System.Linq;
using System.Collections.ObjectModel;

namespace SmartTouch.CRM.WebService.Areas.HelpPage.Controllers
{
    /// <summary>
    /// The controller that will handle requests for the help page.
    /// </summary>
    public class HelpController : Controller
    {
        private const string ErrorViewName = "Error";
        /// <summary>
        /// Help controller
        /// </summary>
        public HelpController()
            : this(GlobalConfiguration.Configuration)
        {
        }
        /// <summary>
        /// Help controller constructor
        /// </summary>
        /// <param name="config"></param>
        public HelpController(HttpConfiguration config)
        {
            Configuration = config;
        }

        /// <summary>
        /// Configuration
        /// </summary>
        public HttpConfiguration Configuration { get; private set; }
        /// <summary>
        /// Geting apiDescriptions
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            ViewBag.DocumentationProvider = Configuration.Services.GetDocumentationProvider();
            return View(Configuration.Services.GetApiExplorer().ApiDescriptions);
        }

        /// <summary>
        /// Geting api details
        /// </summary>
        /// <param name="apiId">apiId</param>
        /// <returns></returns>
        public ActionResult Api(string apiId)
        {
            if (!String.IsNullOrEmpty(apiId))
            {
                HelpPageApiModel apiModel = Configuration.GetHelpPageApiModel(apiId);
                if (apiModel != null)
                {
                    return View(apiModel);
                }
            }

            return View(ErrorViewName);
        }
        /// <summary>
        /// Getting resource model
        /// </summary>
        /// <param name="modelName">modelName</param>
        /// <returns></returns>
        public ActionResult ResourceModel(string modelName)
        {
            if (!String.IsNullOrEmpty(modelName))
            {
                ModelDescriptionGenerator modelDescriptionGenerator = Configuration.GetModelDescriptionGenerator();
                ModelDescription modelDescription;
                if (modelDescriptionGenerator.GeneratedModels.TryGetValue(modelName, out modelDescription))
                {
                    if(modelName == "PersonViewModel")
                    {
                        List<string> excludedProperties = new List<string>() { "FirstName", "LastName", "CompanyName", "Title", "SelectedLeadSource", "LifecycleStage", "ContactImageUrl", "AccountID", "Communication", "Addresses", "Phones", "Emails", "CustomFields", "FormId" };
                        var modelDescripter = modelDescription as ComplexTypeModelDescription;
                        modelDescripter.Properties = new ObservableCollection<ParameterDescription>(modelDescripter.Properties.Where(p => excludedProperties.Contains(p.Name)));

                    }

                    return View(modelDescription);
                }
            }

            return View(ErrorViewName);
        }

        /// <summary>
        /// Gets the API Method Description
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="methodId"></param>
        /// <returns></returns>
        public ActionResult GetMethodDescription(string methodName, string methodId, string displayName)
        {
            HelpPageApiModel aPI = Configuration.GetHelpPageApiModel(methodId);
            ViewBag.DisplayName = displayName;
            IList<SmartTouchApiParameterDescription> parameterDescriptions = new List<SmartTouchApiParameterDescription>();
            if(aPI != null && aPI.RequestBodyParameters!= null)
            {
                foreach(ParameterDescription paramDescription in aPI.RequestBodyParameters)
                {
                    parameterDescriptions.Add(new SmartTouchApiParameterDescription()
                    {
                        Name = paramDescription.Name,
                        Documentation = paramDescription.Documentation,
                        ParameterType = paramDescription.TypeDescription.Name
                    });
                }
            }
            return PartialView(methodName, parameterDescriptions);
        }

        /// <summary>
        /// Redirects to SmartTouch Web API Documentation
        /// </summary>
        /// <returns></returns>
        public ActionResult Docs()
        {
            var apiDescriptions = Configuration.Services.GetApiExplorer().ApiDescriptions.ToList();
            IEnumerable<SmartTouchAPIDescription> apis = apiDescriptions.Select(c => new SmartTouchAPIDescription()
            {
                ID = c.ID.ToString(),
                Name = c.GetFriendlyId(),
                Documentation = c.Documentation,
                ParameterDescriptions = c.ParameterDescriptions.Select(p => new SmartTouchApiParameterDescription()
                {
                    Name = p.Name,
                    Documentation = p.Documentation,
                    ParameterType = p.ParameterDescriptor.ParameterType.Name

                })
            });

            return View("APIDocument", apis);
        }
    }
}
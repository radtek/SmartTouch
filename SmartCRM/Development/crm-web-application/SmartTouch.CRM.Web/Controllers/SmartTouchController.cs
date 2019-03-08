using Kendo.Mvc.UI;
using LandmarkIT.Enterprise.Utilities.Logging;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Web.Utilities;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Principal;
using System.Threading;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Script.Serialization;
using ARM = Antlr.Runtime.Misc;

namespace SmartTouch.CRM.Web.Controllers
{
    [SmartTouchAOSPActionLogFilter]
    [SmartTouchHandleException]
    public class SmartTouchController : Controller
    {
        readonly ICachingService cacheService;

        public SmartTouchController()
        {
            this.cacheService = IoC.Container.GetInstance<ICachingService>();
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            /*Check if this action has AllowAnonymousAttribute*/
            string actionname = filterContext.ActionDescriptor.ActionName;
            int AccountID = Thread.CurrentPrincipal.Identity.ToAccountID();
            if (AccountID > 0 && (actionname != "Suspended" && actionname != "Maintanance"))
            {
                AccountViewModel account = cacheService.GetAccount(AccountID);
                if (account != null)
                {
                    if (account.Status == 3)/*paused*/
                    {
                        filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new
                        {
                            controller = "Error",
                            action = "Suspended"
                        }));
                        return;
                    }
                    else if (account.Status == 5)
                    {
                        filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new
                        {
                            controller = "Error",
                            action = "Maintanance"
                        }));
                        return;
                    }
                }
            }
            var attributes = filterContext.ActionDescriptor.GetCustomAttributes(true);
            #region route data
            var routeData = Request.RequestContext.RouteData;
            this.CurrentArea = routeData.DataTokens["area"] as string;
            this.CurrentController = routeData.GetRequiredString("controller");
            this.CurrentAction = routeData.GetRequiredString("action");
            #endregion
            if (attributes.Any(a => a is MenuTypeAttribute))
                TempData[Keys.MENU_TYPE] = ((MenuTypeAttribute)attributes.Where(a => a is MenuTypeAttribute).Single()).Category;
            else
                TempData[Keys.MENU_TYPE] = default(MenuCategory);
            if (attributes.Any(a => a is MenuTypeAttribute))
                TempData[Keys.LEFT_MENU_TYPE] = ((MenuTypeAttribute)attributes.Where(a => a is MenuTypeAttribute).Single()).LeftMenuType;
            else
                TempData[Keys.LEFT_MENU_TYPE] = default(MenuCategory);
            if (attributes.Any(a => a is AllowAnonymousAttribute))
                return;
            /*must login*/
            if (!filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                Logger.Current.Informational("Request not authenticated 1 " + filterContext.HttpContext.User.Identity.ToUserEmail());
                filterContext.Result = new HttpUnauthorizedResult();
                return;
            }
            /*check authorization*/
            if (attributes.Any(a => a is AppFeatureAttribute) && ((AppFeatureAttribute)attributes.Where(a => a is AppFeatureAttribute).Single()).Feature != AppFeatures.NOT_APPLICABLE && !Thread.CurrentPrincipal.Identity.IsInFeature(((AppFeatureAttribute)attributes.Where(a => a is AppFeatureAttribute).Single()).Feature))
            {
                Logger.Current.Informational("Request not authenticated 2 " + filterContext.HttpContext.User.Identity.ToUserEmail());
                filterContext.Result = new HttpUnauthorizedResult();
            }
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            System.Diagnostics.EventLog.WriteEntry("Application", filterContext.Exception.ToString(), System.Diagnostics.EventLogEntryType.Error, 1616);
            base.OnException(filterContext);
        }

        protected virtual string DomainName
        {
            get
            {
                return "qasandbox.smarttouch.net";//Request.Url.Host.ToLower().Replace("www.", string.Empty); //"qasandbox.smarttouch.net";//
            }
        }

        protected virtual string GetPropertyName<T, TR>(Expression<ARM.Func<T, TR>> property)
        {
            var propertyInfo = (property.Body as MemberExpression).Member as PropertyInfo;
            if (propertyInfo == null)
            {
                throw new ArgumentException("The lambda expression 'property' should point to a valid Property");
            }
            return propertyInfo.Name;
        }

        protected IIdentity Identity
        {
            get
            {
                return Thread.CurrentPrincipal.Identity;
            }
        }

        public string ReadMyCookie(string strValue)
        {
            string strValues = string.Empty;
            if (Request.Cookies[strValue] != null)
            {
                strValues = Request.Cookies[strValue].Value;
            }
            return strValues;
        }

        new protected JsonResult Json(object data, JsonRequestBehavior jsonBehaviour)
        {
            if (data is DataSourceResult)
            {
                Logger.Current.Informational("Request received for serializing datasource type data to json result");
                var serializer = new JavaScriptSerializer();
                serializer.MaxJsonLength = Int32.MaxValue;
                var valid = new JsonResult();
                try
                {
                    valid = serializer.ConvertToType<JsonResult>(base.Json(data, jsonBehaviour));
                }
                catch (Exception ex)
                {
                    Logger.Current.Error("An error occured while serializing data to json result : ", ex);
                    throw ex;
                }
                return valid;
            }
            else
            {
                var jsonResult = base.Json(data, jsonBehaviour);
                jsonResult.MaxJsonLength = Int32.MaxValue;
                return jsonResult;
            }
        }

        #region Properties
        private string _CurrentArea;

        private string _CurrentController;

        private string _CurrentAction;

        public string CurrentArea
        {
            get
            {
                return _CurrentArea == null ? string.Empty : _CurrentArea.ToLower();
            }
            set
            {
                _CurrentArea = value;
            }
        }

        public string CurrentController
        {
            get
            {
                return _CurrentController == null ? string.Empty : _CurrentController.ToLower();
            }
            set
            {
                _CurrentController = value;
            }
        }

        public string CurrentAction
        {
            get
            {
                return _CurrentAction == null ? string.Empty : _CurrentAction.ToLower();
            }
            set
            {
                _CurrentAction = value;
            }
        }
        #endregion
    }
}

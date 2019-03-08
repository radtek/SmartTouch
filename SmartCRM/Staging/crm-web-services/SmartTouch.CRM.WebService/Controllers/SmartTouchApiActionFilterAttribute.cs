using System.Linq;
using System.Web.Mvc;

namespace SmartTouch.CRM.WebService.Controllers
{
    /// <summary>
    /// Creating ction filter class
    /// </summary>
    public class SmartTouchApiActionFilterAttribute : System.Web.Mvc.ActionFilterAttribute
    {
        /// <summary>
        /// Autherization
        /// </summary>
        /// <param name="filterContext">Filter Content</param>
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var attributes = filterContext.ActionDescriptor.GetCustomAttributes(true);
            if (attributes.Any(a => a is System.Web.Mvc.AllowAnonymousAttribute)) return;

            // must login
            if (!filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                filterContext.Result = new HttpUnauthorizedResult();
                return;
            }
        }        
    }
}
using SmartTouch.CRM.Web.Utilities;
using System.Web;
using System.Web.Mvc;

namespace SmartTouch.CRM.Web
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new SmartTouchHandleExceptionAttribute());
        }
    }
}

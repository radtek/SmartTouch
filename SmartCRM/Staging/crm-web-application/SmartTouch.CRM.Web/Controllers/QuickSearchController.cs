using System.Web.Mvc;

namespace SmartTouch.CRM.Web.Controllers
{
    public class QuickSearchController : SmartTouchController
    {        
        public ActionResult QuickSearch(string query)
        {
            return View();
        }
	}
}
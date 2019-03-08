using System.Collections.Generic;
using System.Web;

namespace SmartTouch.CRM.Web.Utilities
{
    public class CacheManager
    {
        public List<MenuItem> MenuItems
        {
            get { return HttpContext.Current.Cache[Keys.CACHE_MENU_ITEMS] as List<MenuItem>; }
        }
    }
}
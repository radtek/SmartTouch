using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    public interface IMenuViewModel
    {
        int MenuID { get; set; }
        int MenuCategoryID { get; set; }
        string Name { get; set; }
        string ToolTip { get; set; }
        string Area { get; set; }
        string Controller { get; set; }
        string Action { get; set; }
        string ContentPartialView { get; set; }
        string CssClass { get; set; }
        int ParentMenuID { get; set; }
        int SortingID { get; set; }

    }

    public class MenuViewModel : IMenuViewModel
    {
        public int MenuID { get; set; }
        public int MenuCategoryID { get; set; }
        public string Name { get; set; }
        public string ToolTip { get; set; }
        public string Area { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }
        public string ContentPartialView { get; set; }
        public string CssClass { get; set; }
        public int ParentMenuID { get; set; }
        public int SortingID { get; set; }
    }

    public interface IMenuListViewModel
    {
        IEnumerable<MenuViewModel> Menu { get; set; }
    }

    public class MenuListViewModel : IMenuListViewModel
    {
        public IEnumerable<MenuViewModel> Menu { get; set; }
    }
}

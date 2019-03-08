using SmartTouch.CRM.Repository.Database;
using System.Collections.Generic;

namespace SmartTouch.CRM.Web.Utilities
{
    public class MenuItem
    {
        public MenuItem()
        {
            this.Children = new List<MenuItem>();
        }

        public short MenuId { get; set; }
        public MenuCategory Category { get; set; }
        public string Title { get; set; }
        public string ToolTip { get; set; }
        public string Area { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }
        public string Content { get; set; }
        public short? ParentMenuId { get; set; }
        public byte SortingId { get; set; }
        public string CssClass { get; set; }
        public bool? OpenAsMenuView { get; set; }
        public int? ModuleOperationsMapID { get; set; }
        public byte ModuleId { get; set; }
        public IList<MenuItem> Children { get; set; }

        public static implicit operator MenuItem(MenuDB source)
        {
            return new MenuItem
            {
                MenuId = source.MenuID,
                Title = source.Name,
                ToolTip = source.ToolTip,
                Area = source.Area,
                Controller = source.Controller,
                Action = source.Action,
                ParentMenuId = source.ParentMenuID,
                Content = source.ContentPartialView,
                SortingId = source.SortingID,
                CssClass = source.CssClass,
                OpenAsMenuView = source.OpenAsMenuView,
                ModuleId = source.ModuleID,
                Category = (MenuCategory)System.Enum.Parse( typeof(MenuCategory), source.MenuCategory.Name, true )
            };
        }
    }

    public class MenuItemViewModel
    {
        public List<MenuItem> TopMenuItems { get; set; }
        public List<MenuItem> LeftMenuItems { get; set; }
        public bool IsAccountSettings { get; set; }
    }
}
using System;

namespace SmartTouch.CRM.Web.Utilities
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class MenuTypeAttribute : Attribute
    {
        public MenuCategory Category { get; set; }

        public MenuCategory LeftMenuType { get; set; }

        public MenuTypeAttribute(MenuCategory category, MenuCategory leftMenuType)
        {
            this.Category = category;
            this.LeftMenuType = leftMenuType;
        }
    }
}
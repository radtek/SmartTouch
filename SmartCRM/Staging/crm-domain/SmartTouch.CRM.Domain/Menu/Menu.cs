using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SmartTouch.CRM.Infrastructure.Domain;
using SmartTouch.CRM.Entities;

namespace SmartTouch.CRM.Domain.Menu
{
    public class Menu : EntityBase<int>, IAggregateRoot
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

        protected override void Validate()
        {
        }
    }
}

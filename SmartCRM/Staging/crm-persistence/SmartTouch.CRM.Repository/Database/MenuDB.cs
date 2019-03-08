using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
    public class MenuDB
    {
        [Key]
        public Int16 MenuID { get; set; }
        public byte MenuCategoryID { get; set; }
        [ForeignKey("MenuCategoryID")]
        public MenuCategoryDB MenuCategory { get; set; }
        public string Name { get; set; }
        public string ToolTip { get; set; }
        public string Area { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }
        public string ContentPartialView { get; set; }
        public string CssClass { get; set; }
        public Int16? ParentMenuID { get; set; }
        public byte SortingID { get; set; }
        public bool? OpenAsMenuView { get; set; }
        public int? ModuleOperationsMapID { get; set; }
        [ForeignKey("Module")]
        public virtual byte ModuleID { get; set; }
        public virtual ModulesDb Module { get; set; }
    }

    public class MenuCategoryDB
    {
        [Key]
        public byte MenuCategoryID { get; set; }
        public string Name { get; set; }
    }
}

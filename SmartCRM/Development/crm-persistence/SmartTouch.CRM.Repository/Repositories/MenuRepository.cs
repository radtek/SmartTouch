using SmartTouch.CRM.Repository.Database;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace SmartTouch.CRM.Repository.Repositories
{
    public class MenuRepository
    {
        /// <summary>
        /// Gets the menu.
        /// </summary>
        /// <param name="currentMenuView">The current menu view.</param>
        /// <param name="leftMenuView">The left menu view.</param>
        /// <param name="moduleOperationsArray">The module operations array.</param>
        /// <returns></returns>
        public List<MenuDB> GetMenu(string currentMenuView, string leftMenuView,List<byte> moduleOperationsArray)
        {
            var db = new ObjectContextFactory().Create();
            var Menus = db.Menu.Include(i => i.MenuCategory).Where(s => (s.MenuCategory.Name == currentMenuView || s.MenuCategory.Name == leftMenuView) && moduleOperationsArray.Contains(s.ModuleID)).ToList();
            return Menus;
        }

        /// <summary>
        /// Gets the left menu.
        /// </summary>
        /// <param name="moduleOperationsArray">The module operations array.</param>
        /// <returns></returns>
        public List<MenuDB> GetLeftMenu(int[] moduleOperationsArray)
        {

            var db = new ObjectContextFactory().Create();
            int[] rolePermissions = moduleOperationsArray;
            List<MenuDB> lstMenu = db.Menu.Where(c => c.ToolTip == null).ToList();

            List<MenuDB> query = new List<MenuDB>();
            foreach (var rolePermission in rolePermissions)
            {
                IList<MenuDB> menuDb = lstMenu.Where(l => l.ModuleOperationsMapID == rolePermission).ToList();
                if (menuDb != null)
                {
                    foreach (var menu in menuDb)
                    {
                        query.Add(menu);
                    }
                }
            }
            return query;
        }
    }
}

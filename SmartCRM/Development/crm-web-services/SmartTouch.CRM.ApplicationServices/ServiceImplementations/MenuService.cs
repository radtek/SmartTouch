using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.Repository.Repositories;
using System.Collections.Generic;

namespace SmartTouch.CRM.ApplicationServices.ServiceImplementations
{
    /// <summary>
    /// 
    /// </summary>
    public class MenuService : IMenuServices
    {
        /// <summary>
        /// Gets the menu.
        /// </summary>
        /// <param name="topMenuCategory">The top menu category.</param>
        /// <param name="leftMenucategory">The left menucategory.</param>
        /// <param name="moduleOperations">The module operations.</param>
        /// <returns></returns>
        public List<Repository.Database.MenuDB> GetMenu(string topMenuCategory,string leftMenucategory, List<byte> moduleOperations)
        {
            MenuRepository menuRepository = new MenuRepository();
            List<Repository.Database.MenuDB> menu = menuRepository.GetMenu(topMenuCategory, leftMenucategory, moduleOperations);
            return menu;
        }

        /// <summary>
        /// Gets the left menu.
        /// </summary>
        /// <param name="moduleOperations">The module operations.</param>
        /// <returns></returns>
        public List<Repository.Database.MenuDB> GetLeftMenu(int[] moduleOperations)
        {
            MenuRepository menuRepository = new MenuRepository();
            List<Repository.Database.MenuDB> menu = menuRepository.GetLeftMenu(moduleOperations);
            return menu;
        }
    }
}

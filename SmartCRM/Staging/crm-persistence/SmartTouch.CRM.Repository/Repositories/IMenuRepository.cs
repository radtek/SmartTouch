using SmartTouch.CRM.Domain.Menu;
using SmartTouch.CRM.Infrastructure.Domain;
using SmartTouch.CRM.Repository.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SmartTouch.CRM.Repository.Repositories
{
    public interface IMenuRepository : IRepository<Menu, int>
    {
        List<MenuDB> GetMenu(string currentView);

        IEnumerable<Menu> FindBy(int id);

        MenuDB ConvertToDatabaseType(Menu domainType, CRMDb context);

        void PersistValueObjects(Menu domainType, MenuDB dbType, CRMDb context);
        
    }
}

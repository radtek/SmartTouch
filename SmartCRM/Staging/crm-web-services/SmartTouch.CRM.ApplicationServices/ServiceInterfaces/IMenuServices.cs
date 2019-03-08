using SmartTouch.CRM.ApplicationServices.Messaging.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ServiceInterfaces
{
    public interface IMenuServices
    {
        List<Repository.Database.MenuDB> GetMenu(string category,string leftMenuItems, List<byte> moduleOperations);
        List<Repository.Database.MenuDB> GetLeftMenu(int[] moduleOperations);
    }
}

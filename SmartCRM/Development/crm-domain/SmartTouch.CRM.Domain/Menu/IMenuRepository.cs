using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Menu
{
    public interface IMenuRepository
    {
        void GetMenu(string currentView);

    }
}

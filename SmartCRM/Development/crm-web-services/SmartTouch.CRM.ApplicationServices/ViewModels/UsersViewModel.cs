using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    public interface IUsersViewModel
    {
        int UserID { get; set; }
        string UserName { get; set; }
    }

    public class UsersViewModel : IUsersViewModel
    {
        public int UserID { get; set; }
        public string UserName { get; set; }
    }
}

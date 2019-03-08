using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{

    public interface IMyProfileViewModel
    {
         UserViewModel userViewModel { get; set; }
         UserSettingsViewModel userSettingsViewModel { get; set; }
    }

    public class MyProfileViewModel : IMyProfileViewModel
    {
        public UserViewModel userViewModel { get; set; }
        public UserSettingsViewModel userSettingsViewModel { get; set; }
    }
}

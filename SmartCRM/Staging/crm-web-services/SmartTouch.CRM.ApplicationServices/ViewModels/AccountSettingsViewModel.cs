using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    /// <summary>
    /// 
    /// </summary>
    public class AccountSettingsViewModel
    {
        /// <summary>
        /// 
        /// </summary>
        public int AccountSettingsID { get; set; }
        /// <summary>
        /// 
        /// </summary> 
        public int AccountID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public short StatusID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ViewName { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
    public class EnvironmentSettingsDb
    {
        [Key]
        public int EnvironmentSettingID { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
    public class FormNotificationSettingsDb
    {
        [Key]
        public int FormNotificationSettingID { get; set; }

        [ForeignKey("Form")]
        public int FormID { get; set; }
        public virtual FormsDb Form { get; set; }
    }
}

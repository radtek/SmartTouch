using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
    public class UserActivitiesDb
    {
        [Key]
        public byte UserActivityID { get;set; }
        public string ActivityName { get; set; }
    }
}

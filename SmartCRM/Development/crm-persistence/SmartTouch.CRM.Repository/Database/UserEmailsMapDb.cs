using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
    public class UserEmailsMapDb
    {
        [Key]
        public int UserEmailsMapID { get; set; }
        public int UserID { get; set; }
        public int EmailID { get; set; }       
    }
}

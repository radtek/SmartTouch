using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
    public class DataAccessPermissionDb
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AccountDataAccessPermissionID { get; set; }
        public int AccountID { get; set; }
        public byte ModuleID { get; set; }
        public bool IsPrivate { get; set; }
    }
}

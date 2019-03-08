using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
    public class SmartSearchContactsDb
    {
        [Key]
        public int ResultID { get; set; }
        public int SearchDefinitionID { get; set; }
        public int AccountID { get; set; }
        public int ContactID { get; set; }
        public bool IsActive { get; set; }
    }
}

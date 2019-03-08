using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTouch.CRM.Repository.Database
{
    public class SpamIPAddressesDb
    {
        [Key]
        public int SpamIPAddressID { get; set; }
        public string IPAddress { get; set; }
        public bool IsSpam { get; set; }
        public int AcountID { get; set; }
    }
}

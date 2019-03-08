using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
    public class ContactSourceDb
    {
        [Key]
        public byte SourceID { get; set; }
        public string FirstSource { get; set; }
    }
}

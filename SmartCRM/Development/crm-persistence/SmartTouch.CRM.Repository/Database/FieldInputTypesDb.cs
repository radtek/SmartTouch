using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
    public class FieldInputTypesDb
    {
        [Key]
        public byte FieldInputTypeID { get; set; }      
        public string Type { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace SmartTouch.CRM.Repository.Database
{
   public class SubmittedFormFieldDataDb
    {
        [Key]
        public int SubmittedFormFieldDataID { get; set; }
        public int SubmittedFormDataID { get; set; }
        public string Field { get; set; }
        public string Value { get; set; }
    }
}

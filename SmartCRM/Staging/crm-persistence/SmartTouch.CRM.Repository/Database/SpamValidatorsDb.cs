using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTouch.CRM.Repository.Database
{
    public class SpamValidatorsDb
    {
        [Key]
        public int SpamValidatorID { get; set; }
        public string Validator { get; set; }
        public string Value { get; set; }
        public int AccountID { get; set; }
        public bool RunValidation { get; set; }
        public int Order { get; set; }
    }
}

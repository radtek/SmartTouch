using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
    public class CurrenciesDb
    {
        [Key]
        public byte CurrencyID { get; set; }
        public string Symbol { get; set; }
        public string Format { get; set; }
    }
}

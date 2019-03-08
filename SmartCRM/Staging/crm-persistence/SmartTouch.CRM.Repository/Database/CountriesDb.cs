using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTouch.CRM.Repository.Database
{
    public class CountriesDb
    {
        [Key]
        public string CountryID { get; set; }
        public string CountryName { get; set; }
    }
}

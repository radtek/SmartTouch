using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTouch.CRM.Repository.Database
{
    public class StatesDb
    {
        [Key]
        public string StateID { get; set; }
        public string StateName { get; set; }
        
        [ForeignKey("Country")]
        public virtual string CountryID { get; set; }
        public virtual CountriesDb Country { get; set; }
    }
}

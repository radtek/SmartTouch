using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
    public class SearchPredicateTypesDb
    {
        [Key]
        public Int16 SearchPredicateTypeID { get; set; }
        public string PredicateType { get; set; }
    }
}

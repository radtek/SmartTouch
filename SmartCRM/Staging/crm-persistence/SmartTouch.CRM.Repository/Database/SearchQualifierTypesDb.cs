using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
    public class SearchQualifierTypesDb
    {
        [Key]
        public Int16 SearchQualifierTypeID { get; set; }
        public string QualifierName { get; set; }
    }
}

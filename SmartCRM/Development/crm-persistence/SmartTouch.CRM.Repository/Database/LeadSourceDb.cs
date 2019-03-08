using System;
using System.ComponentModel.DataAnnotations;

namespace SmartTouch.CRM.Repository.Database
{
    public class LeadSourceDb
    {
        [Key]
        public Int16 LeadSourceID { get; set; }
        public string Name { get; set; }
    }
}

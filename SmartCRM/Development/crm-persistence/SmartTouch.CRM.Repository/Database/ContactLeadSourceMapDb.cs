using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTouch.CRM.Repository.Database
{
    public class ContactLeadSourceMapDb
    {
        [Key]
        public int ContactLeadSourceMapID { get; set; }

        [ForeignKey("LeadSource")]
        public virtual Int16 LeadSouceID { get; set; }
        public virtual DropdownValueDb LeadSource { get; set; }

        [ForeignKey("Contact")]
        public virtual int ContactID { get; set; }
        public virtual ContactsDb Contact { get; set; }

        public bool IsPrimaryLeadSource { get; set; }
        public DateTime LastUpdatedDate { get; set; }
    }
}

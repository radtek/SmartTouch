using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTouch.CRM.Repository.Database
{
    public class ContactLeadAdapterMapDb
    {
        [Key]
        public int ContactLeadAdapterMapID { get; set; }

        [ForeignKey("Contacts")]
        public virtual int ContactID { get; set; }
        public virtual ContactsDb Contacts { get; set; }

        //[ForeignKey("LeadAdapters")]
        //public virtual byte LeadAdapterID { get; set; }
        //public virtual LeadAdapterDb LeadAdapters { get; set; }
    }
}

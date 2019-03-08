using System;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTouch.CRM.Repository.Database
{
    public class ContactCommunityMapDb
    {
        [Key]
        public int ContactCommunityMapID { get; set; }

        [ForeignKey("Contact")]
        public int ContactID { get; set; }
        public virtual ContactsDb Contact { get; set; }

        [ForeignKey("Community")]
        public short CommunityID { get; set; }
        public virtual DropdownValueDb Community { get; set; }

        public DateTime CreatedOn { get; set; }

        [ForeignKey("User")]
        public int? CreatedBy { get; set; }
        public virtual UsersDb User { get; set; }

        public DateTime? LastModifiedOn { get; set; }

        [ForeignKey("User1")]
        public int? LastModifiedBy { get; set; }
        public virtual UsersDb User1 { get; set; }

        public bool IsDeleted { get; set; }
    }
}

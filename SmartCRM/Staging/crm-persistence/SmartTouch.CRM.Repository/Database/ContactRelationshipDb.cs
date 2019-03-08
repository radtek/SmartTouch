using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SmartTouch.CRM.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTouch.CRM.Repository.Database
{
    public class ContactRelationshipDb
    {
        [Key]
        public int ContactRelationshipMapID { get; set; }
        [ForeignKey("Contact")]
        public int ContactID { get; set; }
        public ContactsDb Contact { get; set; }

        //public RelationshipTypes RelationshipType { get; set; }
        [ForeignKey("DropdownValues")]
        public short RelationshipType { get; set; }
        public DropdownValueDb DropdownValues { get; set; }

        [ForeignKey("RelatedContact")]
        public int? RelatedContactID { get; set; }
        public ContactsDb RelatedContact { get; set; }

        [ForeignKey("User")]
        public int? RelatedUserID { get; set; }
        public UsersDb User { get; set; }
        public UsersDb User1 { get; set; }
        [ForeignKey("User1")]
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}

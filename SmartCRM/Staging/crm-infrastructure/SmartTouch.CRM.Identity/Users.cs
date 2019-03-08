using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTouch.CRM.Identity
{
    public class Users
    {
        [Key]
        public int UserID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Company { get; set; }
        public string HomePhone { get; set; }
        public string WorkPhone { get; set; }
        public string MobilePhone { get; set; }
        [Column("PrimaryEmail")]
        public string UserName { get; set; }
        public string Password { get; set; }
        [ForeignKey("Roles")]
        public short RoleID { get; set; }
        public bool IsActive { get; set; }
        public string Title { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public int ModifiedBy { get; set; }
        public DateTime ModifiedOn { get; set; }

        public virtual Roles Roles { get; set; }
    }
}

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTouch.CRM.Identity
{
    public class Roles
    {
        [Key]
        public short RoleID { get; set; }
        [Column("RoleName")]
        public string Name { get; set; }
        [ForeignKey("Accounts")]
        public short AccountID { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public int ModifiedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
        public virtual Accounts Accounts { get; set; }
    }
}

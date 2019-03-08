using System;
using System.ComponentModel.DataAnnotations;

namespace SmartTouch.CRM.Identity
{
    public class Accounts
    {
        [Key]
        public short AccountID { get; set; }
        public string AccountName { get; set; }
        public string AccountCode { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Company { get; set; }
        public string PrimaryEmail { get; set; }
        public string HomePhone { get; set; }
        public string WorkPhone { get; set; }
        public string MobilePhone { get; set; }
        public AccountStatus Status { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public int ModifiedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
    }
}

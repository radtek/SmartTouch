using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SmartTouch.CRM.Entities;

namespace SmartTouch.CRM.Repository.Database
{
   public class UsersDb
    {
       [Key]
       [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
       public int UserID { get; set; }
       [ForeignKey("Account")]
       public int AccountID { get; set; }
       public virtual AccountsDb Account { get; set; }
       public string FirstName { get; set; }
       public string LastName { get; set; }
       public string Title { get; set; }
       public string Company { get; set; }
       public string Password { get; set; }
       public string PrimaryEmail { get; set; }

       [ForeignKey("Role")]
       public virtual short RoleID { get; set; }
       public virtual RolesDb Role { get; set; }

       public string WorkPhone { get; set; }
       public string HomePhone { get; set; }
       public string MobilePhone { get; set; }

       [ForeignKey("Communication")]
       public Nullable<int> CommunicationID { get; set; }
       public virtual CommunicationsDb Communication { get; set; }
  

       public Status Status { get; set; }
       public bool IsDeleted { get; set; }
       public int CreatedBy { get; set; }
       public DateTime CreatedOn { get; set; }
       public DateTime? ModifiedOn { get; set; }
       public Nullable<int> ModifiedBy { get; set; }


       public ICollection<AddressesDb> Addresses { get; set; }
       public DateTime? PasswordResetOn { get; set; }
       public bool? PasswordResetFlag { get; set; }
       public ICollection<AccountEmailsDb> Emails { get; set; }
       public string EmailSignature { get; set; }
       public string PrimaryPhoneType { get; set; }
       [NotMapped]
       public bool? IsSTAdmin { get; set; }
       public bool? HasTourCompleted { get; set; }
    }
}

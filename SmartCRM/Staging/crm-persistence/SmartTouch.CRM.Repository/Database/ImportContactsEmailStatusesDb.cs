using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTouch.CRM.Repository.Database
{
    public class ImportContactsEmailStatusesDb
    {
        [Key]
        public Int64 MailGunVerificationID { get; set; }
        public Guid? ReferenceID { get; set; }
        public int? EmailStatus { get; set; }

    }
}

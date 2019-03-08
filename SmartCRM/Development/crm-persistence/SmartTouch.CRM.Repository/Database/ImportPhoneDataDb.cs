using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTouch.CRM.Repository.Database
{
    public class ImportPhoneData
    {
        [Key]
        public Int32 ImportPhoneDataID { get; set; }
        public int? PhoneType { get; set; }
        public string PhoneNumber { get; set; }
        public Guid? ReferenceID { get; set; }
    }
}

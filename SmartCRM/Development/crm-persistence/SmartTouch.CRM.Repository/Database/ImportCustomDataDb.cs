using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTouch.CRM.Repository.Database
{
    public class ImportCustomData
    {
        [Key]
        public Int32 FieldID { get; set; }
        public int? FieldTypeID { get; set; }
        public string FieldValue { get; set; }
        public Guid? ReferenceID { get; set; }
    }
}

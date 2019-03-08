using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTouch.CRM.Repository.Database
{
    public class IndexData
    {
        [Key]
        public Guid ReferenceID { get; set; }
        public int EntityID { get; set; }
        public int IndexType { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? IndexedOn { get; set; }
        public int Status { get; set; }
        public bool IsPercolationNeeded { get; set; }
    }
}

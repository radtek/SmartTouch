using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
    public class ImageDomainsDb
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public byte ImageDomainID { get; set; }
        public string ImageDomain { get; set; }
        public bool Status { get; set; }
        public bool IsDefault { get; set; }
        
        [ForeignKey("User")]
        public int CreatedBy { get; set; }
        public UsersDb User { get; set; }
        
        public DateTime CreatedOn { get; set; }
        
        [ForeignKey("User1")]
        public int? LastModifiedBy { get; set; }
        public UsersDb User1 { get; set; }
        
        public DateTime? LastModifiedOn { get; set; }
    }
}

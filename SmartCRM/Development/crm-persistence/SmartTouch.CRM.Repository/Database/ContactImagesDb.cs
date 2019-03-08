using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
    [Table("ContactImages")]
    public class ContactImagesDb
    {
        [Key]
        public int ContactImageID { get; set; }
        public string OriginalName { get; set; }
        public string ImageContent { get; set; }
        public string ImageType { get; set; }
        public Guid StorageName { get; set; }
    }
}

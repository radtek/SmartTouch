using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
    public class FormTagsDb
    {
        [Key]
        public int FormTagID { get; set; }
        
        [ForeignKey("Tag")]
        public int TagID { get; set; }
        
        [ForeignKey("Form")]
        public int FormID { get; set; }
        
        public virtual TagsDb Tag { get; set; }
        public virtual FormsDb Form { get; set; }
    }
}

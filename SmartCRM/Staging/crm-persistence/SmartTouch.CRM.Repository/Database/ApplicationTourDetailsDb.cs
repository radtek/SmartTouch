using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
    public class ApplicationTourDetailsDb
    {
        [Key]
        public int ApplicationTourDetailsID { get; set; }

        [ForeignKey("Division")]
        public Int16 DivisionID { get; set; }
        public DivisionsDb Division { get; set; }

        [ForeignKey("Section")]
        public Int16 SectionID { get; set; }
        public SectionsDB Section { get; set; }

        public Int16 order { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }

        [ForeignKey("CreatedUser")]
        public int CreatedBy { get; set; }
        public UsersDb CreatedUser { get; set; }

        [ForeignKey("UpdatedUser")]
        public int LastUpdatedBy { get; set; }
        public UsersDb UpdatedUser { get; set; }

        public DateTime CreatedOn { get; set; }
        public DateTime LastUpdatedOn { get; set; }

        public string HTMLID { get; set; }
        public string PopUpPlacement { get; set; }
        public bool Status { get; set; }
    }
}

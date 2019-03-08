using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
    public class ContactNoteSummaryDb
    {
        [Key]
        public int ContactID { get; set; }
        public string NoteDetails { get; set; }
        public DateTime? LastNoteDate { get; set; }
    }
}

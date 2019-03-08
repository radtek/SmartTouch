using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
    public class OpportunityNoteMap
    {
        [Key]
        public int OpportunityNoteMapID { get; set; }
        [ForeignKey("Opportunities")]
        public virtual int OpportunityID { get; set; }
        public virtual OpportunitiesDb Opportunities { get; set; }
        [ForeignKey("Note")]
        public virtual int NoteID { get; set; }
        public virtual NotesDb Note { get; set; }

    }
}

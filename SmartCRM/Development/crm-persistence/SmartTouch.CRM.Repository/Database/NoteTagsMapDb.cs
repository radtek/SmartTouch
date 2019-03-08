using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTouch.CRM.Repository.Database
{
    public class NoteTagsMapDb
    {
        [Key]
        public int NoteTagMapID { get; set; }

        [ForeignKey("Note")]
        public virtual int NoteID { get; set; }
        public virtual NotesDb Note { get; set; }

        [ForeignKey("Tags")]
        public virtual int TagID { get; set; }
        public virtual TagsDb Tags { get; set; }
    }
}

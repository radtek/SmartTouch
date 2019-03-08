using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTouch.CRM.Repository.Database
{
    public class TagsDb
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TagID { get; set; }
        public string TagName { get; set; }
        public string Description { get; set; }
        //[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public int? Count { get; set; }
        public int AccountID { get; set; }
        public int? CreatedBy { get; set; }
        public bool? IsDeleted { get; set; }

        [NotMapped]
        public bool LeadScoreTag { get; set; }

        public ICollection<ActionsDb> Actions { get; set; }
        public ICollection<NotesDb> Notes { get; set; }
        public ICollection<CampaignsDb> Campaigns { get; set; }
    }      
}

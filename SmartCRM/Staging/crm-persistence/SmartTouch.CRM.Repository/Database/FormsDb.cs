using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Entities;

namespace SmartTouch.CRM.Repository.Database
{
    public class FormsDb
    {
        [Key]
        public int FormID { get; set; }
        public string Name { get; set; }
        public string Acknowledgement { get; set; }
        public AcknowledgementType AcknowledgementType { get; set; }
        public string HTMLContent { get; set; }

        [ForeignKey("Statuses")]
        public short Status { get; set; }
        public virtual StatusesDb Statuses { get; set; }

        [ForeignKey("Accounts")]
        public int AccountID { get; set; }
        public virtual AccountsDb Accounts { get; set; }

        [ForeignKey("LeadSource")]
        public short? LeadSourceID { get; set; }
        public virtual DropdownValueDb LeadSource { get; set; }
       
        public int CreatedBy { get; set; }       
        public int LastModifiedBy { get; set; }

        public bool? IsDeleted { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public int? Submissions { get; set; }

        [ForeignKey("CreatedBy")]
        public virtual UsersDb Users { get; set; }
        [ForeignKey("LastModifiedBy")]
        public virtual UsersDb Users1 { get; set; }

        public DateTime CreatedOn { get; set; }        
        public DateTime LastModifiedOn { get; set; }
        public bool IsAPIForm { get; set; }

        [NotMapped]
        public int AllSubmissions { get; set; }
        [NotMapped]
        public int UniqueSubmissions { get; set; }

        public ICollection<FormTagsDb> FormTags { get; set; }
        public ICollection<FormFieldsDb> FormFields { get; set; }
    }
}
